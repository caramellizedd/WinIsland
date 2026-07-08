using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using UserControl = System.Windows.Controls.UserControl;

namespace WinIsland.Controls
{
    public partial class TextMarquee : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(TextMarquee),
                new PropertyMetadata(string.Empty, OnTextChanged));

        // Pause (seconds) at each end before scrolling again
        public static readonly DependencyProperty PauseDurationProperty =
            DependencyProperty.Register(nameof(PauseDuration), typeof(double), typeof(TextMarquee),
                new PropertyMetadata(1.5));

        // Pixels per second scroll speed
        public static readonly DependencyProperty ScrollSpeedProperty =
            DependencyProperty.Register(nameof(ScrollSpeed), typeof(double), typeof(TextMarquee),
                new PropertyMetadata(40.0));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public double PauseDuration
        {
            get => (double)GetValue(PauseDurationProperty);
            set => SetValue(PauseDurationProperty, value);
        }

        public double ScrollSpeed
        {
            get => (double)GetValue(ScrollSpeedProperty);
            set => SetValue(ScrollSpeedProperty, value);
        }

        private enum State { Idle, PauseStart, ScrollingOut, PauseEnd, ScrollingBack }

        private State _state = State.Idle;
        private double _distance;      // total px the text needs to travel
        private double _elapsed;       // seconds spent in the current state
        private DateTime _lastTick;
        private bool _isRendering;

        private static readonly LinearGradientBrush FadeMaskBrush = CreateFadeMaskBrush();

        private static LinearGradientBrush CreateFadeMaskBrush()
        {
            var brush = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 0) };
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 0, 0, 0), 0.00));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0, 0, 0), 0.04));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0, 0, 0), 0.96));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 0, 0, 0), 1.00));
            brush.Freeze();
            return brush;
        }

        public TextMarquee()
        {
            InitializeComponent();
            Loaded += (_, __) => Restart();
            Unloaded += (_, __) => StopRendering();
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TextMarquee)d).Restart();
        }

        private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e) => Restart();

        private void PART_Text_SizeChanged(object sender, SizeChangedEventArgs e) => Restart();

        private void Restart()
        {
            StopRendering();
            TextTransform.X = 0;

            double containerWidth = RootGrid.ActualWidth;
            double textWidth = PART_Text.ActualWidth;

            // Not laid out yet -- Loaded/SizeChanged will call Restart again once it is.
            if (containerWidth <= 0 || textWidth <= 0)
            {
                _state = State.Idle;
                RootGrid.OpacityMask = null;
                return;
            }

            _distance = textWidth - containerWidth;

            if (_distance <= 0)
            {
                _state = State.Idle;
                Root.OpacityMask = null;
                return;
            }

            Root.OpacityMask = FadeMaskBrush;
            _elapsed = 0;
            _state = State.PauseStart;
            StartRendering();
        }

        private void StartRendering()
        {
            if (_isRendering) return;
            _isRendering = true;
            _lastTick = DateTime.Now;
            CompositionTarget.Rendering += OnRendering;
        }

        private void StopRendering()
        {
            if (!_isRendering) return;
            _isRendering = false;
            CompositionTarget.Rendering -= OnRendering;
        }

        private void OnRendering(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            double dt = (now - _lastTick).TotalSeconds;
            _lastTick = now;
            _elapsed += dt;

            double speed = Math.Max(ScrollSpeed, 1);
            double travelTime = _distance / speed;

            switch (_state)
            {
                case State.PauseStart:
                    if (_elapsed >= PauseDuration)
                    {
                        _elapsed = 0;
                        _state = State.ScrollingOut;
                    }
                    break;

                case State.ScrollingOut:
                    TextTransform.X = -Math.Min(_elapsed * speed, _distance);
                    if (_elapsed >= travelTime)
                    {
                        TextTransform.X = -_distance;
                        _elapsed = 0;
                        _state = State.PauseEnd;
                    }
                    break;

                case State.PauseEnd:
                    if (_elapsed >= PauseDuration)
                    {
                        _elapsed = 0;
                        _state = State.ScrollingBack;
                    }
                    break;

                case State.ScrollingBack:
                    TextTransform.X = -_distance + Math.Min(_elapsed * speed, _distance);
                    if (_elapsed >= travelTime)
                    {
                        TextTransform.X = 0;
                        _elapsed = 0;
                        _state = State.PauseStart;
                    }
                    break;
            }
        }
    }
}
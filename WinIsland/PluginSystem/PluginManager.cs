using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace WinIsland.PluginSystem
{
    public class PluginManager
    {
        public List<Page> islandPages = new List<Page>();
        public List<Assembly> plugins = new List<Assembly>();

        int pageIndex = 0;
        public void register(Assembly assembly)
        {
            plugins.Add(assembly);
        }
        public void registerPage(Page page)
        {
            islandPages.Add(page);
        }

        public void navigatePage(Frame contentFrame, bool reverse)
        {
            if (!reverse)
            {
                Navigate(islandPages.ToArray()[pageIndex], reverse, contentFrame);
                pageIndex++;
            }
            else if(pageIndex == islandPages.Count && !reverse)
            {
                pageIndex = 0;
                Navigate(islandPages.ToArray()[pageIndex], reverse, contentFrame);
                pageIndex++;
            }else if (reverse)
            {
                Console.WriteLine(pageIndex);
                Navigate(islandPages.ToArray()[pageIndex], reverse, contentFrame);
                pageIndex--;
            }
            else if (pageIndex == 0 && reverse)
            {
                pageIndex = islandPages.Count-1;
                Navigate(islandPages.ToArray()[pageIndex], reverse, contentFrame);
                pageIndex--;
            }
            if (pageIndex >= islandPages.Count)
            {
                pageIndex = 0;
            }else if(pageIndex < 0)
            {
                pageIndex = islandPages.Count-1;
            }
        }

        public void Navigate(object content, bool previous, Frame islandContent)
        {
            MainWindow.instance.prevAnim = previous;
            DoubleAnimation ta = new DoubleAnimation
            {
                From = 0,
                To = previous ? -MainWindow.instance.ActualWidth : MainWindow.instance.ActualWidth,
                Duration = new TimeSpan(0, 0, 0, 0, 250),
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseIn }
            };

            ta.Completed += (sender, e) =>
            {
                islandContent.Navigate(content);
            };


            if (Settings.instance.config.blurEverywhere)
            {
                DoubleAnimation blur = new DoubleAnimation
                {
                    From = 0,
                    To = 20,
                    Duration = new TimeSpan(0, 0, 0, 0, 300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                BlurEffect be = new BlurEffect();
                be.RenderingBias = RenderingBias.Performance;
                be.BeginAnimation(BlurEffect.RadiusProperty, blur);

                islandContent.Effect = be;
            }

            MainWindow.instance.frameAnimation.BeginAnimation(TranslateTransform.XProperty, ta);

        }
    }
}

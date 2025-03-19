using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinIsland
{
    public class Easing
    {
        public static double EaseInOutCubic(double t)
        {
            t = Math.Clamp(t, 0, 1); // Ensure t is within 0 and 1
            return t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;
        }
        public static double EaseOutBack(double t, double overshootRadius)
        {
            t = Math.Clamp(t, 0, 1); // Ensure t is between 0 and 1

            double overshoot = overshootRadius;  // Controls how much it overshoots, old value 1.70158
            double c3 = overshoot + 1;

            return 1 + c3 * Math.Pow(t - 1, 3) + overshoot * Math.Pow(t - 1, 2);
        }
    }
}

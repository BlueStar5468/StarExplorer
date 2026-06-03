using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Controls
{
    internal static class Animations
    {
        /// <summary>
        /// 渐变动画
        /// </summary>
        /// <returns></returns>
        public static Animation GetGradientAnimation(int Duration,Color color0, Color color1)
        {
            return new Animation
            {
                Duration = TimeSpan.FromMilliseconds(Duration),
                FillMode = FillMode.Forward,
                IterationCount = new IterationCount(1),
                Easing = new CubicEaseInOut(),
                Children =
                {
                    new KeyFrame()
                    {
                        Setters =
                        {
                        new Setter(Border.BackgroundProperty, new SolidColorBrush(color0))
                        },
                        KeyTime = TimeSpan.FromMilliseconds(0)
                    },
                    new KeyFrame()
                    {
                        Setters =
                        {
                        new Setter(Border.BackgroundProperty, new SolidColorBrush(color1))
                        },
                        KeyTime = TimeSpan.FromMilliseconds(Duration)
                    },
                }
            };
        }

    }
}

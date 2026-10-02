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
        public static Animation GetGradientAnimation(int Duration, Color color0, Color color1)
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

        //使用左边距和上边距来实现滑动动画
        public static Animation GetSlideAnimation(int Duration, bool DirectionFlag, double startLeft, double startTop, double endLeft, double endTop)
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
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(Canvas.TopProperty, startTop),
                            new Setter(Canvas.LeftProperty, startLeft),
                            new Setter(Canvas.OpacityProperty, DirectionFlag ? 0d : 1d)
                        },
                    },
                    new KeyFrame()
                    {
                        Cue = new Cue(1d),
                        Setters =
                        {
                            new Setter(Canvas.TopProperty, endTop),
                            new Setter(Canvas.LeftProperty, endLeft),
                            new Setter(Canvas.OpacityProperty, DirectionFlag ? 1d : 0d)
                        },
                    },
                }
            };
        }

        //使用右边距和下边距来实现滑动动画
        public static Animation GetSlideAnimation2(int Duration, bool DirectionFlag, double startRight, double startBottom, double endRight, double endBottom)
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
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(Canvas.RightProperty, startRight),
                            new Setter(Canvas.BottomProperty, startBottom),
                            new Setter(Canvas.OpacityProperty, DirectionFlag ? 0d : 1d)
                        },
                    },
                    new KeyFrame()
                    {
                        Cue = new Cue(1d),
                        Setters =
                        {
                            new Setter(Canvas.RightProperty, endRight),
                            new Setter(Canvas.BottomProperty, endBottom),
                            new Setter(Canvas.OpacityProperty, DirectionFlag ? 1d : 0d)
                        },
                    },
                }
            };
        }

        public static Animation GetCubicMoveAnimation(int Duration, double startLeft, double startTop, double endLeft, double endTop)
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
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(Canvas.TopProperty, startTop),
                            new Setter(Canvas.LeftProperty, startLeft)
                        },
                    },
                    new KeyFrame()
                    {
                        Cue = new Cue(1d),
                        Setters =
                        {
                            new Setter(Canvas.TopProperty, endTop),
                            new Setter(Canvas.LeftProperty, endLeft)
                        },
                    },
                }
            };
        }

        public static Animation GetCubicMoveAnimation2(int Duration, double startRight, double startBottom, double endRight, double endBottom)
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
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(Canvas.RightProperty, startRight),
                            new Setter(Canvas.BottomProperty, startBottom)
                        },
                    },
                    new KeyFrame()
                    {
                        Cue = new Cue(1d),
                        Setters =
                        {
                            new Setter(Canvas.RightProperty, endRight),
                            new Setter(Canvas.BottomProperty, endBottom)
                        },
                    },
                }
            };

        }

        public static Animation GetFadeAwayAnimation(int duration)
        {
            return new Animation
            {
                Duration = TimeSpan.FromMilliseconds(duration),
                FillMode = FillMode.Forward,
                IterationCount = new IterationCount(1),
                Easing = new CubicEaseInOut(),
                Children =
                {
                    new KeyFrame()
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(Canvas.OpacityProperty, 1d)
                        },
                    },
                    new KeyFrame()
                    {
                        Cue = new Cue(1d),
                        Setters =
                        {
                            new Setter(Canvas.OpacityProperty, 0d)
                        },
                    },
                }
            };
        }
        
    }
}

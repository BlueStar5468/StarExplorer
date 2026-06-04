using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using StarExplorer.Logic;
using System;
using System.Threading;
using System.Xml.Serialization;

namespace StarExplorer.Controls
{
    internal class TabControl : IDisposable
    {
        //动画
        Animation pointerEnteredAnimation;
        Animation pointerExitedAnimation;
        CancellationTokenSource? pointerEnteredAnimationCancelTokenSource;
        CancellationTokenSource? pointerExitedAnimationCancelTokenSource;
        //控件实例
        Border tab;
        int id;
        ITabContent dataContext;

        public TabControl(Color backGroundColor, int width, int height,int sideMargin,int CornerRadius, int imageSize, IImage icon, int imageMargin, string label, int closeButtonSize, int id, String HoverColor, ITabContent dataContext)
        {
            this.id = id;
            this.dataContext = dataContext;

            tab = new Border()
            {
                //Background = new SolidColorBrush(backGroundColor),
                Width = width,
                Height = height,
                Margin = new Thickness(sideMargin,0,sideMargin,0),
                CornerRadius = new CornerRadius(CornerRadius)
            };


            DockPanel dockPanel = new DockPanel();

            Image image = new Image()
            {
                //Source = icon,
                Width = imageSize,
                Height = imageSize,
                Margin = new Thickness(imageMargin),
            };
            
            Panel TextPanel = new Panel()
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                MaxWidth = width - imageSize - 2 * imageMargin,
            };
            TextBlock textBlock = new TextBlock()
            {
                //Text = label,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            };
            TextPanel.Children.Add(textBlock);

            Button button = new Button()
            {
                Background = Brushes.Transparent,
                Content = "X",
            };

            button.Click += (s, e) =>
            { TabClosed?.Invoke(this.id); };

            DockPanel.SetDock(image, Dock.Left);
            DockPanel.SetDock(button, Dock.Right);
            dockPanel.Children.Add(image);
            dockPanel.Children.Add(TextPanel);
            dockPanel.Children.Add(button);

            tab.Child = dockPanel;

            //属性绑定
            textBlock.DataContext = dataContext;
            textBlock.Bind(TextBlock.TextProperty, new Avalonia.Data.Binding(nameof(ITabContent.Label)));
            image.DataContext = dataContext;
            image.Bind(Image.SourceProperty, new Avalonia.Data.Binding(nameof(ITabContent.Icon)));
            tab.DataContext = dataContext;
            tab.Bind(Border.BackgroundProperty, new Avalonia.Data.Binding(nameof(ITabContent.BackgroundColor)));

            //创建动画
            pointerEnteredAnimation = Animations.GetGradientAnimation(200, backGroundColor, Color.Parse(HoverColor));
            pointerExitedAnimation = Animations.GetGradientAnimation(200, Color.Parse(HoverColor), backGroundColor);
            //事件绑定
            tab.PointerEntered += OnPointerEntered;
            tab.PointerExited += OnPointerExited;
            tab.PointerPressed += StopAnimations; //注：此处停止动画必须先于触发点击事件的处理程序，以确保在点击时动画被正确停止
            tab.PointerPressed += (s, e) =>
            {
                if (e.GetCurrentPoint(tab).Properties.IsLeftButtonPressed)
                {
                    TabClicked?.Invoke(this.id);
                    e.Handled = true;
                }
            };
        }

        //事件处理
        private void OnPointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (dataContext.IsSelected == true) return;//如果标签页被选中，鼠标移入时不播放动画
            pointerExitedAnimationCancelTokenSource?.Cancel();
            pointerExitedAnimationCancelTokenSource?.Dispose();
            pointerExitedAnimationCancelTokenSource = null;

            pointerEnteredAnimationCancelTokenSource?.Cancel();
            pointerEnteredAnimationCancelTokenSource?.Dispose();
            pointerEnteredAnimationCancelTokenSource = new CancellationTokenSource();

            try
            {
                pointerEnteredAnimation.RunAsync(tab, pointerEnteredAnimationCancelTokenSource.Token);
            }
            catch (OperationCanceledException){//动画被取消，安全地忽略异常
            }

            e.Handled = true;
        }

        private void OnPointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (dataContext.IsSelected == true) return;//如果标签页被选中，鼠标移出时不播放动画
            pointerEnteredAnimationCancelTokenSource?.Cancel();
            pointerEnteredAnimationCancelTokenSource?.Dispose();
            pointerEnteredAnimationCancelTokenSource = null;

            pointerExitedAnimationCancelTokenSource?.Cancel();
            pointerExitedAnimationCancelTokenSource?.Dispose();
            pointerExitedAnimationCancelTokenSource = new CancellationTokenSource();

            try
            {
                pointerExitedAnimation.RunAsync(tab, pointerExitedAnimationCancelTokenSource.Token);
            }
            catch (OperationCanceledException)
            {//动画被取消，安全地忽略异常
            }

            e.Handled = true;
        }

        private void StopAnimations(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            pointerEnteredAnimationCancelTokenSource?.Cancel();
            pointerEnteredAnimationCancelTokenSource?.Dispose();
            pointerEnteredAnimationCancelTokenSource = null;
            pointerExitedAnimationCancelTokenSource?.Cancel();
            pointerExitedAnimationCancelTokenSource?.Dispose();
            pointerExitedAnimationCancelTokenSource = null;
        }
        //事件
        public delegate void TabClosedEventHandler(int id);
        public event TabClosedEventHandler? TabClosed;
        public delegate void TabClickedEventHandler(int id);
        public event TabClickedEventHandler? TabClicked;
        public void Dispose()
        {
            //停止动画
            pointerEnteredAnimationCancelTokenSource?.Cancel();
            pointerEnteredAnimationCancelTokenSource?.Dispose();
            pointerEnteredAnimationCancelTokenSource = null;
            pointerExitedAnimationCancelTokenSource?.Cancel();
            pointerExitedAnimationCancelTokenSource?.Dispose();
            pointerExitedAnimationCancelTokenSource = null;
            //解除事件绑定
            tab.PointerEntered -= OnPointerEntered;
            tab.PointerExited -= OnPointerExited;
            tab.PointerPressed -= StopAnimations;
            tab.PointerPressed -= (s, e) =>
            {
                if (e.GetCurrentPoint(tab).Properties.IsLeftButtonPressed)
                {
                    TabClicked?.Invoke(this.id);
                    e.Handled = true;
                }
            };
        }

        public Border GetInstance()
        {
            return tab;
        }
    }
}

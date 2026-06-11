using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using StarExplorer.Logic;
using System;
using System.Threading;

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
        Border tabItem;
        int id;
        ITabDisplayContent displayContent;

        public TabControl(TabBarPanelData dataContext, ITabDisplayContent displayContent)
        {
            this.id = displayContent.Index;
            this.displayContent = displayContent;

            tabItem = new Border();
            tabItem.DataContext = dataContext;
            tabItem.Bind(Border.BackgroundProperty, new Avalonia.Data.Binding(nameof(ITabDisplayContent.CurrentBackgroundColor)) { Source = displayContent , Converter = new BrushConverter() } );
            tabItem.Bind(Border.CornerRadiusProperty, new Avalonia.Data.Binding(nameof(TabBarPanelData.ItemCornerRadius)) {Converter = new CornerRadiusConverter() });
            tabItem.Bind(Border.WidthProperty, new Avalonia.Data.Binding(nameof(TabBarPanelData.ItemWidth)));
            tabItem.Bind(Border.HeightProperty, new Avalonia.Data.Binding(nameof(TabBarPanelData.ItemHeight)));
            tabItem.Bind(Border.MarginProperty, new Avalonia.Data.Binding(nameof(TabBarPanelData.ItemMargin)) { Converter = new ThicknessConverter() });

            DockPanel dockPanel = new DockPanel();
            dockPanel.DataContext = dataContext;

            Image image = new Image();
            image.DataContext = displayContent;
            image.Bind(Image.SourceProperty, new Avalonia.Data.Binding(nameof(ITabDisplayContent.Icon)));
            image.Bind(Image.WidthProperty, new Avalonia.Data.Binding("DataContext.ImageSize") { RelativeSource = new Avalonia.Data.RelativeSource() {Mode= Avalonia.Data.RelativeSourceMode.FindAncestor, AncestorType = typeof(DockPanel) } } );
            image.Bind(Image.HeightProperty, new Avalonia.Data.Binding("DataContext.ImageSize") { RelativeSource = new Avalonia.Data.RelativeSource() { Mode = Avalonia.Data.RelativeSourceMode.FindAncestor, AncestorType = typeof(DockPanel) } });
            image.Bind(Image.MarginProperty, new Avalonia.Data.Binding("DataContext.ItemMargin") { RelativeSource = new Avalonia.Data.RelativeSource() { Mode = Avalonia.Data.RelativeSourceMode.FindAncestor, AncestorType = typeof(DockPanel) } , Converter = new ThicknessConverter() });

            Panel TextPanel = new Panel()
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            };
            TextPanel.DataContext = dataContext;
            TextPanel.Bind(Panel.MaxWidthProperty, new Avalonia.Data.Binding(nameof(TabBarPanelData.MaxTextWidth)));

            TextBlock textBlock = new TextBlock()
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            };
            textBlock.DataContext = displayContent;
            textBlock.Bind(TextBlock.TextProperty, new Avalonia.Data.Binding(nameof(ITabDisplayContent.Label)));

            TextPanel.Children.Add(textBlock);

            Button button = new Button()
            {
                Background = Brushes.Transparent,
                Content = "X",
            };
            button.DataContext = dataContext;
            button.Bind(Button.WidthProperty, new Avalonia.Data.Binding(nameof(TabBarPanelData.CloseButtonSize)));
            button.Bind(Button.HeightProperty, new Avalonia.Data.Binding(nameof(TabBarPanelData.CloseButtonSize)));

            button.Click += (s, e) =>
            { TabCloseButtonClicked?.Invoke(this.id); };

            DockPanel.SetDock(image, Dock.Left);
            DockPanel.SetDock(button, Dock.Right);
            dockPanel.Children.Add(image);
            dockPanel.Children.Add(TextPanel);
            dockPanel.Children.Add(button);

            tabItem.Child = dockPanel;          

            //创建动画
            pointerEnteredAnimation = Animations.GetGradientAnimation(200, dataContext.TabItemBackGroundColor, dataContext.HoverColor);
            pointerExitedAnimation = Animations.GetGradientAnimation(200, dataContext.HoverColor, dataContext.TabItemBackGroundColor);
            //事件绑定
            tabItem.PointerEntered += OnPointerEntered;
            tabItem.PointerExited += OnPointerExited;
            tabItem.PointerPressed += StopAnimations; //注：此处停止动画必须先于触发点击事件的处理程序，以确保在点击时动画被正确停止
            tabItem.PointerPressed += (s, e) =>
            {
                if (e.GetCurrentPoint(tabItem).Properties.IsLeftButtonPressed)
                {
                    TabClicked?.Invoke(this.id);
                    e.Handled = true;
                }
            };
        }

        //事件处理
        private void OnPointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (displayContent.IsSelected == true) return;//如果标签页被选中，鼠标移入时不播放动画
            pointerExitedAnimationCancelTokenSource?.Cancel();
            pointerExitedAnimationCancelTokenSource?.Dispose();
            pointerExitedAnimationCancelTokenSource = null;

            pointerEnteredAnimationCancelTokenSource?.Cancel();
            pointerEnteredAnimationCancelTokenSource?.Dispose();
            pointerEnteredAnimationCancelTokenSource = new CancellationTokenSource();

            try
            {
                pointerEnteredAnimation.RunAsync(tabItem, pointerEnteredAnimationCancelTokenSource.Token);
            }
            catch (OperationCanceledException){//动画被取消，安全地忽略异常
            }

            e.Handled = true;
        }

        private void OnPointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (displayContent.IsSelected == true) return;//如果标签页被选中，鼠标移出时不播放动画
            pointerEnteredAnimationCancelTokenSource?.Cancel();
            pointerEnteredAnimationCancelTokenSource?.Dispose();
            pointerEnteredAnimationCancelTokenSource = null;

            pointerExitedAnimationCancelTokenSource?.Cancel();
            pointerExitedAnimationCancelTokenSource?.Dispose();
            pointerExitedAnimationCancelTokenSource = new CancellationTokenSource();

            try
            {
                pointerExitedAnimation.RunAsync(tabItem, pointerExitedAnimationCancelTokenSource.Token);
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
        public event TabClosedEventHandler? TabCloseButtonClicked;
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
            tabItem.PointerEntered -= OnPointerEntered;
            tabItem.PointerExited -= OnPointerExited;
            tabItem.PointerPressed -= StopAnimations;
            tabItem.PointerPressed -= (s, e) =>
            {
                if (e.GetCurrentPoint(tabItem).Properties.IsLeftButtonPressed)
                {
                    TabClicked?.Invoke(this.id);
                    e.Handled = true;
                }
            };
        }

        public Border GetInstance()
        {
            return tabItem;
        }
    }
}

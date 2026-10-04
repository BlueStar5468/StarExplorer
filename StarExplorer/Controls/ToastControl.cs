using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
using StarExplorer.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StarExplorer.Controls
{
    internal class ToastControl
    {
        Panel root;
        ToastCanvasData dataContent;

        //超时定时器
        DispatcherTimer? autoCloseTimer;
        //动画
        Animation SlideIn;
        Animation SlideOut = null!;
        Animation FadeAway;
        CancellationTokenSource? SlideInCancellationToken;
        CancellationTokenSource? SlideOutCancellationToken;
        CancellationTokenSource? MoveCancellationToken;
        CancellationTokenSource? FadeAwayCancellationToken;
        //内部属性
        int id;
        double endPositionX;
        //事件
        public Action<int>? ToastClosed;
        private Action? CloseButtonCliked;

        public ToastControl(ToastCanvasData dataContent, Vector2d startPosition, Vector2d endPosition, Message message, int id)
        {
            this.dataContent = dataContent;
            this.id = id;
            //注意这个endPositionX是为了在SlideOut动画中使用的
            //并且请注意endPos并不是移动进来的end 而是移动出去的end 所以是startPosition.X
            this.endPositionX = startPosition.X;

            root = new Panel()
            {
                Background = Avalonia.Media.Brushes.Transparent,
            };

            //由于内容Border使用了Clip限制内容呈现区域，所以需要在外层再套一层Border来实现阴影效果
            Border shadowBorder = new Border()
            {
                Name = "ShadowBorder",
                Background = Avalonia.Media.Brushes.Transparent,
            };
            shadowBorder.DataContext = dataContent;
            shadowBorder.Bind(Border.CornerRadiusProperty, new Binding(nameof(dataContent.ToastCornerRadius)) { Converter = new CornerRadiusConverter() });
            shadowBorder.BoxShadow = new BoxShadows
            (
                new BoxShadow()
                {
                    Color = Color.FromArgb(0x60, 0, 0, 0),
                    OffsetX = 0,
                    OffsetY = 6,
                    Blur = 20,
                    Spread = 0,
                }
            );

            Border border = new Border();
            border.Name = "ContentBorder";
            //TODO:透明颜色待实现 使用alpha通道
            //注:这个边框向Border内部绘制
            border.Bind(Border.BorderBrushProperty, new Binding(nameof(dataContent.ToastBorderColor)) { Converter = new BrushConverter() });       //TODO:边框颜色设置待实现
            border.BorderThickness = new Avalonia.Thickness(1);     //TODO:边框厚度设置待实现
            var ClipRegion = new RectangleGeometry();   //Border剪裁区域 防止圆角溢出
            border.DataContext = dataContent;
            border.Bind(Border.BackgroundProperty, new Binding(nameof(dataContent.ToastBackgroundColor)) {Converter = new BrushConverter() });
            border.Bind(Border.WidthProperty, new Binding(nameof(dataContent.ToastWidth)));
            //TODO:自动高度设置待实现
            border.Bind(Border.HeightProperty, new Binding(nameof(dataContent.ToastHeight)));
            border.Bind(Border.CornerRadiusProperty, new Binding(nameof(dataContent.ToastCornerRadius)) { Converter = new CornerRadiusConverter()});


            ClipRegion.Rect = new Avalonia.Rect(0,0,border.Width,border.Height);//注:此属性对于Rect未实现绑定
            ClipRegion.Bind(RectangleGeometry.RadiusXProperty, new Binding(nameof(dataContent.ToastCornerRadius)) { Source = dataContent } );
            ClipRegion.Bind(RectangleGeometry.RadiusYProperty, new Binding(nameof(dataContent.ToastCornerRadius)) { Source = dataContent } );
            border.Clip = ClipRegion;

            DockPanel dockPanel = new DockPanel();

            Panel LabelPanel = new Panel();
            LabelPanel.Bind(Panel.BackgroundProperty, new Binding(nameof(dataContent.ToastLabelPanelColor)) { Converter = new BrushConverter() });

            Grid LabelGrid = new Grid()
            {
                Height = 30,
            };
            ColumnDefinition LabelRow = new ColumnDefinition();
            ColumnDefinition ButtonRow = new ColumnDefinition()
            {
                Width = (GridLength?)(new GridLengthConverter().Convert(LabelGrid.Height, typeof(GridLength), null, null)) == null ? (GridLength)(new GridLengthConverter().Convert(LabelGrid.Height, typeof(GridLength), null, null)) : new GridLength(1, GridUnitType.Auto)
            };
            LabelGrid.ColumnDefinitions.Add(LabelRow);  //0
            LabelGrid.ColumnDefinitions.Add(ButtonRow); //1

            Button closeButton = new Button()
            {
                Content = "X",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Background = Avalonia.Media.Brushes.Transparent,
                IsHitTestVisible = true,    //注：此属性确保在通知点击穿透的同时，关闭按钮仍然可以被点击
            };
            closeButton.Click += (s, e) => { this.CloseButtonCliked?.Invoke(); };

            TextBlock label = new TextBlock()
            {
                Text = message.label,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };

            Grid.SetColumn(label, 0);
            Grid.SetColumn(closeButton, 1);
            LabelGrid.Children.Add(label);
            LabelGrid.Children.Add(closeButton);

            LabelPanel.Children.Add(LabelGrid);

            Panel messagePanel = new Panel()
            {
                Background = Avalonia.Media.Brushes.Transparent,
            };

            ScrollViewer messageScrollViewer = new ScrollViewer()
            {
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                Background = Avalonia.Media.Brushes.Transparent,
            };

            TextBlock messageBlock = new TextBlock()
            {
                Text = message.message,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            };

            messageScrollViewer.Content = messageBlock;
            messagePanel.Children.Add(messageScrollViewer);

            DockPanel.SetDock(messagePanel, Dock.Bottom);
            DockPanel.SetDock(LabelPanel, Dock.Top);
            dockPanel.Children.Add(LabelPanel);
            dockPanel.Children.Add(messagePanel);

            border.Child = dockPanel;

            shadowBorder.Child = border;

            root.Children.Add(shadowBorder);

            //初始化动画
            SlideIn = Animations.GetSlideAnimation2(300, true, startPosition.X, startPosition.Y, endPosition.X, endPosition.Y);
            FadeAway = Animations.GetFadeAwayAnimation(300);
            //注:SlideOut和MoveTO动画由于目标位置取决于当前Toast位置，因此在运行时动态创建
            //事件绑定
            root.AttachedToVisualTree += async (s, e) => { await SlideInAnimation(); };
            this.CloseButtonCliked += async () => 
            {
                //注:await确保动画完成后再触发事件，避免在动画过程中移除控件导致异常
                await SlideOutAnimation();
                //像Canvas发送关闭事件，通知其移除该Toast控件
                ToastClosed?.Invoke(id);
            };

            //初始化超时定时器
            this.autoCloseTimer = new DispatcherTimer();
            autoCloseTimer.Interval = TimeSpan.FromMilliseconds(dataContent.ToastTimeMS);
            autoCloseTimer.Tick += async (s, e) =>
            {
                autoCloseTimer.Stop();
                await SlideOutAnimation();
                ToastClosed?.Invoke(id);
            };
            autoCloseTimer.Start();
        }

        //封装方法
        internal int GetID()
        {
            return id;
        }

        internal void MoveTo(Vector2d endPositon)
        {
            //取消之前的动画（如果有）
            this.StopAllAnimations();
            MoveCancellationToken = new CancellationTokenSource();
            var moveAnimation = Animations.GetCubicMoveAnimation2(300,Canvas.GetRight(this.root), Canvas.GetBottom(this.root), endPositon.X, endPositon.Y);
            try 
            {
                moveAnimation.RunAsync(root, MoveCancellationToken.Token);
            }
            catch (OperationCanceledException)
            {
                
            }
        }

        private async Task SlideInAnimation()
        {
            //取消之前的动画（如果有）
            this.StopAllAnimations();

            SlideInCancellationToken = new CancellationTokenSource();
            try 
            {
                await SlideIn.RunAsync(root, SlideInCancellationToken.Token);
            }
            catch (OperationCanceledException)
            {
                
            }
        }

        private async Task SlideOutAnimation()
        {
            //取消之前的动画（如果有）
            this.StopAllAnimations();

            SlideOutCancellationToken = new CancellationTokenSource();
            SlideOut = Animations.GetSlideAnimation2(300, false, Canvas.GetRight(this.root), Canvas.GetBottom(this.root), endPositionX, Canvas.GetBottom(this.root));
            try 
            {
                await SlideOut.RunAsync(root, SlideOutCancellationToken.Token);
            }
            catch (OperationCanceledException)
            {
                
            }
        }

        //本来设计给在通知满时自动消失动画
        //但若异步播放会还没开始播放就被删除 若同步播放又会阻塞UI线程 所以暂时弃用
        internal async Task FadeAwayAnimation()
        {
            //取消之前的动画（如果有）
            this.StopAllAnimations();
            FadeAwayCancellationToken = new CancellationTokenSource();
            try 
            {
                await FadeAway.RunAsync(root, FadeAwayCancellationToken.Token);
            }
            catch (OperationCanceledException)
            {

            }
        }

        private void StopAllAnimations()
        {
            SlideInCancellationToken?.Cancel();
            SlideInCancellationToken?.Dispose();
            SlideInCancellationToken = null;
            SlideOutCancellationToken?.Cancel();
            SlideOutCancellationToken?.Dispose();
            SlideOutCancellationToken = null;
            MoveCancellationToken?.Cancel();
            MoveCancellationToken?.Dispose();
            MoveCancellationToken = null;
            FadeAwayCancellationToken?.Cancel();
            FadeAwayCancellationToken?.Dispose();
            FadeAwayCancellationToken = null;
        }

        private void OnCloseButtonClicked()
        {
            ToastClosed?.Invoke(id);
        }

        internal Panel GetInstance()
        {
            return root;
        }

    }

    //仅供内部使用的消息结构体
    //会在外部调用时解耦成普通的string类型
    internal struct Message
    {
        internal string label;
        internal string message;
    }
}

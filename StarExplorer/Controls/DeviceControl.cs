using System;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Animation;
using System.Threading;
using Avalonia.Data;
using System.ComponentModel;

namespace StarExplorer.Controls
{
    internal class DeviceControl : IDisposable
    {
        Border deviceBorder;

        Animation? pointerEnteredAnimation;
        Animation? pointerExitedAnimation;
        CancellationTokenSource? pointerEnteredAnimationCancelTokenSource;
        CancellationTokenSource? pointerExitedAnimationCancelTokenSource;

        //事件
        public event Action<int>? doubleTapped;
        public event Action<int>? leftClicked;
        public event Action<int>? rightClicked;

        //内部委托
        PropertyChangedEventHandler? selectStatusUpdated;
        EventHandler<Avalonia.Input.PointerEventArgs>? pointerEnteredHandler;
        EventHandler<Avalonia.Input.PointerEventArgs>? pointerExitedHandler;
        EventHandler<Avalonia.Input.TappedEventArgs>? doubleTappedHandler;
        EventHandler<Avalonia.Input.PointerPressedEventArgs>? pointerPressedHandler;
        IDeviceDataContent deviceDataContent;
        public DeviceControl(DevicePanelData dataContext, IDeviceDataContent deviceDataContent)
        {
            this.deviceDataContent = deviceDataContent;

            deviceBorder = new Border();
            deviceBorder.DataContext = dataContext;
            deviceBorder.Bind(Border.WidthProperty, new Avalonia.Data.Binding(nameof(dataContext.ItemWidth)));
            deviceBorder.Bind(Border.HeightProperty, new Avalonia.Data.Binding(nameof(dataContext.ItemHeight)));
            deviceBorder.Bind(Border.CornerRadiusProperty, new Avalonia.Data.Binding(nameof(dataContext.ItemCornerRadius)) { Converter = new CornerRadiusConverter() });
            deviceBorder.Bind(Border.BackgroundProperty, new Avalonia.Data.Binding(nameof(dataContext.DeviceItemColor)) { Source = dataContext ,Converter = new BrushConverter() });

            Grid grid = new Grid();
            grid.DataContext = dataContext;
            
            //用于显示设备图标的列
            ColumnDefinition icon = new ColumnDefinition();

            //已完成:想办法把这个宽度绑到dataContext.ItemHeight上，保持图标为正方形（目前只能在构造函数里设置一次，无法响应 ItemHeight 的变化）
            icon.Bind(ColumnDefinition.WidthProperty, new Binding("ItemHeight") { Source = dataContext , Converter = new GridLengthConverter()});

            grid.ColumnDefinitions.Add(icon);

            Image image = new Image();
            image.DataContext = dataContext;
            image.Bind(Image.MarginProperty, new Avalonia.Data.Binding(nameof(dataContext.ItemMargin)) { Converter = new ThicknessConverter() });
            image.Bind(Image.SourceProperty, new Avalonia.Data.Binding(nameof(dataContext.DriveImage_Normal)));
            image.Stretch = Stretch.Fill;
            image.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            image.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;

            Grid.SetColumn(image, 0);
            grid.Children.Add(image);

            //用于显示设备名称和其他信息的列
            ColumnDefinition other = new ColumnDefinition();
            grid.ColumnDefinitions.Add(other);

            StackPanel stackPanel = new StackPanel();
            stackPanel.DataContext = dataContext;
            stackPanel.Bind(StackPanel.WidthProperty, new Avalonia.Data.Binding(nameof(dataContext.InfomationPanelWidth)));
            stackPanel.Spacing = 5;

            TextBlock deviceNameText = new TextBlock();
            deviceNameText.DataContext = deviceDataContent.Device;
            deviceNameText.Bind(TextBlock.TextProperty, new Avalonia.Data.Binding(nameof(deviceDataContent.Device.Title)));
            deviceNameText.Bind(TextBlock.FontSizeProperty, new Avalonia.Data.Binding("DataContext.TextSize") { RelativeSource = new RelativeSource() { Mode = RelativeSourceMode.FindAncestor, AncestorType = typeof(StackPanel) } });
            stackPanel.Children.Add(deviceNameText);

            ProgressBar spaceBar = new ProgressBar();
            spaceBar.DataContext = deviceDataContent.Device;
            spaceBar.Bind(ProgressBar.ValueProperty, new Avalonia.Data.Binding(nameof(deviceDataContent.Device.UsedSize)));
            spaceBar.Bind(ProgressBar.MaximumProperty, new Avalonia.Data.Binding(nameof(deviceDataContent.Device.TotalSize)));
            spaceBar.Bind(ProgressBar.CornerRadiusProperty, new Avalonia.Data.Binding(nameof(dataContext.ItemCornerRadius)) { Source = dataContext ,Converter = new CornerRadiusConverter() });
            spaceBar.Bind(ProgressBar.MaxWidthProperty, new Avalonia.Data.Binding(nameof(dataContext.InfomationPanelWidth)) { Source = dataContext });
            spaceBar.Bind(ProgressBar.MinWidthProperty, new Avalonia.Data.Binding(nameof(dataContext.InfomationPanelWidth)) { Source = dataContext });
            spaceBar.ShowProgressText = true;
            spaceBar.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            stackPanel.Children.Add(spaceBar);

            //TODO: 添加其他设备信息的显示，例如设备类型、容量等


            Grid.SetColumn(stackPanel, 1);
            grid.Children.Add(stackPanel);

            deviceBorder.Child = grid;

            //创建动画
            CreateAnimation(dataContext);

            //事件处理
            InitHandler(dataContext);

            deviceBorder.PointerEntered += pointerEnteredHandler; 
            deviceBorder.PointerExited += pointerExitedHandler;
            deviceBorder.DoubleTapped += this.doubleTappedHandler;
            deviceBorder.PointerPressed += this.pointerPressedHandler;
            deviceDataContent.PropertyChanged += this.selectStatusUpdated;
        }

        private void InitHandler(DevicePanelData dataContext)
        {
            //此方法用于初始化事件处理器，所有事件处理器均动态获取数据 无需后期绑定
            //此函数不应该在构造函数外调用
            //注意:Handler没有静态分析支持 请确保调用前指向Handler不为null
            this.pointerEnteredHandler = async (s, e) =>
            {
                if (deviceDataContent.IsSelected == true) return;
                if (pointerEnteredAnimation == null) return;
                //取消鼠标离开动画（如果正在运行）
                pointerExitedAnimationCancelTokenSource?.Cancel();
                pointerExitedAnimationCancelTokenSource?.Dispose();
                pointerExitedAnimationCancelTokenSource = null;
                //启动新动画
                pointerEnteredAnimationCancelTokenSource?.Cancel();
                pointerEnteredAnimationCancelTokenSource?.Dispose();
                pointerEnteredAnimationCancelTokenSource = new CancellationTokenSource();
                try
                {
                    await pointerEnteredAnimation.RunAsync(deviceBorder, pointerEnteredAnimationCancelTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    //动画被取消，安全地忽略异常
                }
                e.Handled = true;
            };

            this.pointerExitedHandler = async (s, e) =>
            {
                if (deviceDataContent.IsSelected == true) return;
                if (pointerExitedAnimation == null) return;
                //取消鼠标进入动画（如果正在运行）
                pointerEnteredAnimationCancelTokenSource?.Cancel();
                pointerEnteredAnimationCancelTokenSource?.Dispose();
                pointerEnteredAnimationCancelTokenSource = null;
                //启动新动画
                pointerExitedAnimationCancelTokenSource?.Cancel();
                pointerExitedAnimationCancelTokenSource?.Dispose();
                pointerExitedAnimationCancelTokenSource = new CancellationTokenSource();
                try
                {
                    await pointerExitedAnimation.RunAsync(deviceBorder, pointerExitedAnimationCancelTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    //动画被取消，安全地忽略异常
                }
                e.Handled = true;
            };

            this.doubleTappedHandler = (s, e) =>
            {
                doubleTapped?.Invoke(deviceDataContent.ID);
            };

            this.pointerPressedHandler = (s, e) =>
            {
                StopAllAnimations();
                if (e.GetCurrentPoint(deviceBorder).Properties.IsLeftButtonPressed)
                {
                    leftClicked?.Invoke(deviceDataContent.ID);
                }
                else if (e.GetCurrentPoint(deviceBorder).Properties.IsRightButtonPressed)
                {
                    rightClicked?.Invoke(deviceDataContent.ID);
                }
            };

            this.selectStatusUpdated = (s, e) =>
            {
                if (deviceDataContent.IsSelected)
                {
                    deviceBorder.Background = new SolidColorBrush(dataContext.SelectedColor);
                }
                else
                {
                    deviceBorder.Background = new SolidColorBrush(dataContext.DeviceItemColor);
                }
            };
        }

        private void CreateAnimation(DevicePanelData dataContext)
        {
            pointerEnteredAnimation = Animations.GetGradientAnimation(200, dataContext.DeviceItemColor, dataContext.HoverColor);
            pointerExitedAnimation = Animations.GetGradientAnimation(200, dataContext.HoverColor, dataContext.DeviceItemColor);
        }

        public Border GetInstance()
        {
            return deviceBorder;
        }

        internal int GetID()
        {
            return deviceDataContent.ID;
        }

        internal string GetDeviceName()
        {
            if (deviceDataContent.Device.Name != null)
            {
                return deviceDataContent.Device.Name; 
            }
            else
            {
                return "Unknown"; //设备可能未挂载
            }
        }

        internal bool GetDeviceStatus()
        {
            return deviceDataContent.Device.IsReady;
        }

        private void StopAllAnimations()
        {
            pointerEnteredAnimationCancelTokenSource?.Cancel();
            pointerEnteredAnimationCancelTokenSource?.Dispose();
            pointerEnteredAnimationCancelTokenSource = null;
            pointerExitedAnimationCancelTokenSource?.Cancel();
            pointerExitedAnimationCancelTokenSource?.Dispose();
            pointerExitedAnimationCancelTokenSource = null;
        }

        public void Dispose()
        {
            //释放资源
            pointerEnteredAnimationCancelTokenSource?.Cancel();
            pointerEnteredAnimationCancelTokenSource?.Dispose();
            pointerExitedAnimationCancelTokenSource?.Cancel();
            pointerExitedAnimationCancelTokenSource?.Dispose();
            //解除引用
            pointerEnteredAnimationCancelTokenSource = null;
            pointerExitedAnimationCancelTokenSource = null;
            //内部委托解除引用
            deviceBorder.PointerEntered -= pointerEnteredHandler;
            deviceBorder.PointerExited -= pointerExitedHandler;
            deviceBorder.DoubleTapped -= this.doubleTappedHandler;
            deviceBorder.PointerPressed -= this.pointerPressedHandler;
            deviceDataContent.PropertyChanged -= this.selectStatusUpdated;
            //事件释放
            doubleTapped = null;
            leftClicked = null;
            rightClicked = null;
        }
    }
}

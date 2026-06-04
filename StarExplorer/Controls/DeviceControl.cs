using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvaloniaUI;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Styling;
using Avalonia.Animation.Easings;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using StarExplorer.Shared;
using Avalonia.Data;

namespace StarExplorer.Controls
{
    internal class DeviceControl : IDisposable
    {
        Border deviceBorder;

        Animation pointerEnteredAnimation;
        Animation pointerExitedAnimation;
        CancellationTokenSource? pointerEnteredAnimationCancelTokenSource;
        CancellationTokenSource? pointerExitedAnimationCancelTokenSource;

        public DeviceControl(DevicePanelData dataContext, LogicDevices device)
        {
            deviceBorder = new Border();
            deviceBorder.DataContext = dataContext;
            deviceBorder.Bind(Border.WidthProperty, new Avalonia.Data.Binding(nameof(dataContext.ItemWidth)));
            deviceBorder.Bind(Border.HeightProperty, new Avalonia.Data.Binding(nameof(dataContext.ItemHeight)));
            deviceBorder.Bind(Border.CornerRadiusProperty, new Avalonia.Data.Binding(nameof(dataContext.ItemCornerRadius)) { Converter = new CornerRadiusConverter()});
            deviceBorder.Bind(Border.BackgroundProperty, new Avalonia.Data.Binding(nameof(dataContext.DeviceItemColor)) { Converter = new BrushConverter() });

            Grid grid = new Grid();
            grid.DataContext = dataContext;

            //用于显示设备图标的列
            ColumnDefinition icon = new ColumnDefinition();

            //TODO:想办法把这个宽度绑到dataContext.ItemHeight上，保持图标为正方形（目前只能在构造函数里设置一次，无法响应 ItemHeight 的变化）
            icon.Width = new(dataContext.ItemHeight);

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
            stackPanel.Spacing = 5;

            TextBlock deviceNameText = new TextBlock();
            deviceNameText.DataContext = device;
            deviceNameText.Bind(TextBlock.TextProperty, new Avalonia.Data.Binding(nameof(device.Title)));
            deviceNameText.Bind(TextBlock.FontSizeProperty, new Avalonia.Data.Binding("DataContext.TextSize") {  RelativeSource = new RelativeSource() { Mode = RelativeSourceMode.FindAncestor, AncestorType = typeof(StackPanel)} });
            stackPanel.Children.Add(deviceNameText);

            //TODO: 添加其他设备信息的显示，例如设备类型、容量等


            Grid.SetColumn(stackPanel, 1);
            grid.Children.Add(stackPanel);

            deviceBorder.Child = grid;

            //创建动画
            pointerEnteredAnimation = Animations.GetGradientAnimation(200, dataContext.DeviceItemColor, dataContext.HoverColor);
            pointerExitedAnimation = Animations.GetGradientAnimation(200, dataContext.HoverColor, dataContext.DeviceItemColor);

            //事件处理
            deviceBorder.PointerEntered += async (s, e) =>
            {
                //取消鼠标离开动画（如果正在运行）
                pointerExitedAnimationCancelTokenSource?.Cancel();
                pointerEnteredAnimationCancelTokenSource?.Dispose();
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
            deviceBorder.PointerExited += async (s, e) =>
            {
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
        }


        public Border GetInstance()
        {
            return deviceBorder;
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
        }
    }
}

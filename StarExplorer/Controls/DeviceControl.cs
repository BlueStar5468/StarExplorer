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

namespace StarExplorer.Controls
{
    internal class DeviceControl : IDisposable
    {
        Border deviceBorder;

        Animation pointerEnteredAnimation;
        Animation pointerExitedAnimation;
        CancellationTokenSource? pointerEnteredAnimationCancelTokenSource;
        CancellationTokenSource? pointerExitedAnimationCancelTokenSource;

        public DeviceControl(int fontSize, int width, int height, int cornerRadius, String backgroundColor, String HoverColor, String SelectedColor, IDeviceContent dataContext)
        {
            deviceBorder = new Border
            {
                Width = width,
                Height = height,
                CornerRadius = new CornerRadius(cornerRadius),
                Background = new SolidColorBrush(Color.Parse(backgroundColor)),
            };

            Grid grid = new Grid();

            //用于显示设备图标的列
            ColumnDefinition icon = new ColumnDefinition();
            icon.Width = new GridLength(height);

            grid.ColumnDefinitions.Add(icon);

            Image image = new Image();
            image.DataContext = dataContext;
            image.Margin = new Thickness(5);
            image.Bind(Image.SourceProperty, new Avalonia.Data.Binding(nameof(dataContext.Icon)));
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
            deviceNameText.Bind(TextBlock.TextProperty, new Avalonia.Data.Binding(nameof(dataContext.Name)));
            deviceNameText.FontSize = fontSize;
            stackPanel.Children.Add(deviceNameText);

            //TODO: 添加其他设备信息的显示，例如设备类型、容量等


            Grid.SetColumn(stackPanel, 1);
            grid.Children.Add(stackPanel);

            deviceBorder.Child = grid;

            //创建动画
            pointerEnteredAnimation = Animations.GetGradientAnimation(200, Color.Parse(backgroundColor), Color.Parse(HoverColor));
            pointerExitedAnimation = Animations.GetGradientAnimation(200, Color.Parse(HoverColor), Color.Parse(backgroundColor));

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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Media;
using StarExplorer.Shared;
using StarExplorer.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarExplorer.Controls
{
    internal class DivicesPanel
    {
        StackPanel root;
        public DivicesPanel(ExplorerData dataContext)
        {
            root = new StackPanel();
            //添加标签
            Border CatgoryBorder_1 = new Border()
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Child = new TextBlock()
                {
                    Text = "Local",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    FontSize = 24,
                    Foreground = Brush.Parse("rgb(0, 0, 0)")    //黑色字体
                },
            };
            CatgoryBorder_1.DataContext = dataContext;
            CatgoryBorder_1.Bind(Border.BackgroundProperty, new Avalonia.Data.Binding(nameof(dataContext.ThemeColor)));
            CatgoryBorder_1.Bind(Border.MarginProperty, new Avalonia.Data.Binding(nameof(dataContext.DisplayMargin)) { Converter = new ThicknessConverter() });
            CatgoryBorder_1.Bind(Border.CornerRadiusProperty, new Avalonia.Data.Binding(nameof(dataContext.ItemCornerRadius)) { Converter = new CornerRadiusConverter() });
            root.Children.Add(CatgoryBorder_1);

            if ((dataContext._coreData == null || dataContext._coreData.Devices.Count == 0))
            {
                //未识别到设备，显示错误提示
                Border border = new Border()
                {
                    //填充整个显示区
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                };
                border.DataContext = dataContext;
                border.Bind(Border.BackgroundProperty, new Avalonia.Data.Binding(nameof(dataContext.mainDisplayBackgroundColor)));
                border.Bind(Border.MarginProperty, new Avalonia.Data.Binding(nameof(dataContext.DisplayMargin)) { Converter = new ThicknessConverter() });
                border.Bind(Border.CornerRadiusProperty, new Avalonia.Data.Binding(nameof(dataContext.ItemCornerRadius)) { Converter = new CornerRadiusConverter() });

                TextBlock textBlock = new TextBlock()
                {
                    Text = "未识别到任何设备",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    FontSize = 32,
                    Foreground = Brush.Parse("rgb(0, 0, 0)")    //黑色字体
                };

                border.Child = textBlock;

                root.Children.Add(border);
            }
            else
            {
                //运行时动态设置模板和数据源，利用 ItemsControl 的虚拟化和模板功能高效显示设备列表
                var itemsControl = new ItemsControl
                {
                    ItemsPanel = new FuncTemplate<Panel?>(() =>
                    {
                        WrapPanel panel = new WrapPanel();
                        panel.DataContext = dataContext;
                        panel.Bind(WrapPanel.MarginProperty, new Avalonia.Data.Binding(nameof(dataContext.DisplayMargin)) {Converter = new ThicknessConverter()});
                        panel.Bind(WrapPanel.ItemSpacingProperty, new Avalonia.Data.Binding(nameof(dataContext.DeviceSpacing)));
                        return panel;
                    }),

                    // 使用强类型的 FuncDataTemplate：为每个 LogicDevices 构建一个 DeviceControl 的实例并返回其 Visual（Border）
                    ItemTemplate = new FuncDataTemplate<LogicDevices>((device, _) =>
                    {
                        IImage icon;
                        if (dataContext.driveImage_Normal == null) icon = null!;
                        else icon = dataContext.driveImage_Normal;
                        DeviceControl deviceControl = new DeviceControl(
                                device.GetTitle(),
                                16,
                                icon,
                                dataContext.deviceDisplayWidth,
                                dataContext.deviceDisplayHeight,
                                dataContext.itemCornerRadius,
                                dataContext.itemBackgroundColor,    //设备项背景色
                                dataContext.HoverBackgroundColor,   //设备项Hover背景色
                                dataContext.SelectedBackgroundColor //设备项选中背景色
                            );
                        //diviceControl的数据绑定会在其本身进行
                        return deviceControl.GetInstance();
                    }, supportsRecycling: true)
                };

                // 设置数据源（ItemsSorce 可以接受 List<T> 或 ObservableCollection<T>
                itemsControl.ItemsSource = dataContext._coreData.Devices;

                // 把 ItemsControl 放入主显示区
                root.Children.Add(itemsControl);
            }
        }

        public StackPanel GetInstance()
        {
            return root;
        }
    }
    public class ThicknessConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int margin)
            {
                return new Thickness(margin);
            }
            return new Thickness(0);
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Thickness thickness)
            {
                return (int)thickness.Left;
            }
            return 0;
        }
    }

    public class CornerRadiusConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int radius)
            {
                return new CornerRadius(radius);
            }
            return new CornerRadius(0);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is CornerRadius cornerRadius)
            {
                return (int)cornerRadius.TopLeft; 
            }
            return null;
        }
    }
}

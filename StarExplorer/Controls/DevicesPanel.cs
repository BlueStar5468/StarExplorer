using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Media;
using StarExplorer.Shared;
using StarExplorer.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;


namespace StarExplorer.Controls
{
    internal class DevicesPanel : IDisposable
    {
        //数据
        StackPanel root;
        ScrollViewer scrollViewer;
        Panel scrollViewerPanel;
        DevicePanelData data;

        //已不再维护所有Control引用，避免与Context不同步

        //事件
        public event Action<int>? ControlDoubleTapped;
        public event Action<int>? ControlLeftClicked;
        public event Action<int>? ControlRightClicked;
        public event Action? BackgroundPanelClicked;
        public event Action? PanelDisposed;
        //内部委托
        EventHandler<Avalonia.Input.PointerPressedEventArgs>? pointerPressedHandler;

        public DevicesPanel(DevicePanelData dataContext)
        {
            this.data = dataContext;

            scrollViewerPanel = new Panel()
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            };

            scrollViewer = new ScrollViewer()
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            };

            root = new StackPanel();
            //添加标签
            Border CatgoryBorder_1 = new Border()
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,

            };
            TextBlock text = new TextBlock()
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            text.DataContext = dataContext;
            text.Bind(TextBlock.TextProperty, new Avalonia.Data.Binding(nameof(dataContext.Localized_LocalLabel)));
            text.Bind(TextBlock.FontSizeProperty, new Avalonia.Data.Binding(nameof(dataContext.LabelFontSize)));
            text.Bind(TextBlock.ForegroundProperty, new Avalonia.Data.Binding(nameof(dataContext.LabelColor)) { Converter = new BrushConverter() });

            CatgoryBorder_1.Child = text;

            CatgoryBorder_1.DataContext = dataContext;
            CatgoryBorder_1.Bind(Border.BackgroundProperty, new Avalonia.Data.Binding(nameof(dataContext.ThemeColor)) { Converter = new BrushConverter() });
            CatgoryBorder_1.Bind(Border.MarginProperty, new Avalonia.Data.Binding(nameof(dataContext.ItemMargin)) { Converter = new ThicknessConverter() });
            CatgoryBorder_1.Bind(Border.CornerRadiusProperty, new Avalonia.Data.Binding(nameof(dataContext.ItemCornerRadius)) { Converter = new CornerRadiusConverter() });
            root.Children.Add(CatgoryBorder_1);

            if (dataContext.DevicesContent.Count == 0)
            {
                //未识别到设备，显示错误提示
                Border border = new Border()
                {
                    //填充整个显示区
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                };
                border.DataContext = dataContext;
                border.Bind(Border.BackgroundProperty, new Avalonia.Data.Binding(nameof(dataContext.ThemeColor)));
                border.Bind(Border.MarginProperty, new Avalonia.Data.Binding(nameof(dataContext.ItemMargin)) { Converter = new ThicknessConverter() });
                border.Bind(Border.CornerRadiusProperty, new Avalonia.Data.Binding(nameof(dataContext.ItemCornerRadius)) { Converter = new CornerRadiusConverter() });

                TextBlock textBlock = new TextBlock()
                {
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                };
                textBlock.DataContext = dataContext;
                textBlock.Bind(TextBlock.TextProperty, new Avalonia.Data.Binding(nameof(dataContext.Localized_NoDevicesLabel)));
                textBlock.Bind(TextBlock.ForegroundProperty, new Avalonia.Data.Binding(nameof(dataContext.TextColor)) { Converter = new BrushConverter() });
                textBlock.Bind(TextBlock.FontSizeProperty, new Avalonia.Data.Binding(nameof(dataContext.LabelFontSize)));

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
                        panel.Bind(WrapPanel.MarginProperty, new Avalonia.Data.Binding(nameof(dataContext.ItemMargin)) { Converter = new ThicknessConverter() });
                        panel.Bind(WrapPanel.ItemSpacingProperty, new Avalonia.Data.Binding(nameof(dataContext.ItemSpacing)));
                        return panel;
                    }),

                    // 使用强类型的 FuncDataTemplate：为每个 LogicDevices 构建一个 DeviceControl 的实例并返回其 Visual（Border）
                    ItemTemplate = new FuncDataTemplate<IDeviceDataContent>((device, _) =>
                    {
                        IImage icon;
                        if (dataContext.DriveImage_Normal == null) icon = null!;
                        else icon = dataContext.DriveImage_Normal;
                        DeviceControl deviceControl = new DeviceControl(dataContext, device);
                        //事件绑定
                        deviceControl.doubleTapped += (id) => ControlDoubleTapped?.Invoke(id);
                        deviceControl.leftClicked += (id) => ControlLeftClicked?.Invoke(id);
                        deviceControl.rightClicked += (id) => ControlRightClicked?.Invoke(id);
                        //diviceControl的数据绑定会在其本身进行
                        //用于释放资源的事件绑定
                        this.PanelDisposed += deviceControl.Dispose;
                        
                        return deviceControl.GetInstance();
                    }, supportsRecycling: true)
                };

                // 设置数据源（ItemsSorce 可以接受 List<T> 或 ObservableCollection<T>
                itemsControl.ItemsSource = dataContext.DevicesContent;

                // 把 ItemsControl 放入主显示区
                root.Children.Add(itemsControl);

                scrollViewer.Content = root;
                scrollViewerPanel.Children.Add(scrollViewer);

                root.Background = new SolidColorBrush(Colors.Transparent); //确保背景可点击

                //事件处理
                InitHandler();

                root.PointerPressed += pointerPressedHandler;
            }
        }

        private void InitHandler()
        {
            pointerPressedHandler = (s, e) =>
            {
                if (e.Properties.IsLeftButtonPressed && ReferenceEquals(e.Source, root))
                {
                    BackgroundPanelClicked?.Invoke();
                }
            };
        }

        public Panel GetInstance()
        {
            return scrollViewerPanel;
        }

        internal String GetDeviceNameById(int id)
        {
            foreach (var control in data.DevicesContent)
            {
                if (control.ID == id)
                {
                    if (control.Device.Name != null)
                    {
                        return control.Device.Name;
                    }
                    else
                    {
                        return "Unknown"; //设备可能未挂载
                    }
                }
            }
            return "UnFind"; //未找到设备
        }

        internal bool GetDeviceStatusById(int id)
        {
            foreach (var control in data.DevicesContent)
            {
                if (control.ID == id)
                {
                    return control.Device.IsReady;
                }
            }
            return false; //设备未就绪
        }

        public void Dispose()
        {
            //不要忘记释放 DeviceControl 的资源
            this.PanelDisposed?.Invoke();
            //本体Dispose
            root.PointerPressed -= pointerPressedHandler;
            this.PanelDisposed = null;
        }
    }
}

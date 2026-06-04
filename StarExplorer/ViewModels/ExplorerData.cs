using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using StarExplorer.Controls;
using StarExplorer.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StarExplorer.ViewModels
{
    public class ExplorerData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        //数据
        //依赖
        internal ICoreData? _coreData;
        internal ITabManager tabManager;
        internal IDeviceManager deviceManager;
        internal TabBarControlViewModel tabBarControlVM;
        //配置 注:以下配置尚未实现设置以及存储功能，且部分配置项可能会在未来的版本中被修改或移除，请谨慎使用和修改这些配置项。
        //      注:大部分设置项均未设置数据绑定，修改后需要重新创建标签页才能生效，未来可能会增加动态更新功能。
        internal StartLocation startLocation = StartLocation.Devices;
        
        private int _deviceDisplayWidth = 200;
        private int _deviceDisplayHeight = 50;
        private int deviceSpacing = 10;
        private int displayMargin = 10;
        private int itemCornerRadius = 10;
        internal String itemBackgroundColor = Colors.LightGray.ToString();

        private int tabItemWidth = 100;
        private int tabHeight = 40;
        internal String tabItemBackgroundColor = Colors.LightBlue.ToString();
        private int maxTabCount = 5;
        private int barMarginValue = 2;
        private int itemSideMargin = 3;
        private int itemHeightMargin = 2;
        private int BarConrnerRadius = 5;

        private int fontSizeLabel = 24;
        private int fontSizeText = 14;
        
        private String themeColor = Colors.AliceBlue.ToString();
        internal String mainDisplayBackgroundColor = Colors.Gray.ToString();
        internal String HoverBackgroundColor = "rgb(224,238,249)";
        internal String SelectedBackgroundColor = Colors.Blue.ToString();
        public IImage? driveImage_Normal;
        private String label = "StarExplorer";
        private ExplorerLayout layout;

        //显示区大小
        private double mainDisplayWidth;
        private double mainDisplayHeight;
        //当前显示的内容类型
        internal DisplayMode displayMode;

        //属性封装，带有变更通知
        public String Label { get => label; set { if (label != value) { label = value; OnPropertyChanged(nameof(Label)); } } }
        public ExplorerLayout Layout { get => layout; set { if (layout != value) { layout = value; OnPropertyChanged(nameof(Layout)); } } }
        public int TabItemWidth { get => tabItemWidth; set { if (tabItemWidth != value) { tabItemWidth = value; OnPropertyChanged(nameof(TabItemWidth)); } } }
        public int TabHeight { get => tabHeight; set { if (tabHeight != value) { tabHeight = value; OnPropertyChanged(nameof(TabHeight)); } } }
        public string ThemeColor { get => themeColor; set { if (themeColor != value) { themeColor = value; OnPropertyChanged(nameof(ThemeColor)); } } }
        public int DisplayMargin { get => displayMargin; set { if (displayMargin != value) { displayMargin = value; OnPropertyChanged(nameof(DisplayMargin)); } } }
        public int ItemCornerRadius { get => itemCornerRadius; set { if (itemCornerRadius != value) { itemCornerRadius = value; OnPropertyChanged(nameof(ItemCornerRadius)); } } }
        public int DeviceSpacing { get => deviceSpacing; set { if (deviceSpacing != value) { deviceSpacing = value; OnPropertyChanged(nameof(DeviceSpacing)); } } }
        public double MainDisplayWidth { get => mainDisplayWidth; set { if (mainDisplayWidth != value) { mainDisplayWidth = value; OnPropertyChanged(nameof(MainDisplayWidth)); } } }
        public double MainDisplayHeight { get => mainDisplayHeight; set { if (mainDisplayHeight != value) { mainDisplayHeight = value; OnPropertyChanged(nameof(MainDisplayHeight)); } } }
        public Size MainDisplaySize { get { return new Size(mainDisplayWidth, mainDisplayHeight); } }
        public int FontSizeLabel { get => fontSizeLabel; set { if (fontSizeLabel != value) { fontSizeLabel = value; OnPropertyChanged(nameof(fontSizeLabel)); } } }
        public int FontSizeText { get => fontSizeText; set { if (fontSizeText != value) { fontSizeText = value; OnPropertyChanged(nameof(fontSizeText)); } } }    
        public int deviceDisplayWidth { get => _deviceDisplayWidth; set { if (_deviceDisplayWidth != value) { _deviceDisplayWidth = value; OnPropertyChanged(nameof(deviceDisplayWidth)); } } }
        public int deviceDisplayHeight { get => _deviceDisplayHeight; set { if (_deviceDisplayHeight != value) { _deviceDisplayHeight = value; OnPropertyChanged(nameof(deviceDisplayHeight)); } } }
        //public Color ItemDisplayColor { get => Color.Parse(tabItemBackgroundColor); set { if (value.ToString() != tabItemBackgroundColor) { tabItemBackgroundColor = value.ToString(); OnPropertyChanged(nameof(ItemDisplayColor)); } } }
        public Color ItemBackgroundColor { get => Color.Parse(itemBackgroundColor); set { if (value.ToString() != itemBackgroundColor) { itemBackgroundColor = value.ToString(); OnPropertyChanged(nameof(ItemBackgroundColor)); } } }
        public IImage? DriveImage_Normal { get => driveImage_Normal; set { if (value != driveImage_Normal) { driveImage_Normal = value; OnPropertyChanged(nameof(DriveImage_Normal)); } } }
        public TabBarControlViewModel TabBarControlVM { get => tabBarControlVM; }
        public int BarMarginValue { get => barMarginValue; set { if (barMarginValue != value) { barMarginValue = value; OnPropertyChanged(nameof(BarMarginValue)); } } }
        public int ItemSideMargin { get => itemSideMargin; set { if (itemSideMargin != value) { itemSideMargin = value; OnPropertyChanged(nameof(ItemSideMargin)); } } }
        public int ItemHeightMargin { get => itemHeightMargin; set { if (itemHeightMargin != value) { itemHeightMargin = value; OnPropertyChanged(nameof(ItemHeightMargin)); } } }
        public int BarCornerRadius { get => BarConrnerRadius; set { if (BarConrnerRadius != value) { BarConrnerRadius = value; OnPropertyChanged(nameof(BarCornerRadius)); } } }
        public int MaxTabCount { get => maxTabCount; set { if (maxTabCount != value) { maxTabCount = value; OnPropertyChanged(nameof(MaxTabCount)); } } }

        //事件
        public event Action? DisplayContentChanged;

        //设计时数据构造器
        public static ExplorerData DesignInstance => new ExplorerData()
        {
            Label = "StarExplorer (Design Mode)",
            Layout = ExplorerLayout.Desktop
        };

        public ExplorerData() 
        {
            //子模块实例化
            tabManager = new TabManager(this.maxTabCount);
            //控件VM实例化
            deviceManager = new DeviceManager();
            tabBarControlVM = new TabBarControlViewModel(this);
            //事件绑定
            tabManager.TabChanged += OnSelectedTabChanged;
        }

        internal async Task LoadResorces()
        {
            driveImage_Normal = await GetDeviceIcon();
            RefreshDevice();
        }

        //向UI线程委托修改前端属性的事件通知，确保线程安全
        private void OnPropertyChanged(string propertyName)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });

#if DEBUG
            Debug.WriteLine($"MainDisplaySize:{MainDisplayWidth},{MainDisplayHeight}");
#endif
        }

        //关闭所有标签页后退出应用程序
        public void CheckExit(int id)
        {
            if (tabManager.TabCount == 0)
            {
                _coreData?.Exit();
            }
        }

        //事件处理方法
        private void OnSelectedTabChanged(int sourceID,int targetId)
        {
            foreach (var tab in tabManager.tabContents)
            {
                tab.IsSelected = false;
                tab.BackgroundColor = tabItemBackgroundColor;
            }
            tabManager.GetTabByID(targetId).BackgroundColor = SelectedBackgroundColor;
            tabManager.GetTabByID(targetId).IsSelected = true;
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                DisplayContentChanged?.Invoke();
            });
        }

        //封装方法
        public int GetSelectedPanelID()
        {
            return tabManager.CurrentTabId;
        }

        public Panel GetSelectedPanelContent()
        {
            return tabManager.GetSelectedTab().Content;
        }

        public Panel GetPanelContentByID(int id)
        {
            return tabManager.GetTabByID(id).Content;
        }

        public void MainDisplayPanelSizeBind(object? sender, SizeChangedEventArgs e)
        {
            MainDisplayWidth = e.NewSize.Width;
            MainDisplayHeight = e.NewSize.Height;
        }

        internal void RefreshDevice()
        {
            deviceManager.RefreshDevices(_coreData?.Devices ?? new List<LogicDevices>(), driveImage_Normal!);
        }

        //从文件路径加载 IImage（Bitmap）
        internal async Task<IImage> LoadBitmapFromPath(string path)
        {
            //使用文件流以避免锁定文件
            await using var fs = System.IO.File.OpenRead(path);
            return new Avalonia.Media.Imaging.Bitmap(fs);
        }

        //获取设备图标的 IImage 对象
        internal async Task<IImage> GetDeviceIcon()
        {
            IImage result;
            try
            {
                result = await Task.Run(() => LoadBitmapFromPath("Assets/drive_icon.png"));
            }
            catch (Exception)
            {
                //FallBack图标
                var bitmap = await Task.Run(() => new Avalonia.Media.Imaging.Bitmap(AssetLoader.Open(new Uri($"avares://StarExplorer/Assets/avalonia-logo.ico"))));
                result = bitmap;
            }
            return result;
        }
    }
}

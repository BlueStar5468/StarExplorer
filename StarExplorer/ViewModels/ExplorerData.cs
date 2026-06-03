using Avalonia.Media;
using Avalonia.Platform;
using StarExplorer.Controls;
using StarExplorer.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        //配置 注:以下配置尚未实现设置以及存储功能，且部分配置项可能会在未来的版本中被修改或移除，请谨慎使用和修改这些配置项。
        internal int deviceDisplayWidth = 200;
        internal int deviceDisplayHeight = 50;
        internal int deviceSpacing = 10;
        internal int displayMargin = 10;
        internal int itemCornerRadius = 10;
        private int tabWidth = 100;
        private int tabHeight = 30;
        private int maxTabCount = 5;
        internal StartLocation startLocation = StartLocation.Devices;
        private String themeColor = Colors.AliceBlue.ToString();
        internal String mainDisplayBackgroundColor = Colors.Gray.ToString();
        internal String HoverBackgroundColor = "rgb(224,238,249)";
        internal String SelectedBackgroundColor = Colors.Blue.ToString();
        internal String itemBackgroundColor = Colors.LightGray.ToString();
        internal String tabBackgroundColor = Colors.LightBlue.ToString();
        public IImage? driveImage_Normal;
        private ObservableCollection<LogicDevices> devices { get; set; } = new ObservableCollection<LogicDevices>();

        //主要显示区大小
        internal Avalonia.Size mainDisplaySize;
        //当前显示的内容类型
        internal DisplayMode displayMode;


        //设计时数据构造器
        public static ExplorerData DesignInstance => new ExplorerData()
        {
            Label = "StarExplorer (Design Mode)",
            Layout = ExplorerLayout.Desktop
        };

        public ExplorerData() 
        {
            tabManager = new TabManager(this.maxTabCount);
        }

        internal async Task LoadResorces()
        {
            driveImage_Normal = await GetDeviceIcon();
        }

        //刷新设备列表数据，供UI调用
        internal void RefreshDevice()
        {
            ObservableCollection<LogicDevices> newDevices = new ObservableCollection<LogicDevices>(_coreData?.Devices ?? new List<LogicDevices>());
            devices = newDevices;
        }

        //向UI线程委托修改前端属性的事件通知，确保线程安全
        private void OnPropertyChanged(string propertyName)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        private String label = "StarExplorer";
        private ExplorerLayout layout;
        public String Label
        {
            get => label;
            set
            {
                if (label != value)
                {
                    label = value;
                    OnPropertyChanged(nameof(Label));
                }
            }
        }

        public ExplorerLayout Layout { get => layout; set { if (layout != value) { layout = value; OnPropertyChanged(nameof(Layout)); } } }
        public int TabWidth { get => tabWidth; set { if (tabWidth != value) { tabWidth = value; OnPropertyChanged(nameof(TabWidth)); } } }
        public int TabHeight { get => tabHeight; set { if (tabHeight != value) { tabHeight = value; OnPropertyChanged(nameof(TabHeight)); } } }
        public string ThemeColor { get => themeColor; set { if (themeColor != value) { themeColor = value; OnPropertyChanged(nameof(ThemeColor)); } } }
        public int DisplayMargin { get => displayMargin; set { if (displayMargin != value) { displayMargin = value; OnPropertyChanged(nameof(DisplayMargin)); } } }
        public int ItemCornerRadius { get => itemCornerRadius; set { if (itemCornerRadius != value) { itemCornerRadius = value; OnPropertyChanged(nameof(ItemCornerRadius)); } } }
        public int DeviceSpacing { get => deviceSpacing; set { if (deviceSpacing != value) { deviceSpacing = value; OnPropertyChanged(nameof(DeviceSpacing)); } } }
        //关闭所有标签页后退出应用程序
        public void CheckExit(int id)
        {
            if (tabManager.TabCount == 0)
            {
                _coreData?.Exit();
            }
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

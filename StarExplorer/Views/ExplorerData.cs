using Avalonia.Media;
using Avalonia.Platform;
using StarExplorer.Logic;
using StarExplorer.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace StarExplorer.Views
{
    public class ExplorerData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        

        //数据
        //依赖
        internal ITabManager tabManager;
        //配置 注:以下配置尚未实现设置以及存储功能，且部分配置项可能会在未来的版本中被修改或移除，请谨慎使用和修改这些配置项。
        internal int deviceDisplayWidth = 200;
        internal int deviceDisplayHeight = 50;
        internal int deviceSpacing = 10;
        internal int displayMargin = 10;
        internal int deviceItemCornerRadius = 10;
        private int tabWidth = 100;
        private int tabHeight = 30;
        private int maxTabCount = 5;
        internal StartLocation startLocation = StartLocation.Devices;
        private string themeColor = Colors.AliceBlue.ToString();
        internal string mainDisplayBackgroundColor = Colors.Gray.ToString();
        internal string HoverBackgroundColor = "rgb(224,238,249)";
        internal string SelectedBackgroundColor = Colors.Blue.ToString();
        internal string itemBackgroundColor = Colors.LightGray.ToString();
        internal string tabBackgroundColor = Colors.LightBlue.ToString();
        public IImage? driveImage_Normal;
        public ObservableCollection<LogicDevices> devices { get; set; } = new ObservableCollection<LogicDevices>();

        //当前显示的内容类型
        internal DisplayMode displayMode;


        //设计时数据构造器
        public static ExplorerData DesignInstance => new ExplorerData()
        {
            Label = "StarExplorer (Design Mode)",
            Layout = ExplorerLayout.Desktop
        };
        public ExplorerData() { }

        public ExplorerData(ISettings settings, ICoreData coreData) 
        {
            tabManager = new TabManager(maxTabCount);
            
            InitSettings(settings); 
            BindSettings(settings);
            InitAndBindResorces(coreData);
        }


        private void InitSettings(ISettings settings)
        {
            deviceDisplayWidth = settings.DeviceDisplayWidth;
            deviceDisplayHeight = settings.DeviceDisplayHeight;
            deviceSpacing = settings.DeviceSpacing;
            displayMargin = settings.DevicePanelMargin;
            deviceItemCornerRadius = settings.DeviceCornerRadius;
            ThemeColor = settings.ThemeColor;
            mainDisplayBackgroundColor = settings.WindowDisplayBackgroundColor;

            //以下是临时数据
            driveImage_Normal = settings.DriveImage_Normal;
        }
        private void BindSettings(ISettings settings)
        {
            settings.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(settings.DeviceDisplayWidth):
                        deviceDisplayWidth = settings.DeviceDisplayWidth;
                        break;
                    case nameof(settings.DeviceDisplayHeight):
                        deviceDisplayHeight = settings.DeviceDisplayHeight;
                        break;
                    case nameof(settings.DeviceSpacing):
                        DeviceSpacing = settings.DeviceSpacing;
                        break;
                    case nameof(settings.DevicePanelMargin):
                        DisplayMargin = settings.DevicePanelMargin;
                        break;
                    case nameof(settings.DeviceCornerRadius):
                        DeviceItemCornerRadius = settings.DeviceCornerRadius;
                        break;
                    case nameof(settings.ThemeColor):
                        ThemeColor = settings.ThemeColor;
                        break;
                    case nameof(settings.WindowDisplayBackgroundColor):
                        mainDisplayBackgroundColor = settings.WindowDisplayBackgroundColor;
                        break;
                    case nameof(settings.DriveImage_Normal):
                        driveImage_Normal = settings.DriveImage_Normal;
                        break;
                }
            };
        }

        private void InitAndBindResorces(ICoreData coreData)
        {
            devices = new ObservableCollection<LogicDevices>(coreData.Devices);

            coreData.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(coreData.Devices))
                {
                    devices = new ObservableCollection<LogicDevices>(coreData.Devices);
                    OnPropertyChanged(nameof(devices));
                }
            };
        }

        //向UI线程委托修改前端属性的事件通知，确保线程安全
        private void OnPropertyChanged(string propertyName)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        private string label = "StarExplorer";
        private ExplorerLayout layout;
        public string Label
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
        public int DeviceItemCornerRadius { get => deviceItemCornerRadius; set { if (deviceItemCornerRadius != value) { deviceItemCornerRadius = value; OnPropertyChanged(nameof(DeviceItemCornerRadius)); } } }
        public int DeviceSpacing { get => deviceSpacing; set { if (deviceSpacing != value) { deviceSpacing = value; OnPropertyChanged(nameof(DeviceSpacing)); } } }

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

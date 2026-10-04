using Avalonia.Media;
using Avalonia.Platform;
using StarExplorer.Shared;
using System;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace StarExplorer.Logic
{
    internal class Settings : ISettings, INotifyPropertyChanged
    {
        #region 设置存储区

        #region tab栏设置
        int tabTotalHeight = 40;
        int tabBarMargin = 3;
        int tabBarCornerRadius = 5;
        int tabItemMargin = 2;
        int tabItemCornerRadius = 5;
        int tabItemWidth = 100;
        int tabItemHeight = 30;
        int maxTabCount = 5;
        #endregion

        #region 设备显示模式的显示区设置
        int panelMargin = 5;
        int deviceDisplayWidth = 200;
        int deviceDisplayHeight = 50;
        int deviceSpacing = 10;
        int deviceCornerRadius = 10;

        #endregion 文件显示模式的显示区设置
        int fileItemHeight = 40;
        int fileItemSpacing = 10;
        int fileItemInternalMargin = 5;
        int fileItemNameWidth = 200;

        IImage? folderIcon;
        IImage? folderLinkIcon;
        IImage? fileIcon;

        #region 

        #endregion

        #region 子窗口设置
        int sideBarWidth = 200;
        int adressBoxHeight = 30;
        int adressBoxCornerRadius = 5;
        #endregion

        #region Toast通知设置
        int maxToastCount = 5;
        int toastDurationMS = 3000;
        int toastSpacing = 10;
        int toastWidth = 150;
        int toastHeight = 75;
        int toastCornerRadius = 5;
        bool toastAutoHeight = false;
        int toastTimeMS = 3000;
        #endregion

        #region 全局设置
        StartLocation startLocation = StartLocation.Devices;
        private int textFontSize = 14;
        private int labelFontSize = 20;


        #endregion

        #region 配色设置
        string themeColor = Colors.AliceBlue.ToString();     //主题色 大部分控件的背景板颜色
        string windowDisplayBackgroundColor = Colors.Gray.ToString();//窗口的底色 主要显示区背景板会与其保持同色

        string hoverBackgroundColor = "rgb(224,238,249)";   //Hover背景色 可选择控件在Hover时的背景色
        string selectedBackgroundColor = Colors.Blue.ToString();//选中背景色 可选择控件在选中时的背景色
        string deviceBackgroundColor = Colors.LightGray.ToString();//设备项背景色 设备显示模式下设备项的背景色
        string tabItemBackgroundColor = Colors.LightBlue.ToString();//标签页标的背景色 

        string devicePanelLabelTextColor = Colors.Black.ToString(); //设备显示模式下显示区标签的文字颜色
        string devicePanelTextColor = Colors.Black.ToString(); //设备显示模式下显示区的文字颜色

        string toastBackGroundColor = Colors.LightGray.ToString(); //toast通知的背景色
        string toastBorderColor = Colors.Aqua.ToString(); //toast通知的边框颜色
        string toastLabelPanelColor = Colors.Aqua.ToString(); //toast通知的标题栏颜色
        #endregion 图片资源
        IImage? driveImage_System; 
        IImage? driveImage_Normal;


        #endregion

        #region 封装的属性

        #region tab栏设置
        public int TabTotalHeight { get => tabTotalHeight; set { tabTotalHeight = value; OnPropertyChanged(nameof(TabTotalHeight)); } }
        public int TabBarMargin { get => tabBarMargin; set { tabBarMargin = value; OnPropertyChanged(nameof(TabBarMargin)); } }
        public int TabBarCornerRadius { get => tabBarCornerRadius; set { tabBarCornerRadius = value; OnPropertyChanged(nameof(TabBarCornerRadius)); } }
        public int TabItemMargin { get => tabItemMargin; set { tabItemMargin = value; OnPropertyChanged(nameof(TabItemMargin)); } }
        public int TabItemCornerRadius { get => tabItemCornerRadius; set { tabItemCornerRadius = value; OnPropertyChanged(nameof(TabItemCornerRadius)); } }
        public int TabItemWidth { get => tabItemWidth; set { tabItemWidth = value; OnPropertyChanged(nameof(TabItemWidth)); } }
        public int TabItemHeight { get => tabItemHeight; set { tabItemHeight = value; OnPropertyChanged(nameof(TabItemHeight)); } }
        public int MaxTabCount { get => maxTabCount; set { maxTabCount = value; OnPropertyChanged(nameof(MaxTabCount)); } }
        #endregion

        #region 设备显示模式的显示区设置
        public int DevicePanelMargin { get => panelMargin; set { panelMargin = value; OnPropertyChanged(nameof(DevicePanelMargin)); } }
        public int DeviceDisplayWidth { get => deviceDisplayWidth; set { deviceDisplayWidth = value; OnPropertyChanged(nameof(DeviceDisplayWidth)); } }
        public int DeviceDisplayHeight { get => deviceDisplayHeight; set { deviceDisplayHeight = value; OnPropertyChanged(nameof(DeviceDisplayHeight)); } }
        public int DeviceSpacing { get => deviceSpacing; set { deviceSpacing = value; OnPropertyChanged(nameof(DeviceSpacing)); } }
        public int DeviceCornerRadius { get => deviceCornerRadius; set { deviceCornerRadius = value; OnPropertyChanged(nameof(DeviceCornerRadius)); } }
        #endregion

        #region 文件显示模式的显示区设置
        public int FileItemHeight { get => fileItemHeight; set { fileItemHeight = value; OnPropertyChanged(nameof(FileItemHeight)); } }
        public int FileItemSpacing { get => fileItemSpacing; set { fileItemSpacing = value; OnPropertyChanged(nameof(FileItemSpacing)); } }
        public int FileItemInternalMargin { get => fileItemInternalMargin; set { fileItemInternalMargin = value; OnPropertyChanged(nameof(FileItemInternalMargin)); } }
        public int FileItemNameWidth { get => fileItemNameWidth; set { fileItemNameWidth = value; OnPropertyChanged(nameof(FileItemNameWidth)); } }

        public IImage? FolderIcon { get => folderIcon; set { folderIcon = value; OnPropertyChanged(nameof(FolderIcon)); } }
        public IImage? FolderLinkIcon { get => folderLinkIcon; set { folderLinkIcon = value; OnPropertyChanged(nameof(FolderLinkIcon)); } }
        public IImage? FileIcon { get => fileIcon; set { fileIcon = value; OnPropertyChanged(nameof(FileIcon)); } }


        #endregion


        #region 子窗口设置
        public int SideBarWidth { get => sideBarWidth; set { sideBarWidth = value; OnPropertyChanged(nameof(SideBarWidth)); } }
        public int AdressBoxHeight { get => adressBoxHeight; set { adressBoxHeight = value; OnPropertyChanged(nameof(AdressBoxHeight)); } }
        public int AdressBoxCornerRadius { get => adressBoxCornerRadius; set { adressBoxCornerRadius = value; OnPropertyChanged(nameof(AdressBoxCornerRadius)); } }
        #endregion

        #region toast通知设置
        public int MaxToastCount { get => maxToastCount; set { maxToastCount = value; OnPropertyChanged(nameof(MaxToastCount)); } }
        public int ToastDurationMS { get => toastDurationMS; set { toastDurationMS = value; OnPropertyChanged(nameof(ToastDurationMS)); } }
        public int ToastSpacing { get => toastSpacing; set { toastSpacing = value; OnPropertyChanged(nameof(ToastSpacing)); } }
        public int ToastWidth { get => toastWidth; set { toastWidth = value; OnPropertyChanged(nameof(ToastWidth)); } }
        public int ToastHeight { get => toastHeight; set { toastHeight = value; OnPropertyChanged(nameof(ToastHeight)); } }
        public int ToastCornerRadius { get => toastCornerRadius; set { toastCornerRadius = value; OnPropertyChanged(nameof(ToastCornerRadius)); } }
        public bool ToastAutoHeight { get => toastAutoHeight; set { toastAutoHeight = value; OnPropertyChanged(nameof(ToastAutoHeight)); } }
        public int ToastTimeMS { get => toastTimeMS; set { toastTimeMS = value; OnPropertyChanged(nameof(ToastTimeMS)); } }
        #endregion

        #region 全局设置
        public StartLocation StartLocation { get => startLocation; set { startLocation = value; OnPropertyChanged(nameof(StartLocation)); } }
        #endregion
        public int TextFontSize { get => textFontSize; set { textFontSize = value; OnPropertyChanged(nameof(TextFontSize)); } }
        public int LabelFontSize { get => labelFontSize; set { labelFontSize = value; OnPropertyChanged(nameof(LabelFontSize)); } }


        #region 配色设置
        public string ThemeColor { get => themeColor; set { themeColor = value; OnPropertyChanged(nameof(ThemeColor)); OnPropertyChanged(nameof(ThemeColor_Color)); } }
        public string WindowDisplayBackgroundColor { get => windowDisplayBackgroundColor; set { windowDisplayBackgroundColor = value; OnPropertyChanged(nameof(WindowDisplayBackgroundColor)); OnPropertyChanged(nameof(WindowDisplayBackgroundColor_Color)); } }
        public string HoverBackgroundColor { get => hoverBackgroundColor; set { hoverBackgroundColor = value; OnPropertyChanged(nameof(HoverBackgroundColor)); OnPropertyChanged(nameof(HoverBackgroundColor_Color)); } }
        public string SelectedBackgroundColor { get => selectedBackgroundColor; set { selectedBackgroundColor = value; OnPropertyChanged(nameof(SelectedBackgroundColor)); OnPropertyChanged(nameof(SelectedBackgroundColor_Color)); } }
        public string DeviceBackgroundColor { get => deviceBackgroundColor; set { deviceBackgroundColor = value; OnPropertyChanged(nameof(DeviceBackgroundColor)); OnPropertyChanged(nameof(DeviceBackgroundColor_Color)); } }
        public string TabItemBackgroundColor { get => tabItemBackgroundColor; set { tabItemBackgroundColor = value; OnPropertyChanged(nameof(TabItemBackgroundColor)); OnPropertyChanged(nameof(TabItemBackgroundColor_Color)); } }
        public string DevicePanelLabelTextColor { get => devicePanelLabelTextColor; set { devicePanelLabelTextColor = value; OnPropertyChanged(nameof(DevicePanelLabelTextColor)); OnPropertyChanged(nameof(DevicePanelLabelTextColor_Color)); } }
        public string DevicePanelTextColor { get => devicePanelTextColor; set { devicePanelTextColor = value; OnPropertyChanged(nameof(DevicePanelTextColor)); OnPropertyChanged(nameof(DevicePanelTextColor_Color)); } }
        public string ToastBackGroundColor { get => toastBackGroundColor; set { toastBackGroundColor = value; OnPropertyChanged(nameof(ToastBackGroundColor)); OnPropertyChanged(nameof(ToastBackGroundColor_Color)); } }
        public string ToastBorderColor { get => toastBorderColor; set { toastBorderColor = value; OnPropertyChanged(nameof(ToastBorderColor)); OnPropertyChanged(nameof(ToastBorderColor_Color)); } }
        public string ToastLabelPanelColor { get => toastLabelPanelColor; set { toastLabelPanelColor = value; OnPropertyChanged(nameof(ToastLabelPanelColor)); OnPropertyChanged(nameof(ToastLabelPanelColor_Color)); } }

        //以下是提供转换器的颜色属性
        public Color ThemeColor_Color { get => Color.Parse(ThemeColor); }
        public Color WindowDisplayBackgroundColor_Color { get => Color.Parse(WindowDisplayBackgroundColor); }
        public Color HoverBackgroundColor_Color { get => Color.Parse(HoverBackgroundColor); }
        public Color SelectedBackgroundColor_Color { get => Color.Parse(SelectedBackgroundColor); }
        public Color DeviceBackgroundColor_Color { get => Color.Parse(DeviceBackgroundColor); }
        public Color TabItemBackgroundColor_Color { get => Color.Parse(TabItemBackgroundColor); }
        public Color DevicePanelLabelTextColor_Color {  get => Color.Parse(DevicePanelLabelTextColor); }
        public Color DevicePanelTextColor_Color { get => Color.Parse(DevicePanelTextColor); }
        public Color ToastBackGroundColor_Color { get => Color.Parse(ToastBackGroundColor); }
        public Color ToastBorderColor_Color { get => Color.Parse(ToastBorderColor); }
        public Color ToastLabelPanelColor_Color { get => Color.Parse(ToastLabelPanelColor); }


        #endregion

        #region 图片资源
        public IImage? DriveImage_System { get => driveImage_System; set { driveImage_System = value; OnPropertyChanged(nameof(DriveImage_System)); } }
        public IImage? DriveImage_Normal { get => driveImage_Normal; set { driveImage_Normal = value; OnPropertyChanged(nameof(DriveImage_Normal)); } }

        #endregion

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Initialize(InitializeDataPack initializeDataPack)
        {
            LoadFormFile();
        }

        //封装方法
        private void LoadFormFile()
        {
            //耗时加载操作请放在此处，此时UI还未启动 不需要调用OnPropertyChanged方法通知UI线程
            //此处不能使用异步方法，否则可能会导致UI线程在设置加载完成之前就访问了设置数据，从而引发异常

            //以下为测试数据，在实现从文件加载设置之前请勿删除
            driveImage_Normal = new Avalonia.Media.Imaging.Bitmap(AssetLoader.Open(new Uri($"avares://StarExplorer/Assets/Disk.png")));
            driveImage_System = new Avalonia.Media.Imaging.Bitmap(AssetLoader.Open(new Uri($"avares://StarExplorer/Assets/Disk_System.png")));
        
            folderIcon = new Avalonia.Media.Imaging.Bitmap(AssetLoader.Open(new Uri($"avares://StarExplorer/Assets/Folder.png")));
            folderLinkIcon = new Avalonia.Media.Imaging.Bitmap(AssetLoader.Open(new Uri($"avares://StarExplorer/Assets/Folder_Junction.png")));
            fileIcon = new Avalonia.Media.Imaging.Bitmap(AssetLoader.Open(new Uri($"avares://StarExplorer/Assets/File.png")));
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void GeneratePropertyChangedEvent(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        public void GeneratePropertyChangedEventForAllProperties()
        {
            //TODO:暂未想到好方法实现这个功能
            throw new NotImplementedException("尚未实现GeneratePropertyChangedEventForAllProperties方法");
        }
    }

    public interface ISettings : IModule
    {
        #region 封装的属性

        #region tab栏设置
        public int TabTotalHeight { get; set; }
        public int TabBarMargin { get; set; }
        public int TabBarCornerRadius { get; set; }
        public int TabItemMargin { get; set; }
        public int TabItemCornerRadius { get; set; }
        public int TabItemWidth { get; set; }
        public int TabItemHeight { get; set; }
        public int MaxTabCount { get; set; }
        #endregion

        #region 设备显示模式的显示区设置
        public int DevicePanelMargin { get; set; }
        public int DeviceDisplayWidth { get; set; }
        public int DeviceDisplayHeight { get; set; }
        public int DeviceSpacing { get; set; }
        public int DeviceCornerRadius { get; set; }
        #endregion

        #region 文件显示模式的显示区设置
        public int FileItemHeight { get; set; }
        public int FileItemSpacing { get; set; }
        public int FileItemInternalMargin { get; set; }
        public int FileItemNameWidth { get; set; }

        public IImage? FolderIcon { get; set; }
        public IImage? FolderLinkIcon { get; set; }
        public IImage? FileIcon { get; set; }

        #endregion

        #region 子窗口设置
        public int SideBarWidth { get; set; }
        public int AdressBoxHeight { get; set; }
        public int AdressBoxCornerRadius { get; set; }
        #endregion

        #region Toast通知设置
        public int MaxToastCount { get; set; }
        public int ToastDurationMS { get; set; }
        public int ToastSpacing { get; set; }
        public int ToastWidth { get; set; }
        public int ToastHeight { get; set; }
        public int ToastCornerRadius { get; set; }
        public bool ToastAutoHeight { get; set; }
        public int ToastTimeMS { get; set; }
        #endregion

        #region 全局设置
        public StartLocation StartLocation { get; set; }
        public int TextFontSize { get; set; } 
        public int LabelFontSize { get; set; }
        #endregion

        #region 配色设置
        public string ThemeColor { get; set; }
        public string WindowDisplayBackgroundColor { get; set; }
        public string HoverBackgroundColor { get; set; }
        public string SelectedBackgroundColor { get; set; }
        public string DeviceBackgroundColor { get; set; }
        public string TabItemBackgroundColor { get; set; }
        public string DevicePanelLabelTextColor { get; set; }
        public string DevicePanelTextColor { get; set; }
        public string ToastBackGroundColor { get; set; }
        public string ToastBorderColor { get; set; }
        public string ToastLabelPanelColor { get; set; }
        //以下是提供转换器的颜色属性
        public Color ThemeColor_Color { get ; }
        public Color WindowDisplayBackgroundColor_Color { get; }
        public Color HoverBackgroundColor_Color { get; }
        public Color SelectedBackgroundColor_Color { get; }
        public Color DeviceBackgroundColor_Color { get; }
        public Color TabItemBackgroundColor_Color { get; }
        public Color DevicePanelLabelTextColor_Color { get; }
        public Color DevicePanelTextColor_Color { get; }
        public Color ToastBackGroundColor_Color { get; }
        public Color ToastBorderColor_Color { get; }
        public Color ToastLabelPanelColor_Color { get; }

        #endregion

        #region 图片资源
        public IImage? DriveImage_System { get; set; }
        public IImage? DriveImage_Normal { get; set; }
        #endregion

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;
        public void GeneratePropertyChangedEvent(string propertyName);
        public void GeneratePropertyChangedEventForAllProperties();
    }
}

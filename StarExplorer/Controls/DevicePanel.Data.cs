using Avalonia.Controls;
using Avalonia.Media;
using StarExplorer.Logic;
using StarExplorer.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Controls
{
    internal class DevicePanelData : INotifyPropertyChanged
    {
        IDManager manager = new IDManager(0);

        private string? localized_LocalLabel;
        private string? localized_NoDevicesLabel;

        private Color textColor;
        private Color themeColor;
        private Color labelColor;
        private Color deviceItemColor;
        private Color hoverColor;
        private Color selectedColor;

        private int itemMargin;
        private int itemCornerRadius;
        private int textSize;
        private int labelFontSize;
        private int itemSpacing;
        private int itemWidth;
        private int itemHeight;

        private IImage? driveImage_System;
        private IImage? driveImage_Normal;

        private ObservableCollection<IDeviceDataContent> devices = new ObservableCollection<IDeviceDataContent>();

        public string? Localized_LocalLabel { get => localized_LocalLabel; set { localized_LocalLabel = value; OnPropertyChanged(nameof(Localized_LocalLabel)); } }
        public string? Localized_NoDevicesLabel { get => localized_NoDevicesLabel; set { localized_NoDevicesLabel = value; OnPropertyChanged(nameof(Localized_NoDevicesLabel)); } }
        public Color TextColor { get => textColor; set { textColor = value; OnPropertyChanged(nameof(TextColor)); } }
        public Color LabelColor { get => labelColor; set { labelColor = value; OnPropertyChanged(nameof(LabelColor)); } }
        public Color ThemeColor { get => themeColor; set { themeColor = value; OnPropertyChanged(nameof(ThemeColor));} }
        public Color DeviceItemColor { get => deviceItemColor; set { deviceItemColor = value; OnPropertyChanged(nameof(DeviceItemColor)); } }
        public Color HoverColor { get => hoverColor; set { hoverColor = value; OnPropertyChanged(nameof(HoverColor)); } }
        public Color SelectedColor { get => selectedColor; set { selectedColor = value; OnPropertyChanged(nameof(SelectedColor)); } }
        public int ItemMargin { get => itemMargin; set { itemMargin = value; OnPropertyChanged(nameof(ItemMargin)); } }
        public int ItemCornerRadius { get => itemCornerRadius; set { itemCornerRadius = value; OnPropertyChanged(nameof(ItemCornerRadius)); } }
        public int TextSize { get => textSize; set { textSize = value; OnPropertyChanged(nameof(TextSize)); } }
        public int LabelFontSize { get => labelFontSize; set { labelFontSize = value; OnPropertyChanged(nameof(labelFontSize)); } }
        public int ItemSpacing { get => itemSpacing; set { itemSpacing = value; OnPropertyChanged(nameof(ItemSpacing)); } }
        public int ItemWidth { get => itemWidth; set { itemWidth = value; OnPropertyChanged(nameof(ItemWidth)); OnPropertyChanged(nameof(InfomationPanelWidth)); } }
        public int ItemHeight { get => itemHeight; set { itemHeight = value; OnPropertyChanged(nameof(ItemHeight)); OnPropertyChanged(nameof(InfomationPanelWidth)); } }
        public IImage? DriveImage_System { get => driveImage_System; set { driveImage_System = value; OnPropertyChanged(nameof(DriveImage_System)); } }
        public IImage? DriveImage_Normal { get => driveImage_Normal; set { driveImage_Normal = value; OnPropertyChanged(nameof(DriveImage_Normal)); } }
        public ObservableCollection<IDeviceDataContent> DevicesContent { get => devices; set { devices = value; OnPropertyChanged(nameof(DevicesContent)); } }


        public int InfomationPanelWidth { get { return (ItemWidth - ItemHeight); } }

        public DevicePanelData(ISettings settings, ICoreData coreData)
        {
            InitSettings(settings, coreData);
            BindSettings(settings, coreData);
        }

        private void InitSettings(ISettings settings, ICoreData coreData)
        {
            Localized_LocalLabel = "本地设备";
            Localized_NoDevicesLabel = "未检测到设备";

            LabelColor = settings.DevicePanelLabelTextColor_Color;
            TextColor = settings.DevicePanelTextColor_Color;
            ThemeColor = settings.ThemeColor_Color;
            DeviceItemColor = settings.DeviceBackgroundColor_Color;
            HoverColor = settings.HoverBackgroundColor_Color;
            SelectedColor = settings.SelectedBackgroundColor_Color; 
            ItemMargin = settings.DevicePanelMargin;
            ItemCornerRadius = settings.DeviceCornerRadius;
            TextSize = settings.TextFontSize;
            LabelFontSize = settings.LabelFontSize;
            ItemSpacing = settings.DeviceSpacing;
            DriveImage_System = settings.DriveImage_System ?? null!;
            DriveImage_Normal = settings.DriveImage_Normal ?? null!;
            ItemWidth = settings.DeviceDisplayWidth;
            ItemHeight = settings.DeviceDisplayHeight;

            SyncDevice(coreData);
        }

        private void BindSettings(ISettings settings, ICoreData coreData)
        {
            settings.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(settings.DevicePanelLabelTextColor_Color):
                        LabelColor = settings.DevicePanelLabelTextColor_Color;
                        break;
                    case nameof(settings.DevicePanelTextColor_Color):
                        TextColor = settings.DevicePanelTextColor_Color;
                        break;
                    case nameof(settings.ThemeColor):
                        TextColor = settings.ThemeColor_Color;
                        ThemeColor = settings.ThemeColor_Color;
                        break;
                    case nameof(settings.DeviceBackgroundColor_Color):
                        DeviceItemColor = settings.DeviceBackgroundColor_Color;
                        break;
                    case nameof(settings.HoverBackgroundColor_Color):
                        HoverColor = settings.HoverBackgroundColor_Color;
                        break;
                    case nameof(settings.SelectedBackgroundColor_Color):
                        SelectedColor = settings.SelectedBackgroundColor_Color;
                        break;
                    case nameof(settings.DevicePanelMargin):
                        ItemMargin = settings.DevicePanelMargin;
                        break;
                    case nameof(settings.DeviceCornerRadius):
                        ItemCornerRadius = settings.DeviceCornerRadius;
                        break;
                    case nameof(settings.TextFontSize):
                        TextSize = settings.TextFontSize;
                        break;
                    case nameof(settings.LabelFontSize):
                        LabelFontSize = settings.LabelFontSize;
                        break;
                    case nameof(settings.DeviceSpacing):
                        ItemSpacing = settings.DeviceSpacing;
                        break;
                    case nameof(settings.DriveImage_System):
                        DriveImage_System = settings.DriveImage_System ?? null!;
                        break;
                    case nameof(settings.DriveImage_Normal):
                        DriveImage_Normal = settings.DriveImage_Normal ?? null!;
                        break;
                    case nameof(settings.DeviceDisplayWidth):
                        ItemWidth = settings.DeviceDisplayWidth;
                        break;
                    case nameof(settings.DeviceDisplayHeight):
                        ItemHeight = settings.DeviceDisplayHeight;
                        break;
                }
            };

            coreData.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(coreData.Devices):
                        SyncDevice(coreData);
                        break;
                }
            };
        }

        private void SyncDevice(ICoreData coreData)
        {
            DevicesContent.Clear();
            manager.Clear();
            foreach (var logicDevice in coreData.Devices)
            {
                DevicesContent.Add(new DeviceDataContent(logicDevice, this, manager.GetID()));
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

    }




    internal class DeviceDataContent : IDeviceDataContent
    {
        LogicDevices device;
        bool isSelected;
        int id;
        Color currentBackgroundColor;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int ID { get => id; }
        public LogicDevices Device { get => device; }
        public bool IsSelected { get => isSelected; set { isSelected = value; OnPropertyChanged(nameof(IsSelected)); } }
        public Color CurrentBackgroundColor { get => currentBackgroundColor; set { currentBackgroundColor = value; OnPropertyChanged(nameof(CurrentBackgroundColor)); } }

        public DeviceDataContent(LogicDevices device, DevicePanelData data, int id)
        {
            this.device = device;
            isSelected = false;
            this.id = id;
            this.currentBackgroundColor = data.DeviceItemColor;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public interface IDeviceDataContent : INotifyPropertyChanged
    {
        int ID { get; }
        LogicDevices Device { get; }
        bool IsSelected { get; set; }
        Color CurrentBackgroundColor { get; set; }
    }

}

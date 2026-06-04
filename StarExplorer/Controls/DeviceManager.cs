using Avalonia.Media;
using StarExplorer.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Controls
{
    internal class DeviceManager : INotifyPropertyChanged , IDeviceManager
    {
        private ObservableCollection<IDeviceContent> devices = new ObservableCollection<IDeviceContent>();
        public ObservableCollection<IDeviceContent> Devices { get => devices; private set { devices = value; OnPropertyChanged(nameof(Devices)); } }
        public void RefreshDevices(List<LogicDevices> devices, IImage icon)
        {
            Devices = new ObservableCollection<IDeviceContent>();
            foreach (LogicDevices device in devices)
            {
                Devices.Add(new DeviceContent(
                    device.GetTitle(),
                    icon,
                    device.Type,
                    device.Name
                ));
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }
    }

    public interface IDeviceManager : INotifyPropertyChanged
    {
        public ObservableCollection<IDeviceContent> Devices { get; }
        public void RefreshDevices(List<LogicDevices> devices, IImage icon);
    }

    internal class DeviceContent : IDeviceContent, INotifyPropertyChanged
    {
        private String name = "";
        private IImage icon = null!;
        private String type = "";
        private String path = "";
        public String Name { get => name; set { if (value != name) { name = value; OnPropertyChanged(nameof(Name)); } } }
        public IImage Icon { get => icon; set { if (value != icon) { icon = value; OnPropertyChanged(nameof(Icon)); } } }
        public String Type { get => type; set { if (value != type) { type = value; OnPropertyChanged(nameof(Type)); } } }
        public String Path { get => path; set { if (value != path) { path = value; OnPropertyChanged(nameof(Path)); } } }
        public DeviceContent(String name, IImage icon, String type, String path)
        {
            Name = name;
            Icon = icon;
            Type = type;
            Path = path;
        }

        private void OnPropertyChanged(string propertyName)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public interface IDeviceContent : INotifyPropertyChanged
    {
        String Name { get; }
        IImage Icon { get; }
        String Type { get; }
        String Path { get; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using StarExplorer.Abstractions;
using StarExplorer.Shared;

namespace StarExplorer.Logic
{
    public class CoreData : ICoreData, INotifyPropertyChanged
    {
        //依赖
        private IAbstractionLayer _FAL;

        private List<LogicDevices> devices = new List<LogicDevices>(); //逻辑设备列表

        public event PropertyChangedEventHandler? PropertyChanged;

        public List<LogicDevices> Devices
        {
            get { return devices; }
            set { devices = value; OnPropertyChanged(nameof(Devices)); }
        }

        public CoreData(IAbstractionLayer abstractionLayer)
        {
            _FAL = abstractionLayer;
        }

        public void Initialize()
        {
            RefreshDevices();
        }

        //封装方法
        public void RefreshDevices()
        {
            Devices = _FAL.GetDevices();
        }

        public List<LogicDevices> GetDevices()
        {
            return Devices;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public interface ICoreData : IModule
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public List<LogicDevices> Devices { get; set; }
    }
}

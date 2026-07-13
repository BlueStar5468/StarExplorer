using System;
using System.Collections.Generic;
using System.ComponentModel;
using StarExplorer.Abstractions;
using StarExplorer.Shared;

namespace StarExplorer.Logic
{
    public class CoreData : ICoreData, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        //依赖
        private IAbstractionLayer _FAL;

        //数据
        private List<LogicDevices> devices = new List<LogicDevices>(); //逻辑设备列表
        //封装属性
        public List<LogicDevices> Devices { get { return devices; } set { devices = value; OnPropertyChanged(nameof(Devices)); } }

        public CoreData(IAbstractionLayer abstractionLayer)
        {
            _FAL = abstractionLayer;
        }


        //外部接口
        public void Initialize()
        {
            RefreshDevices();
        }

        public List<Item> GetFoldersAndFilesByPath(string path)
        {
            if (!isLegalPath(path))
                throw new ArgumentException("路径不合法");
            return _FAL.GetFoldersAndFilesByPath(path);
        }

        //封装方法
        public void RefreshDevices()
        {
            Devices = _FAL.GetDevices();
        }

        public List<LogicDevices> GetDevices()
        {
            RefreshDevices();
            return Devices;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool isLegalPath(string path)
        {
            //检查路径是否合法
            if (string.IsNullOrEmpty(path))
                return false;
            if (path.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
                return false;
            return true;
        }
    }

    public interface ICoreData : IModule
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public List<LogicDevices> Devices { get; set; }
    }
}

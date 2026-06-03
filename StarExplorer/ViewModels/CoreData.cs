using System;
using System.Collections.Generic;
using StarExplorer.Shared;

namespace StarExplorer.ViewModels
{
    /// <summary>
    /// 上层内核
    /// </summary>
    public class CoreData : ICoreData
    {
        private List<LogicDevices> devices = new List<LogicDevices>(); //逻辑设备列表

        public List<LogicDevices> Devices
        {
            get { return devices; }
            set { devices = value; }
        }

        //退出程序
        public void Exit()
        {

            Environment.Exit(0);
        }
    }

    public interface ICoreData
    {
        public List<LogicDevices> Devices { get; set; }
        public void Exit();
    }
}

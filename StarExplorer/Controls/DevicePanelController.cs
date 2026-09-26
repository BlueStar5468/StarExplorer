using Avalonia.Controls;
using StarExplorer.Logic;
using StarExplorer.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Controls
{
    internal class DevicePanelController
    {
        //依赖
        ISettings settings;
        ICoreData coreData;
        //管理的DevicePanel
        DevicePanelData panelData = null!;
        DevicesPanel panel = null!;
        //事件
        public Action<String>? DeviceOpened; 


        public DevicePanelController(ISettings settings, ICoreData coreData)
        {
            this.settings = settings;
            this.coreData = coreData;

            CreateWindow();
            BindEvents();
        }

        public Panel GetInstance()
        {
            return panel.GetInstance();
        }

        private void CreateWindow()
        {
            panelData = new DevicePanelData(settings, coreData);
            panel = new DevicesPanel(panelData);
        }

        private void BindEvents()
        {
            //在此处绑定DevicePanel相关事件
            panel.ControlLeftClicked += OnControlLeftClicked;
            panel.ControlRightClicked += OnControlRightClicked;
            panel.ControlDoubleTapped += OnControlDoubleTapped;
            panel.BackgroundPanelClicked += OnBackgroundPanelClicked;
        }

        //事件响应方法
        private void OnControlLeftClicked(int id)
        {
            //处理设备项被点击的事件
            foreach (var deviceDataContent in panelData.DevicesContent)
            {
                if (deviceDataContent.ID == id)
                {
                    SelectDevice(deviceDataContent, true);
                }
                else
                {
                    SelectDevice(deviceDataContent, false);
                }
            }
        }

        private void SelectDevice(IDeviceDataContent deviceDataContent, bool isSelected)
        {
            if (isSelected)
            {
                //选中设备
                deviceDataContent.IsSelected = true;
                deviceDataContent.CurrentBackgroundColor = panelData.SelectedColor;
            }
            else
            {
                //取消选中设备
                deviceDataContent.IsSelected = false;
                deviceDataContent.CurrentBackgroundColor = panelData.DeviceItemColor;
            }
        }

        private void OnControlRightClicked(int id)
        {
            //处理设备项被右键点击的事件
        }

        private void OnControlDoubleTapped(int id)
        {
            //处理鼠标双击设备项的事件
            String Temp = panel.GetDeviceNameById(id);
            String OpenedDeviceName;
            if (Temp == "Unknown")
            {
                //TODO: 弹出提示，设备未挂载
                return;
            }
            else if (Temp == "UnFind")
            {
                //TODO: 弹出提示，设备未找到
                return; 
            }
            else
            {
                OpenedDeviceName = panel.GetDeviceNameById(id);
                this.DeviceOpened?.Invoke(OpenedDeviceName);
            }
        }

        private void OnBackgroundPanelClicked()
        {
            //取消选择所有
            foreach (var deviceDataContent in panelData.DevicesContent)
            {
                SelectDevice(deviceDataContent, false);
            }
        }
    }
}

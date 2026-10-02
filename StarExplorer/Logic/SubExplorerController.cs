using Avalonia.Controls;
using StarExplorer.Controls;
using StarExplorer.Views;
using System;
using System.Diagnostics;

namespace StarExplorer.Logic
{
    internal class SubExplorerController
    {
        //数据
        Panel root = null!;
        SubExplorerData data = null!;
        SubExplorer view = null!;

        CurrentPanelType currentPanelType;
        CurrentPanelType defaltPanelType;
        //依赖
        ICoreData coreData;
        ISettings settings;
        IToastCanvasController toastCanvasController;
        //事件
        public Action? DisposeDevicePanel;
        public Action? DisposeItemsPanel;

        public SubExplorerController(ICoreData coreData, ISettings settings, IToastCanvasController toastCanvasController)
        {
            this.coreData = coreData;
            this.settings = settings;
            this.toastCanvasController = toastCanvasController;

            CreatWindow();
            BindEvent();

            CreateStartPanel();
        }

        //事件处理方法
        private void OpenDevice(string devicePath, bool isReady)
        {
            //处理设备打开事件
            Debug.WriteLine($"Device opened: {devicePath}");
            toastCanvasController.ShowToast($"设备已打开: {devicePath}", $"设备已打开: {devicePath}");
            //光驱等设备处理
            if (!isReady)   //TODO:实现提示框或 toast通知管理器 在 MainWindowController中
            { 
                toastCanvasController.ShowToast($"设备未就绪: {devicePath}", $"请将媒体插入设备 {devicePath}");
                return;
            }

            //创建ItemsPanel
            Panel itemsPanel = CreateItemPanel(devicePath);
            currentPanelType = CurrentPanelType.ItemsPanel;
            view.ClearDisplayContent();
            this.DisposeDevicePanel?.Invoke();  //触发DisposeDevicePanel事件，解除DevicePanel的事件绑定
            view.SetMainDisplayContent(itemsPanel);
        }

        private void OnHomeButtonClicked()
        {
            //处理Home按钮点击事件
            Debug.WriteLine("Home button clicked");

            if (currentPanelType == defaltPanelType)
                return; //如果当前面板类型已经是默认面板类型，则不需要返回到起始面板
            //返回到起始面板
            if (currentPanelType == CurrentPanelType.ItemsPanel)
            {
                view.ClearDisplayContent();
                this.DisposeItemsPanel?.Invoke();  //触发DisposeItemsPanel事件，解除ItemsPanel的事件绑定
                CreateStartPanel();
            }
            else if (currentPanelType == CurrentPanelType.DevicePanel)
            {
                view.ClearDisplayContent();
                this.DisposeDevicePanel?.Invoke();  //触发DisposeDevicePanel事件，解除DevicePanel的事件绑定
                CreateStartPanel();
            }
        }

        //封装方法
        public Panel GetInstance()
        {
            return root;
        }
        private void CreatWindow()
        {
            //创建窗口逻辑
            data = new SubExplorerData(settings);
            view = new SubExplorer(data);

            root = view.GetInstance();
        }

        private void BindEvent()
        {
            view.HomeButtonCliked += OnHomeButtonClicked;
        }

        private void CreateStartPanel()
        {
            if (settings.StartLocation == StartLocation.Devices)
            {
                currentPanelType = CurrentPanelType.DevicePanel;
                defaltPanelType = CurrentPanelType.DevicePanel;
                Panel devicePanel = CreateDevicePanel();
                view.SetMainDisplayContent(devicePanel);
            }
            else
            {
                //TODO
            }
        }

        private Panel CreateDevicePanel()
        {
            DevicePanelController devicePanelController = new DevicePanelController(settings, coreData);
            //事件绑定
            devicePanelController.DeviceOpened += OpenDevice;
            //Dispose事件绑定
            this.DisposeDevicePanel += () => { devicePanelController.DeviceOpened -= OpenDevice; };

            return devicePanelController.GetInstance();
        }

        private Panel CreateItemPanel(String path)
        {
            //在path路径下创建ItemsPanel
            ItemsPanelController itemsPanelController = new ItemsPanelController(settings, coreData);
            itemsPanelController.SetPathAndRefresh(path);
            //事件绑定

            //Dispose事件绑定
            this.DisposeItemsPanel += () => { };

            return itemsPanelController.GetInstance();
        }

    }

    internal enum CurrentPanelType
    {
        DevicePanel,
        ItemsPanel
    }
}

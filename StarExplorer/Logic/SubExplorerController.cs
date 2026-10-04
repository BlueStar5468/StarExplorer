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
            //光驱等设备处理
            if (!isReady)
            { 
                toastCanvasController.ShowToast($"设备未就绪: {devicePath}", $"请将媒体插入设备 {devicePath}");
                return;
            }
            toastCanvasController.ShowToast($"设备已打开: {devicePath}", $"设备已打开: {devicePath}");

            data.SetPath(devicePath);
            //创建ItemsPanel
            Panel itemsPanel = CreateItemPanel(data.Path);
            currentPanelType = CurrentPanelType.ItemsPanel;
            view.ClearDisplayContent();
            this._DisposeDevicePanel();
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
                this._DisposeItemsPanel();  //触发DisposeItemsPanel事件，解除ItemsPanel的事件绑定
                CreateStartPanel();
            }
            else if (currentPanelType == CurrentPanelType.DevicePanel)
            {
                view.ClearDisplayContent();
                this._DisposeDevicePanel();  //触发DisposeDevicePanel事件，解除DevicePanel的事件绑定
                CreateStartPanel();
            }
        }

        private void OnMessageGenerated(string title, string message)
        {
            //处理消息生成事件
            toastCanvasController.ShowToast(title, message);
        }

        //来自ItemsPanel的路径变更事件
        private void OnPathChanged(string newPath)
        {
            //处理路径变更事件
            Debug.WriteLine($"Core Path changed to: {newPath}");
            data.SetPath(newPath);
            //刷新ItemsPanel由对应的ItemsPanelController处理
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

                this.data.SetPath("/");
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
            itemsPanelController.PathChanged += OnPathChanged;
            itemsPanelController.MessageGenerated += OnMessageGenerated;
            //Dispose事件绑定
            this.DisposeItemsPanel += () => { itemsPanelController.PathChanged -= OnPathChanged; };
            this.DisposeItemsPanel += () => { itemsPanelController.MessageGenerated -= OnMessageGenerated; };

            return itemsPanelController.GetInstance();
        }

        private void DisposeAll()
        {
            //触发Dispose事件，解除所有事件绑定
            this._DisposeDevicePanel();
            this._DisposeItemsPanel();
        }

        private void _DisposeItemsPanel()
        {
            this.DisposeItemsPanel?.Invoke();
            this.DisposeItemsPanel = null;
        }

        private void _DisposeDevicePanel()
        {
            this.DisposeDevicePanel?.Invoke();
            this.DisposeDevicePanel = null;
        }
    }

    internal enum CurrentPanelType
    {
        DevicePanel,
        ItemsPanel
    }
}

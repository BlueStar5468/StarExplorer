using Avalonia.Controls;
using StarExplorer.Controls;
using StarExplorer.Views;
using System;
using System.Diagnostics;

namespace StarExplorer.Logic
{
    internal class SubExplorerController : IDisposable
    {
        //数据
        Panel root = null!;
        SubExplorerData data = null!;
        SubExplorer view = null!;

        CurrentPanelType currentPanelType;
        CurrentPanelType defaltPanelType;

        ItemsPanelController? currentItemPanelCtl;   //当前ItemsPanel实例 不保证随时可用
        //依赖
        ICoreData coreData;
        ISettings settings;
        IToastCanvasController toastCanvasController;
        //事件
        public Action? DisposeDevicePanel;
        public Action? DisposeItemsPanel;
        //内部处理委托
        Action<AddressBoxEventArgs>? addressBoxEventHandler;

        public SubExplorerController(ICoreData coreData, ISettings settings, IToastCanvasController toastCanvasController)
        {
            this.coreData = coreData;
            this.settings = settings;
            this.toastCanvasController = toastCanvasController;

            CreatWindow();
            InitHandler();
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
            currentItemPanelCtl = CreateItemPanel(data.Path);
            currentPanelType = CurrentPanelType.ItemsPanel;
            view.ClearDisplayContent();
            this._DisposeDevicePanel();
            view.SetMainDisplayContent(currentItemPanelCtl.GetInstance());
        }

        private void OnHomeButtonClicked()
        {
            //处理Home按钮点击事件
            Debug.WriteLine("Home button clicked");
            GC.Collect();

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
            Debug.WriteLine($"Path changed to: {newPath}");
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
            view.AddressBoxEvent += addressBoxEventHandler;
        }

        private void InitHandler()
        {
            this.addressBoxEventHandler = (args) =>
            {
                if (args.InputORDispose == true)
                {
                    //处理地址栏文本变更事件
                    string newPath = args.TargetPath;
                    Debug.WriteLine($"Trying Change path to: {newPath}");
                    //根切换
                    if (newPath == "/")
                    {
                        if (currentPanelType == CurrentPanelType.ItemsPanel)
                        {
                            view.ClearDisplayContent();
                            this._DisposeItemsPanel();

                            view.SetMainDisplayContent(CreateDevicePanel());
                            this.currentPanelType = CurrentPanelType.DevicePanel;
                        }
                        else if (currentPanelType == CurrentPanelType.DevicePanel)
                        {
                            //设备页面的地址本身就是"/" 无需切换
                            return;
                        }
                    }
                    else
                    {
                        //普通目录切换
                        if (currentPanelType == CurrentPanelType.ItemsPanel)
                        {
                            //此方法会尝试以此路径刷新ItemsPanel，如果路径无法切换会自动调用事件恢复地址栏内容
                            currentItemPanelCtl?.SetPathAndRefresh(newPath);
                        }
                        else if (currentPanelType == CurrentPanelType.DevicePanel)
                        {
                            //需要销毁DevicePanel并创建新的ItemsPanel
                            view.ClearDisplayContent();
                            this._DisposeDevicePanel();

                            currentItemPanelCtl = CreateItemPanel(args.TargetPath);
                            view.SetMainDisplayContent(currentItemPanelCtl.GetInstance());
                            this.currentPanelType = CurrentPanelType.ItemsPanel;
                        }
                    }
                }
                else
                {
                    //表明用户是点击其他地方导致地址栏失焦的 需要回退地址
                    string currentPath = data.Path;
                    data.SetPath(currentPath); //通过data的SetPath方法来触发地址栏内容回退
                    //注：采用MVVM模式方式的UI不需要直接控制的方法 触发UI同步即可
                }
            };
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
            this.DisposeDevicePanel += () => {
                devicePanelController.DeviceOpened -= OpenDevice;
                devicePanelController.Dispose();
                //所有devicePanel已经Dispose，解除绑定
                this.DisposeDevicePanel = null;
            };

            return devicePanelController.GetInstance();
        }

        private ItemsPanelController CreateItemPanel(String path)
        {
            //在path路径下创建ItemsPanel
            ItemsPanelController itemsPanelController = new ItemsPanelController(settings, coreData);
            itemsPanelController.SetPathAndRefresh(path);
            //事件绑定
            itemsPanelController.PathChanged += OnPathChanged;
            itemsPanelController.MessageGenerated += OnMessageGenerated;
            //Dispose事件绑定
            this.DisposeItemsPanel += () =>
            {
                itemsPanelController.PathChanged -= OnPathChanged;
                itemsPanelController.MessageGenerated -= OnMessageGenerated;
                itemsPanelController.Dispose();
            };

            return itemsPanelController;
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

        public void Dispose()
        {
            //清理子资源
            DisposeAll();
            view.Dispose();

            view.HomeButtonCliked -= OnHomeButtonClicked;
            view.AddressBoxEvent -= addressBoxEventHandler;

            data.Dispose();
            //清理本地事件
            this.DisposeItemsPanel = null;
            this.DisposeDevicePanel = null;
        }
    }

    internal enum CurrentPanelType
    {
        DevicePanel,
        ItemsPanel
    }
}

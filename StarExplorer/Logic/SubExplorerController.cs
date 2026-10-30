using Avalonia.Controls;
using StarExplorer.Controls;
using StarExplorer.Views;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

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
        //本地委托
        Action<AddressBoxEventArgs>? addressBoxEventHandler;
        Action? upButtonHandler;
        Action? homeButtonHandler;
        Action<string,string>? messageGeneratedHandler;
        Action<string,bool>? deviceOpenedHandler;
        Action<string>? pathChangedHandler;

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

        private void CreatWindow()
        {
            //创建窗口逻辑
            data = new SubExplorerData(settings);
            view = new SubExplorer(data);

            root = view.GetInstance();
        }

        private void BindEvent()
        {
            view.HomeButtonCliked += homeButtonHandler;
            view.AddressBoxEvent += addressBoxEventHandler;
            view.UpButtonClicked += upButtonHandler;
        }

        private void InitHandler()
        {
            this.addressBoxEventHandler = async (args) =>
            {
                //不要throw异常 此async void无法处理异常
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

                            data.SetPath(newPath); //更新地址栏内容
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
                            //这里理论上不可能发生这个异常 如果发生请检查CurrentPanelType的状态是否正确维护
                            if (currentItemPanelCtl == null) throw new Exception("currentItemPanelCtl is null");

                            bool result = await currentItemPanelCtl.SetPathAndRefreshAsync(newPath);
                            if (result)
                            {
                                //说明路径切换成功，更新地址栏内容
                                data.SetPath(newPath);
                            }
                            else
                            {
                                //如果路径切换失败，恢复地址栏内容
                                //此时data.Path还未改变，所以会恢复到之前的路径
                                data.SetPath(data.Path);
                                //直接retturn即可 在此情况下重复fallback路径会导致加载负载
                                return;

                                //fallback参考实现
                                //bool result2 = await currentItemPanelCtl.SetPathAndRefreshAsync(data.Path);
                                //if (result2)
                                //{
                                //    //fallback成功，更新地址栏内容
                                //    data.SetPath(data.Path);
                                //    return;
                                //}
                                //else
                                //{
                                //    toastCanvasController.ShowToast("Error", $"Failed to Fallback to:{data.Path}.");
                                //    //如果恢复路径也失败了，说明ItemsPanel已经处于异常状态，直接返回到起始面板
                                //    Debug.WriteLine($"Failed to recover path after failed path change.");
                                //    OnHomeButtonClicked();
                                //}
                            }
                        }
                        else if (currentPanelType == CurrentPanelType.DevicePanel)
                        {
                            //路径检测 路径不存在则不执行 防止创建空的ItemPanel 
                            if (coreData.CheckExists(args.TargetPath) &&
                                coreData.CanAccess(args.TargetPath))
                            {
                                //需要销毁DevicePanel并创建新的ItemsPanel
                                view.ClearDisplayContent();
                                this._DisposeDevicePanel();

                                currentItemPanelCtl = CreateItemPanel(args.TargetPath);
                                view.SetMainDisplayContent(currentItemPanelCtl.GetInstance());
                                this.currentPanelType = CurrentPanelType.ItemsPanel;
                            }
                            else
                            {
                                //路径不存在或无法访问，恢复地址栏内容
                                data.SetPath(data.Path);
                                //由于没有调用ItemPanel的方法 这里要自己把错误信息显示出来
                                toastCanvasController.ShowToast("Error", $"Path does not exist or cannot be accessed: {args.TargetPath}");
                                return;
                            }
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

            this.upButtonHandler = async () =>
            {
                //处理上级目录按钮点击事件
                Debug.WriteLine("Up button clicked");
                if (currentPanelType == CurrentPanelType.ItemsPanel)
                {
                    if (currentItemPanelCtl == null) throw new Exception("currentItemPanelCtl is null");
                    string parentPath = coreData.GetUpperPath(data.Path);
                    if (parentPath == null)
                    {
                        //已经是根目录 无法再上级
                        toastCanvasController.ShowToast("Info", "Already at root directory.");
                        return;
                    }
                    else
                    {
                        //尝试切换到上级目录
                        bool result = await currentItemPanelCtl.SetPathAndRefreshAsync(parentPath);

                        if (result)
                        {
                            data.SetPath(parentPath);
                        }
                        else
                        {
                            //如果路径切换失败，恢复地址栏内容
                            data.SetPath(data.Path);
                            toastCanvasController.ShowToast("Error", $"Failed to change to upper path: {parentPath}");
                            return;
                        }
                    }
                }
                else if (currentPanelType == CurrentPanelType.DevicePanel)
                {
                    //设备页面没有上级目录按钮的功能
                    return;
                }
            };

            this.homeButtonHandler = () =>
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
            };

            this.messageGeneratedHandler = (title, message) =>
            {
                //处理消息生成事件
                toastCanvasController.ShowToast(title, message);
            };

            this.deviceOpenedHandler = (devicePath, isReady) =>
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
            };

            this.pathChangedHandler = (newPath) =>
            {
                //处理路径变更事件
                Debug.WriteLine($"Path changed to: {newPath}");
                data.SetPath(newPath);
            };
        }

        //封装方法
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
            devicePanelController.DeviceOpened += deviceOpenedHandler;
            //Dispose事件绑定
            this.DisposeDevicePanel += () => {
                devicePanelController.DeviceOpened -= deviceOpenedHandler;
                devicePanelController.Dispose();
                //所有devicePanel已经Dispose，解除绑定
                this.DisposeDevicePanel = null;
            };

            return devicePanelController.GetInstance();
        }

        private ItemsPanelController CreateItemPanel(String path)
        {
            //在path路径下创建ItemsPanel
            //TODO:此传入初始路径必须要是一个有效的路径，否则会导致内容为空白
            ItemsPanelController itemsPanelController = new ItemsPanelController(settings, coreData, path);
            //事件绑定
            itemsPanelController.PathChanged += pathChangedHandler;
            itemsPanelController.MessageGenerated += messageGeneratedHandler;
            //Dispose事件绑定
            this.DisposeItemsPanel += () =>
            {
                itemsPanelController.PathChanged -= pathChangedHandler;
                itemsPanelController.MessageGenerated -= messageGeneratedHandler;
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

        public Panel GetInstance()
        {
            return root;
        }

        public void Dispose()
        {
            //清理子资源
            DisposeAll();
            view.Dispose();

            view.HomeButtonCliked -= homeButtonHandler;
            view.AddressBoxEvent -= addressBoxEventHandler;
            view.UpButtonClicked -= upButtonHandler;

            data.Dispose();
            //清理本地事件
            this.DisposeItemsPanel = null;
            this.DisposeDevicePanel = null;
            //清理本地委托
            this.addressBoxEventHandler = null;
            this.deviceOpenedHandler = null;
            this.homeButtonHandler = null;
            this.messageGeneratedHandler = null;
            this.pathChangedHandler = null;
            this.upButtonHandler = null;
        }
    }

    internal enum CurrentPanelType
    {
        DevicePanel,
        ItemsPanel
    }
}

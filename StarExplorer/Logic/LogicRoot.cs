using Avalonia.Controls;
using StarExplorer.Abstractions;
using System;
using System.Runtime.InteropServices;

namespace StarExplorer.Logic
{
    //此类为逻辑层的组合根,负责组合和管理逻辑层的各个组件，并提供对外的接口。
    //它是逻辑层的核心，为控件提供依赖注入
    internal class LogicRoot
    {
        #region 模块存储区
        IAbstractionLayer abstractionLayer;//抽象层，提供对后端的抽象访问接口，屏蔽平台差异
        ICoreData coreData;                //核心数据模块，提供对文件后端的访问和管理
        ISettings settings;                //设置模块，提供应用程序的配置和资源管理功能
        IMainWindowControler mainWindowControler; //主窗口控制器，负责主窗口的创建和管理
        ITabManager tabManager;                //标签页管理器，负责标签页的创建、切换和关闭以及标签页内的数据管理
        #endregion

        #region 事件区
        //无参数事件
        private event Action? Init;          //应用程序启动事件 在此事件中应完成所有模块的初始化
        public event Action? AppExitStarted; //应用程序退出事件 在此事件中应完成所有模块的清理工作
        //有参数事件
        
        #endregion

        //如需替换模块实现，请在此处修改构造器中的实例化代码，并确保新的实现类符合接口要求。
        public LogicRoot()
        {
            abstractionLayer = new AbstractionLayer();
            settings = new Settings();
            tabManager = new TabManager(settings.MaxTabCount); 
            coreData = new CoreData(abstractionLayer);

            mainWindowControler = new MainWindowControler(settings, coreData, tabManager);

            //事件绑定
            BindEvents();
        }

        //注意：Init事件的绑定顺序必须严格按照构造器顺序进行
        private void BindEvents()
        {
            #region Export
            //初始化事件
            Init += abstractionLayer.Initialize;
            Init += settings.Initialize;
            Init += coreData.Initialize;
            Init += mainWindowControler.Initialize;
            Init += tabManager.Initialize;
            //事件广播

            #endregion

            #region Import
            mainWindowControler.AppExitRequested += OnAppExitRequested;

            #endregion
        }

        //封装方法
        public void Initialize()
        {
            //初始化事件发出
            Init?.Invoke();
        }

        public Window GetMainWindow()
        {
            return mainWindowControler.GetWindow();
        }

        //事件响应方法
        private void OnAppExitRequested()
        {
            AppExitStarted?.Invoke();

            Environment.Exit(0);
        }
    }
}

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using StarExplorer.Views;
using StarExplorer.Abstractions;
using StarExplorer.ViewModels;

namespace StarExplorer
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        //框架初始化完毕后的入口方法
        public override void OnFrameworkInitializationCompleted()
        {
            //初始化抽象层和平台后端
            AbstractionLayer.Instance.InitFAL();
            CoreData coreData = new CoreData();

            //启动时获取设备信息
            coreData.Devices = AbstractionLayer.Instance.GetDevices();
            
            //数据模型初始化
            StarExplorer.ViewModels.ExplorerData explorerData = new StarExplorer.ViewModels.ExplorerData();
            explorerData.displayMode = DisplayMode.Devices;//默认显示设备列表


            //窗口初始化
            StarExplorer.Views.Explorer explorer = new StarExplorer.Views.Explorer(explorerData);
            //依赖注入
            explorer.DataContext = explorerData;
            explorerData._coreData = coreData;

            //跨平台检查 和 生命周期设置
            if (Design.IsDesignMode == false)
            {

                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime) //经典桌面应用(Windows, Linux, MacOS)
                {
                    var lifetime = ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                    if (lifetime == null) throw new InvalidOperationException("无法获取经典桌面应用生命周期");
                    lifetime.MainWindow = explorer;

                    //获取布局
                    explorerData.Layout = ExplorerLayout.Desktop;
                }
                else if (ApplicationLifetime is ISingleViewApplicationLifetime) //单视图应用(UWP, Android, iOS)
                {
                    //TODO: 目前单视图应用的布局和功能尚未完成.

                    var lifetime = ApplicationLifetime as ISingleViewApplicationLifetime;
                    if (lifetime == null) throw new InvalidOperationException("无法获取单视图应用生命周期");
                    lifetime.MainView = explorer;

                    //获取布局
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        explorerData.Layout = ExplorerLayout.Desktop;
                    else explorerData.Layout = ExplorerLayout.Mobile;
                }
                else
                {
                    //fallback: 其他类型的应用生命周期，暂时按照经典桌面应用处理
                    var lifetime = ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                    if (lifetime == null) throw new InvalidOperationException("无法获取经典桌面应用生命周期");
                    lifetime.MainWindow = explorer;
                    //获取布局
                    explorerData.Layout = ExplorerLayout.Desktop;
                }
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
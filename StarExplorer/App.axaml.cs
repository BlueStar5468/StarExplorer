using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using StarExplorer.Abstractions;
using StarExplorer.Logic;
using StarExplorer.Views;

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
            //初始化逻辑层 整个程序会从逻辑层开始展开
            LogicRoot logicRoot = new LogicRoot();

            //目标系统初始化
            System.OperatingSystem targetSystem = GetTargetSystem();

            //开始启动核心层
            logicRoot.Initialize(targetSystem);
            
            Window explorer = logicRoot.GetMainWindow();
#if DEBUG
            explorer.AttachDevTools();
#endif

            //跨平台检查 和 生命周期设置
            if (Design.IsDesignMode == false)
            {

                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime) //经典桌面应用(Windows, Linux, MacOS)
                {
                    var lifetime = ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                    if (lifetime == null) throw new InvalidOperationException("无法获取经典桌面应用生命周期");
                    lifetime.MainWindow = explorer;
                }
                else if (ApplicationLifetime is ISingleViewApplicationLifetime) //单视图应用(UWP, Android, iOS)
                {
                    //TODO: 目前单视图应用的布局和功能尚未完成.

                    var lifetime = ApplicationLifetime as ISingleViewApplicationLifetime;
                    if (lifetime == null) throw new InvalidOperationException("无法获取单视图应用生命周期");
                    lifetime.MainView = explorer;
                }
                else
                {
                    //fallback: 其他类型的应用生命周期，暂时按照经典桌面应用处理
                    var lifetime = ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                    if (lifetime == null) throw new InvalidOperationException("无法获取经典桌面应用生命周期");
                    lifetime.MainWindow = explorer;
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        private System.OperatingSystem GetTargetSystem()
        {
            var os = Environment.OSVersion;
            return os;
        }
    }
}
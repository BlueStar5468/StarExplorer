using System;
using Avalonia;

namespace StarExplorer
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            //不要在此处尝试创建窗口或使用Avalonia的任何功能，因为Avalonia尚未初始化。

            //初始化窗口
            var app = BuildAvaloniaApp();
            app.StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
#if debug
                .withdevelopertools()
#endif
                .WithInterFont()
                .LogToTrace();
    }
}

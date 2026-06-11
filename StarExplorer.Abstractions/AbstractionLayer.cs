using System.Runtime.InteropServices;
using StarExplorer.Shared;

namespace StarExplorer.Abstractions
{
    /// <summary>
    /// 文件系统抽象层(FAL)，提供跨平台的文件系统访问接口。
    /// </summary>
    /// </note> FAL运行在单例模式
    public class AbstractionLayer : IAbstractionLayer
    {
        //系统信息
        Platform? platform = null;
        IBackend? backend = null;

        public AbstractionLayer()
        {

        }

        public void Initialize()
        {
            InitFAL();
        }

        public void InitFAL()
        {
            //检测平台并初始化后端
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                platform = Platform.Windows;
                backend = new WindowsBackend.WindowsBackend();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                platform = Platform.Linux;
                //TODO:设计linux后端
            }
            else
            {
                platform = Platform.Android;
                //TODO:设计安卓后端
            }
        }

        public List<LogicDevices> GetDevices()
        {
            List<LogicDevices> devices = new List<LogicDevices>();
            if (platform == Platform.Windows)
            {
                if (backend != null)
                    backend.GetDevices(out devices);
                else
                    throw new InvalidOperationException("指定后端为空");
                    //TODO:记录空后端日志
            }
            return devices;
        }


    }

    public interface IAbstractionLayer : IModule
    {
            void InitFAL();
            List<LogicDevices> GetDevices();
    }

    internal enum Platform
    {
        Windows,
        Linux,
        Android
    }
}

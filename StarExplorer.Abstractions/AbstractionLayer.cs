using System.Runtime.InteropServices;
using StarExplorer.Shared;
using StarExplorer.Shared.SharedInterface;

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
        ExternalInvoker externalInvoker = null!;

        public AbstractionLayer()
        {

        }

        public void Initialize(InitializeDataPack dataPack)
        {
            this.platform = dataPack.Platform;

            InitFAL(dataPack);
        }

        public void InitFAL(InitializeDataPack dataPack)
        {
            //检测平台并初始化后端
            if (this.platform == Platform.Windows)
            {
                platform = Platform.Windows;
                backend = new WindowsBackend.WindowsBackend();
            }
            else if (this.platform == Platform.Linux)
            {
                platform = Platform.Linux;
                //TODO:设计linux后端
            }
            else if (this.platform == Platform.Android)
            {
                platform = Platform.Android;
                //TODO:设计安卓后端
            }
            else 
            {
                throw new NotSupportedException("不支持的平台");
            }
            //启动外部调用器
            externalInvoker = new ExternalInvoker(this.backend!);
            externalInvoker.Initialize(dataPack);
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

        public List<Item> GetFoldersAndFilesByPath(string path)
        {
            List<Item> items = new List<Item>();
            if (platform == Platform.Windows)
            {
                if (backend != null)
                    try
                    {
                        backend.GetFoldersAndFilesByPath(path, out items);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                else
                    throw new InvalidOperationException("指定后端为空");
            }
            return items;
        }

        public byte[] GetItemsIcon(string path, bool isLarge = true, bool isOverlay = true)
        {
            return externalInvoker.GetFileIconWindows(path, isLarge, isOverlay);
        }
    }

    public interface IAbstractionLayer : IModule
    {
        List<LogicDevices> GetDevices();
        List<Item> GetFoldersAndFilesByPath(string path);
    }
}

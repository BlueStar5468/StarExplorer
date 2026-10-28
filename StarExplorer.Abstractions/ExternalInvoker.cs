using StarExplorer.Shared;
using StarExplorer.Shared.SharedInterface;
using System.Runtime.InteropServices;

namespace StarExplorer.Abstractions
{
    public class ExternalInvoker
    {
        //此类专门用于处理P/Invoker相关方法的封装 并处理托管与非托管代码的交互
        //由于不同系统的系统调用不同 具体实现会在对应后端内 并在必要时拆成小模块 以便于维护

        //数据
        OperatingSystem os = null!;
        IBackend backend = null!;

        public ExternalInvoker(IBackend backend)
        {
            this.backend = backend ?? throw new ArgumentNullException(nameof(backend));
        }

        public void Initialize(InitializeDataPack dataPack)
        {
            this.os = dataPack.OperatingSystem ?? throw new ArgumentNullException(nameof(dataPack.OperatingSystem));
        }

        //仅限Windows--通过Explorer获取文件图标
        public RawIconData GetFileIconWindows(string fullPath, bool isLarge = false, bool isOverlay = false)
        {
            if (OperatingSystem.IsWindows())
            {
                return backend.GetFileOrFolderIcon(fullPath, isLarge, isOverlay);
            }
            else
            {
                throw new PlatformNotSupportedException("GetFileIconWindows is only supported on Windows.");
            }
        }
    }
}

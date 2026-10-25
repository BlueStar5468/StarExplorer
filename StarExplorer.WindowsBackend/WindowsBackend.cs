using StarExplorer.Shared;
using StarExplorer.Shared.SharedInterface;
using StarExplorer.Shared.SharedFlag;
using System.Runtime.InteropServices;

namespace StarExplorer.WindowsBackend
{
    public class WindowsBackend : IBackend
    {
        //外部接口
        public void GetDevices(out List<LogicDevices> devices)
        {
            devices = new List<LogicDevices>();

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                LogicDevices device;
                try
                {
                    //获取一个逻辑存储设备
                    if (drive.IsReady == true)
                    {
                        device = new LogicDevices(
                            drive.Name,
                            drive.DriveType.ToString(),
                            drive.VolumeLabel,
                            drive.DriveFormat,
                            drive.TotalSize,
                            drive.AvailableFreeSpace,
                            drive.TotalFreeSpace,
                            true
                        );
                    }
                    else
                    {
                        device = new LogicDevices(
                            drive.Name,
                            drive.DriveType.ToString(),
                            "未知",
                            "未知",
                            0,
                            0,
                            0,
                            false
                        );
                    }

                    //设备添加至设备列表
                    devices.Add(device);
                }
                catch (Exception e)
                {
                    //TODO: 处理获取设备信息时可能出现的异常，例如访问权限不足、设备故障等
                }
            }
        }

        public void GetFoldersByPath(string path, out List<Item> items)
        {
            items = new List<Item>();
            //构建当前目录的DirectoryInfo对象
            DirectoryInfo currentDirectory = new DirectoryInfo(path);
            DirectoryInfo[] _subDirectories = currentDirectory.GetDirectories();
            if (_subDirectories.Length > 0)
            {
                List<DirectoryInfo> subDirectories = new List<DirectoryInfo>(_subDirectories);
                foreach (DirectoryInfo directory in subDirectories)
                {
                    Item item = new Item(
                        directory.Name,
                        directory.FullName,
                        directory.LinkTarget == null ? ItemType.Folder : ItemType.Folder_Link,
                        directory.CreationTime.ToString(),
                        directory.LastWriteTime.ToString(),
                        directory.LinkTarget
                    );
                    items.Add(item);
                }
            }
        }

        public void GetFilesByPath(string path, out List<Item> items)
        {
            items = new List<Item>();
            //构建当前目录的DirectoryInfo对象
            DirectoryInfo currentDirectory = new DirectoryInfo(path);
            FileInfo[] _files = currentDirectory.GetFiles();
            if (_files.Length > 0)
            {
                List<FileInfo> files = new List<FileInfo>(_files);
                foreach (FileInfo file in files)
                {
                    Item item = new Item(
                        file.Name,
                        file.FullName,
                        file.LinkTarget == null ? ItemType.File : ItemType.HardLink,
                        file.CreationTime.ToString(),
                        file.LastWriteTime.ToString(),
                        file.LinkTarget
                    );
                    items.Add(item);
                }
            }
        }

        public void GetFoldersAndFilesByPath(string path, out List<Item> items)
        {
            items = new List<Item>();
            GetFoldersByPath(path, out List<Item> folders);
            GetFilesByPath(path, out List<Item> files);
            items.AddRange(folders);
            items.AddRange(files);
        }

        //获取文件图标
        public byte[] GetFileOrFolderIcon(string path, bool isLarge, bool isOverlay)
        {
            //获取图标句柄
            var fileTargetInfo = new WindowsInvoker.SHFILEINFO();
            uint flags = (uint)UnManagedFlag.SHGFI.SHGFI_ICON;
            if (isLarge)
            {
                flags |= (uint)UnManagedFlag.SHGFI.SHGFI_LARGEICON;
            }
            else
            {
                flags |= (uint)UnManagedFlag.SHGFI.SHGFI_SMALLICON;
            }
            if (isOverlay)
            {
                flags |= (uint)UnManagedFlag.SHGFI.SHGFI_ADDOVERLAYS;
            }
            //句柄缓存
            nint iconHandle = IntPtr.Zero;
            WindowsInvoker.ICONINFO iconInfo = new WindowsInvoker.ICONINFO() { hbmColor = IntPtr.Zero, hbmMask = IntPtr.Zero };
            IntPtr hdc = IntPtr.Zero;

            try
            {
                IntPtr result = WindowsInvoker.SHGetFileInfo(path, 0, ref fileTargetInfo, (uint)Marshal.SizeOf(fileTargetInfo), flags);
                if (result == IntPtr.Zero)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Exception($"未知P/Invoke错误 位于图标获取:错误代码:{errorCode}");
                }
                iconHandle = fileTargetInfo.hIcon;  //需要释放
                                                    //将图标句柄转换为字符数组
                try
                {
                    iconInfo = new WindowsInvoker.ICONINFO();
                    bool successFlag = WindowsInvoker.GetIconInfo(iconHandle, out iconInfo);
                    //iconInfo内两个句柄需要释放
                    if (successFlag == false) throw new Exception("未知P/Invoke错误 位于图标获取:句柄解包失败");

                    if (iconInfo.hbmColor == IntPtr.Zero)
                    {
                        //说明为单色图标
                        //TODO:从iconInfo.hbmMask获取单色图标数据
                        return null!;
                    }
                    else
                    {
                        var bitmap = new WindowsInvoker.BITMAP();
                        int ret = WindowsInvoker.GetObject(iconInfo.hbmColor, Marshal.SizeOf(bitmap), ref bitmap);
                        if (ret == 0)
                            throw new Exception("未知P/Invoke错误 位于图标获取:位图信息获取失败");
                        int width = bitmap.bmWidth;
                        int height = bitmap.bmHeight;

                        try
                        {
                            //创建DC
                            //DC需要释放
                            hdc = WindowsInvoker.CreateCompatibleDC(IntPtr.Zero);
                            if (hdc == IntPtr.Zero) throw new Exception("未知P/Invoke错误 位于图标获取:DC创建失败");
                            var bitmapInfo = new WindowsInvoker.BITMAPINFO()
                            {
                                bmiHeader = new WindowsInvoker.BITMAPINFOHEADER()
                                {
                                    biSize = (uint)Marshal.SizeOf<WindowsInvoker.BITMAPINFOHEADER>(),
                                    biWidth = width,
                                    biHeight = -height, //注意此处负数
                                    biPlanes = 1,
                                    biBitCount = 32,
                                    biCompression = 0, // BI_RGB
                                }
                            };

                            byte[] pixelData = new byte[width * height * 4];    //原始位图数据 为BGRA格式 注意不是RGBA
                            int resultBits = WindowsInvoker.GetDIBits(hdc, iconInfo.hbmColor, 0, (uint)height, pixelData, ref bitmapInfo, 0);
                            if (resultBits == 0) throw new Exception("未知P/Invoke错误 位于图标获取:位图数据获取失败");
                            //此时原始数据已经获取

                            //alpha通道修正
                            //吐槽：你这WindowsAPI怎么还同时存在多种不透明表示方式
                            //感谢WindowsAPI让我多写这么多行
                            bool isAlphaAvilable = false;
                            for (int i = 3; i < pixelData.Length; i += 4)
                            {
                                if (pixelData[i] != 0)
                                {
                                    isAlphaAvilable = true;
                                    break;
                                    //注：极少部分图标的alpha通道是全0但Alpha有效 此处未处理这种畸形数据
                                }
                            }
                            if (!isAlphaAvilable)
                            {
                                //alpha通道无效 需要使用掩码修正
                                byte[] mask = new byte[width * height * 4];
                                int result1 = WindowsInvoker.GetDIBits(hdc, iconInfo.hbmMask, 0, (uint)height, mask, ref bitmapInfo, 0);
                                if (result1 == 0) throw new Exception("未知P/Invoke错误 位于图标获取:掩码数据获取失败");
                                //混合alpha通道
                                for (int j = 3; j < pixelData.Length; j += 4)
                                {
                                    //对于这种老掉牙的图标 只有纯透明和纯不透明两种状态 不透明时掩码为0 纯透明时掩码不为0
                                    pixelData[j] = (mask[j] == 0 ? (byte)255 : (byte)0);
                                }
                            }
                            //颜色通道反转
                            //吐槽：Windows你说话! 为什么搞个BGRA格式!
                            //需求BGRA->RGBA 反转第0和2字节
                            for (int k = 0; k < pixelData.Length; k += 4)
                            {
                                byte temp = pixelData[k];
                                pixelData[k] = pixelData[k + 2];
                                pixelData[k + 2] = temp;
                            }
                            //最终数据处理完成
                            return pixelData;
                        }
                        finally
                        {
                            if (hdc != IntPtr.Zero)
                            {
                                WindowsInvoker.DeleteDC(hdc);
                            }
                        }
                    }
                }
                finally
                {
                    if (iconInfo.hbmColor != IntPtr.Zero)
                    {
                        WindowsInvoker.DeleteObject(iconInfo.hbmColor);
                        WindowsInvoker.DeleteObject(iconInfo.hbmMask);
                    }
                }
            }
            finally
            {
                if (iconHandle != IntPtr.Zero)
                {
                    WindowsInvoker.DestroyIcon(iconHandle); //释放图标句柄
                }
            }
        
        }
    }
}

using StarExplorer.Shared;

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
                        file.LinkTarget == null ? ItemType.File : ItemType.hardLink,
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
    }
}

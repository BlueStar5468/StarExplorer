using StarExplorer.Shared;

namespace StarExplorer.WindowsBackend
{
    public class WindowsBackend : IBackend
    {
        /// <summary>
        /// 用于获取windows存储设备
        /// </summary>
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



    }
}

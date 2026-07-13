using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Shared
{
    public interface IBackend
    {
        public void GetDevices(out List<LogicDevices> devices);
        public void GetFilesByPath(string path, out List<Item> items);
        public void GetFoldersByPath(string path, out List<Item> items);
        public void GetFoldersAndFilesByPath(string path, out List<Item> items);

    }
}

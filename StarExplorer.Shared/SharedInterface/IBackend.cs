using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarExplorer.Shared;

namespace StarExplorer.Shared.SharedInterface
{
    public interface IBackend
    {
        public void GetDevices(out List<LogicDevices> devices);
        public void GetFilesByPath(string path, out List<Item> items);
        public void GetFoldersByPath(string path, out List<Item> items);
        public void GetFoldersAndFilesByPath(string path, out List<Item> items);
        public RawIconData GetFileOrFolderIcon(string path, bool isLarge, bool isOverlay);
        public bool CheckExists(string path);
        public bool CanAccessPath(string path);
        public string GetUpperPath(string path);

    }
}

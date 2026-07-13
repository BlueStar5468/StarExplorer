using Avalonia.Controls;
using StarExplorer.Controls;
using StarExplorer.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Logic
{
    internal class SubExplorerController
    {
        //数据
        Panel root = null!;
        SubExplorerData data;
        SubExplorer view;

        public SubExplorerController(ICoreData coreData,ISettings settings, StartLocation startLocation)
        {
            data = new SubExplorerData(settings);
            view = new SubExplorer(data, startLocation, coreData);

            root = view.GetInstance();
        }

        //封装方法
        public Panel GetInstance()
        {
            return root;
        }

    }
}

using StarExplorer.Logic;
using Avalonia.Controls;
using System.Collections.Generic;
using StarExplorer.Shared;
using System;

namespace StarExplorer.Controls
{
    internal class ItemsPanelController
    {
        //依赖
        ISettings settings;
        ICoreData coreData;
        //管理的ItemsPanel
        ItemsPanelData panelData = null!;
        ItemsPanel panel = null!;

        //数据
        string? path;

        public ItemsPanelController(ISettings settings, ICoreData coreData)
        {
            this.settings = settings;
            this.coreData = coreData;
            
            CreateWindow();
            BindEvents();
        }

        //封装方法
        private void CreateWindow()
        {
            panelData = new ItemsPanelData(settings);
            panel = new ItemsPanel(panelData);
        }

        private void BindEvents()
        {
            //在此处绑定ItemsPanel相关事件

        }

        private void RefereshPathItem()
        {
            if (path != null)
            {
                List<Item> newItems = coreData.GetFoldersAndFilesByPath(this.path);
                panelData.UpdateItems(newItems); //更新Items后data会向panel发送通知，panel会刷新显示
            }
            else
            {
                throw new Exception("Path is null, cannot refresh items.");
            }
        }

        public void SetPathAndRefresh(string path)
        {
            this.path = path;
            RefereshPathItem();
        }

        public Panel GetInstance()
        {
            return panel.GetInstance();
        }
    }
}

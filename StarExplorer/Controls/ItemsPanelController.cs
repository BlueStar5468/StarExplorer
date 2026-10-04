using StarExplorer.Logic;
using Avalonia.Controls;
using System.Collections.Generic;
using StarExplorer.Shared;
using System;
using System.Net.Sockets;
using System.Diagnostics;

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
        //事件
        public Action<string>? PathChanged;
        public Action<string, string>? MessageGenerated;
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
            //必须确定OpenFolder正常后才能向外部发送PathChanged事件
            panel.PathChanged += OnPathChanged;
        }

        private void RefereshPathItem()
        {
            if (path != null)
            {
                //此处Exception在上层调用捕获
                List<Item> newItems = coreData.GetFoldersAndFilesByPath(this.path);
                panelData.UpdateItems(newItems); //更新Items后data会向panel发送通知，panel会刷新显示
            }
            else
            {
                throw new Exception("Path is null, cannot refresh items.");
            }
        }

        private bool OpenFolder(string path)
        {
            try
            {
                this.SetPathAndRefresh(path);
                return true;
            }
            catch (Exception ex)
            {
                if (ex is System.UnauthorizedAccessException)
                {
                    MessageGenerated?.Invoke("Error", $"Access denied to the path: {path}. Please check your permissions.");
                }
                else if (ex is System.IO.DirectoryNotFoundException)
                {
                    MessageGenerated?.Invoke("Error", $"The directory was not found: {path}. Please check the path.");
                }
                else
                {
                    MessageGenerated?.Invoke("Error", $"An unexpected error occurred: {ex.Message}");
                }
                return false;
            }
        }

        private void OnPathChanged(string path)
        {
            var result = this.OpenFolder(path);
            if (result)
                this.PathChanged?.Invoke(path);
        }

        public void SetPathAndRefresh(string path)
        {
            Debug.WriteLine($"Setting path to: {path}");
            this.path = path;
            RefereshPathItem();
        }

        public Panel GetInstance()
        {
            return panel.GetInstance();
        }
    }
}

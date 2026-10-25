using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using StarExplorer.Controls;
using StarExplorer.Shared;
using StarExplorer.Shared.SharedInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace StarExplorer.Logic
{
    internal class TabManager : ITabManager
    {
        IDManager iDManager;
        
        //管理的控件
        TabBarPanelData data = null!;
        TabBarPanel panel = null!;
        
        //配置
        public int TabCount { get => data?.TabContents.Count ?? -1; }

        //事件
        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action? SelectedTabChanged;
        

        public TabManager(ISettings settings ,int maxIDCount)
        {
            CreateWindow(settings);
            BindEvents();

            if (maxIDCount < 10) maxIDCount = 10;
            iDManager = new IDManager(maxIDCount);
        }

        private void BindEvents()
        {
            //此处没有处理解除绑定
            if (panel == null) return;
            panel.TabCloseButtonClicked += (id) =>
            {
                CloseTab(id);
            };
            panel.TabClicked += (id) =>
            {
                SelectTab(id);
            };
        }

        private void CreateWindow(ISettings settings)
        {
            data = new TabBarPanelData(settings, this);
            panel = new TabBarPanel(data);
        }

        public void NewTab(Panel content, Action disposehandler,out int newid)
        {
            int id = iDManager.GetID();
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
            {
                TabDisplayContent newTabContent = new TabDisplayContent(
                    id,
                    content,
                    disposehandler,
                    data.TabItemIcon,
                    $"标签{id}");
                data?.TabContents.Add(newTabContent);
            });
            newid = id;
        }

        public void CloseTab(int id)
        {
            ITabDisplayContent? tabToRemove = null;
            if (data == null) return;
            foreach (var tab in data.TabContents)
            {
                if (tab.Index == id)
                {
                    tabToRemove = tab;
                    break;
                }
            }
            if (tabToRemove != null)
            {
                Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
                {
                    tabToRemove.DisposeHandler?.Invoke();
                    tabToRemove.Content.Children.Clear();
                    data.TabContents.Remove(tabToRemove);
                });
                iDManager.RecycleID(id);
            }
        }

        private ITabDisplayContent? GetSelectedTab()
        {
            if (data == null) return null!;
            foreach (var tab in data.TabContents)
            {
                if (tab.IsSelected == true)
                {
                    return tab;
                }
            }
            return null;
        }

        public Panel? GetSelectedTabContent()
        {
            var tab = GetSelectedTab();
            if (tab == null) return null;
            if (tab.Content == null) return null;
            return tab.Content;
        }

        public void SelectTab(int id)
        {
            if (GetSelectedTab()?.Index == id) return;
            ResetSelection();
            GetDisplayContentById(id).IsSelected = true;
            this.SelectedTabChanged?.Invoke();
        }

        private ITabDisplayContent GetDisplayContentById(int id)
        {
            foreach (var tab in data.TabContents)
            {
                if (tab.Index == id)
                {
                    return tab;
                }
            }
            throw new Exception("未找到指定ID的标签页");
        }

        private void ResetSelection()
        {
            foreach (var tab in data.TabContents)
            {
                tab.IsSelected = false;
            }
        }

        public Panel GetTabbarPanel()
        {
            return panel?.GetInstance() ?? throw new Exception("TabBarPanel未初始化");
        }

        public void Initialize(InitializeDataPack initializeDataPack)
        {
            
        }
    }

    public interface ITabManager : INotifyPropertyChanged , IModule
    {
        public event Action? SelectedTabChanged;
        int TabCount { get; }
        public void NewTab(Panel content, Action disposehandler, out int newid);
        public void CloseTab(int id);
        public void SelectTab(int id);
        public Panel GetTabbarPanel();
        public Panel? GetSelectedTabContent();
    }
}

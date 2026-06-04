using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace StarExplorer.Controls
{
    internal class TabManager : ITabManager , INotifyPropertyChanged
    {
        //用于id分配的id栈
        Stack<int> IDStack = new Stack<int>();
        private int maxIDCount;
        private int currentTabId = -1;
        public int UsedIDCount = 0; 
        public int TabCount { get => tabContents.Count; }
        public int CurrentTabId { get => currentTabId; }
        //标签页抽象数据列表
        public ObservableCollection<ITabContent> tabContents { get; set; } = new ObservableCollection<ITabContent>();
        public event PropertyChangedEventHandler? PropertyChanged;
        public TabManager(int maxIDCount)
        {
            //id栈初始化(最少要有10个可分配id)
            IDStack.Push(0);
            if (maxIDCount > 10) this.maxIDCount = maxIDCount;
            else this.maxIDCount = 10;
        }

        public void NewTab(Panel content,String backgroundColor, out int newid)
        {
            int id = GetID();
            TabContent newTabContent = new TabContent(
                $"标签{id}",
                new Avalonia.Media.Imaging.Bitmap(AssetLoader.Open(new Uri($"avares://StarExplorer/Assets/avalonia-logo.ico"))), 
                content,
                id,
                backgroundColor);
            tabContents.Add(newTabContent);

            newid = id;
        }

        public void CloseTab(int id)
        {
            ITabContent? tabToRemove = null;
            foreach (var tab in tabContents)
            {
                if (tab.Index == id)
                {
                    tabToRemove = tab;
                    break;
                }
            }
            if (tabToRemove != null)
            {
                tabToRemove.Content.Children.Clear();
                tabContents.Remove(tabToRemove);
                RecycleID(id);
            }
        }

        private void SwitchTab(int sourceId, int targetId)
        {
            GetTabByID(targetId);
            currentTabId = targetId;
            
            OnPropertyChanged(nameof(CurrentTabId));
            TabChanged?.Invoke(sourceId, targetId);
        }

        public void OnTabClicked(int id)
        {
            if (id != currentTabId)
            {
                SwitchTab(currentTabId, id);
            }
        }

        private int GetID()
        {
            if (UsedIDCount < maxIDCount)
            {
                int currentStack = IDStack.Pop();
                UsedIDCount += 1;
                if (IDStack.Count == 0)
                {
                    IDStack.Push(currentStack + 1);
                    return currentStack;
                }
                else
                {
                    return currentStack;
                }
            }
            else
            {
                //TODO:接入内部通知系统，通知用户无法创建新标签页
                throw new Exception("已达到最大标签页数量，无法创建新标签页");
            }
        }

        public ITabContent GetSelectedTab()
        {
            foreach (var tab in tabContents)
            {
                if (tab.Index == CurrentTabId)
                {
                    return tab;
                }
            }
            throw new Exception("未找到选中的标签页");
        }

        public ITabContent GetTabByID(int id)
        {
            foreach (var tab in tabContents)
            {
                if (tab.Index == id)
                {
                    return tab;
                }
            }
            throw new Exception("未找到指定ID的标签页");
        }

        public void SelectAndSwitchTab(int id)
        {
            SwitchTab(-1, id);
        }

        private void RecycleID(int id)
        {
            IDStack.Push(id);
            UsedIDCount -= 1;
        }

        private void OnPropertyChanged(string propertyName)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        public event ITabManager.TabChangedEventHandler? TabChanged;
    }

    public interface ITabManager : INotifyPropertyChanged
    {
        public ObservableCollection<ITabContent> tabContents { get; set; }
        public int CurrentTabId { get; }
        int TabCount { get; }
        
        public delegate void TabChangedEventHandler(int sourceid, int targetid);
        public event TabChangedEventHandler? TabChanged;
        
        public void NewTab(Panel content, String backgroundColor,out int newid);
        public void CloseTab(int id);
        public void SelectAndSwitchTab(int id);
        public void OnTabClicked(int id);
        public ITabContent GetSelectedTab();
        public ITabContent GetTabByID(int id);
    }
}

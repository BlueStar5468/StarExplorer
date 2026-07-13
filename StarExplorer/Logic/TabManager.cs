using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using StarExplorer.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace StarExplorer.Logic
{
    internal class TabManager : ITabManager , INotifyPropertyChanged
    {
        IDManager iDManager;

        private int currentTabId = -1;
        public int TabCount { get => tabContents.Count; }
        public int CurrentTabId { get => currentTabId; }
        //标签页抽象数据列表
        public ObservableCollection<ITabContent> tabContents { get; set; } = new ObservableCollection<ITabContent>();
        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action? SelectedTabChanged;
        public TabManager(int maxIDCount)
        {
            if (maxIDCount < 10) maxIDCount = 10;
            iDManager = new IDManager(maxIDCount);
        }

        public void NewTab(Panel content, out int newid)
        {
            int id = iDManager.GetID();
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
            {
                TabContent newTabContent = new TabContent(
                    content,
                    id);
                tabContents.Add(newTabContent);
            });
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
                Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
                {
                    tabContents.Remove(tabToRemove);
                });
                iDManager.RecycleID(id);
            }
        }

        private void SwitchTab(int id)
        {
            foreach (var tab in tabContents)
            {
                if (tab.Index == id)
                {
                    currentTabId = id;
                    OnPropertyChanged(nameof(CurrentTabId));
                    return;
                }
            }
            throw new Exception("未找到要切换的标签页");
        }

        public void OnTabClicked(int id)
        {
            if (id != currentTabId)
            {
                SwitchTab(id);
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

        public void SelectTab(int id)
        {
            currentTabId = id;
            OnPropertyChanged(nameof(CurrentTabId));
        }

        private void OnPropertyChanged(string propertyName)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
            if (propertyName == nameof(CurrentTabId))
            {
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SelectedTabChanged?.Invoke();
                });
            }
        }

        public void Initialize()
        {
            
        }
    }

    public interface ITabManager : INotifyPropertyChanged , IModule
    {
        public event Action? SelectedTabChanged;
        public ObservableCollection<ITabContent> tabContents { get; set; }
        public int CurrentTabId { get; }
        int TabCount { get; }
        public void NewTab(Panel content, out int newid);
        public void CloseTab(int id);
        public void SelectTab(int id);
        public void OnTabClicked(int id);
        public ITabContent GetSelectedTab();
    }

    public class TabContent : INotifyPropertyChanged , ITabContent
    {
        private int index = 0;
        private Panel content = null!;//标签页的通用内容容器，具体内容由外部设置和管理
        public int Index { get => index; set { if (index != value) { index = value; OnPropertyChanged(nameof(Index)); } } }
        public Panel Content { get => content; set { if (content != value) { content = value; OnPropertyChanged(nameof(Content)); } } }

        public TabContent(Panel content,int index)
        {
            Index = index;
            Content = content;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }
    }

    public interface ITabContent : INotifyPropertyChanged
    {
        public int Index { get; set; }
        public Panel Content { get; set; }
    }
}

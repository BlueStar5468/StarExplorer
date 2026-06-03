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

        public void SelectTab(int id)
        {
            currentTabId = id;
            OnPropertyChanged(nameof(CurrentTabId));
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

    }

    public interface ITabManager : INotifyPropertyChanged
    {
        public ObservableCollection<ITabContent> tabContents { get; set; }
        public int CurrentTabId { get; }
        int TabCount { get; }
        public void NewTab(Panel content, String backgroundColor,out int newid);
        public void CloseTab(int id);
        public void SelectTab(int id);
        public void OnTabClicked(int id);
        public ITabContent GetSelectedTab();
    }

    public class TabContent : INotifyPropertyChanged , ITabContent
    {
        private String label = "未命名标签页";
        private String backgroundColor = null!;
        private IImage icon = null!;
        private int index = 0;
        private Panel content = null!;//标签页的通用内容容器，具体内容由外部设置和管理
        private bool isSelected = false;
        public bool IsSelected { get => isSelected; set { if (isSelected != value) { isSelected = value; OnPropertyChanged(nameof(IsSelected)); } } }
        public String Label { get => label; set { if (label != value) { label = value; OnPropertyChanged(nameof(Label)); } } }
        public IImage Icon { get => icon; set { if (icon != value) { icon = value; OnPropertyChanged(nameof(Icon)); } } }
        public int Index { get => index; set { if (index != value) { index = value; OnPropertyChanged(nameof(Index)); } } }
        public Panel Content { get => content; set { if (content != value) { content = value; OnPropertyChanged(nameof(Content)); } } }
        public String BackgroundColor { get => backgroundColor; set { if (backgroundColor != value) { backgroundColor = value; OnPropertyChanged(nameof(BackgroundColor)); } } }

        public TabContent(String label, IImage icon, Panel content,int index, string backgroundColor)
        {
            this.Label = label;
            this.Icon = icon;
            this.Index = index;
            this.Content = content;
            this.BackgroundColor = backgroundColor;
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
        public String Label { get; set; }
        public IImage Icon { get; set; }
        public int Index { get; set; }
        public Panel Content { get; set; }
        public String BackgroundColor { get; set; }
        public bool IsSelected { get; set; }
    }
}

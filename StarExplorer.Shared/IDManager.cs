using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Shared
{
    public class IDManager
    {
        //id栈
        Stack<int> IDStack = new Stack<int>();
        
        private int usedIDCount = 0;
        private int maxIDCount;         //0表示不限制id分配数量
        public int MaxIDCount { get => maxIDCount; }
        public int UsedIDCount { get => usedIDCount; }

        public IDManager(int maxIDCount)
        {
            if (maxIDCount < 0) throw new InvalidOperationException("不能令可分配的id小于0");
            IDStack.Push(0);
            this.maxIDCount = maxIDCount;
        }

        public int GetID()
        {
            int id;
            if (UsedIDCount < maxIDCount || maxIDCount == 0)
            {
                id = IDStack.Pop();
                if (IDStack.Count == 0) IDStack.Push(id + 1);
                usedIDCount += 1;
                return id;
            }
            else throw new InvalidOperationException("已达到最大id分配数量，无法分配新的id");
        }

        public void RecycleID(int id)
        {
            if (IsIDLegal(id))
            {
                IDStack.Push(id);
                usedIDCount -= 1;
            }
            else throw new InvalidOperationException("试图回收一个不合法的id");
        }

        private bool IsIDLegal(int id)
        {
            if (maxIDCount != 0)
            {
                if (id < 0 || id > maxIDCount) return false;
                else return true;
            }
            else
            {
                if (id < 0) return false;
                else return true;
            }
        }

        public void Clear()
        {
            IDStack.Clear();
            IDStack.Push(0);
            usedIDCount = 0;
        }

    }
}

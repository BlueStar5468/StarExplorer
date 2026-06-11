using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarExplorer.Shared
{
    public interface IModule
    {
        //模块接口，定义模块应实现的基本功能
        void Initialize(); //模块初始化方法，在应用程序启动时调用
    }
}

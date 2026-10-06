# StarExplorer

用 C# / .NET 8 / Avalonia 编写的**文件资源管理器**，多标签页界面，目标是做成跨平台（Windows → Linux → Android）的文件管理器。

目前处于**早期开发阶段**：主窗口、多标签页、磁盘列表、文件夹浏览、Toast 通知已可用；侧边栏、设置保存、Linux/Android 后端尚未实现。

## 功能状态

| 功能 | 状态 |
|---|:---:|
| 主窗口、标题栏、工具栏 | ✅ |
| 磁盘列表（卷标、盘符、容量进度条、图标） | ✅ |
| 悬停动画与选中效果 | ✅ |
| 多标签页：新增 / 切换 / 关闭 | ✅ |
| 文件夹浏览（图标 / 名称 / 修改时间） | ✅ |
| 路径显示 + 返回起始页（"H" 按钮） | ✅ |
| Toast 通知（右下角堆叠、自动消失） | ✅ |
| 双击进入下一层文件夹 | ⚠️ 有实现，但目标路径计算有误 |
| 侧边栏（快速访问 / 位置） | ❌ 仅灰色占位 |
| 地址栏手动输入跳转 | ❌ 只读 |
| 窗口最小化 / 最大化 / 关闭 | ❌ 按钮未接事件 |
| 设置保存到文件 | ❌ 全内存，重启恢复默认 |
| Linux / Android 后端 | ❌ 非 Windows 平台会抛异常 |

图例：✅ 可用 ｜ ⚠️ 部分可用 ｜ ❌ 未实现

## 技术栈

- **.NET 8** + **Avalonia 12.0.4**（Fluent 主题），当前仅支持 Windows 10+
- 界面用 **C# 代码构建**，不用 XAML —— 除主窗口 `Views/Explorer.axaml` 外，所有控件都是 `new` 出来再 `.Bind()` 绑定的，**不要去找对应的 `.axaml` 文件**

## 快速开始

需要 .NET 8 SDK 与 Windows 10+。

```bash
cd StarExplorer                    # 含 StarExplorer.sln 的目录
dotnet build StarExplorer.sln
dotnet run --project StarExplorer/StarExplorer.csproj
```

Visual Studio：打开 `StarExplorer.sln`，把 `StarExplorer` 设为启动项目，F5。

启动后双击磁盘即可进入浏览，点地址栏左侧 "H" 返回。**DEBUG 下按 F12** 可打开 Avalonia DevTools，排查界面问题很有用。

## 项目结构

```
StarExplorer/                 主程序（UI + 逻辑层）
├── Logic/                      逻辑层：组合根、核心数据、设置、控制器、标签页管理
├── Views/                      视图层：主窗口、子浏览器
├── Controls/                   控件层：设备面板、文件列表、标签栏、Toast
└── Assets/                     图标资源

StarExplorer.Shared/          共享契约与数据模型（接口、实体、ID 管理器）
StarExplorer.Abstractions/    文件系统抽象层（平台门面）
StarExplorer.WindowsBackend/  Windows 后端实现（基于 System.IO）
```

## 架构

分四层：**UI 层**（`Views` / `Controls`）→ **逻辑层**（`Logic`）→ **抽象层**（`Abstractions`）→ **后端层**（`WindowsBackend`），另有一个无依赖的 **`Shared`** 存放接口与数据模型。

程序从 `LogicRoot`（组合根）展开：它创建全部模块，按固定顺序调用各自的 `Initialize`，再由 `MainWindowControler` 建窗口、建标签页。

> ⚠️ 注意：依赖方向与直觉相反。`Abstractions` 反过来引用了 `WindowsBackend`，并直接 `new WindowsBackend()`，所以抽象层**并未真正屏蔽平台差异**。这是已知的架构债。

## 已知问题

**功能性**

1. `Controls/ItemsPanel.cs:146` —— `Path.Combine(currentPath, "")` 第二个参数是空串，**双击文件夹只会重开同一目录**。改用 `ItemToDisplay.Path` 即可（它本身就是子项的完整路径）。
2. `Controls/DevicesPanel.cs:95` —— 无设备时走 `if` 分支，而挂载 `ScrollViewer` 的两行只在 `else` 分支执行，导致**"未检测到设备"提示永远不显示**。
3. `Logic/TabManager.cs:42` —— `CloseTab` 不更新 `currentTabId`，关闭选中的标签后 `GetSelectedTab()` 会抛异常。
4. `Controls/ToastControl.cs:189` —— 自动关闭定时器在构造器里就启动，早于滑入动画，可能先滑出再滑入。
5. `Controls/DevicesPanel.cs:77` —— `ThemeColor` 绑到 `Background` 时漏了 `BrushConverter`，该绑定失败。
6. `Controls/TabBarPanel.Data.cs:138` —— `case` 监听的是 `WindowDisplayBackgroundColor`，处理的却是标签项背景色。

**架构**

- `AbstractionLayer` 注释称"单例模式"，实际无单例；非 Windows 平台的 `GetDevices` 会静默返回空列表。
- `WindowsBackend` 的目录读取没有 `try/catch`，靠上层兜住；设备枚举的 `catch` 体为空。
- 全项目无日志、无全局异常处理。
- `Settings` 300+ 行、40+ 属性，是"上帝对象"。
- `MainWindowControler.CreateDevicePanel()` 是死代码；`CreateSubExplorer` 每次覆盖控制器字段，多标签时旧实例无法释放。
- `Controls/ItemsControl.cs` 订阅了 `itemDataContext.PropertyChanged` 但从不解除。

**作者自己的记录**

`StarExplorer/Review.md` 记了一条**未修复**的隐患：`ItemsPanel` 的模板绑定用显式 `Source` 钉死了旧 item，容器复用时不会更新。当前靠把 `supportsRecycling` 设为 `false` 规避（关闭复用换正确性），换虚拟化面板会暴露问题。

## 上手建议

**阅读顺序**：`Program.cs` → `App.axaml.cs` → `Logic/LogicRoot.cs` → `Shared/Miscs.cs` → `WindowsBackend.cs` → `Logic/CoreData.cs` → `Logic/MainWindowControler.cs` → `Controls/DevicePanel*` + `DevicesPanel.cs` + `DeviceControl.cs`（一个完整的 Data / View / Controller 范例）。

**动手顺序**（收益/难度比从高到低）：

1. 修 `ItemsPanel.cs:146`，让双击能真正进入子文件夹
2. 修 `DevicesPanel.cs` 无设备分支
3. 修 `TabManager.CloseTab` 的 `currentTabId`
4. 接窗口按钮 `m` / `M` / `C`
5. 设置持久化
6. 侧边栏与地址栏

## 代码约定

- **沿用现有拼写，不要"顺手修正"**：`Controler`、`Adress`、`Infomation`、`LoadFormFile`、`Catgory` 等横跨接口、实现类、`nameof` 与字符串绑定，而**字符串绑定改名不会报编译错误**，只会静默失效。
- **新增 UI 用三件套模式**：`XxxData`（数据 + `INotifyPropertyChanged`）、`Xxx`（纯视觉构建）、`XxxController`（组装 + 业务逻辑）。
- **绑定类型不匹配必须用转换器**：`Color`→`Background` 用 `BrushConverter`，`int`→`Margin` 用 `ThicknessConverter`。
- **通知名一律用 `nameof()`**；加派生属性时记得在相关 setter 里补通知。
- **加了新模块**：在 `LogicRoot` 加字段 → 构造器 `new` → `BindEvents()` 里**按正确位置**注册（顺序错了会抛"尚未创建"异常）。

---

更详细的技术解析（架构、核心机制、启动流程、逐文件说明、扩展教程）见上一级目录的 `StarExplorer-Guide.html`，但需注意它描述的是**更早的版本**，文件浏览与 Toast 系统当时尚未实现。

`StarExplorer/Status.md` 与 `StarExplorer/Review.md` 是作者的开发便签，了解"当前卡在哪、踩过哪些坑"可以优先一读。

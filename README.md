# 📸 AutoScreenShotAI

**AutoScreenShotAI** 是一款 Windows 桌面工具，专注于**定时自动截图 + 调用视觉 AI 分析截图内容**。它使用 WPF 构建简洁界面，通过 Python 脚本对接**智谱 GLM 视觉大模型**，支持多语言、系统托盘运行，适合长期挂机监控、内容记录或自动化工作流。

## ✨ 功能特色

- ⏱️ 定时截图：可配置间隔（秒），循环截取所有屏幕内容
- 🧠 AI 分析：调用智谱 GLM-4V 视觉模型，生成详细的文字描述
- 📝 灵活的输出：
  - 截图保存为 JPG/PNG
  - AI 分析结果自动保存为同名的 `.md` 文件
  - 文件名使用 C# DateTime 格式模板，例如 `yyyy-MM-dd HH-mm-ss`
- 🌍 双语界面：中文 / 英文一键切换
- 🗔 系统托盘：支持最小化到托盘、开机自启、启动时最小化
- 🧹 自动清理：可设置 X 天后自动删除旧截图文件夹
- 🐍 智能 Python 调用：自动检测虚拟环境，超时自动终止

## 🖥️ 系统要求

- **操作系统**：Windows 10/11 x64
- **.NET 运行库**：.NET 8.0 Desktop Runtime（或更高）
- **Python**：3.10+，并安装 `langchain-openai` 和 `langchain-core`
- **智谱 API Key**：需注册 [智谱开放平台](https://open.bigmodel.cn/) 并获取 API Key

## 📦 安装

### 1. 下载/编译应用

你可以直接从 [Releases]() 页面下载预编译版本，或者自行编译：

```bash
git clone https://github.com/yourname/AutoScreenShotAI.git
cd AutoScreenShotAI
dotnet build -c Release
```

生成的可执行文件在 `bin/Release/net8.0-windows/` 下。

### 2. 安装 Python 依赖

在脚本目录下（默认与应用同目录）执行：

```bash
pip install langchain-openai langchain-core
```

如果有虚拟环境，将 `venv/Scripts/python.exe` 放在脚本同目录，工具会自动识别。

### 3. 配置 API Key（重要！）

**方式一（推荐）**：设置系统环境变量 `ZHIPU_API_KEY`，值是你的智谱 API Key。  
**方式二**：在启动应用后，手动设置环境变量，或修改 Python 脚本（不推荐）。

## ⚙️ 配置说明

所有界面上的设置都会自动保存到 `settings.json`（位于可执行文件旁边）。主要配置项：

| 设置项 | 说明 |
|--------|------|
| 输出目录 | 截图和 `.md` 文件的存放位置 |
| Python 脚本 | 用于 AI 分析的 Python 脚本路径（默认 `analyze_screenshot.py`） |
| 文件名模板 | C# DateTime 格式字符串，如 `yyyy-MM-dd HH-mm-ss` |
| 截图间隔 | 定时截图的间隔秒数 |
| 提示词 | 发送给视觉模型的提问 |
| 自动删除 | 开启后，将删除 X 天前的截图文件夹 |

## 🚀 使用方法

1. 启动 **AutoScreenShotAI**  
2. 设置**输出目录**、**Python 脚本**和**截图间隔**  
3. （可选）修改提示词，定制 AI 分析方向  
4. 点击 **“立即截图”** 进行单次截图，或点击 **“运行”** 开始定时任务  
5. 截图会被保存，AI 分析结果会生成在相同目录下的 `.md` 文件中  
6. 可最小化到系统托盘，后台静默工作

## 🌐 语言切换

界面默认跟随系统语言，可通过右上角的 **语言** 下拉框手动切换中/英文。

## 📂 项目结构

```
AutoScreenShotAI/
├── AutoScreenShotAI.csproj       # 项目文件
├── MainWindow.xaml               # WPF 界面
├── MainWindow.xaml.cs            # 界面逻辑
├── Services/                     # 核心服务层
│   ├── SettingsService.cs        # 配置文件读写
│   ├── TrayService.cs            # 托盘管理
│   ├── ScreenshotService.cs      # 截图功能
│   ├── PythonService.cs          # Python 进程调用
│   └── LanguageService.cs        # 多语言文本
├── Helpers/
│   └── Win32Helper.cs            # Win32 API 封装
├── ico/                          # 图标资源
└── analyze_screenshot.py         # AI 分析 Python 脚本
```

## 🔒 安全提示

- **不要将 API Key 硬编码或提交到仓库**。请使用环境变量或本地加密存储。
- Python 脚本通过标准输入输出传递数据，不会产生中间文件，保证安全。
- 应用本身不会上传任何数据，所有 API 调用均经过你的密钥和网络。

## 🧪 常见问题

**Q: 点击“运行”后没有反应？**  
A: 请检查截图间隔是否为正整数，以及输出目录是否有效。

**Q: 分析失败，提示 `[ERROR] 缺少依赖库`？**  
A: 确认 Python 环境中已安装 `langchain-openai` 和 `langchain-core`。

**Q: 为什么没有生成 .md 文件？**  
A: 确保“启用脚本”已勾选，且 Python 脚本路径正确指向 `analyze_screenshot.py`。

**Q: 如何更换其他视觉模型？**  
A: 修改 `analyze_screenshot.py` 中的 `VISION_MODEL` 环境变量，或使用 `--model` 参数（需 C# 端传递）。

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！如果你有好的想法或发现 Bug，请随时交流。

## 📄 许可证

本项目使用 MIT 许可证，详情见 [LICENSE](LICENSE) 文件。

---

如果对你有帮助，欢迎给个 ⭐ Star 支持一下！

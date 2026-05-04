namespace AutoScreenShotAI.Services
{
    public class LanguageService
    {
        public bool IsChinese { get; private set; }

        public LanguageTexts Texts { get; private set; } = new LanguageTexts();

        public void SetLanguage(bool isChinese)
        {
            IsChinese = isChinese;
            if (isChinese)
            {
                Texts = new LanguageTexts
                {
                    OutputDir = "输出目录",
                    PythonScript = "Python 脚本",
                    FilenameTemplate = "文件名模板",
                    ShotInterval = "截图间隔（秒）",
                    Format = "格式",
                    Prompt = "提示词",
                    Days = "天",
                    Language = "语言",
                    AutoStartup = "开机自启",
                    MinRunning = "启动时最小化",
                    MinToTray = "关闭时最小化到托盘",
                    EnableScript = "启用脚本",
                    AutoDelete = "自动删除",
                    TakeShot = "立即截图",
                    Run = "运行",
                    Stop = "停止",
                    StatusReady = "状态：就绪",
                    DefaultPrompt = "请详细描述此截图的内容，包括所有可见的文字、用户界面元素以及布局情况",
                    OutputDirTip = "截图保存位置",
                    PythonPathTip = "脚本所在路径",
                    TemplateTip = "C# ToString() 格式，例如：yyyy-MM-dd HH-mm-ss",
                    IntervalTip = "截图间隔（秒）",
                    PromptTip = "AI 提示词",
                    BrowseOutputTip = "选择文件夹",
                    OpenOutputTip = "打开文件夹",
                    BrowsePythonTip = "选择文件",
                    OpenScriptTip = "打开所在目录",
                    ResetTemplateTip = "重置模板"
                };
            }
            else
            {
                Texts = new LanguageTexts
                {
                    OutputDir = "Output Directory",
                    PythonScript = "Python Script",
                    FilenameTemplate = "Filename Template",
                    ShotInterval = "Shot Interval (s)",
                    Format = "Format",
                    Prompt = "Prompt",
                    Days = "Days",
                    Language = "Language",
                    AutoStartup = "Auto Startup",
                    MinRunning = "Running at Startup",
                    MinToTray = "Minimize to Tray by Closing",
                    EnableScript = "Enable Script",
                    AutoDelete = "Auto Delete after",
                    TakeShot = "Take a Shot Now",
                    Run = "Run",
                    Stop = "Stop",
                    StatusReady = "Status: Ready",
                    DefaultPrompt = "Please describe the content of this screenshot in detail...",
                    OutputDirTip = "Where to Save Screenshots",
                    PythonPathTip = "Where the Script is",
                    TemplateTip = "C# Method ToString() Param: yyyy-MM-dd HH-mm-ss",
                    IntervalTip = "Shot per Interval in Seconds",
                    PromptTip = "AI Prompt",
                    BrowseOutputTip = "Choose a Folder",
                    OpenOutputTip = "Open the Folder",
                    BrowsePythonTip = "Choose a File",
                    OpenScriptTip = "Open the Folder",
                    ResetTemplateTip = "Reset the Template"
                };
            }
        }
    }

    // 简单 DTO
    public class LanguageTexts
    {
        public string OutputDir { get; set; } = string.Empty;
        public string PythonScript { get; set; } = string.Empty;
        public string FilenameTemplate { get; set; } = string.Empty;
        public string ShotInterval { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public string Days { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string AutoStartup { get; set; } = string.Empty;
        public string MinRunning { get; set; } = string.Empty;
        public string MinToTray { get; set; } = string.Empty;
        public string EnableScript { get; set; } = string.Empty;
        public string AutoDelete { get; set; } = string.Empty;
        public string TakeShot { get; set; } = string.Empty;
        public string Run { get; set; } = string.Empty;
        public string Stop { get; set; } = string.Empty;
        public string StatusReady { get; set; } = string.Empty;
        public string DefaultPrompt { get; set; } = string.Empty;
        public string OutputDirTip { get; set; } = string.Empty;
        public string PythonPathTip { get; set; } = string.Empty;
        public string TemplateTip { get; set; } = string.Empty;
        public string IntervalTip { get; set; } = string.Empty;
        public string PromptTip { get; set; } = string.Empty;
        public string BrowseOutputTip { get; set; } = string.Empty;
        public string OpenOutputTip { get; set; } = string.Empty;
        public string BrowsePythonTip { get; set; } = string.Empty;
        public string OpenScriptTip { get; set; } = string.Empty;
        public string ResetTemplateTip { get; set; } = string.Empty;
    }
}
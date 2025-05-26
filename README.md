Workaround for the issue: https://github.com/microsoft/microsoft-ui-xaml/issues/10529

NuGet package version
WinUI 3 - Windows App SDK 1.7.1: 1.7.250401001

After researching the issue, I created a workaround.

Basically, if you want a custom title bar, don’t use Window.ExtendsContentIntoTitleBar or Window.SetTitleBar.

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();

        // Not recommended:
        this.ExtendsContentIntoTitleBar = true;
        this.SetTitleBar(AppTitleBar);
    }
}

Use the AppWindow.TitleBar API:

var titleBar = this.AppWindow.TitleBar;
titleBar.ExtendsContentIntoTitleBar = true;

Then, disable the default title bar buttons (Minimize, Maximize, Close):

Presenter.IsResizable = true;
Presenter.SetBorderAndTitleBar(true, false);

and handle the drag area and top resize manually:

this._newWndProc = new WndProcDelegate(CustomWndProc);
this._prevWndProc = SetWindowLongPtr(this.HWnd, GWLP_WNDPROC, this._newWndProc);
using Microsoft.UI.Windowing;
using Microsoft.UI;
using System;
using System.Runtime.InteropServices;
using WinRT;

namespace PoitnerFlickering_Workaround.Helpers;

public class CustomTitlebarWorkaround
{
    /// <summary>
    /// The window handle
    /// </summary>
    public IntPtr HWnd { get; private set; }
    public WindowId WindowId { get; private set; }
    public AppWindow AppWindow { get; private set; }
    public OverlappedPresenter Presenter { get; private set; }

    private WndProcDelegate _newWndProc;
    private nint _prevWndProc;

    public DraggableWindow DragHelper;

    public CustomTitlebarWorkaround(object target)
    {
        // Get the window handle using the WinRT.Interop library
        this.HWnd = WinRT.Interop.WindowNative.GetWindowHandle(target);

        // Get the window ID using the Win32Interop library
        this.WindowId = Win32Interop.GetWindowIdFromWindow(HWnd);

        // Get the AppWindow object from the window ID
        this.AppWindow = AppWindow.GetFromWindowId(WindowId);

        this.DragHelper = new(this.AppWindow);

        // Get the presenter object from the AppWindow object and set its border and title bar
        // Native AOT workaround for NET 9 https://github.com/microsoft/CsWinRT/issues/1930
        this.Presenter = AppWindow.Presenter.As<OverlappedPresenter>();
    }

    public void ApplyCustomTitleBar()
    {
        // Check to see if customization is supported.
        // The method returns true on Windows 10 since Windows App SDK 1.2, and on all versions of
        // Windows App SDK on Windows 11.
        if (Microsoft.UI.Windowing.AppWindowTitleBar.IsCustomizationSupported())
        {
            var titleBar = this.AppWindow.TitleBar;
            titleBar.ExtendsContentIntoTitleBar = true;
            //titleBar.PreferredTheme = TitleBarTheme.Dark;

            // workaround to hide the contextmenu of the window
            //this._windowHelper.appWindow.TitleBar.IconShowOptions = Microsoft.UI.Windowing.IconShowOptions.HideIconAndSystemMenu;

            // Set active window colors
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        // Set the presenter and disable default resizing
        // Disable system titlebar to enable full customization
        Presenter.IsResizable = true;
        Presenter.SetBorderAndTitleBar(true, false); // workaround - disable the default Minimize, Maximize, Close Buttons

        // Set up the custom window procedure for handling hit test messages
        this._newWndProc = new WndProcDelegate(CustomWndProc);
        this._prevWndProc = SetWindowLongPtr(this.HWnd, GWLP_WNDPROC, this._newWndProc);
    }

    /// <summary>
    /// WndProc That remove the only Top resize and Drag area
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="msg"></param>
    /// <param name="wParam"></param>
    /// <param name="lParam"></param>
    /// <returns></returns>
    private IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            //case WM_NCCALCSIZE:
            //    return IntPtr.Zero;

            case WM_NCHITTEST:
                return HitTestAllResize(hWnd, lParam);

            case WM_SYSCOMMAND:
                break;
        }

        return CallWindowProc(_prevWndProc, hWnd, msg, wParam, lParam);
    }

    private IntPtr HitTestAllResize(IntPtr hWnd, IntPtr lParam)
    {
        int x = (short)(lParam.ToInt32() & 0xFFFF);
        int y = (short)((lParam.ToInt32() >> 16) & 0xFFFF);

        GetWindowRect(hWnd, out RECT rect);

        //Debug.WriteLine($"[CustomTitlebarWorkaround] HitTestAllResize: Mouse at ({x},{y}), window top={rect.top}, bottom={rect.bottom}");

        bool top = y >= rect.top && y < rect.top + BORDER_WIDTH;
        bool bottom = y <= rect.bottom && y > rect.bottom - BORDER_WIDTH;
        bool left = x >= rect.left && x < rect.left + BORDER_WIDTH;
        bool right = x <= rect.right && x > rect.right - BORDER_WIDTH;

        if (top && left) return HTTOPLEFT;
        if (top && right) return HTTOPRIGHT;
        if (bottom && left) return HTBOTTOMLEFT;
        if (bottom && right) return HTBOTTOMRIGHT;

        // if (top) return HTTOP;
        if (top) return IntPtr.Zero; // workaround - disable the top size

        if (bottom) return HTBOTTOM;
        if (left) return HTLEFT;
        if (right) return HTRIGHT;

        // Allow dragging within the top title bar area (first 32px)
        const int TITLE_BAR_HEIGHT = 32;
        const int DRAG_MARGIN_TOP = 7;
        const int DRAG_MARGIN_BOTTOM = 0;

        if (y >= rect.top + DRAG_MARGIN_TOP && y < rect.top + TITLE_BAR_HEIGHT - DRAG_MARGIN_BOTTOM)
        {
            //return HTCAPTION;
            return IntPtr.Zero; // workaround - disable the drag area
        }

        return IntPtr.Zero;
    }

    private const int BORDER_WIDTH = 8;

    private const int WM_NCCALCSIZE = 0x0083;
    private const int WM_NCHITTEST = 0x0084;
    private const int WM_SYSCOMMAND = 0x0112;
    private const int WM_GETMINMAXINFO = 0x0024;

    private const int GWLP_WNDPROC = -4;

    private const int IGNORE = -2;
    private const int HTCLIENT = 1;
    private const int HTCAPTION = 2;

    private const int HTLEFT = 10;
    private const int HTRIGHT = 11;
    private const int HTTOP = 12;
    private const int HTTOPLEFT = 13;
    private const int HTTOPRIGHT = 14;
    private const int HTBOTTOM = 15;
    private const int HTBOTTOMLEFT = 16;
    private const int HTBOTTOMRIGHT = 17;

    [DllImport("user32.dll")]
    private static extern nint CallWindowProc(nint lpPrevWndFunc, nint hWnd, uint msg, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    private static extern nint SetWindowLongPtr(nint hWnd, int nIndex, WndProcDelegate newProc);

    [DllImport("user32.dll")]
    static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}
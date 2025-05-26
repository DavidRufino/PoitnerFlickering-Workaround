using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;

namespace PoitnerFlickering_Workaround.Helpers;

/// <summary>
/// Special Thanks
/// castorix - https://github.com/castorix/WinUI3_Transparent/blob/master/MainWindow.xaml.cs
/// </summary>
public class DraggableWindow
{
    private readonly AppWindow _appWindow;  // The AppWindow instance

    private int nX = 0;
    private int nY = 0;
    private int nXWindow = 0;
    private int nYWindow = 0;

    private bool bMoving = false;

    public DraggableWindow(AppWindow window)
    {
        this._appWindow = window;
    }

    public void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var properties = e.GetCurrentPoint((UIElement)sender).Properties;
        // Check if the left mouse button is pressed
        if (properties.IsLeftButtonPressed)
        {
            ((UIElement)sender).CapturePointer(e.Pointer);
            nXWindow = this._appWindow.Position.X;
            nYWindow = this._appWindow.Position.Y;
            Windows.Graphics.PointInt32 pt;
            GetCursorPos(out pt);
            nX = pt.X;
            nY = pt.Y;
            //Console.Beep(1000, 10);
            bMoving = true;
        }
    }

    public void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        //Microsoft.UI.Input.PointerPoint pp = e.GetCurrentPoint((UIElement)sender);
        //Point ptElement = new Point(pp.Position.X, pp.Position.Y);
        //IEnumerable<UIElement> elementStack = VisualTreeHelper.FindElementsInHostCoordinates(ptElement, (UIElement)sender);
        //foreach (UIElement item in elementStack)
        //{
        //    FrameworkElement feItem = item as FrameworkElement;
        //    //cast to FrameworkElement, need the Name property
        //    if (feItem != null)
        //    {
        //        if (feItem.Name.Equals("myButton"))
        //        {
        //            return;
        //        }
        //    }
        //}

        var properties = e.GetCurrentPoint((UIElement)sender).Properties;
        if (properties.IsLeftButtonPressed)
        {
            //Console.Beep(8000, 10);

            Windows.Graphics.PointInt32 pt;
            GetCursorPos(out pt);

            //if (((UIElement)sender).GetType() == typeof(StackPanel))
            if (bMoving)
                this._appWindow.Move(new Windows.Graphics.PointInt32(nXWindow + (pt.X - nX), nYWindow + (pt.Y - nY)));

            //Microsoft.UI.Input.PointerPoint pp = e.GetCurrentPoint((UIElement)sender);
            //pt.X -= (int)pp.Position.X;
            //pt.Y -= (int)pp.Position.Y;
            //pt.X -=8;
            //pt.Y -= 31;
            //Windows.Graphics.PointInt32 pt = new Windows.Graphics.PointInt32((int)pp.Position.X, (int)pp.Position.Y);               
            //IntPtr pPoint = Marshal.AllocHGlobal(Marshal.SizeOf(pt));
            //Marshal.StructureToPtr(pt, pPoint, false);
            //PostMessage(hWnd, WM_NCLBUTTONDOWN, HTCAPTION, pPoint);
            e.Handled = true;
        }
    }

    public void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        //nXWindow = _apw.Position.X;
        //nYWindow = _apw.Position.Y;
        ((UIElement)sender).ReleasePointerCaptures();
        bMoving = false;
    }

    [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GetCursorPos(out Windows.Graphics.PointInt32 lpPoint);
}
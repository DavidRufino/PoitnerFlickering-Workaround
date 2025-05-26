using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using PoitnerFlickering_Workaround.Helpers;
using PoitnerFlickering_Workaround.Pages;
using System;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PoitnerFlickering_Workaround;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private CustomTitlebarWorkaround _customTitlebarWorkaround { get; set; }

    public MainWindow()
    {
        this.InitializeComponent();

        // Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/10529
        this._customTitlebarWorkaround = new(this);

        // Apply custom titleBar ExtendsContentIntoTitleBar to true via AppWindowTitleBar;
        this._customTitlebarWorkaround.ApplyCustomTitleBar();

        // Dont use titleBar ExtendsContentIntoTitleBar via Window;
        //this.ExtendsContentIntoTitleBar = true; // dont use the Window.ExtendsContentIntoTitleBar
        //this.SetTitleBar(AppTitleBar);  // dont use the Window.SetTitleBar
    }

    private void Grid_Loaded(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[MainWindow] Grid_Loaded");

        ApplySettings();
    }

    private void AppTitleBar_Loaded(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[MainWindow] AppTitleBar_Loaded");

        // Call the method to update the drag region based on the new size of the title bar
        UpdateDragRegion();
    }

    private void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
    {
        Debug.WriteLine("[MainWindow] AppWindow_Closing");
    }

    private void OnButtonClicked(object sender, RoutedEventArgs e)
    {
        // Check if the sender is a FrameworkElement
        if (!(sender is FrameworkElement element)) return;

        string name = element.Name;

        Debug.WriteLine($"[MainWindow] OnButtonClicked: {name}");

        switch (name)
        {
            case "BtnMinimize":
                this._customTitlebarWorkaround.Presenter.Minimize();
                break;

            case "BtnMaximize":
                if (this._customTitlebarWorkaround.Presenter.State == OverlappedPresenterState.Maximized)
                {
                    this._customTitlebarWorkaround.Presenter.Restore();
                }
                else
                {
                    this._customTitlebarWorkaround.Presenter.Maximize();
                }
                break;

            case "BtnClose":

                // WARNING: The method Close() dont trigger the event AppWindow.Closing
                this.Close();
                break;
        }
    }

    /// <summary>
    /// Event handler for the SizeChanged event of the AppTitleBar.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        Debug.WriteLine("[MainWindow] AppTitleBar_SizeChanged");

        // Call the method to update the drag region based on the new size of the title bar
        UpdateDragRegion();
    }

    /// <summary>
    /// Sets up a custom title bar for the page.
    /// </summary>
    public void UpdateDragRegion()
    {
        try
        {
            // Call the method to update the drag region based on the new size of the title bar
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[MainWindow] UpdateDragRegion Error {ex}");
        }
    }

    private void ApplySettings()
    {
        MainContent.Content = new MainPage();

        UnregisterEvents();
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        // Subscribe event handle
        //this._customTitlebarWorkaround.AppWindow.Closing += AppWindow_Closing;

        AppTitleBar.Loaded += AppTitleBar_Loaded;
        AppTitleBar.SizeChanged += AppTitleBar_SizeChanged; // event to Update the Drag Region when window size changes

        if (this._customTitlebarWorkaround is not null)
        {
            GridDraggableWindow.PointerPressed += this._customTitlebarWorkaround.DragHelper.OnPointerPressed;
            GridDraggableWindow.PointerMoved += this._customTitlebarWorkaround.DragHelper.OnPointerMoved;
            GridDraggableWindow.PointerReleased += this._customTitlebarWorkaround.DragHelper.OnPointerReleased;
        }
    }

    private void UnregisterEvents()
    {
        // Unsubscribe event handle
        //this._customTitlebarWorkaround.AppWindow.Closing -= AppWindow_Closing;

        AppTitleBar.Loaded -= AppTitleBar_Loaded;
        AppTitleBar.SizeChanged -= AppTitleBar_SizeChanged;

        if (this._customTitlebarWorkaround is not null)
        {
            GridDraggableWindow.PointerPressed -= this._customTitlebarWorkaround.DragHelper.OnPointerPressed;
            GridDraggableWindow.PointerMoved -= this._customTitlebarWorkaround.DragHelper.OnPointerMoved;
            GridDraggableWindow.PointerReleased -= this._customTitlebarWorkaround.DragHelper.OnPointerReleased;
        }
    }
}
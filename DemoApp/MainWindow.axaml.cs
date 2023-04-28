using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Threading;
using KosmikAutoUpdate.NET;

namespace DemoApp;

public partial class MainWindow : Window {
    public MainWindow() { InitializeComponent(); }

    private void Button_OnClick(object? sender, RoutedEventArgs e) {
        var u = Updater.Create();
        Task.Run(() => u.Update()).ContinueWith(async b => {
            await Dispatcher.UIThread.InvokeAsync(() => {
                if (b.Result) (App.Instance.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
            });
        });
    }
}
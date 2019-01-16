using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace WinLZW
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Length > 0)
            {
                EncodeMaster.EnOrDecode(e.Args[0]);
                Shutdown();
            }

            var vm = new MainWindowViewModel();
            var window = new MainWindow();
            window.DataContext = vm;
            this.MainWindow = window;
            window.Show();
        }
    }
}

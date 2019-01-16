using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WinLZW
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private SettingsViewModel VM;

        public SettingsWindow(SettingsViewModel vm)
        {
            InitializeComponent();

            VM = vm;
            this.DataContext = VM;
            VM.CloseEvent += VM_CloseEvent;
        }

        void VM_CloseEvent(object sender, EventArgs e)
        {
            VM.CloseEvent -= VM_CloseEvent;
            Close();
        }
    }
}

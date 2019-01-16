using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using Microsoft.Win32;

namespace WinLZW
{
    /// <summary>
    /// Serves as the ViewModel for encoding and decoding
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void PropChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region Backing Fields

        private string _inputFilename;

        private ICommand _enOrDecodeCommand;
        private ICommand _fileBrowserCommand;
        private ICommand _settingsCommand;
        private ICommand _exitCommand;
        private ICommand _aboutCommand;

        private bool _isWorking;

        #endregion // Backing Fields

        public string InputFilename
        {
            get { return _inputFilename; }
            set
            {
                _inputFilename = value;
                PropChanged("InputFilename");
            }
        }

        public bool IsWorking
        {
            get { return _isWorking; }
            set
            {
                _isWorking = value;
                PropChanged("IsWorking");
            }
        }

        #region Commands

        /// <summary>
        /// Encodes or Decodes file named <c>InputFilename</c> based on whether it has a *.lzw extension
        /// </summary>
        public ICommand EnOrDecodeCommand
        {
            get
            {
                if (_enOrDecodeCommand == null)
                {
                    _enOrDecodeCommand = new RelayCommand((o) => EncodeMaster.EnOrDecode(InputFilename),
                                                          (o) => { return !String.IsNullOrWhiteSpace(InputFilename); });
                }
                return _enOrDecodeCommand;
            }
        }

        public ICommand FileBrowserCommand
        {
            get
            {
                if (_fileBrowserCommand == null)
                {
                    _fileBrowserCommand = new RelayCommand((o) => FileDialog());
                }
                return _fileBrowserCommand;
            }
        }

        public ICommand SettingsCommand
        {
            get
            {
                if (_settingsCommand == null)
                {
                    _settingsCommand = new RelayCommand((o) => LaunchSettingsWindow());
                }
                return _settingsCommand;
            }
        }

        public ICommand ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                {
                    _exitCommand = new RelayCommand((o) => Application.Current.Shutdown());
                }
                return _exitCommand;
            }
        }

        public ICommand AboutCommand
        {
            get
            {
                if (_aboutCommand == null)
                {
                    _aboutCommand = new RelayCommand((o) => ShowAboutWindow());
                }
                return _aboutCommand;
            }
        }

        #endregion // Commands

        public MainWindowViewModel()
        {
            InputFilename = String.Empty;
            IsWorking = false;
            EncodeMaster.LZWBegin += EncodeMaster_LZWBegin;
            EncodeMaster.LZWEnd += EncodeMaster_LZWEnd;
        }

        void EncodeMaster_LZWBegin(object sender, EventArgs e)
        {
            IsWorking = true;
        }

        void EncodeMaster_LZWEnd(object sender, EventArgs e)
        {
            IsWorking = false;
        }

        /// <summary>
        /// Launches an <c>OpenFileDialog</c> for <c>InputFilename</c>
        /// </summary>
        private void FileDialog()
        {
            var dialog = new OpenFileDialog();

            dialog.Filter = "All Files (*.*)|*.*|LZW Files (*.lzw)|*.lzw";
            dialog.Multiselect = false;
            bool? selectedFile = dialog.ShowDialog();

            if (selectedFile == true)
            {
                InputFilename = dialog.FileName;
            }
        }

        /// <summary>
        /// Launches a modal dialog to allow user to change settings
        /// </summary>
        private void LaunchSettingsWindow()
        {
            var vm = new SettingsViewModel();
            var window = new SettingsWindow(vm);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        /// <summary>
        /// Shows a modal dialog with information about this application
        /// </summary>
        private void ShowAboutWindow()
        {
            string aboutText = "WinLZW was created for fun by Alexander Schurman in 2012. " +
                               "It can compress files, appending a \".lzw\" file extension, and decompress the *.lzw files it creates.\n\n" +
                               "After implementing the LZW compression algorithm for Yale University's CS 323 course, " +
                               "he thought it would be fun to make a Windows application with this functionality using C# and WPF.";

            MessageBox.Show(Application.Current.MainWindow, aboutText, "About WinLZW", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

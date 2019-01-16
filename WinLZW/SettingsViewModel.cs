using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace WinLZW
{
    /// <summary>
    /// ViewModel interface between UI and code to change settings
    /// </summary>
    public class SettingsViewModel : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        protected void PropChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event EventHandler CloseEvent;

        #endregion // Events


        #region Private Fields

        private LZWSettings _oldSettings;
        private LZWSettings _settings;

        private bool _integrateWithShell;

        private ObservableCollection<uint> _maxBitsOptions;

        private ICommand _applyCommand;
        private ICommand _cancelCommand;
        private ICommand _okCommand;

        #endregion // Private Fields


        #region Public Properties

        public bool? InitEmpty
        {
            get { return _settings.InitEmpty; }
            set
            {
                _settings.InitEmpty = (value == null) ? false : (bool)value;
                PropChanged("InitEmpty");
            }
        }
        public uint MaxBits
        {
            get { return _settings.MaxBits; }
            set
            {
                _settings.MaxBits = value;
                PropChanged("MaxBits");
            }
        }
        public bool? ShouldPrune
        {
            get { return _settings.ShouldPrune; }
            set
            {
                _settings.ShouldPrune = (value == null) ? false : (bool)value;
                PropChanged("ShouldPrune");
            }
        }
        public string PruneWindow
        {
            get { return Convert.ToString(_settings.PruneWindow); }
            set
            {
                uint tempWindow;
                if (UInt32.TryParse(value, out tempWindow))
                {
                    _settings.PruneWindow = tempWindow;
                }
                PropChanged("PruneWindow");
            }
        }

        public bool? IntegrateWithShell
        {
            get { return _integrateWithShell; }
            set
            {
                _integrateWithShell = (value == null) ? false : (bool)value;
                PropChanged("IntegrateWithShell");
            }
        }

        /// <summary>
        /// True if the settings contained in this class differ from those saved
        /// </summary>
        private bool SettingsChanged
        {
            get
            {
                return !_oldSettings.Equals(_settings) || _integrateWithShell != RegistryMaster.IsIntegrated;
            }
        }

        /// <summary>
        /// Valid values for <c>MaxBits</c>
        /// </summary>
        public ObservableCollection<uint> MaxBitsOptions
        {
            get
            {
                if (_maxBitsOptions == null)
                {
                    _maxBitsOptions = new ObservableCollection<uint>();

                    for (uint i = 9; i <= 24; i++)
                    {
                        _maxBitsOptions.Add(i);
                    }
                }
                return _maxBitsOptions;
            }
        }

        #endregion // Public Properties

        #region Commands

        public ICommand ApplyCommand
        {
            get
            {
                if (_applyCommand == null)
                {
                    _applyCommand = new RelayCommand((o) => ApplySettings(),
                                                     (o) => SettingsChanged);
                }
                return _applyCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand((o) => CloseEvent.Raise(this));
                }
                return _cancelCommand;
            }
        }

        public ICommand OKCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand((o) =>
                        {
                            ApplySettings();
                            CloseEvent.Raise(this);
                        });
                }
                return _okCommand;
            }
        }

        #endregion // Commands


        public SettingsViewModel()
        {
            _oldSettings = LZWSettings.Current;
            _settings = new LZWSettings(_oldSettings);
            _integrateWithShell = RegistryMaster.IsIntegrated;
        }

        private void ApplySettings()
        {
            if (!_oldSettings.Equals(_settings))
            {
                LZWSettings.UpdateSettings(_settings);
            }

            if (_integrateWithShell != RegistryMaster.IsIntegrated)
            {
                RegistryMaster.ChangeIntegration(_integrateWithShell);
            }
        }
    }
}

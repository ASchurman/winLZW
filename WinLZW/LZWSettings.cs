using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;

namespace WinLZW
{
#pragma warning disable 0659
    /// <summary>
    /// Contains a set of encoding and shell integration settings
    /// </summary>
    class LZWSettings
    {
        private const uint DEFAULT_MAXBITS = 12;
        private const bool DEFAULT_SHOULDPRUNE = false;
        private const uint DEFAULT_PRUNEWINDOW = 1000;
        private const bool DEFAULT_INITEMPTY = false;

        public uint MaxBits;
        public bool ShouldPrune;
        public uint PruneWindow;
        public bool InitEmpty;

        public static LZWSettings Current
        {
            get
            {
                LZWSettings settings;

                try
                {
                    var appSettings = ConfigurationManager.AppSettings;

                    string maxBitsStr = appSettings["MaxBits"];
                    string shouldPruneStr = appSettings["Pruning"];
                    string windowStr = appSettings["PruneWindow"];
                    string initEmptyStr = appSettings["InitEmptyStringTable"];

                    settings = new LZWSettings(maxBitsStr, shouldPruneStr, windowStr, initEmptyStr);
                }
                catch (ConfigurationErrorsException)
                {
                    settings = new LZWSettings();
                }

                return settings;
            }
        }

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        private LZWSettings()
        {
            MaxBits = DEFAULT_MAXBITS;
            ShouldPrune = DEFAULT_SHOULDPRUNE;
            PruneWindow = DEFAULT_PRUNEWINDOW;
            InitEmpty = DEFAULT_INITEMPTY;
        }

        /// <summary>
        /// Parses strings into the settings values, using default values if the strings are invalid
        /// </summary>
        private LZWSettings(string maxbitsStr, string shouldpruneStr, string prunewindowStr, string initEmptyStr)
        {
            if (!UInt32.TryParse(maxbitsStr, out MaxBits))
            {
                MaxBits = DEFAULT_MAXBITS;
            }

            if(!Boolean.TryParse(shouldpruneStr, out ShouldPrune))
            {
                ShouldPrune = DEFAULT_SHOULDPRUNE;
            }

            if (!UInt32.TryParse(prunewindowStr, out PruneWindow))
            {
                PruneWindow = DEFAULT_PRUNEWINDOW;
            }

            if (!Boolean.TryParse(initEmptyStr, out InitEmpty))
            {
                InitEmpty = DEFAULT_INITEMPTY;
            }
        }

        /// <summary>
        /// Makes a copy of an LZWSettings object
        /// </summary>
        public LZWSettings(LZWSettings copyFrom)
        {
            MaxBits = copyFrom.MaxBits;
            ShouldPrune = copyFrom.ShouldPrune;
            PruneWindow = copyFrom.PruneWindow;
            InitEmpty = copyFrom.InitEmpty;
        }

        #endregion // Constructors

        public override bool Equals(object obj)
        {
            var otherSettings = obj as LZWSettings;

            if (otherSettings == null)
            {
                return base.Equals(obj);
            }
            else
            {
                bool equalMaxbits = MaxBits == otherSettings.MaxBits;
                bool equalShouldPrune = ShouldPrune == otherSettings.ShouldPrune;
                bool equalWindow = PruneWindow == otherSettings.PruneWindow;
                bool equalInitEmpty = InitEmpty == otherSettings.InitEmpty;

                return equalMaxbits && equalShouldPrune && equalWindow && equalInitEmpty;
            }
        }

        public static void UpdateSettings(LZWSettings settings)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = config.AppSettings.Settings;

            appSettings.Remove("MaxBits");
            appSettings.Remove("Pruning");
            appSettings.Remove("PruneWindow");
            appSettings.Remove("InitEmptyStringTable");

            appSettings.Add("MaxBits", Convert.ToString(settings.MaxBits));
            appSettings.Add("Pruning", Convert.ToString(settings.ShouldPrune));
            appSettings.Add("PruneWindow", Convert.ToString(settings.PruneWindow));
            appSettings.Add("InitEmptyStringTable", Convert.ToString(settings.InitEmpty));

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
#pragma warning restore 0659
}

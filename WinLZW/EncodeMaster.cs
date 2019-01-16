using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Threading;

namespace WinLZW
{
    /// <summary>
    /// Handles calling the encoding and decoding functions
    /// </summary>
    public static class EncodeMaster
    {
        public static event EventHandler LZWBegin;
        public static event EventHandler LZWEnd;

        /// <summary>
        /// Indicates if we're currently in the process of encoding or decoding a file
        /// in another thread.
        /// </summary>
        public static bool IsWorking
        {
            get
            {
                if (_lzwThread == null)
                {
                    return false;
                }
                else
                {
                    return _lzwThread.IsAlive;
                }
            }
        }

        private static Thread _lzwThread;

        private static ManagedLZW _lzw;

        private static ManagedLZW LZW
        {
            get
            {
                if (_lzw == null)
                {
                    _lzw = new ManagedLZW();
                }
                return _lzw;
            }
        }

        private class LZWArgs
        {
            public string InputFilename;
            public string OutputFilename;
            public LZWSettings Settings;

            /// <summary>
            /// Decoding constructor
            /// </summary>
            public LZWArgs(string input, string output)
            {
                InputFilename = input;
                OutputFilename = output;
                Settings = null;
            }

            /// <summary>
            /// Encoding constructor
            /// </summary>
            public LZWArgs(string input, string output, LZWSettings setting)
            {
                InputFilename = input;
                OutputFilename = output;
                Settings = setting;
            }
        }

        /// <summary>
        /// Calls Encode or Decode based on whether <param name="file"/> has a *.lzw extension.
        /// When encoding, appends ".lzw" to the output filename.
        /// When decoding, removes the ".lzw" from the filename.
        /// </summary>
        public static void EnOrDecode(string file)
        {
            while (IsWorking) ; // don't start another LZW thread while we're still working on the last one

            LZWArgs args;

            if (file.EndsWith(".lzw"))
            {
                string decodedName = file.Substring(0, file.LastIndexOf('.'));
                if (File.Exists(decodedName))
                {
                    MessageBox.Show("The decoded file already exists in this directory.", "File Already Exists", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                args = new LZWArgs(file, decodedName);
                _lzwThread = new Thread(new ParameterizedThreadStart(Decode));
            }
            else
            {
                string encodedName = file + ".lzw";
                if(File.Exists(encodedName))
                {
                    MessageBox.Show("The encoded file already exists in this directory.", "File Already Exists", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                args = new LZWArgs(file, encodedName, LZWSettings.Current);
                _lzwThread = new Thread(new ParameterizedThreadStart(Encode));
            }

            LZWBegin.Raise(null);
            _lzwThread.Start(args);
        }

        /// <summary>
        /// Calls the encoding function from the LZW assembly and handles any errors.
        /// </summary>
        /// <param name="inFilename">The name of the file to encode</param>
        /// <param name="outFilename">The name of a file to output the encoded bits to</param>
        private static void Encode(Object argsObj)
        {
            var args = argsObj as LZWArgs;
            if (args == null) return;

            var settings = args.Settings;

            try
            {
                if (settings.ShouldPrune == true)
                {
                    LZW.ManagedEncode(args.InputFilename, args.OutputFilename, settings.MaxBits, settings.PruneWindow, settings.InitEmpty);
                }
                else
                {
                    LZW.ManagedEncode(args.InputFilename, args.OutputFilename, settings.MaxBits, 0, settings.InitEmpty);
                }
            }
            catch (ArgumentException e)
            {
                switch (e.ParamName)
                {
                    case "filenameIn":
                        MessageBox.Show("The input file name you chose is invalid.", "Invalid File Name", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case "filenameOut":
                        MessageBox.Show("The output file name you chose is invalid.", "Invalid File Name", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    default:
                        MessageBox.Show("Unknown Error");
                        break;
                }
            }

            LZWEnd.Raise(null);
        }

        /// <summary>
        /// Calls the decoding function from the LZW assembly and handles any errors.
        /// </summary>
        /// <param name="inFilename">The name of the encoded file to decode</param>
        /// <param name="outFilename">The name of a file into which to decode</param>
        private static void Decode(Object argsObj)
        {
            var args = argsObj as LZWArgs;
            if (args == null) return;

            try
            {
                LZW.ManagedDecode(args.InputFilename, args.OutputFilename);
            }
            catch (ArgumentException e)
            {
                switch (e.ParamName)
                {
                    case "filenameIn":
                        // TODO: distinguish between malformed paths and invalid encoded file
                        MessageBox.Show("The input file name you chose is invalid.", "Invalid File Name", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case "filenameOut":
                        MessageBox.Show("The output file name you chose is invalid.", "Invalid File Name", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    default:
                        MessageBox.Show("Unknown Error");
                        break;
                }
            }

            LZWEnd.Raise(null);
        }
    }
}

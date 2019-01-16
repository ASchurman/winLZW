using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace RegistryAdmin
{
    /// <summary>
    /// Since the registry can only be edited as an administrator, this seperate executable
    /// can be run as an administrator to make changes to the registry.
    /// </summary>
    class Program
    {
        private const string MENUPATH    = @"*\shell\WinLZW";
        private const string COMMANDPATH = @"*\shell\WinLZW\command";
        private const string MENUTEXT    = "Compress/Decompress with WinLZW";

        /// <summary>
        /// A call to this executable must be of the form:
        /// assemblyPath reg/unreg (command)
        /// where assemblyPath is the path to this assembly,
        /// reg/unreg is either "reg" or "unreg",
        /// and (command) is the command to register if the previous arg was "reg".
        /// </summary>
        static void Main(string[] args)
        {
            if (args[0] == "reg")
            {
                string command = args[1];

                for (int i = 2; i < args.Length; i++)
                {
                    command += " " + args[i];
                }

                Register(command);
            }
            else if (args[0] == "unreg")
            {
                Unregister();
            }
        }

        private static void CreateKey(string keyPath, string value)
        {
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(keyPath))
            {
                key.SetValue(null, value);
            }
        }

        private static void Register(string Command)
        {
            CreateKey(MENUPATH, MENUTEXT);
            CreateKey(COMMANDPATH, Command);
        }

        private static void Unregister()
        {
            Registry.ClassesRoot.DeleteSubKeyTree(MENUPATH, false);
        }
    }
}

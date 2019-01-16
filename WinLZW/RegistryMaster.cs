using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Diagnostics;

namespace WinLZW
{
    public static class RegistryMaster
    {
        private const string MENUPATH = @"*\shell\WinLZW";

        public static bool IsIntegrated
        {
            get
            {
                var subkey = Registry.ClassesRoot.OpenSubKey(MENUPATH);
                bool integrated = (subkey != null);

                if (subkey != null) subkey.Close();
                return integrated;
            }
        }

        /// <summary>
        /// Modify the registry to change whether this application integrates with the Windows shell
        /// </summary>
        /// <param name="shouldIntegrate">True if we should integrate, false if we should remove integration</param>
        public static void ChangeIntegration(bool shouldIntegrate)
        {
            bool alreadyIntegrated = IsIntegrated;

            if (shouldIntegrate && !alreadyIntegrated)
            {
                ExecuteRegistryAdminProcess(true);
            }
            else if(!shouldIntegrate && alreadyIntegrated)
            {
                ExecuteRegistryAdminProcess(false);
            }
        }

        private static void ExecuteRegistryAdminProcess(bool shouldRegister)
        {
            var adminProcess = new Process();

            if (shouldRegister)
            {
                string pathToThisExe = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string commandToRegister = String.Format("\"{0}\" \"%L\"", pathToThisExe);

                adminProcess.StartInfo.Arguments = String.Format("reg {0}", commandToRegister);
            }
            else
            {
                adminProcess.StartInfo.Arguments = "unreg";
            }

            adminProcess.StartInfo.FileName = "winlzw_registry_master.exe"; // should be in the same directory as this assembly
            adminProcess.StartInfo.CreateNoWindow = true;
            adminProcess.Start();
        }
    }
}

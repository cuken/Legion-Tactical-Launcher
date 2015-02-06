using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace Legion_Tactical_Launcher
{
    public partial class MainWindow : Window
    {        
        #region Helpers
        public string getArmaDirectory () 
        {
            string armaExePath = String.Empty;

            // Search Steam Directory First
            RegistryKey regKey = Registry.CurrentUser;
            regKey = regKey.OpenSubKey(@"Software\Valve\Steam");

            
            if (regKey != null)
            {
                string steamDir = regKey.GetValue("SteamPath").ToString() + @"\SteamApps\common\Arma 3\";
                string[] arma3Exists = Directory.GetFiles(steamDir, "arma3.exe", SearchOption.TopDirectoryOnly);

                if (arma3Exists.Length > 0)
                {
                    // It exists here! - Lets set the directory
                    armaExePath = steamDir;
                    return armaExePath;
                }
                else
                {
                    // It doesn't exist here, lets check for a SteamLibrary on each fixed drive
                    DriveInfo[] allDrives = DriveInfo.GetDrives();
                    foreach (DriveInfo d in allDrives) 
                    {
                        if (d.DriveType == DriveType.Fixed)
                        {
                            // Its a fixed drive - lets search here.
                            steamDir = d.Name + @"Program Files\SteamLibrary\SteamApps\common\Arma 3\";
                            arma3Exists = Directory.GetFiles(steamDir, "arma3.exe", SearchOption.TopDirectoryOnly);

                            if (arma3Exists.Length > 0)
                            {
                                armaExePath = steamDir;
                                return armaExePath;
                            }
                        }
                    }

                    return armaExePath;
                }                
            }
            else
            {
               // Doesnt Look like it exists - Let the user point us to the directory (We can only be so clever)                
                return armaExePath;
            }

        }

        public string getTS3Directory()
        {
            string ts3Directory = String.Empty;

            RegistryKey ts3RegKey = Registry.LocalMachine;
            ts3RegKey = ts3RegKey.OpenSubKey(@"SOFTWARE\TeamSpeak 3 Client");

            if (ts3RegKey != null)
            {
                ts3Directory = ts3RegKey.GetValue(null).ToString();
            }

            return ts3Directory;
        }

        // Return a List of Addon SubDirectories to be added to the GUI
        private List<string> getAddonSubDirs()
        {
            List<string> addonSubDirs = new List<string>();

            // Find all Subfolders of the Mod Directory
            String[] addonDirs = Directory.GetDirectories(tb_addondir.Text);

            foreach (string dir in addonDirs) {
                // Check for Mod.cpp 
                Boolean modFileExists = Directory.GetFiles(dir, "mod.cpp", SearchOption.TopDirectoryOnly).Length > 0 ? true : false;
                // Check for a addons subfolder
                Boolean addonDirExists = Directory.GetDirectories(dir, "addons", SearchOption.TopDirectoryOnly).Length > 0 ? true : false;
                
                // Add Sub Directory to List because it has a mod in it.
                if (modFileExists || addonDirExists)
                {
                    addonSubDirs.Add(dir);
                }                
                
            }            
            
            return addonSubDirs;                        
        }

        private void updateAddonListBox() 
        {
            List<string> addonDirs = getAddonSubDirs();

            foreach (string dir in addonDirs)
            {
                string returnDir = "@" + dir.Split('@').Last();
                AddonListing.Items.Add(returnDir);
            }
        }
        
        #endregion
    }
}


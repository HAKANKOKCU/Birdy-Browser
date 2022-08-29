using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Birdy_Browser
{
    public class EnvironmentFolders
    {

        public enum SpecialFolder
        {
            AdministrativeTools = 48,
            //{user name}\Start Menu\Programs\Administrative Tools 
            ApplicationData = 26,
            //{user name}\Application Data 
            CommonAdministrativeTools = 47,
            //All Users\Start Menu\Programs\Administrative Tools 
            CommonApplicationData = 35,
            //All Users\Application Data 
            CommonDesktopDirectory = 25,
            //All Users\Desktop 
            CommonDocuments = 46,
            //All Users\Documents 
            CommonFavorites = 31,
            CommonNonLocalizedStartup = 30,
            //non localized common startup 
            CommonPrograms = 23,
            //All Users\Programs 
            CommonStartMenu = 22,
            //All Users\Start Menu 
            CommonStartup = 24,
            //All Users\Startup 
            CommonTemplates = 45,
            //All Users\Templates 
            ControlPanel = 3,
            //My Computer\Control Panel 
            Cookies = 33,
            DesktopDirectory = 16,
            //{user name}\Desktop 
            Favorites = 6,
            //{user name}\Favorites 
            Fonts = 20,
            //windows\fonts 
            History = 34,
            InternetCache = 32,
            LocalApplicationData = 28,
            //{user name}\Local Settings\Application Data (non roaming) 
            MyDocuments = 5,
            //My Documents 
            MyPictures = 39,
            //C:\Program Files\My Pictures 
            NetworkShortcuts = 19,
            //{user name}\nethood 
            NonLocalizedStartup = 29,
            //non localized startup 
            Printers = 4,
            //My Computer\Printers 
            PrintHood = 27,
            //{user name}\PrintHood 
            ProgramFiles = 38,
            //C:\Program Files 
            ProgramFilesCommon = 43,
            //C:\Program Files\Common 
            Programs = 2,
            //Start Menu\Programs 
            Recent = 8,
            //{user name}\Recent 
            RecycleBin = 10,
            //{desktop}\Recycle Bin 
            SendTo = 9,
            //{user name}\SendTo 
            StartMenu = 11,
            //{user name}\Start Menu 
            Startup = 7,
            //Start Menu\Programs\Startup 
            System = 37,
            //GetSystemDirectory() 
            Templates = 21,
            UserProfile = 40,
            //USERPROFILE 
            Windows = 36
            //GetWindowsDirectory() 
        }

        [DllImport("shfolder.dll", CharSet = CharSet.Auto)]
        private static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags, StringBuilder lpszPath);

        /// <summary> 
        /// Get an environment folder path for Windows environment folders 
        /// </summary> 
        /// <returns>A string pointing to the special path</returns> 
        /// <remarks></remarks> 
        public static string GetPath(SpecialFolder folder)
        {
            StringBuilder lpszPath = new StringBuilder(260);
            SHGetFolderPath(IntPtr.Zero, (int)folder, IntPtr.Zero, 0, lpszPath);
            return lpszPath.ToString();
        }
    }
}

using s3pi.Interfaces;
using s3pi.Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TS3_Dream_Launcher.Controls.ListItems
{
    /*
     * This script is resposible by the work of the "Save Item" that represents each
     * Save Game found in documents...
    */

    public partial class SaveItem : UserControl
    {
        //Private variables
        private string[] saveGameNhdFilesPaths = new string[0];
        private int saveGameMainNhdIndex = -1;

        //Public variables
        public MainWindow instantiatedBy = null;
        public string saveGameDirPath = "";

        //Core methods

        public SaveItem(MainWindow instantiatedBy, string saveGameDirPath)
        {
            //Initialize the component
            InitializeComponent();

            //Inform that is the DataConext of this User Control
            this.DataContext = this;

            //Store all the data
            this.instantiatedBy = instantiatedBy;
            this.saveGameDirPath = saveGameDirPath;
        }
    
        //Public methods

        public void Prepare()
        {
            //Prepare the color highlight
            background.MouseEnter += (s, e) =>
            {
                background.Background = new SolidColorBrush(Color.FromArgb(255, 239, 250, 255));
            };
            background.MouseLeave += (s, e) =>
            {
                background.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            };

            //Determine the main NHD file path of this save game
            DetermineTheMainNhdPath();

            //Render the thumbnail of this save game
            RenderThumbnail();

            //Show the name of the save
            name.Content = (new DirectoryInfo(saveGameDirPath)).Name.Split(".")[0];
            if (saveGameMainNhdIndex == -1)
                lastEdit.Content = instantiatedBy.GetStringApplicationResource("launcher_save_unknownEdit");
            if (saveGameMainNhdIndex != -1)
            {
                //Get main NHD last edit time
                DateTime lastEditTime = File.GetLastWriteTime(saveGameNhdFilesPaths[saveGameMainNhdIndex]);

                //Show the last edit
                lastEdit.Content = instantiatedBy.GetStringApplicationResource("launcher_save_lastEdit").Replace("%d%", lastEditTime.ToString("dd/MM/yyyy")).Replace("%h%", lastEditTime.ToString("HH:mm:ss"));
            }
            size.Content = GetFormattedFileSize(GetSaveGameTotalSize()).Replace("~", "");
        }

        //Auxiliar methods

        private void DetermineTheMainNhdPath()
        {
            //Get a list of all NHD files inside the save game dir
            List<string> nhdFiles = new List<string>();
            int indexForMainNhdFile = -1;

            //Fill the list
            foreach (FileInfo file in (new DirectoryInfo(saveGameDirPath).GetFiles()))
                if (System.IO.Path.GetExtension(file.FullName).Replace(".", "").ToLower() == "nhd")
                    nhdFiles.Add(file.FullName);

            //Prepare the possible names for the main NHD file
            string possibleName0 = ((new DirectoryInfo(saveGameDirPath)).Name.Split(".")[0].Replace(" ", "") + "_0x");
            string possibleName1 = ((new DirectoryInfo(saveGameDirPath)).Name.Split(".")[0] + "_0x");
            //Try to determine the main NHD file index in the list
            for(int i = 0; i < nhdFiles.Count; i++)
            {
                //Get the file name
                string currentfileName = System.IO.Path.GetFileNameWithoutExtension(nhdFiles[i]);

                //If have one of the possible names, determine as main NHD file
                if (currentfileName.Contains(possibleName0) == true || currentfileName.Contains(possibleName1) == true)
                {
                    indexForMainNhdFile = i;
                    break;
                }
            }

            //Inform the collected data
            saveGameNhdFilesPaths = nhdFiles.ToArray();
            saveGameMainNhdIndex = indexForMainNhdFile;
        }

        private void RenderThumbnail()
        {
            //If not found the main NHD file, cancel
            if (saveGameMainNhdIndex == -1)
                return;

            //Load the main NHD file package
            IPackage mainNhdPackage = Package.OpenPackage(0, saveGameNhdFilesPaths[saveGameMainNhdIndex], false);

            //Search inside the package, by the thumbnail of type "SNAP" or "0x6B6D837E"
            foreach (IResourceIndexEntry item in mainNhdPackage.GetResourceList)
                if(GetLongConvertedToHexStr(item.ResourceType, 8) == "0x6B6D837E")
                {
                    //Get the base stream
                    Stream aPackageStream = (mainNhdPackage as APackage).GetResource(item);
                    //Get the base resource using the "ImageResource" s3pi wrapper
                    IResource baseResource = (IResource)(new ImageResource.ImageResource(0, aPackageStream));
                    //Get the bitmap from base resource stream
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = baseResource.Stream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    //Put the bitmap on place
                    portrait.Source = bitmapImage;

                    //Cancel the search
                    break;
                }

            //Close the package
            Package.ClosePackage(0, mainNhdPackage);
        }

        private static string GetLongConvertedToHexStr(ulong originalValue, int digits)
        {
            //Prepare the result
            string result = "0x";

            //Convert long to hex string
            string tmpHex = originalValue.ToString("X");

            //Add the digits if necessary
            while (tmpHex.Length < digits)
                tmpHex = "0" + tmpHex;

            //Concat the values
            result += tmpHex;

            //Return the result
            return result;
        }
    
        private long GetSaveGameTotalSize()
        {
            //Prepare the value to return
            long toReturn = 0;

            //Count total media files size
            foreach (FileInfo file in (new DirectoryInfo(saveGameDirPath)).GetFiles())
                toReturn += file.Length;

            //Return the value
            return toReturn;
        }

        private string GetFormattedFileSize(double bytesSize)
        {
            //Calculate to MB, KB and GB
            float gbSize = (float)(((bytesSize / 1000.0f) / 1000.0f) / 1000.0f);
            float mbSize = (float)((bytesSize / 1000.0f) / 1000.0f);
            float kbSize = (float)(bytesSize / 1000.0f);

            //Fix the KB size
            if (kbSize > 0.0f && kbSize < 1.0f)
                kbSize = 1.0f;
            //Calculate the pre size prefix
            string prefix = ((kbSize > 0.0f) ? "~" : "");

            //Prepare the final size
            string formattedSize = "";

            //Select the correct unit
            if (mbSize < 1)
                formattedSize = (prefix + kbSize.ToString("F0") + " KB");
            if (mbSize >= 1 && mbSize < 1000)
                formattedSize = (prefix + mbSize.ToString("F1") + " MB");
            if (mbSize >= 1000)
                formattedSize = (prefix + gbSize.ToString("F1") + " GB");

            //Return the cache size
            return formattedSize;
        }
    }
}

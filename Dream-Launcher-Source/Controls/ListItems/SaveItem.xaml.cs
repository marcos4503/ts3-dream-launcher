using Ionic.Zip;
using MarcosTomaz.ATS;
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
        //Cache variables
        private MenuItem restoreOfVaultItem = null;

        //Private variables
        private string[] saveGameNhdFilesPaths = new string[0];
        private int saveGameMainNhdIndex = -1;
        private string expectedSaveGameCopyVaultPath = "";

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

            //Build the expected path for the copy of this save game in the vault
            expectedSaveGameCopyVaultPath = (instantiatedBy.myDocumentsPath + "/!DL-Static/save-vault/" + (new DirectoryInfo(saveGameDirPath)).Name + ".zip");

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
            //Show the total size of this save
            size.Content = GetFormattedFileSize(GetSaveGameTotalSize()).Replace("~", "");

            //Prepare the three dots menu
            PrepareTreeDotsMenu();

            //Update the status of vault copy of this save game
            UpdateVaulyCopyStatus();
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

        private void UpdateVaulyCopyStatus()
        {
            //Update the last vault copy status
            if (File.Exists(expectedSaveGameCopyVaultPath) == false)
                lastVault.Content = instantiatedBy.GetStringApplicationResource("launcher_save_lastVaultEmpty");
            if (File.Exists(expectedSaveGameCopyVaultPath) == true)
            {
                //Get vault copy last edit time
                DateTime lastEditTime = File.GetLastWriteTime(expectedSaveGameCopyVaultPath);

                //Show the last vault copy
                lastVault.Content = instantiatedBy.GetStringApplicationResource("launcher_save_lastVaultDate").Replace("%d%", lastEditTime.ToString("dd/MM/yyyy")).Replace("%h%", lastEditTime.ToString("HH:mm:ss"));
            }

            //Hide or enable the "restore from vault" menu item
            restoreOfVaultItem.IsEnabled = File.Exists(expectedSaveGameCopyVaultPath);
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
    
        private void PrepareTreeDotsMenu()
        {
            //Prepare the more options button
            moreButton.ContextMenu = new ContextMenu();
            //Setup the context menu display
            moreButton.Click += (s, e) =>
            {
                ContextMenu contextMenu = moreButton.ContextMenu;
                contextMenu.PlacementTarget = moreButton;
                contextMenu.IsOpen = true;
                e.Handled = true;
            };

            //Add "clean" option to options menu
            MenuItem cleanItem = new MenuItem();
            cleanItem.Header = instantiatedBy.GetStringApplicationResource("launcher_save_clean");
            cleanItem.Click += (s, e) => { Clean(); };
            moreButton.ContextMenu.Items.Add(cleanItem);

            //Add "vault" option to options menu
            MenuItem vaultItem = new MenuItem();
            vaultItem.Header = instantiatedBy.GetStringApplicationResource("launcher_save_moreVault");
            MenuItem saveToVaultItem = new MenuItem();
            saveToVaultItem.Header = instantiatedBy.GetStringApplicationResource("launcher_save_moreVaultSave");
            saveToVaultItem.Click += (s, e) => { SaveCopyOnVault(); };
            restoreOfVaultItem = new MenuItem();
            restoreOfVaultItem.Header = instantiatedBy.GetStringApplicationResource("launcher_save_moreVaultRestore");
            restoreOfVaultItem.Click += (s, e) => { GetCopyFromVault(); };
            vaultItem.Items.Add(saveToVaultItem);
            vaultItem.Items.Add(restoreOfVaultItem);
            moreButton.ContextMenu.Items.Add(vaultItem);

            //Add "copy to" option to options menu
            MenuItem copyItem = new MenuItem();
            copyItem.Header = instantiatedBy.GetStringApplicationResource("launcher_save_copyTo");
            copyItem.Click += (s, e) => { CopyTo(); };
            moreButton.ContextMenu.Items.Add(copyItem);

            //Add "delete" option to options menu
            MenuItem deleteItem = new MenuItem();
            deleteItem.Header = instantiatedBy.GetStringApplicationResource("launcher_save_delete");
            deleteItem.Click += (s, e) => { Delete(); };
            moreButton.ContextMenu.Items.Add(deleteItem);
        }

        private void Clean()
        {

        }

        private void SaveCopyOnVault()
        {
            //Enable the blocker
            instantiatedBy.ShowVaultTaskBlocker(instantiatedBy.GetStringApplicationResource("launcher_save_vaultSavingCopy").Replace("%s%", name.Content.ToString()));
            //Add task to list
            instantiatedBy.AddTask("savingToVault", "Saving copy of Save Game on Vault.");

            //Start a thread to do this work of save copy
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(instantiatedBy, new string[] { });
            asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) => { };
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(2500);

                //Try to do the task
                try
                {
                    //Prepare the path for the resultant zip file
                    string resultZipFile = (instantiatedBy.myDocumentsPath + "/Saves/" + (new DirectoryInfo(saveGameDirPath)).Name + ".zip");

                    //Compact the save game folder to a zip
                    ZipFile zipFile = new ZipFile();
                    zipFile.AddDirectory(saveGameDirPath, (new DirectoryInfo(saveGameDirPath)).Name);
                    zipFile.Save(resultZipFile);

                    //If already exists a copy of this save in vault, delete it
                    if (File.Exists(expectedSaveGameCopyVaultPath) == true)
                        File.Delete(expectedSaveGameCopyVaultPath);

                    //Move the resultant zip file to vault
                    File.Move(resultZipFile, expectedSaveGameCopyVaultPath);

                    //Wait some time
                    threadTools.MakeThreadSleep(2500);

                    //Return a success response
                    return new string[] { "success" };
                }
                catch (Exception ex)
                {
                    //Return a error response
                    return new string[] { "error" };
                }

                //Finish the thread...
                return new string[] { "none" };
            };
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
            {
                //If is success
                if (backgroundResult[0] == "success")
                    instantiatedBy.ShowToast(instantiatedBy.GetStringApplicationResource("launcher_save_vaultSavingCopySuccess").Replace("%s%", name.Content.ToString()), MainWindow.ToastType.Success);
                //If is error
                if (backgroundResult[0] != "success")
                    instantiatedBy.ShowToast(instantiatedBy.GetStringApplicationResource("launcher_save_vaultSavingCopyError").Replace("%s%", name.Content.ToString()), MainWindow.ToastType.Error);

                //Disable the blocker
                instantiatedBy.HideVaultTaskBlocker();
                //Add task to list
                instantiatedBy.RemoveTask("savingToVault");

                //Update the vault copy status of this save game
                UpdateVaulyCopyStatus();
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        private void GetCopyFromVault()
        {
            //Show the confirmation dialog
            MessageBoxResult dialogResult = MessageBox.Show(instantiatedBy.GetStringApplicationResource("launcher_save_vaultRestoreDiagText"),
                                                            instantiatedBy.GetStringApplicationResource("launcher_save_vaultRestoreDiagTitle"), MessageBoxButton.YesNo, MessageBoxImage.Warning);

            //If is desire to cancel, cancel it
            if (dialogResult != MessageBoxResult.Yes)
                return;

            //Enable the blocker
            instantiatedBy.ShowVaultTaskBlocker(instantiatedBy.GetStringApplicationResource("launcher_save_vaultRestoringCopy").Replace("%s%", name.Content.ToString()));
            //Add task to list
            instantiatedBy.AddTask("restoringFromVault", "Restoring copy of Save Game of Vault.");

            //Start a thread to do this work of restore copy
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(instantiatedBy, new string[] { });
            asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) => { };
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(2500);

                //Try to do the task
                try
                {
                    //Delete the save game folder
                    Directory.Delete(saveGameDirPath, true);

                    //Prepare the path to copy of zip file of vault
                    string copyOfZipFileInVault = (instantiatedBy.myDocumentsPath + "/Saves/" + System.IO.Path.GetFileName(expectedSaveGameCopyVaultPath));

                    //Copy the zip file of save game in vault, to the saves
                    File.Copy(expectedSaveGameCopyVaultPath, copyOfZipFileInVault);

                    //Extract the zip file
                    ZipFile zipFile = ZipFile.Read(copyOfZipFileInVault);
                    foreach (ZipEntry entry in zipFile)
                        entry.Extract((instantiatedBy.myDocumentsPath + "/Saves"), ExtractExistingFileAction.OverwriteSilently);
                    zipFile.Dispose();

                    //Delete the copied zip file
                    File.Delete(copyOfZipFileInVault);

                    //Wait some time
                    threadTools.MakeThreadSleep(2500);

                    //Return a success response
                    return new string[] { "success" };
                }
                catch (Exception ex)
                {
                    //Return a error response
                    return new string[] { "error" };
                }

                //Finish the thread...
                return new string[] { "none" };
            };
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
            {
                //If is success
                if (backgroundResult[0] == "success")
                    instantiatedBy.ShowToast(instantiatedBy.GetStringApplicationResource("launcher_save_vaultRestoringCopySuccess").Replace("%s%", name.Content.ToString()), MainWindow.ToastType.Success);
                //If is error
                if (backgroundResult[0] != "success")
                    instantiatedBy.ShowToast(instantiatedBy.GetStringApplicationResource("launcher_save_vaultRestoringCopyError").Replace("%s%", name.Content.ToString()), MainWindow.ToastType.Error);

                //Disable the blocker
                instantiatedBy.HideVaultTaskBlocker();
                //Add task to list
                instantiatedBy.RemoveTask("restoringFromVault");

                //Force update the saves list
                instantiatedBy.UpdateSaveList();
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }
    
        private void CopyTo()
        {
            //Prepare the folder selected path
            string selectedFolderPath = "";
            //Open folder picker dialog
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    selectedFolderPath = dialog.SelectedPath;
            }
            //If no folder was selected, cancel
            if (selectedFolderPath == "")
                return;

            //Get the save game folder name
            string folderName = (new DirectoryInfo(saveGameDirPath)).Name;

            //If already exists a folder with the target name, cancel
            if (Directory.Exists((selectedFolderPath + "/" + folderName)) == true)
            {
                MessageBox.Show(instantiatedBy.GetStringApplicationResource("launcher_save_copyToErrorText"),
                                instantiatedBy.GetStringApplicationResource("launcher_save_copyToErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Create the save folder in the place
            Directory.CreateDirectory((selectedFolderPath + "/" + folderName));

            //Copy each file of the save, to destination
            foreach (FileInfo file in (new DirectoryInfo(saveGameDirPath)).GetFiles())
                File.Copy(file.FullName, (selectedFolderPath + "/" + folderName + "/" + System.IO.Path.GetFileName(file.FullName)));

            //Warn about the copy
            MessageBox.Show(instantiatedBy.GetStringApplicationResource("launcher_save_copyToDoneText"),
                            instantiatedBy.GetStringApplicationResource("launcher_save_copyToDoneTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
        }
    
        private void Delete()
        {
            //Show the confirmation dialog
            MessageBoxResult dialogResult = MessageBox.Show(instantiatedBy.GetStringApplicationResource("launcher_save_deleteDiagText").Replace("%s%", name.Content.ToString()),
                                                            instantiatedBy.GetStringApplicationResource("launcher_save_deleteDiagTitle"), MessageBoxButton.YesNo, MessageBoxImage.Warning);

            //If is desire to cancel, cancel it
            if (dialogResult != MessageBoxResult.Yes)
                return;

            //Delete the save on Vault, if exists
            if (File.Exists(expectedSaveGameCopyVaultPath) == true)
                File.Delete(expectedSaveGameCopyVaultPath);

            //Delete the save backup, if exists
            if (Directory.Exists((saveGameDirPath + ".backup")) == true)
                Directory.Delete((saveGameDirPath + ".backup"), true);

            //Delete the save folder
            Directory.Delete(saveGameDirPath, true);

            //Force update save game list
            instantiatedBy.UpdateSaveList();
        }
    }
}

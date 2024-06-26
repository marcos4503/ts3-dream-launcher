﻿using Ionic.Zip;
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
        public bool isBadSaveGame = false;

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
            //Determine if is a bad save game
            if (File.Exists((saveGameDirPath + "/!bad-game!.bad")) == true)
                isBadSaveGame = true;

            //If is a bad save game, change border color to red
            if(isBadSaveGame == true)
                background.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 234, 130, 130));
            //If is not a bad game, hide the bad warn
            if (isBadSaveGame == false)
                badWarn.Visibility = Visibility.Collapsed;

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

            //Show the main NHD world name in the save as world of residence info
            RenderTheMainNhdWorldNameInScreen();

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

                //Show "today" if is necessary
                DateTime todayDate = DateTime.Now;
                if (((string)lastEdit.Content).Contains(todayDate.ToString("dd/MM/yyyy")) == true)
                    lastEdit.Content = ((string)lastEdit.Content).Replace(todayDate.ToString("dd/MM/yyyy"), instantiatedBy.GetStringApplicationResource("launcher_save_lastEditToday"));
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

            //Try to determine the main NHD file index in the list
            for(int i = 0; i < nhdFiles.Count; i++)
            {
                //Get the file name
                string nhdName = System.IO.Path.GetFileNameWithoutExtension(nhdFiles[i]);

                //If not have the name of "China", "Egypt", "France", "Oasis Landing" or "Sims University", determine as main NHD file
                if (nhdName.Contains("China_0x") == false && nhdName.Contains("Egypt_0x") == false && nhdName.Contains("France_0x") == false &&
                    nhdName.Contains("Oasis Landing_0x") == false && nhdName.Contains("Sims University_0x") == false)
                {
                    indexForMainNhdFile = i;
                    break;
                }
            }

            //Inform the collected data
            saveGameNhdFilesPaths = nhdFiles.ToArray();
            saveGameMainNhdIndex = indexForMainNhdFile;
        }

        private void RenderTheMainNhdWorldNameInScreen()
        {
            //If known the main NHD file, show it
            if (saveGameMainNhdIndex != -1)
                saveWorld.Content = GetSpacedStringOnUpperCaseLetters(System.IO.Path.GetFileNameWithoutExtension(saveGameNhdFilesPaths[saveGameMainNhdIndex]).Split("_")[0]);

            //If don't know the main NHD file, just show unknown
            if (saveGameMainNhdIndex == -1)
                saveWorld.Content = instantiatedBy.GetStringApplicationResource("launcher_save_unknownWorld");
        }

        private string GetSpacedStringOnUpperCaseLetters(string sourceString)
        {
            //Prepare the string to return
            string toReturn = sourceString;

            //Prepare the list of upper case letters
            List<string> letters = new List<string>();
            letters.Add("A");
            letters.Add("B");
            letters.Add("C");
            letters.Add("Ç");
            letters.Add("D");
            letters.Add("E");
            letters.Add("F");
            letters.Add("G");
            letters.Add("H");
            letters.Add("I");
            letters.Add("J");
            letters.Add("K");
            letters.Add("L");
            letters.Add("M");
            letters.Add("N");
            letters.Add("O");
            letters.Add("P");
            letters.Add("Q");
            letters.Add("R");
            letters.Add("S");
            letters.Add("T");
            letters.Add("U");
            letters.Add("V");
            letters.Add("W");
            letters.Add("X");
            letters.Add("Y");
            letters.Add("Z");

            //Set the spaces
            for (int i = 0; i < letters.Count; i++)
                toReturn = toReturn.Replace(letters[i], (" " + letters[i]));

            //If have double spaces, remove it
            toReturn = toReturn.Replace("  ", " ");

            //If the first character is a space, remove it
            if (toReturn[0] == ' ')
                toReturn = toReturn.Remove(0, 1);

            //Return the result
            return toReturn;
        }

        private void RenderThumbnail()
        {
            //If not found the main NHD file, cancel
            if (saveGameMainNhdIndex == -1)
                return;

            //Try to load the save game thumbnail from the main NHD package file
            try
            {
                //Load the main NHD file package
                IPackage mainNhdPackage = Package.OpenPackage(0, saveGameNhdFilesPaths[saveGameMainNhdIndex], false);

                //Search inside the package, by the thumbnail of type "SNAP" or "0x6B6D837E"
                foreach (IResourceIndexEntry item in mainNhdPackage.GetResourceList)
                    if (GetLongConvertedToHexStr(item.ResourceType, 8) == "0x6B6D837E")
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
            catch (Exception ex) { Console.WriteLine((ex.Message + "\n\n" + ex.StackTrace)); }
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

                //Show "today" if is necessary
                DateTime todayDate = DateTime.Now;
                if (((string)lastVault.Content).Contains(todayDate.ToString("dd/MM/yyyy")) == true)
                    lastVault.Content = ((string)lastVault.Content).Replace(todayDate.ToString("dd/MM/yyyy"), instantiatedBy.GetStringApplicationResource("launcher_save_lastEditToday"));
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
            if (isBadSaveGame == true)
                cleanItem.IsEnabled = false;
            cleanItem.Click += (s, e) => { Clean(); };
            moreButton.ContextMenu.Items.Add(cleanItem);

            //Add "vault" option to options menu
            MenuItem vaultItem = new MenuItem();
            vaultItem.Header = instantiatedBy.GetStringApplicationResource("launcher_save_moreVault");
            MenuItem saveToVaultItem = new MenuItem();
            saveToVaultItem.Header = instantiatedBy.GetStringApplicationResource("launcher_save_moreVaultSave");
            if (isBadSaveGame == true)
                saveToVaultItem.IsEnabled = false;
            saveToVaultItem.Click += (s, e) => { SaveCopyOnVault(); };
            restoreOfVaultItem = new MenuItem();
            restoreOfVaultItem.Header = instantiatedBy.GetStringApplicationResource("launcher_save_moreVaultRestore");
            restoreOfVaultItem.Click += (s, e) => { GetCopyFromVault(); };
            vaultItem.Items.Add(saveToVaultItem);
            vaultItem.Items.Add(restoreOfVaultItem);
            moreButton.ContextMenu.Items.Add(vaultItem);

            //Add "backup" option to options menu
            MenuItem backupItem = new MenuItem();
            backupItem.Header = instantiatedBy.GetStringApplicationResource("launcher_save_moreBackup");
            MenuItem exportBackupItem = new MenuItem();
            exportBackupItem.Header = instantiatedBy.GetStringApplicationResource("launcher_save_moreBackupExport");
            exportBackupItem.IsEnabled = Directory.Exists((saveGameDirPath + ".backup"));
            exportBackupItem.Click += (s, e) => { ExportAutomaticBackup(); };
            backupItem.Items.Add(exportBackupItem);
            moreButton.ContextMenu.Items.Add(backupItem);

            //Add "copy to" option to options menu
            MenuItem copyItem = new MenuItem();
            copyItem.Header = instantiatedBy.GetStringApplicationResource("launcher_save_copyTo");
            if (isBadSaveGame == true)
                copyItem.IsEnabled = false;
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
            //Block the UI
            instantiatedBy.SetInteractionBlockerEnabled(true);
            //Add the task to queue
            instantiatedBy.AddTask("saveCleaning", "Running Save Game cleaner.");

            //Open the save cleaner window
            WindowSaveCleaner saveCleaner = new WindowSaveCleaner(instantiatedBy, saveGameDirPath, saveGameNhdFilesPaths, saveGameMainNhdIndex);
            saveCleaner.Closed += (s, e) =>
            {
                instantiatedBy.SetInteractionBlockerEnabled(false);
                instantiatedBy.RemoveTask("saveCleaning");
            };
            saveCleaner.Show();
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
    
        private void ExportAutomaticBackup()
        {
            //Get the last edition time of the automatic backup
            string lastEditDate = "";
            try
            {
                DateTime lastEdit = File.GetLastWriteTime((saveGameDirPath + ".backup/" + System.IO.Path.GetFileName(saveGameNhdFilesPaths[saveGameMainNhdIndex])));
                lastEditDate = instantiatedBy.GetStringApplicationResource("launcher_save_lastEdit").Replace("%d%", lastEdit.ToString("dd/MM/yyyy")).Replace("%h%", lastEdit.ToString("HH:mm:ss"));
            }
            catch (Exception ex) { }

            //Show the confirmation dialog
            MessageBoxResult dialogResult = MessageBox.Show(instantiatedBy.GetStringApplicationResource("launcher_save_backupExportDiagText").Replace("§", "\n").Replace("%d%", lastEditDate),
                                                            instantiatedBy.GetStringApplicationResource("launcher_save_backupExportDiagTitle"), MessageBoxButton.YesNo, MessageBoxImage.Information);

            //If is desire to cancel, cancel it
            if (dialogResult != MessageBoxResult.Yes)
                return;

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
            string folderName = (new DirectoryInfo((saveGameDirPath + ".backup"))).Name;

            //If already exists a folder with the target name, cancel
            if (Directory.Exists((selectedFolderPath + "/" + folderName)) == true)
            {
                MessageBox.Show(instantiatedBy.GetStringApplicationResource("launcher_save_backupExportErrorText"),
                                instantiatedBy.GetStringApplicationResource("launcher_save_backupExportErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Create the save folder in the place
            Directory.CreateDirectory((selectedFolderPath + "/" + folderName));

            //Copy each file of the save, to destination
            foreach (FileInfo file in (new DirectoryInfo((saveGameDirPath + ".backup"))).GetFiles())
                File.Copy(file.FullName, (selectedFolderPath + "/" + folderName + "/" + System.IO.Path.GetFileName(file.FullName)));

            //Rename the folder post copied to have the same name of original save game
            Directory.Move((selectedFolderPath + "/" + folderName), (selectedFolderPath + "/" + (new DirectoryInfo(saveGameDirPath)).Name));

            //Warn about the copy
            MessageBox.Show(instantiatedBy.GetStringApplicationResource("launcher_save_backupExportDoneText"),
                            instantiatedBy.GetStringApplicationResource("launcher_save_backupExportDoneTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
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

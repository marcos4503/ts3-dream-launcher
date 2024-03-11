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
using System.Windows.Shapes;

namespace TS3_Dream_Launcher
{
    /*
    * This is the code responsible by the Save Cleaner window
    */

    public partial class WindowSaveCleaner : Window
    {
        //Cache variables
        private bool isCleaningInProgress = false;

        //Private variables
        private MainWindow mainWindowRef = null;
        private string saveGameDirPath = "";
        private string[] saveGameNhdFilesPaths = new string[0];
        private int saveGameMainNhdIndex = -1;

        //Core methods

        public WindowSaveCleaner(MainWindow mainWindowRef, string saveGameDirPath, string[] saveGameNhdFilesPaths, int saveGameMainNhdIndex)
        {
            //Prepare the window
            InitializeComponent();

            //Store reference to main window
            this.mainWindowRef = mainWindowRef;

            //Store data
            this.saveGameDirPath = saveGameDirPath;
            this.saveGameNhdFilesPaths = saveGameNhdFilesPaths;
            this.saveGameMainNhdIndex = saveGameMainNhdIndex;

            //Prepare the UI
            PrepareTheUI();
        }

        private void PrepareTheUI()
        {
            //Block the window close if a clean is in progress
            this.Closing += (s, e) =>
            {
                if (isCleaningInProgress == true)
                {
                    MessageBox.Show(mainWindowRef.GetStringApplicationResource("launcher_save_cleanExitErrorText"),
                                    mainWindowRef.GetStringApplicationResource("launcher_save_cleanExitErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                }
            };

            //Show the title
            this.Title = mainWindowRef.GetStringApplicationResource("launcher_save_clean_title");

            //Render the language
            title.Text = (new DirectoryInfo(saveGameDirPath)).Name.Split(".")[0];
            prefsTitle.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_box");
            snapsThumbsLbl.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_snapsAndThumbs");
            snapsThumbsHlp.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_save_clean_snapsAndThumbsHelp");
            memsPhotosPaintLbl.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_memsPhotosPaint");
            memsPhotosPaintHlp.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_save_clean_memsPhotosPaintHelp");
            seasonsCardsLbl.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_seasonsCards");
            seasonsCardsHlp.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_save_clean_seasonsCardsHelp");
            promPhotosLbl.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_promPhotos");
            promPhotosHlp.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_save_clean_promPhotosHelp");
            cabinPhotosLbl.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_cabinPhotos");
            cabinPhotosHlp.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_save_clean_cabinPhotosHelp");
            shangSimlaWorldLbl.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_shangSimla");
            shangSimlaWorldHlp.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_save_clean_worldHelp");
            champsLesSimsLbl.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_champsLesSims");
            champsLesSimsHlp.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_save_clean_worldHelp");
            alSimharaLbl.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_alSimhara");
            alSimharaHlp.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_save_clean_worldHelp");
            simsUniversityLbl.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_simsUniversity");
            simsUniversityHlp.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_save_clean_worldHelp");
            oasisLandingLbl.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_oasisLanding");
            oasisLandingHlp.ToolTip = mainWindowRef.GetStringApplicationResource("launcher_save_clean_worldHelp");
            tipText.Text = mainWindowRef.GetStringApplicationResource("launcher_save_clean_tipText");
            clearButton.Content = mainWindowRef.GetStringApplicationResource("launcher_save_clean_clearButton");

            //Load all user preferences
            snapsThumbsCbx.IsChecked = mainWindowRef.launcherPrefs.loadedData.saveCleanSnapsThumbs;
            memsPhotosPaintCbx.IsChecked = mainWindowRef.launcherPrefs.loadedData.saveCleanMemsPhotoPaint;
            seasonsCardsCbx.IsChecked = mainWindowRef.launcherPrefs.loadedData.saveCleanSeasonCards;
            promPhotosCbx.IsChecked = mainWindowRef.launcherPrefs.loadedData.saveCleanPromPhotos;
            cabinPhotosCbx.IsChecked = mainWindowRef.launcherPrefs.loadedData.saveCleanCabinPhotos;
            shangSimlaWorldCbx.IsChecked = mainWindowRef.launcherPrefs.loadedData.saveCleanWorldShangSimla;
            champsLesSimsCbx.IsChecked = mainWindowRef.launcherPrefs.loadedData.saveCleanWorldChampsLesSims;
            alSimharaCbx.IsChecked = mainWindowRef.launcherPrefs.loadedData.saveCleanWorldAlSimhara;
            simsUniversityCbx.IsChecked = mainWindowRef.launcherPrefs.loadedData.saveCleanWorldSimsUniversity;
            oasisLandingCbx.IsChecked = mainWindowRef.launcherPrefs.loadedData.saveCleanWorldOasisLanding;

            //If don't have basic optimization patch, hide the world clear data options
            if(mainWindowRef.launcherPrefs.loadedData.patchBasicOptimization == false)
            {
                shangSimlaWorldCbx.IsEnabled = false;
                champsLesSimsCbx.IsEnabled = false;
                alSimharaCbx.IsEnabled = false;
                simsUniversityCbx.IsEnabled = false;
                oasisLandingCbx.IsEnabled = false;
            }

            //Prepare the buttons
            clearButton.Click += (s, e) => { StartSaveGameCleaning(); };
        }

        private void StartSaveGameCleaning()
        {
            //Save the cleaning preferences
            SaveCleaningPreferences();

            //Notify that cleaning is started
            isCleaningInProgress = true;

            //Change the UI
            clearButton.Visibility = Visibility.Collapsed;
            cleaningGif.Visibility = Visibility.Visible;
            content.IsHitTestVisible = false;
            content.Opacity = 0.25f;

            //Start a thread to do the cleaning process
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(mainWindowRef, new string[] { });
            asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) => { };
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Wait some time
                threadTools.MakeThreadSleep(2500);

                //Try to do the task
                try
                {
                    //------------- BACKUP CREATION -------------//

                    //Prepare the path for the backup of the save
                    string backupPath = (saveGameDirPath + ".backup");
                    
                    //If the backup exists, delete it
                    if (Directory.Exists(backupPath) == true)
                        Directory.Delete(backupPath, true);

                    //Create the backup
                    Directory.CreateDirectory(backupPath);
                    //Copy original files to backup
                    foreach (FileInfo file in (new DirectoryInfo(saveGameDirPath)).GetFiles())
                        File.Copy(file.FullName, (backupPath + "/" + System.IO.Path.GetFileName(file.FullName)));

                    //------------- SAVE CLEANING -------------//

                    //Open the main NHD file
                    IPackage mainNhdPackage = Package.OpenPackage(0, saveGameNhdFilesPaths[saveGameMainNhdIndex], true);
                    //Do the cleaning...
                    if (mainWindowRef.launcherPrefs.loadedData.saveCleanSnapsThumbs == true)
                        ClearSnapAndThumbnailsResources(ref mainNhdPackage);
                    if (mainWindowRef.launcherPrefs.loadedData.saveCleanMemsPhotoPaint == true)
                        ClearMemoriesPhotosPaintsResources(ref mainNhdPackage);
                    if (mainWindowRef.launcherPrefs.loadedData.saveCleanSeasonCards == true)
                        ClearSeasonsGreetingsCardsResources(ref mainNhdPackage);
                    if (mainWindowRef.launcherPrefs.loadedData.saveCleanPromPhotos == true)
                        ClearPromPhotosResources(ref mainNhdPackage);
                    if (mainWindowRef.launcherPrefs.loadedData.saveCleanCabinPhotos == true)
                        ClearCabinPhotosResources(ref mainNhdPackage);
                    //Save the changes
                    mainNhdPackage.SavePackage();
                    //Close the main NHD file
                    Package.ClosePackage(0, mainNhdPackage);



                    //Open the travel db file
                    IPackage travelDbPackage = Package.OpenPackage(0, (saveGameDirPath + "/TravelDB.package"), true);
                    //Do the cleaning...
                    if (mainWindowRef.launcherPrefs.loadedData.saveCleanSnapsThumbs == true)
                        ClearSnapAndThumbnailsResources(ref travelDbPackage);
                    if (mainWindowRef.launcherPrefs.loadedData.saveCleanMemsPhotoPaint == true)
                        ClearMemoriesPhotosPaintsResources(ref travelDbPackage);
                    if (mainWindowRef.launcherPrefs.loadedData.saveCleanSeasonCards == true)
                        ClearSeasonsGreetingsCardsResources(ref travelDbPackage);
                    if (mainWindowRef.launcherPrefs.loadedData.saveCleanPromPhotos == true)
                        ClearPromPhotosResources(ref travelDbPackage);
                    if (mainWindowRef.launcherPrefs.loadedData.saveCleanCabinPhotos == true)
                        ClearCabinPhotosResources(ref travelDbPackage);
                    //Save the changes
                    travelDbPackage.SavePackage();
                    //Close the travel db file
                    Package.ClosePackage(0, travelDbPackage);

                    //------------- WORLD CLEANING -------------//

                    //Do the cleaning...
                    if (mainWindowRef.launcherPrefs.loadedData.saveCleanWorldShangSimla == true)
                        if (File.Exists((saveGameDirPath + "/China_0x0859db4c.nhd")) == true)
                        {
                            File.Delete((saveGameDirPath + "/China_0x0859db4c.nhd"));
                            File.Delete((saveGameDirPath + "/China_0x0859db4cExportDB.package"));
                        }
                    if (mainWindowRef.launcherPrefs.loadedData.saveCleanWorldChampsLesSims == true)
                        if (File.Exists((saveGameDirPath + "/France_0x0859db50.nhd")) == true)
                        {
                            File.Delete((saveGameDirPath + "/France_0x0859db50.nhd"));
                            File.Delete((saveGameDirPath + "/France_0x0859db50ExportDB.package"));
                        }
                    if (mainWindowRef.launcherPrefs.loadedData.saveCleanWorldAlSimhara == true)
                        if (File.Exists((saveGameDirPath + "/Egypt_0x0859db48.nhd")) == true)
                        {
                            File.Delete((saveGameDirPath + "/Egypt_0x0859db48.nhd"));
                            File.Delete((saveGameDirPath + "/Egypt_0x0859db48ExportDB.package"));
                        }
                    if (mainWindowRef.launcherPrefs.loadedData.saveCleanWorldSimsUniversity == true)
                        if (File.Exists((saveGameDirPath + "/Sims University_0x0e41c954.nhd")) == true)
                        {
                            File.Delete((saveGameDirPath + "/Sims University_0x0e41c954.nhd"));
                            File.Delete((saveGameDirPath + "/Sims University_0x0e41c954ExportDB.package"));
                        }
                    if (mainWindowRef.launcherPrefs.loadedData.saveCleanWorldOasisLanding == true)
                        if (File.Exists((saveGameDirPath + "/Oasis Landing_0x0f36012a.nhd")) == true)
                        {
                            File.Delete((saveGameDirPath + "/Oasis Landing_0x0f36012a.nhd"));
                            File.Delete((saveGameDirPath + "/Oasis Landing_0x0f36012aExportDB.package"));
                        }

                    //------------- END -------------//

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
                //If cleaning was success
                if(backgroundResult[0] == "success")
                {
                    //Get the size before, and after the clear
                    string beforeSize = GetFormattedFileSize(GetSaveGameTotalSize((saveGameDirPath + ".backup"))).Replace("~", "");
                    string afterSize = GetFormattedFileSize(GetSaveGameTotalSize(saveGameDirPath)).Replace("~", "");

                    //Show the notification
                    mainWindowRef.ShowToast(mainWindowRef.GetStringApplicationResource("launcher_save_clean_clearSuccess").Replace("%s%", title.Text)
                                                                                                                          .Replace("%b%", beforeSize)
                                                                                                                          .Replace("%a%", afterSize), MainWindow.ToastType.Success);
                }

                //If cleaning was problematic
                if (backgroundResult[0] != "success")
                {
                    //Create a bad file inside the save
                    File.WriteAllText((saveGameDirPath + "/!bad-game!.bad"), "BAD GAME!");

                    //Show the notification
                    mainWindowRef.ShowToast(mainWindowRef.GetStringApplicationResource("launcher_save_clean_clearError").Replace("%s%", title.Text), MainWindow.ToastType.Error);
                }

                //Force update the saves list
                mainWindowRef.UpdateSaveList();

                //Notify that cleaning is ended
                isCleaningInProgress = false;

                //Close this window
                this.Close();
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        private void SaveCleaningPreferences()
        {
            //Store the cleaning preferences
            mainWindowRef.launcherPrefs.loadedData.saveCleanSnapsThumbs = (bool)(snapsThumbsCbx.IsChecked);
            mainWindowRef.launcherPrefs.loadedData.saveCleanMemsPhotoPaint = (bool)(memsPhotosPaintCbx.IsChecked);
            mainWindowRef.launcherPrefs.loadedData.saveCleanSeasonCards = (bool)(seasonsCardsCbx.IsChecked);
            mainWindowRef.launcherPrefs.loadedData.saveCleanPromPhotos = (bool)(promPhotosCbx.IsChecked);
            mainWindowRef.launcherPrefs.loadedData.saveCleanCabinPhotos = (bool)(cabinPhotosCbx.IsChecked);
            mainWindowRef.launcherPrefs.loadedData.saveCleanWorldShangSimla = (bool)(shangSimlaWorldCbx.IsChecked);
            mainWindowRef.launcherPrefs.loadedData.saveCleanWorldChampsLesSims = (bool)(champsLesSimsCbx.IsChecked);
            mainWindowRef.launcherPrefs.loadedData.saveCleanWorldAlSimhara = (bool)(alSimharaCbx.IsChecked);
            mainWindowRef.launcherPrefs.loadedData.saveCleanWorldSimsUniversity = (bool)(simsUniversityCbx.IsChecked);
            mainWindowRef.launcherPrefs.loadedData.saveCleanWorldOasisLanding = (bool)(oasisLandingCbx.IsChecked);

            //Save the preferences
            mainWindowRef.launcherPrefs.Save();
        }

        //Auxiliar methods

        private long GetSaveGameTotalSize(string dirPath)
        {
            //Prepare the value to return
            long toReturn = 0;

            //Count total media files size
            foreach (FileInfo file in (new DirectoryInfo(dirPath)).GetFiles())
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

        private void ClearSnapAndThumbnailsResources(ref IPackage targetPackage)
        {
            //Delete resources
            foreach (IResourceIndexEntry item in targetPackage.GetResourceList)
            {
                //Get this resource TGI
                string typeHex = GetLongConvertedToHexStr(item.ResourceType, 8);
                string groupHex = GetLongConvertedToHexStr(item.ResourceGroup, 8);
                string instanceHex = GetLongConvertedToHexStr(item.Instance, 16);

                //If is a SNAP resource, delete it
                if (typeHex == "0x0580A2CD" || typeHex == "0x0580A2CE" || typeHex == "0x0580A2CF" || typeHex == "0x6B6D837D" || typeHex == "0x6B6D837F")
                    targetPackage.DeleteResource(item);
            }
        }

        private void ClearMemoriesPhotosPaintsResources(ref IPackage targetPackage)
        {
            //Delete resources
            foreach (IResourceIndexEntry item in targetPackage.GetResourceList)
            {
                //Get this resource TGI
                string typeHex = GetLongConvertedToHexStr(item.ResourceType, 8);
                string groupHex = GetLongConvertedToHexStr(item.ResourceGroup, 8);
                string instanceHex = GetLongConvertedToHexStr(item.Instance, 16);

                //If is a _IMG resource
                if (typeHex == "0x00B2D882")
                    if (groupHex == "0x0269D005") //<- If is a Memmory, Smartphone/Camera Photo or Painting, delete it
                        targetPackage.DeleteResource(item);
            }
        }

        private void ClearSeasonsGreetingsCardsResources(ref IPackage targetPackage)
        {
            //Delete resources
            foreach (IResourceIndexEntry item in targetPackage.GetResourceList)
            {
                //Get this resource TGI
                string typeHex = GetLongConvertedToHexStr(item.ResourceType, 8);
                string groupHex = GetLongConvertedToHexStr(item.ResourceGroup, 8);
                string instanceHex = GetLongConvertedToHexStr(item.Instance, 16);

                //If is a _IMG resource
                if (typeHex == "0x00B2D882")
                    if (groupHex == "0x024B9FCA") //<- If is a Season Greeting Card, delete it
                        targetPackage.DeleteResource(item);
            }
        }

        private void ClearPromPhotosResources(ref IPackage targetPackage)
        {
            //Delete resources
            foreach (IResourceIndexEntry item in targetPackage.GetResourceList)
            {
                //Get this resource TGI
                string typeHex = GetLongConvertedToHexStr(item.ResourceType, 8);
                string groupHex = GetLongConvertedToHexStr(item.ResourceGroup, 8);
                string instanceHex = GetLongConvertedToHexStr(item.Instance, 16);

                //If is a _IMG resource
                if (typeHex == "0x00B2D882")
                    if (groupHex == "0x02722299") //<- If is a Prom Photo, delete it
                        targetPackage.DeleteResource(item);
            }
        }

        private void ClearCabinPhotosResources(ref IPackage targetPackage)
        {
            //Delete resources
            foreach (IResourceIndexEntry item in targetPackage.GetResourceList)
            {
                //Get this resource TGI
                string typeHex = GetLongConvertedToHexStr(item.ResourceType, 8);
                string groupHex = GetLongConvertedToHexStr(item.ResourceGroup, 8);
                string instanceHex = GetLongConvertedToHexStr(item.Instance, 16);

                //If is a _IMG resource
                if (typeHex == "0x00B2D882")
                    if (groupHex == "0x02BD69A0") //<- If is a Photo Booth Photo, delete it
                        targetPackage.DeleteResource(item);
            }
        }
    }
}

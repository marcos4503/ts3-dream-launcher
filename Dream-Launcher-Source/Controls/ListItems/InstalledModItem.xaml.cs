using MarcosTomaz.ATS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    * This script is resposible by the work of the "InstalledModItem" that represents each
    * installed mod from patches...
    */

    public partial class InstalledModItem : UserControl
    {
        //Public enums
        public enum ModCategory
        {
            Unknown,
            Contents,
            Graphics,
            Sounds,
            Fixes,
            Gameplay,
            Sliders,
            Others,
            Patches
        }

        //Cache variables
        private Process s3peProcess = null;
        private Process s3ocProcess = null;

        //Private variables
        private string thisModPath = "";
        private string contentsDirPath = "";

        //Public variables
        public MainWindow instantiatedByWindow = null;
        public ModCategory modCategory = ModCategory.Unknown;

        //Core methods

        public InstalledModItem(MainWindow instantiatedBy)
        {
            //Initialize the component
            InitializeComponent();

            //Inform that is the DataConext of this User Control
            this.DataContext = this;

            //Store reference for window that was instantiated this item
            this.instantiatedByWindow = instantiatedBy;
        }

        //Public methods

        public void SetModPath(string path)
        {
            //Set the mod path
            this.thisModPath = path;
        }

        public void SetContentsPath(string path)
        {
            //Set the contents folder path
            this.contentsDirPath = path;
        }

        public void Prepare()
        {
            //Detect this mod category
            this.modCategory = GetModCategory();

            //Show the category name
            if (modCategory == ModCategory.Contents)
                category.Text = ConvertFirstCharsOfStringToUpperCase(instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_cat_contents"));
            if (modCategory == ModCategory.Graphics)
                category.Text = ConvertFirstCharsOfStringToUpperCase(instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_cat_graphics"));
            if (modCategory == ModCategory.Sounds)
                category.Text = ConvertFirstCharsOfStringToUpperCase(instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_cat_sounds"));
            if (modCategory == ModCategory.Fixes)
                category.Text = ConvertFirstCharsOfStringToUpperCase(instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_cat_fixes"));
            if (modCategory == ModCategory.Gameplay)
                category.Text = ConvertFirstCharsOfStringToUpperCase(instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_cat_gameplay"));
            if (modCategory == ModCategory.Sliders)
                category.Text = ConvertFirstCharsOfStringToUpperCase(instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_cat_sliders"));
            if (modCategory == ModCategory.Others)
                category.Text = ConvertFirstCharsOfStringToUpperCase(instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_cat_others"));
            if (modCategory == ModCategory.Patches)
                category.Text = ConvertFirstCharsOfStringToUpperCase(instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_cat_patches"));

            //If is a patch mod, show the file name converted to human readable
            if (modCategory == ModCategory.Patches)
            {
                //Prepare the title to show and real title
                string titleToShow = "#ERROR-UNKNOWN-PATCH-MOD#";
                string realModFileName = System.IO.Path.GetFileNameWithoutExtension(thisModPath);
                //Prepare the dictionary
                Dictionary<string, string> humanReadableDictionary = GetPatchesModsToHumanReadableNamesDictionary();
                //Get the title from dictionary
                if(humanReadableDictionary.ContainsKey(realModFileName) == true)
                    titleToShow = humanReadableDictionary[realModFileName];
                //Show the title
                title.Text = titleToShow;
            }
            //If is not a patch mod, show the file name
            if (modCategory != ModCategory.Patches && modCategory != ModCategory.Unknown)
                title.Text = System.IO.Path.GetFileNameWithoutExtension(thisModPath).Split(" --- ")[1];
            //If is a unknown mod, show the error
            if (modCategory == ModCategory.Unknown)
                title.Text = "#ERROR-UNKNOWN-MOD#";

            //Hide star and gear icon
            star.Visibility = Visibility.Collapsed;
            patch.Visibility = Visibility.Collapsed;

            //Get the parent directory name for this mod
            string parentDirectoryName = new FileInfo(thisModPath).Directory.Name;
            //Enable star icon if is a recommended mod
            if (parentDirectoryName == "DL3-Recommended")
                star.Visibility = Visibility.Visible;
            //Enable star icon if is a patch mod
            if (parentDirectoryName == "DL3-Patches" || parentDirectoryName == "Packages")
                patch.Visibility = Visibility.Visible;

            //Disable interaction if is a patch mod
            if (parentDirectoryName == "DL3-Patches" || parentDirectoryName == "Packages")
            {
                enabled.IsEnabled = false;
                uninstallButton.IsEnabled = false;
                moreButton.Opacity = 0.25f;
                moreButton.IsHitTestVisible = false;
            }

            //Get the extension of this mod
            string modExtension = System.IO.Path.GetExtension(thisModPath).Replace(".", "").ToLower();
            //Show if this mod is currently enabled or disabled
            if (modExtension == "package")
                enabled.IsChecked = true;
            if (modExtension != "package")
                enabled.IsChecked = false;

            //Set the enabler and disabler control
            enabled.Checked += (s, e) => { ChangeModExtension("package"); };
            enabled.Unchecked += (s, e) => { ChangeModExtension("disabled"); };

            //Prepare the delete button
            uninstallButton.Click += (s, e) => { DeleteThisMod(); };

            //Prepare the more options context menu
            PrepareTheMoreOptionsContextMenu();
        }

        //Auxiliar methods

        private ModCategory GetModCategory()
        {
            //Prepare to return
            ModCategory toReturn = ModCategory.Unknown;

            //Get the name of this mod
            string modFileName = System.IO.Path.GetFileNameWithoutExtension(thisModPath).ToUpper();
            string parentDirName = new FileInfo(thisModPath).Directory.Name;

            //Check category of this mod
            if (modFileName.Contains("CONTENTS --- ") == true)
                toReturn = ModCategory.Contents;
            if (modFileName.Contains("GRAPHICS --- ") == true)
                toReturn = ModCategory.Graphics;
            if (modFileName.Contains("SOUNDS --- ") == true)
                toReturn = ModCategory.Sounds;
            if (modFileName.Contains("FIXES --- ") == true)
                toReturn = ModCategory.Fixes;
            if (modFileName.Contains("GAMEPLAY --- ") == true)
                toReturn = ModCategory.Gameplay;
            if (modFileName.Contains("SLIDERS --- ") == true)
                toReturn = ModCategory.Sliders;
            if (modFileName.Contains("OTHERS --- ") == true)
                toReturn = ModCategory.Others;
            if (parentDirName == "DL3-Patches" || parentDirName == "Packages")
                toReturn = ModCategory.Patches;

            //Return the category
            return toReturn;
        }

        private string ConvertFirstCharsOfStringToUpperCase(string toConvert)
        {
            //Prepare to return the string
            string toReturn = "";

            //Split string by spaces
            string[] words = toConvert.Split(" ");

            //Change each word to use first character with upper case
            for(int i = 0; i < words.Length; i++)
            {
                string firstChar = words[i][0].ToString();
                words[i] = (firstChar.ToUpper() + (words[i].Substring(1, (words[i].Length -1))).ToLower());
            }

            //Rebuild the string
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < words.Length; i++)
            {
                if(i > 0)
                    stringBuilder.Append(" ");
                stringBuilder.Append(words[i]);
            }
            //Inform the result string
            toReturn = stringBuilder.ToString();

            //Return the result
            return toReturn;
        }

        private Dictionary<string, string> GetPatchesModsToHumanReadableNamesDictionary()
        {
            //Create the dictionary
            Dictionary<string, string> toReturn = new Dictionary<string, string>();

            //Add all names of patch mods and the readable version
            toReturn.Add("NoIntro", "Rick - No Intro");
            toReturn.Add("NoModInfo", "Arro - No Mod Info");
            toReturn.Add("ZoomInCAS", "Shimrod101 - Zoom In CAS");
            toReturn.Add("AutoLightsOverhaul", "Lazy Duchess - Auto Lights Overhaul");
            toReturn.Add("AutoLightsOverhaulCommon", "Lazy Duchess - Auto Lights Overhaul Common");
            toReturn.Add("BetterRoutingForGameObjects", "bluegenjutsu - Better Routing For Game Objects");
            toReturn.Add("BoringBonesFixedLightning", "Boring Bones - Fixed Lightning");
            toReturn.Add("ErrorTrap", "NRaas - Error Trap");
            toReturn.Add("FasterElevatorMoving", "bluegenjutsu - Faster Elevator Moving");
            toReturn.Add("GoHere", "NRaas - Go Here");
            toReturn.Add("GoHere_Tuning", "NRaas - Go Here - Default Settings");
            toReturn.Add("HideExpansionPacksGameIcons", "tuzlakserif232 - Hide Expansion Packs Game Icons");
            toReturn.Add("ImprovedEnvironmentalShadows", "simsi45 - Improved Environmental Shadows");
            toReturn.Add("MasterController", "NRaas - Master Controller");
            toReturn.Add("MasterController_Cheats", "NRaas - Master Controller - Cheats Module");
            toReturn.Add("MasterController_Integration", "NRaas - Master Controller - Integration Module");
            toReturn.Add("MemoriesDisabled", "Shimrod101 - Memories Disabled");
            toReturn.Add("NoFootTapping", "stevebo77 - No Foot Tapping");
            toReturn.Add("NoRouteFailAnimation", "stevebo77 - No Route Fail Animation");
            toReturn.Add("NoWhiningMotives", "Nukael - No Whining Motives");
            toReturn.Add("Overwatch", "NRaas - Overwatch");
            toReturn.Add("Overwatch_Tuning", "NRaas - Overwatch - Default Settings");
            toReturn.Add("Register", "NRaas - Register");
            toReturn.Add("Register_Tuning", "NRaas - Register - Default Settings");
            toReturn.Add("RouteFixF4V9", "Twoftmama - Route Fix Flavour 4 V9");
            toReturn.Add("SmoothPatch", "Lazy Duchess - Smooth Patch");
            toReturn.Add("SmoothPatch_MasterController", "Lazy Duchess - Smooth Patch - Master Controller Support");
            toReturn.Add("Traffic", "NRaas - Traffic");
            toReturn.Add("Traffic_Tuning", "NRaas - Traffic - Default Settings");
            toReturn.Add("Traveler", "NRaas - Traveler");
            toReturn.Add("Traveler_Tuning", "NRaas - Traveler - Default Settings");

            //Return the directionary
            return toReturn;
        }
    
        private void ChangeModExtension(string newExtension)
        {
            //Get informations about mod
            string pathToParentDirectory = new FileInfo(thisModPath).DirectoryName;
            string oldFileName = System.IO.Path.GetFileName(thisModPath);
            string newFileName = (System.IO.Path.GetFileNameWithoutExtension(thisModPath) + "." + newExtension);

            //Change the extension
            File.Move((pathToParentDirectory + "/" + oldFileName), (pathToParentDirectory + "/" + newFileName));

            //Inform the new mod path
            thisModPath = (pathToParentDirectory + "/" + newFileName);
        }
    
        private void DeleteThisMod()
        {
            //Display a dialog
            MessageBoxResult dialogResult = MessageBox.Show(instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_modDeleteDiagText"),
                                                            instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_modDeleteDiagTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question);

            //If is desired to remove the mod, continue
            if (dialogResult == MessageBoxResult.Yes)
            {
                File.Delete(thisModPath);
                instantiatedByWindow.UpdateInstalledMods();
            }
        }
    
        private void PrepareTheMoreOptionsContextMenu()
        {
            //Prepare the context menu
            moreButton.ContextMenu = new ContextMenu();

            //Setup the context menu display
            moreButton.Click += (s, e) =>
            {
                ContextMenu contextMenu = moreButton.ContextMenu;
                contextMenu.PlacementTarget = moreButton;
                contextMenu.IsOpen = true;
                e.Handled = true;
            };

            //Add open in S3PE option
            MenuItem openS3pe = new MenuItem();
            openS3pe.Header = instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_modOptions_s3pe");
            openS3pe.Click += (s, e) => { OpenInS3PE(); };
            moreButton.ContextMenu.Items.Add(openS3pe);

            //Add open in S3OC option
            MenuItem openS3oc = new MenuItem();
            openS3oc.Header = instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_modOptions_s3oc");
            openS3oc.Click += (s, e) => { OpenInS3OC(); };
            moreButton.ContextMenu.Items.Add(openS3oc);

            //Add rename option
            MenuItem renameOption = new MenuItem();
            renameOption.Header = instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_modOptions_rename");
            renameOption.Click += (s, e) => { RenameThisMod(); };
            if (star.Visibility == Visibility.Visible)  //<- Disable rename option if is a recommended mod
                renameOption.IsEnabled = false;
            moreButton.ContextMenu.Items.Add(renameOption);

            //Add copy option
            MenuItem copyToOption = new MenuItem();
            copyToOption.Header = instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_modOptions_copy");
            copyToOption.Click += (s, e) => { CopyThisModTo(); };
            moreButton.ContextMenu.Items.Add(copyToOption);
        }

        private void RenameThisMod()
        {
            //If is doing some task, cancel the rename
            if(instantiatedByWindow.GetRunningTasksCount() > 0)
            {
                MessageBox.Show(instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_modOptions_renameBusyText"),
                                instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_modOptions_renameBusyTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Block interactions
            instantiatedByWindow.SetInteractionBlockerEnabled(true);

            //Open rename window
            WindowModRename modRenameWindow = new WindowModRename(instantiatedByWindow, instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_modOptions_rename"), thisModPath);
            modRenameWindow.Closed += (s, e) =>
            {
                if(modRenameWindow.wasRenamedTheMod == true)
                    instantiatedByWindow.UpdateInstalledMods();
                instantiatedByWindow.SetInteractionBlockerEnabled(false);
            };
            modRenameWindow.Show();
        }

        private void CopyThisModTo()
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

            //Copy the mod to desired folder
            File.Copy(thisModPath, (selectedFolderPath + "/" + title.Text + ".package"));

            //Warn about the copy
            MessageBox.Show(instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_modCopyDiagText"),
                            instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_modCopyDiagTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
        }
    
        private void OpenInS3PE()
        {
            //If don't have S3PE tool installed, cancel
            if(File.Exists((contentsDirPath + "/tool-s3pe/s3pe.exe")) == false)
            {
                instantiatedByWindow.ShowToast(instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_modOptions_s3pe_notInstalled"), MainWindow.ToastType.Error);
                return;
            }

            //Start a thread to open and monitor the tool
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(instantiatedByWindow, new string[] 
            {
                System.IO.Path.Combine(new string[] { contentsDirPath, "tool-s3pe" }),
                System.IO.Path.Combine(new string[] { contentsDirPath, "tool-s3pe", "s3pe.exe" }),
                thisModPath
            });
            asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) =>
            {
                //Get the data
                string s3peFolderPath = startParams[0];
                string s3peExePath = startParams[1];
                string modPath = startParams[2];

                //Start the tool
                Process newProcess = new Process();
                newProcess.StartInfo.FileName = s3peExePath;
                newProcess.StartInfo.WorkingDirectory = s3peFolderPath;
                newProcess.StartInfo.Arguments = ("\"" + modPath + "\"");
                newProcess.Start();
                //Store it
                s3peProcess = newProcess;

                //Add the task to queue
                instantiatedByWindow.AddTask("s3pe_mod_edit_Running", "Running tool.");
                //Block the UI
                instantiatedByWindow.SetInteractionBlockerEnabled(true);
            };
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Create a monitor loop
                while (true)
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(1000);

                    //If was finished the tool process, break the monitor loop
                    if (s3peProcess.HasExited == true)
                        break;
                }

                //Finish the thread...
                return new string[] { "none" };
            };
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
            {
                //Remove the task from queue
                instantiatedByWindow.RemoveTask("s3pe_mod_edit_Running");
                //Unblock the UI
                instantiatedByWindow.SetInteractionBlockerEnabled(false);
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }

        private void OpenInS3OC()
        {
            //If don't have S3PE tool installed, cancel
            if (File.Exists((contentsDirPath + "/tool-s3oc/s3oc.exe")) == false)
            {
                instantiatedByWindow.ShowToast(instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_modOptions_s3oc_notInstalled"), MainWindow.ToastType.Error);
                return;
            }

            //Start a thread to open and monitor the tool
            AsyncTaskSimplified asyncTask = new AsyncTaskSimplified(instantiatedByWindow, new string[]
            {
                System.IO.Path.Combine(new string[] { contentsDirPath, "tool-s3oc" }),
                System.IO.Path.Combine(new string[] { contentsDirPath, "tool-s3oc", "s3oc.exe" }),
                thisModPath
            });
            asyncTask.onStartTask_RunMainThread += (callerWindow, startParams) =>
            {
                //Get the data
                string s3ocFolderPath = startParams[0];
                string s3ocExePath = startParams[1];
                string modPath = startParams[2];

                //Start the tool
                Process newProcess = new Process();
                newProcess.StartInfo.FileName = s3ocExePath;
                newProcess.StartInfo.WorkingDirectory = s3ocFolderPath;
                newProcess.StartInfo.Arguments = ("\"" + modPath + "\"");
                newProcess.Start();
                //Store it
                s3ocProcess = newProcess;

                //Add the task to queue
                instantiatedByWindow.AddTask("s3oc_mod_edit_Running", "Running tool.");
                //Block the UI
                instantiatedByWindow.SetInteractionBlockerEnabled(true);
            };
            asyncTask.onExecuteTask_RunBackground += (callerWindow, startParams, threadTools) =>
            {
                //Create a monitor loop
                while (true)
                {
                    //Wait some time
                    threadTools.MakeThreadSleep(1000);

                    //If was finished the tool process, break the monitor loop
                    if (s3ocProcess.HasExited == true)
                        break;
                }

                //Finish the thread...
                return new string[] { "none" };
            };
            asyncTask.onDoneTask_RunMainThread += (callerWindow, backgroundResult) =>
            {
                //Remove the task from queue
                instantiatedByWindow.RemoveTask("s3oc_mod_edit_Running");
                //Unblock the UI
                instantiatedByWindow.SetInteractionBlockerEnabled(false);
            };
            asyncTask.Execute(AsyncTaskSimplified.ExecutionMode.NewDefaultThread);
        }
    }
}

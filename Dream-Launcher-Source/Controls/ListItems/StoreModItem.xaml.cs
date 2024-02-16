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
    * This script is resposible by the work of the "StoreModItem" that represents each
    * installed mod from recommended tab...
    */

    public partial class StoreModItem : UserControl
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
            Others
        }

        //Cache variables
        private string problemsFoundText = "";
        private string enabledModFileNameInsideRecModDir = "";
        private string disabledModFileNameInsideRecModDir = "";

        //Private variables
        private string gameInstallPath = "";
        private string recommendedModsDirPath = "";
        private string patchModsDirPath = "";
        private int[] requiredEpsNumbers = new int[0];
        private string[] requiredRecommendedModsFiles = new string[0];
        private string[] requiredPatchModsFiles = new string[0];
        private string modPageUrl = "";
        private string modDownloadZipUrl = "";

        //Public variables
        public MainWindow instantiatedByWindow = null;
        public ModCategory modCategory = ModCategory.Unknown;
        public string modCategoryString = "";

        //Core methods

        public StoreModItem(MainWindow instantiatedBy)
        {
            //Initialize the component
            InitializeComponent();

            //Inform that is the DataConext of this User Control
            this.DataContext = this;

            //Store reference for window that was instantiated this item
            this.instantiatedByWindow = instantiatedBy;
        }

        //Public methods

        public void SetGameInstallPath(string path)
        {
            //Store the path
            this.gameInstallPath = path;
        }

        public void SetRecommendedModsPath(string path)
        {
            //Set the contents folder path
            this.recommendedModsDirPath = path;
        }

        public void SetPatchModsPath(string path)
        {
            //Set the contents folder path
            this.patchModsDirPath = path;
        }

        public void SetTitle(string modName)
        {
            //Set the title of the mods
            title.Text = modName;
        }

        public void SetModCategory(string categoryName)
        {
            //Set the mod category
            if(categoryName == "CONTENTS")
            {
                modCategory = ModCategory.Contents;
                category.Text = ConvertFirstCharsOfStringToUpperCase(instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_cat_contents"));
            }
            if (categoryName == "GRAPHICS")
            {
                modCategory = ModCategory.Graphics;
                category.Text = ConvertFirstCharsOfStringToUpperCase(instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_cat_graphics"));
            }
            if (categoryName == "SOUNDS")
            {
                modCategory = ModCategory.Sounds;
                category.Text = ConvertFirstCharsOfStringToUpperCase(instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_cat_sounds"));
            }
            if (categoryName == "FIXES")
            {
                modCategory = ModCategory.Fixes;
                category.Text = ConvertFirstCharsOfStringToUpperCase(instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_cat_fixes"));
            }
            if (categoryName == "GAMEPLAY")
            {
                modCategory = ModCategory.Gameplay;
                category.Text = ConvertFirstCharsOfStringToUpperCase(instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_cat_gameplay"));
            }
            if (categoryName == "SLIDERS")
            {
                modCategory = ModCategory.Sliders;
                category.Text = ConvertFirstCharsOfStringToUpperCase(instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_cat_sliders"));
            }
            if (categoryName == "OTHERS")
            {
                modCategory = ModCategory.Others;
                category.Text = ConvertFirstCharsOfStringToUpperCase(instantiatedByWindow.GetStringApplicationResource("launcher_mods_installedTab_cat_others"));
            }

            //Register the category as string
            modCategoryString = categoryName;
        }

        public void SetAuthor(string authorName)
        {
            //Shows the author
            author.Text = authorName;
        }

        public void SetDescription(string enDescription, string brDescription)
        {
            //Set the description of the mod
            description.Text = "#ERROR-DESCRIPTION-UNVAILABLE#";
            if (instantiatedByWindow.launcherPrefs.loadedData.launcherLang == "en-us")
                description.Text = enDescription;
            if (instantiatedByWindow.launcherPrefs.loadedData.launcherLang == "pt-br")
                description.Text = brDescription;
        }

        public void SetRequiredEPs(int[] requiredEps)
        {
            //Prepare the list of eps icons
            List<Image> listOfEpsIcons = new List<Image>();
            listOfEpsIcons.Add(null);
            listOfEpsIcons.Add(ep1);
            listOfEpsIcons.Add(ep2);
            listOfEpsIcons.Add(ep3);
            listOfEpsIcons.Add(ep4);
            listOfEpsIcons.Add(ep5);
            listOfEpsIcons.Add(ep6);
            listOfEpsIcons.Add(ep7);
            listOfEpsIcons.Add(ep8);
            listOfEpsIcons.Add(ep9);
            listOfEpsIcons.Add(ep10);
            listOfEpsIcons.Add(ep11);

            //Disable all eps icons
            foreach (Image item in listOfEpsIcons)
                if (item != null)
                    item.Visibility = Visibility.Collapsed;

            //Enable the required eps
            for (int i = 0; i < requiredEps.Length; i++)
                if(listOfEpsIcons[requiredEps[i]] != null)
                    listOfEpsIcons[requiredEps[i]].Visibility = Visibility.Visible;

            //Store the information
            requiredEpsNumbers = requiredEps;
        }

        public void SetRequiredRecommendedModsFiles(string[] requiredRecModsFiles)
        {
            //Store the information about required recommended mods files
            this.requiredRecommendedModsFiles = requiredRecModsFiles;
        }

        public void SetRequiredPatchModsFiles(string[] requiredPatchModsFiles)
        {
            //Store the information about required recommended mods files
            this.requiredPatchModsFiles = requiredPatchModsFiles;
        }

        public void SetModPageURL(string modPageUrl)
        {
            //Store the ULR
            this.modPageUrl = modPageUrl;
        }

        public void SetModDownloadURL(string modDownloadZipUrl)
        {
            //Store the ULR
            this.modDownloadZipUrl = modDownloadZipUrl;
        }

        public void Prepare()
        {
            //Change the status for not installed
            status.Background = new SolidColorBrush(Color.FromArgb(255, 148, 148, 148));
            addButton.Visibility = Visibility.Visible;

            //Build the possible paths for this mod
            enabledModFileNameInsideRecModDir = (recommendedModsDirPath + "/" + modCategoryString + " --- " + author.Text + " - " + title.Text + ".package");
            disabledModFileNameInsideRecModDir = (recommendedModsDirPath + "/" + modCategoryString + " --- " + author.Text + " - " + title.Text + ".disabled");

            //If the mod already is installed, change status to installed
            if (File.Exists(enabledModFileNameInsideRecModDir) == true || File.Exists(disabledModFileNameInsideRecModDir) == true)
            {
                background.Background = new SolidColorBrush(Color.FromArgb(255, 245, 255, 245));
                background.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 102, 138, 105));
                status.Background = new SolidColorBrush(Color.FromArgb(255, 0, 127, 19));
                addButton.Visibility = Visibility.Collapsed;
            }

            //Setup the problems button
            problemButton.Click += (s, e) => 
            {
                //Show a dialog informin the problem
                MessageBox.Show((instantiatedByWindow.GetStringApplicationResource("launcher_mods_addNewTab_modErrorText") + "\n\n" + problemsFoundText),
                                instantiatedByWindow.GetStringApplicationResource("launcher_mods_addNewTab_modErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
            };
            //Disable the problems button
            problemButton.Visibility = Visibility.Collapsed;

            //Check installed EPs and inform possible error
            foreach (int epNumber in requiredEpsNumbers)
                if (File.Exists((gameInstallPath + "/EP" + epNumber + "/Game/Bin/skuversion.txt")) == false)
                {
                    problemsFoundText += "- " + instantiatedByWindow.GetStringApplicationResource("launcher_mods_addNewTab_modErrorReason1") + "\n\n";
                    break;
                }
            //Check if have required recommended mod files installed
            foreach(string modFilePath in requiredRecommendedModsFiles)
                if(File.Exists((recommendedModsDirPath + "/" + modFilePath)) == false)
                    problemsFoundText += "- " + instantiatedByWindow.GetStringApplicationResource("launcher_mods_addNewTab_modErrorReason2").Replace("%mod%", modFilePath) + "\n\n";
            //Check if have required patch mod files installed
            foreach (string modFilePath in requiredPatchModsFiles)
                if (File.Exists((patchModsDirPath + "/" + modFilePath)) == false)
                    problemsFoundText += "- " + instantiatedByWindow.GetStringApplicationResource("launcher_mods_addNewTab_modErrorReason3").Replace("%mod%", modFilePath) + "\n\n";
            //If have a error, change this item to problem mode
            if (problemsFoundText != "")
            {
                background.Background = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
                problemButton.Visibility = Visibility.Visible;
                addButton.IsEnabled = false;
                moreButton.Opacity = 0.25f;
                moreButton.IsHitTestVisible = false;
            }

            //Prepare the install button
            addButton.Click += (s, e) => 
            {
                //If don't have 7zip, cancel
                if (File.Exists((Directory.GetCurrentDirectory() + @"/Content/tool-szip/7zG.exe")) == false)
                {
                    instantiatedByWindow.ShowToast(instantiatedByWindow.GetStringApplicationResource("launcher_mods_addNewTab_recommendedAddError"), MainWindow.ToastType.Error);
                    return;
                }

                //Block interactions
                instantiatedByWindow.SetInteractionBlockerEnabled(true);
                //Add the task to queue
                instantiatedByWindow.AddTask("modInstalling", "Running Mod installer.");

                //Open the window of mod installer
                WindowModInstaller modInstaller = new WindowModInstaller(instantiatedByWindow, WindowModInstaller.InstallType.Recommended, modDownloadZipUrl, 
                                                                         recommendedModsDirPath, (modCategoryString + " --- " + author.Text + " - " + title.Text + ".package"));
                modInstaller.Closed += (s, e) =>
                {
                    instantiatedByWindow.UpdateRecommendedModsList();
                    instantiatedByWindow.SetInteractionBlockerEnabled(false);
                    instantiatedByWindow.RemoveTask("modInstalling");
                };
                modInstaller.Show();
            };

            //Prepare the more options context menu
            PrepareTheMoreOptionsContextMenu();
        }

        //Auxiliar methods

        private string ConvertFirstCharsOfStringToUpperCase(string toConvert)
        {
            //Prepare to return the string
            string toReturn = "";

            //Split string by spaces
            string[] words = toConvert.Split(" ");

            //Change each word to use first character with upper case
            for (int i = 0; i < words.Length; i++)
            {
                string firstChar = words[i][0].ToString();
                words[i] = (firstChar.ToUpper() + (words[i].Substring(1, (words[i].Length - 1))).ToLower());
            }

            //Rebuild the string
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < words.Length; i++)
            {
                if (i > 0)
                    stringBuilder.Append(" ");
                stringBuilder.Append(words[i]);
            }
            //Inform the result string
            toReturn = stringBuilder.ToString();

            //Return the result
            return toReturn;
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

            //Add open mod page button
            MenuItem openPage = new MenuItem();
            openPage.Header = instantiatedByWindow.GetStringApplicationResource("launcher_mods_addNewTab_openPage");
            openPage.Click += (s, e) => { OpenModPage(); };
            moreButton.ContextMenu.Items.Add(openPage);
        }

        private void OpenModPage()
        {
            //Open the mod page
            System.Diagnostics.Process.Start(new ProcessStartInfo { FileName = modPageUrl, UseShellExecute = true });
        }
    }
}

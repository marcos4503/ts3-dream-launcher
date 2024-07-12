using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    * This is the code responsible by the Mod Rename window 
    */

    public partial class WindowModRename : Window
    {
        //Private variables
        private MainWindow mainWindowRef = null;
        private Dictionary<string, int> filesNamesInTheModDirectory = new Dictionary<string, int>();
        private string modPath = "";
        private bool isModResultOfMerge = false;

        //Public variables
        public bool wasRenamedTheMod = false;

        //Core methods

        public WindowModRename(MainWindow mainWindow, string windowTitle, string modPath, bool isModResultOfMerge)
        {
            //Prepare the window
            InitializeComponent();

            //Store reference to main window
            this.mainWindowRef = mainWindow;

            //Change the title
            this.Title = windowTitle;

            //Store the mod path
            this.modPath = modPath;
            this.isModResultOfMerge = isModResultOfMerge;

            //Prepare the UI
            PrepareTheUI();
        }
    
        private void PrepareTheUI()
        {
            //Get the mod file name
            nameField.textBox.Text = System.IO.Path.GetFileNameWithoutExtension(modPath).Split(" --- ")[1];

            //Search by all mods names in this directory
            foreach (FileInfo file in (new DirectoryInfo(new FileInfo(modPath).Directory.FullName).GetFiles()))
                if (System.IO.Path.GetExtension(file.FullName).ToLower() == ".package" || System.IO.Path.GetExtension(file.FullName).ToLower() == ".disabled")
                    if (System.IO.Path.GetFileNameWithoutExtension(file.FullName).Split(" --- ")[1] != nameField.textBox.Text)
                        filesNamesInTheModDirectory.Add(System.IO.Path.GetFileNameWithoutExtension(file.FullName).Split(" --- ")[1].ToLower(), 0);

            //Setup the validation in for the text field
            nameField.RegisterOnTextChangedValidationCallback((currentInput) => 
            {
                //Prepare the value to return
                string toReturn = "";

                //Check if is empty, cancel here
                if (currentInput == "")
                {
                    toReturn = mainWindowRef.GetStringApplicationResource("launcher_mods_installedTab_modOptions_renameInvalid_empty");
                    return toReturn;
                }
                //Check if is too long
                if (currentInput.Length > 80)
                    toReturn = mainWindowRef.GetStringApplicationResource("launcher_mods_installedTab_modOptions_renameInvalid_long");
                //Check if the file already exists
                if (filesNamesInTheModDirectory.ContainsKey(currentInput.ToLower()) == true)
                    toReturn = mainWindowRef.GetStringApplicationResource("launcher_mods_installedTab_modOptions_renameInvalid_exists");
                //Check if have more than one hyphens
                if (currentInput.Split("-").Length > 2)
                    toReturn = mainWindowRef.GetStringApplicationResource("launcher_mods_installedTab_modOptions_renameInvalid_hyphen");
                //Check if have only allowed characters
                if (Regex.IsMatch(currentInput, @"^[a-zA-Z0-9_ -]+$") == false)
                    toReturn = mainWindowRef.GetStringApplicationResource("launcher_mods_installedTab_modOptions_renameInvalid_chars");
                //Check if the first character is space
                if (currentInput[0].ToString() == " ")
                    toReturn = mainWindowRef.GetStringApplicationResource("launcher_mods_installedTab_modOptions_renameInvalid_fchar");
                //Check if the last character is space
                if (currentInput[currentInput.Length - 1].ToString() == " ")
                    toReturn = mainWindowRef.GetStringApplicationResource("launcher_mods_installedTab_modOptions_renameInvalid_lchar");
                //Check if contains the reserved symbol
                if (currentInput.Contains("!₢DL-Merge₢!") == true)
                    toReturn = mainWindowRef.GetStringApplicationResource("launcher_mods_installedTab_modOptions_renameInvalid_reservSymbol");

                //Return the value
                return toReturn;
            });

            //If this is a mod result of merge, hide the merge symbol
            if (isModResultOfMerge == true)
                nameField.textBox.Text = nameField.textBox.Text.Replace("!₢DL-Merge₢! ", "");

            //Set the title of the textbox
            nameField.LabelName = mainWindowRef.GetStringApplicationResource("launcher_mods_installedTab_modOptions_renameField");

            //Set the title of the button
            saveButton.Content = mainWindowRef.GetStringApplicationResource("launcher_mods_installedTab_modOptions_renameButton");

            //Setup the done button
            saveButton.Click += (s, e) => { FinishModRenaming(); };
        }

        private void FinishModRenaming()
        {
            //If the name was not changed, cancel
            if(System.IO.Path.GetFileNameWithoutExtension(modPath).Split(" --- ")[1].Replace("!₢DL-Merge₢! ", "") == nameField.textBox.Text)
            {
                MessageBox.Show(mainWindowRef.GetStringApplicationResource("launcher_mods_installedTab_modOptions_renameInvalid_same"),
                                mainWindowRef.GetStringApplicationResource("launcher_mods_installedTab_modOptions_renameErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //If some field have error, ignore
            if (nameField.hasError() == true)
            {
                MessageBox.Show(mainWindowRef.GetStringApplicationResource("launcher_mods_installedTab_modOptions_renameErrorText"),
                                mainWindowRef.GetStringApplicationResource("launcher_mods_installedTab_modOptions_renameErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //Get file info
            string parentDirPath = new FileInfo(modPath).Directory.FullName;
            string fileExtension = System.IO.Path.GetExtension(modPath);
            string categoryPrefix = System.IO.Path.GetFileNameWithoutExtension(modPath).Split(" --- ")[0];

            //If is a mod result of merge, first, rename the folder of original mods too
            if (isModResultOfMerge == true)
            {
                //Get path of original mods folder
                string pathForOriginalMod = (new DirectoryInfo(modPath)).Parent.Parent.Parent.Parent + "/!DL-Static/merged-mods";
                //Get original name of the folder of original mods
                string originalModsFolder = pathForOriginalMod + "/" + System.IO.Path.GetFileNameWithoutExtension(modPath).Split(" --- !₢DL-Merge₢! ")[1];

                //Rename the directory of original mods too
                Directory.Move(originalModsFolder, (pathForOriginalMod + "/" + nameField.textBox.Text));
            }

            //Rename the file
            File.Move(modPath, (parentDirPath + "/" + categoryPrefix + " --- " + ((isModResultOfMerge == true ? "!₢DL-Merge₢! " : "")) + nameField.textBox.Text + fileExtension));

            //Inform that was renamed the mod
            wasRenamedTheMod = true;

            //Close this window
            this.Close();
        }
    }
}
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
    * This script is resposible by the work of the "InstalledModItem" that represents each
    * installed mod from patches...
    */

    public partial class ModToMergeItem : UserControl
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

        //Private variables
        private string thisModPath = "";

        //Public variables
        public MainWindow instantiatedByWindow = null;
        public ModCategory modCategory = ModCategory.Unknown;

        //Core methods

        public ModToMergeItem(MainWindow instantiatedBy)
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
    
        public void SetRemoveButtonDisabled()
        {
            //Disable the remove button
            removeButton.IsEnabled = false;
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

            //Show the mod title
            title.Text = System.IO.Path.GetFileNameWithoutExtension(thisModPath).Split(" --- ")[1];

            //Prepare the remove button
            removeButton.Click += (s, e) => { instantiatedByWindow.RemoveModPackageOfMerge(thisModPath); };
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
    }
}

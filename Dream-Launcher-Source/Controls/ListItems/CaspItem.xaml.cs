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
     * This script is resposible by the work of the "CASP Item" that represents each
     * CASP resource of a package...
    */

    public partial class CaspItem : UserControl
    {
        //Private variables
        private string caspEditorCachePath = "";

        //Public variables
        public MainWindow instantiatedBy = null;
        public bool wasChanged = false;
        public string instanceStr = "";
        public ulong instanceLong = 0l;
        public bool validForRandom = false;
        public bool validForMaternity = false;

        //Core methods

        public CaspItem(MainWindow instantiatedBy, string instStr, ulong instLng, bool validForRandom, bool validForMaternity)
        {
            //Initialize the component
            InitializeComponent();

            //Inform that is the DataConext of this User Control
            this.DataContext = this;

            //Store all the data
            this.instantiatedBy = instantiatedBy;
            this.instanceStr = instStr;
            this.instanceLong = instLng;
            this.validForRandom = validForRandom;
            this.validForMaternity = validForMaternity;
        }

        //Public methods

        public void SetCaspEditorCachePath(string path)
        {
            //Store the path
            this.caspEditorCachePath = path;
        }

        public void SetGender(bool forMale, bool forFemale)
        {
            //Show the gender
            if (forMale == false)
                male.Visibility = Visibility.Collapsed;
            if (forFemale == false)
                female.Visibility = Visibility.Collapsed;
        }

        public void SetAges(bool forToddler, bool forChild, bool forTeen, bool forYoungAdult, bool forAdult, bool forElder)
        {
            //Show the age
            if (forToddler == false)
                toddler.Opacity = 0.25f;
            if (forChild == false)
                child.Opacity = 0.25f;
            if (forTeen == false)
                teen.Opacity = 0.25f;
            if (forYoungAdult == false)
                youngAdult.Opacity = 0.25f;
            if (forAdult == false)
                adult.Opacity = 0.25f;
            if (forElder == false)
                elder.Opacity = 0.25f;
        }

        public void Prepare()
        {
            //Prepare the color highlight
            background.MouseEnter += (s, e) =>
            {
                if (wasChanged == false)
                    background.Background = new SolidColorBrush(Color.FromArgb(255, 233, 251, 255));
                if (wasChanged == true)
                    background.Background = new SolidColorBrush(Color.FromArgb(255, 217, 255, 217));
            };
            background.MouseLeave += (s, e) =>
            {
                if (wasChanged == false)
                    background.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                if (wasChanged == true)
                    background.Background = new SolidColorBrush(Color.FromArgb(255, 240, 255, 240));
            };

            //Set the tooltips
            male.ToolTip = instantiatedBy.GetStringApplicationResource("launcher_mods_installedTab_caspe_tooltipMale");
            female.ToolTip = instantiatedBy.GetStringApplicationResource("launcher_mods_installedTab_caspe_tooltipFemale");
            toddler.ToolTip = instantiatedBy.GetStringApplicationResource("launcher_mods_installedTab_caspe_tooltipToddler");
            child.ToolTip = instantiatedBy.GetStringApplicationResource("launcher_mods_installedTab_caspe_tooltipChild");
            teen.ToolTip = instantiatedBy.GetStringApplicationResource("launcher_mods_installedTab_caspe_tooltipTeen");
            youngAdult.ToolTip = instantiatedBy.GetStringApplicationResource("launcher_mods_installedTab_caspe_tooltipYoungAdult");
            adult.ToolTip = instantiatedBy.GetStringApplicationResource("launcher_mods_installedTab_caspe_tooltipAdult");
            elder.ToolTip = instantiatedBy.GetStringApplicationResource("launcher_mods_installedTab_caspe_tooltipElder");
            changed.ToolTip = instantiatedBy.GetStringApplicationResource("launcher_mods_installedTab_caspe_tooltipCanges");
            validForRandomIcon.ToolTip = instantiatedBy.GetStringApplicationResource("launcher_mods_installedTab_caspe_tooltipValidForRandom");
            validForMaternityIcon.ToolTip = instantiatedBy.GetStringApplicationResource("launcher_mods_installedTab_caspe_tooltipValidForMaternity");

            //Hide the asterisk of change
            changed.Visibility = Visibility.Collapsed;

            //Fill checkboxes with current values
            validForRandomCBx.IsChecked = validForRandom;
            validForMaternityCBx.IsChecked = validForMaternity;

            //Prepare the checkboxes events
            validForRandomCBx.Checked += (s, e) => { wasChanged = true; changed.Visibility = Visibility; };
            validForRandomCBx.Unchecked += (s, e) => { wasChanged = true; changed.Visibility = Visibility; };
        }

        //Auxiliar methods

        public void UpdateThumb()
        {
            //Prepare the path of possible thumb found
            string foundThumbPath = "";

            //Try to find a thumb with this instance code
            if (File.Exists((caspEditorCachePath + "/ICON_" + instanceStr + ".png")) == true)
                foundThumbPath = (caspEditorCachePath + "/ICON_" + instanceStr + ".png");
            if (File.Exists((caspEditorCachePath + "/THUM_" + instanceStr + ".png")) == true)
                foundThumbPath = (caspEditorCachePath + "/THUM_" + instanceStr + ".png");

            //If found a thumb, get the path and show it in the place
            if (foundThumbPath != "")
            {
                //Prepare the bitmap
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = new Uri(foundThumbPath);
                bitmapImage.EndInit();

                //Show on place
                thumb.Source = bitmapImage;
            }

            //Display the thumb of this CAS Part
            //thumb.Source = new BitmapImage(new Uri((caspEditorCachePath + "/THUM_" + instanceStr + ".png"), UriKind.Absolute));
        }
    }
}

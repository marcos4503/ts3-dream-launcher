using System;
using System.Collections.Generic;
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
     * This script is resposible by the work of the "PatchItem" that represents each patch available
     * in the Dream Launcher.
    */

    public partial class PatchItem : UserControl
    {
        //Enums of script
        public enum PatchStatus
        {
            Installed,
            InstalledWithProblem,
            NotInstalled,
            Installing,

            InstalledWithNoActions,
            NotInstalledWithNoActions
        }

        //Classes of script
        public class ClassDelegates
        {
            public delegate void OnClickInstall(PatchItem thisPatchItem);
        }

        //Private variables
        private event ClassDelegates.OnClickInstall onClickInstall;

        //Public variables
        public Window instantiatedByWindow = null;
        public int thisInstantiationIdInList = -1;

        //Core methods

        public PatchItem(Window instantiatedBy, int thisInstantiationIdInList)
        {
            //Initialize the component
            InitializeComponent();

            //Inform that is the DataConext of this User Control
            this.DataContext = this;

            //Store reference for window that was instantiated this item
            this.instantiatedByWindow = instantiatedBy;
            this.thisInstantiationIdInList = thisInstantiationIdInList;
        }

        //Public methods

        public void SetPatchIcon(string iconUri)
        {
            //Set the patch icon
            ImageBrush photoBrush = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/" + iconUri)));
            photoBrush.Stretch = Stretch.UniformToFill;
            icon.Background = photoBrush;
        }

        public void SetPatchTitle(string patchTitle)
        {
            //Set the patch title
            title.Text = patchTitle;
        }

        public void SetPatchDescription(string patchDescription)
        {
            //Set the patch title
            description.Text = patchDescription;
        }

        public void SetPatchStatus(PatchStatus patchStatus)
        {
            //Represents the patch status
            if(patchStatus == PatchStatus.Installed)
            {
                iconStatus.Background = new SolidColorBrush(Color.FromArgb(255, 0, 175, 27));
                status.Text = GetStringApplicationResource("launcher_patches_statusInstalled");
                installButton.Visibility = Visibility.Collapsed;
                reInstallButton.Visibility = Visibility.Visible;
                noActions.Visibility = Visibility.Collapsed;
                installingGif.Visibility = Visibility.Collapsed;
            }
            if (patchStatus == PatchStatus.InstalledWithProblem)
            {
                iconStatus.Background = new SolidColorBrush(Color.FromArgb(255, 218, 0, 0));
                status.Text = GetStringApplicationResource("launcher_patches_statusInstalledWithProblem");
                installButton.Visibility = Visibility.Collapsed;
                reInstallButton.Visibility = Visibility.Visible;
                noActions.Visibility = Visibility.Collapsed;
                installingGif.Visibility = Visibility.Collapsed;
            }
            if (patchStatus == PatchStatus.NotInstalled)
            {
                iconStatus.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                status.Text = GetStringApplicationResource("launcher_patches_statusNotInstalled");
                installButton.Visibility = Visibility.Visible;
                reInstallButton.Visibility = Visibility.Collapsed;
                noActions.Visibility = Visibility.Collapsed;
                installingGif.Visibility = Visibility.Collapsed;
            }
            if (patchStatus == PatchStatus.Installing)
            {
                iconStatus.Background = new SolidColorBrush(Color.FromArgb(255, 255, 152, 0));
                status.Text = GetStringApplicationResource("launcher_patches_statusInstalling");
                installButton.Visibility = Visibility.Collapsed;
                reInstallButton.Visibility = Visibility.Collapsed;
                noActions.Visibility = Visibility.Collapsed;
                installingGif.Visibility = Visibility.Visible;
            }
            if (patchStatus == PatchStatus.InstalledWithNoActions)
            {
                iconStatus.Background = new SolidColorBrush(Color.FromArgb(255, 0, 175, 27));
                status.Text = GetStringApplicationResource("launcher_patches_statusInstalled");
                installButton.Visibility = Visibility.Collapsed;
                reInstallButton.Visibility = Visibility.Collapsed;
                noActions.Visibility = Visibility.Visible;
                installingGif.Visibility = Visibility.Collapsed;
            }
            if (patchStatus == PatchStatus.NotInstalledWithNoActions)
            {
                iconStatus.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                status.Text = GetStringApplicationResource("launcher_patches_statusNotInstalled");
                installButton.Visibility = Visibility.Collapsed;
                reInstallButton.Visibility = Visibility.Collapsed;
                noActions.Visibility = Visibility.Visible;
                installingGif.Visibility = Visibility.Collapsed;
            }
        }

        public void RegisterOnClickInstallCallback(ClassDelegates.OnClickInstall onClickInstall)
        {
            //Register the event
            this.onClickInstall = onClickInstall;

            //Register the callback in the buttons
            installButton.Click += (s, e) => { this.onClickInstall(this); };
            reInstallButton.Click += (s, e) => { this.onClickInstall(this); };
        }

        //Auxiliar methods

        private string GetStringApplicationResource(string resourceKey)
        {
            //Prepare the string to return
            string toReturn = "###";

            //Get the resource
            //string resourceGetted = (Application.Current.Resources[resourceKey] as string);
            string resourceGetted = (instantiatedByWindow.Resources.MergedDictionaries[0][resourceKey] as string);
            if (resourceGetted != null)
                toReturn = resourceGetted;

            //Return the string
            return toReturn;
        }
    }
}

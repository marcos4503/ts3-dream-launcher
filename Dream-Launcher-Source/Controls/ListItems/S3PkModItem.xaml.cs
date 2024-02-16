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
     * This script is resposible by the work of the "Sims3Pack Mod Item" that represents each
     * package file inside a Sims3Pack file.
    */

    public partial class S3PkModItem : UserControl
    {
        //Classes of script
        public class ClassDelegates 
        {
            public delegate void OnClickInstall(S3PkModItem sender);
        }

        //Private variables
        private event ClassDelegates.OnClickInstall onClickInstall;

        //Public variables
        public int thisItemId = -1;
        public string thisPackagePath = "";

        //Core methods

        public S3PkModItem()
        {
            //Initialize the component
            InitializeComponent();

            //Inform that is the DataConext of this User Control
            this.DataContext = this;
        }
    
        //Public methods

        public void SetPackageID(int itemId)
        {
            //Store the id
            this.thisItemId = itemId;
        }

        public void SetPackagePath(string packagePath)
        {
            //Store the package path
            this.thisPackagePath = packagePath;
        }

        public void SetButtonText(string buttonText)
        {
            //Set the button text
            installButton.Content = buttonText;
        }

        public void RegisterOnClickInstall(ClassDelegates.OnClickInstall onClickInstall)
        {
            //Register the event
            this.onClickInstall = onClickInstall;

            //Register the callback in the buttons
            installButton.Click += (s, e) => { this.onClickInstall(this); };
        }
    
        public void Prepare()
        {
            //Show the mod name
            name.Text = System.IO.Path.GetFileName(thisPackagePath);
        }
    }
}

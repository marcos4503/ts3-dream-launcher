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
     * This script is resposible by the work of the "ConflictItem" that represents each conflict
     * found by the Mod Installer
    */

    public partial class ConflictItem : UserControl
    {
        //Public variables
        public MainWindow instantiatedByWindow = null;

        //Core methods

        public ConflictItem(MainWindow instantiatedBy, string modName, string resourceIndex)
        {
            //Initialize the component
            InitializeComponent();

            //Inform that is the DataConext of this User Control
            this.DataContext = this;

            //Store reference for window that was instantiated this item
            this.instantiatedByWindow = instantiatedBy;

            //Fill the UI
            if (modName.Contains(" --- ") == true)
                name.Text = modName.Split(" --- ")[1];
            if (modName.Contains(" --- ") == false)
                name.Text = modName;
            instance.Text = resourceIndex;

            //Setup the copy
            background.MouseDown += (s, e) => 
            {
                //Copy the instance to clipboard
                Clipboard.SetText(instance.Text);

                //Warn about the copy
                MessageBox.Show(instantiatedByWindow.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep3CopyText"),
                                instantiatedByWindow.GetStringApplicationResource("launcher_mods_addNewTab_modInstaller_installStep3CopyTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
            };

            //Setup the color change
            background.MouseLeave += (s, e) => { background.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)); };
            background.MouseEnter += (s, e) => { background.Background = new SolidColorBrush(Color.FromArgb(255, 225, 252, 255)); };
        }
    }
}

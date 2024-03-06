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
     * This script is resposible by the work of the "Save Item" that represents each
     * Save Game found in documents...
    */

    public partial class SaveItem : UserControl
    {
        //Public variables
        public MainWindow instantiatedBy = null;
        public string saveGameDirPath = "";

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

        }
    }
}

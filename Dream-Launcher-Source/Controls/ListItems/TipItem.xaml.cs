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
     * This script is resposible by the work of the "Tip Item" that represents each
     * tip present in Tips Section of home page
    */
    
    public partial class TipItem : UserControl
    {
        //Core methods

        public TipItem(string tipNumber, string tipTitle, string tipText)
        {
            //Initialize the component
            InitializeComponent();

            //Inform that is the DataConext of this User Control
            this.DataContext = this;

            //Render all the data
            number.Content = tipNumber;
            title.Text = tipTitle;
            text.Text = tipText;
        }
    }
}

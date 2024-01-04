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
using System.Windows.Shapes;

namespace TS3_Dream_Launcher
{
    public partial class WindowIconViewer : Window
    {
        /*
         * This is the code responsible by the Icon Viewer window 
        */

        public WindowIconViewer(string windowTitle, string imageUri)
        {
            //Prepare the window
            InitializeComponent();

            //Show the image
            this.Title = windowTitle;
            imageToView.Source = new BitmapImage(new Uri(@"pack://application:,,,/" + imageUri));
        }
    }
}

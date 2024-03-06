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
     * This script is resposible by the work of the "Export Item" that represents each
     * Export resource found in documents...
    */

    public partial class ExportItem : UserControl
    {
        //Classes of script
        public class ClassDelegates
        {
            public delegate void OnClick();
        }

        //Private variables
        private event ClassDelegates.OnClick onClick;

        //Public variables
        public MainWindow instantiatedBy = null;
        public bool isSelected = false;
        public string filePath = "";

        //Core methods

        public ExportItem(MainWindow instantiatedBy, string filePath)
        {
            //Initialize the component
            InitializeComponent();

            //Inform that is the DataConext of this User Control
            this.DataContext = this;

            //Store all the data
            this.instantiatedBy = instantiatedBy;
            this.filePath = filePath;
        }

        //Public methods

        public void RegisterOnClickCallback(ClassDelegates.OnClick onClick)
        {
            //Register the event
            this.onClick = onClick;
        }

        public void Prepare()
        {
            //Prepare the color highlight
            background.MouseEnter += (s, e) =>
            {
                if (isSelected == false)
                    background.Background = new SolidColorBrush(Color.FromArgb(255, 239, 250, 255));
                if (isSelected == true)
                    background.Background = new SolidColorBrush(Color.FromArgb(255, 212, 250, 253));
            };
            background.MouseLeave += (s, e) =>
            {
                if (isSelected == false)
                    background.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                if (isSelected == true)
                    background.Background = new SolidColorBrush(Color.FromArgb(255, 212, 250, 253));
            };

            //Prepare the selection
            background.MouseDown += (s, e) =>
            {
                //If is currently selected
                if (isSelected == true)
                {
                    //Inform selection
                    isSelected = false;
                    background.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

                    //Run callback
                    if (this.onClick != null)
                        this.onClick();

                    //Cancel
                    return;
                }

                //If is currently not selected
                if (isSelected == false)
                {
                    //Inform selection
                    isSelected = true;
                    background.Background = new SolidColorBrush(Color.FromArgb(255, 212, 250, 253));

                    //Run callback
                    if (this.onClick != null)
                        this.onClick();

                    //Cancel
                    return;
                }
            };

            //Show the export name and type
            name.Content = System.IO.Path.GetFileNameWithoutExtension(filePath);
            type.Content = System.IO.Path.GetExtension(filePath).Replace(".", "").ToLower();
        }
    }
}

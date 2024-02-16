using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
     * This script is resposible by the work of the "CacheItem" that represents each cache type available
     * in the Dream Launcher.
    */

    public partial class CacheItem : UserControl
    {
        //Classes of script
        public class ClassDelegates
        {
            public delegate string OnCalculateGarbage(string myDocumentsPath, MainWindow mainWindow);
            public delegate void OnClearGarbage(string myDocumentsPath);
            public delegate void OnClickAdditionalButton();
        }

        //Private variables
        private event ClassDelegates.OnCalculateGarbage onCalculateGarbage;
        private event ClassDelegates.OnClearGarbage onClearGarbage;
        private event ClassDelegates.OnClickAdditionalButton onClickAdditionalButton;
        private string myDocumentsPath = "";
        private string thisCacheTasksId = "";

        //Public variables
        public MainWindow instantiatedByWindow = null;

        //Core methods

        public CacheItem(MainWindow instantiatedBy)
        {
            //Initialize the component
            InitializeComponent();

            //Inform that is the DataConext of this User Control
            this.DataContext = this;

            //Store reference for window that was instantiated this item
            this.instantiatedByWindow = instantiatedBy;
        }
    
        //Public methods

        public void SetAdditionalButton(string buttonTitle, ClassDelegates.OnClickAdditionalButton onClickAdditionalButton)
        {
            //Register the event
            this.onClickAdditionalButton = onClickAdditionalButton;

            //Register the callback in the buttons
            addButton.Content = buttonTitle;
            addButton.Click += (s, e) => { this.onClickAdditionalButton(); };
            addButton.Visibility = Visibility.Visible;
        }

        public void SetIcon(string iconUri)
        {
            //Set the patch icon
            ImageBrush photoBrush = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/" + iconUri)));
            photoBrush.Stretch = Stretch.UniformToFill;
            icon.Background = photoBrush;
        }

        public void SetTitle(string title)
        {
            //Set the title
            this.title.Text = title;
        }

        public void SetDescription(string description)
        {
            //Set the description
            this.description.Text = description;
        }

        public void SetMyDocumentsPath(string myDocumentsPath)
        {
            //Store the patch
            this.myDocumentsPath = myDocumentsPath;
        }

        public void SetTasksID(string tasksId)
        {
            //Store reference for tasks id
            this.thisCacheTasksId = tasksId;
        }

        public void RegisterOnCalculateGarbageCallback(ClassDelegates.OnCalculateGarbage onCalculateGarbage)
        {
            //Register the event
            this.onCalculateGarbage = onCalculateGarbage;
        }

        public void RegisterOnClickClearCallback(ClassDelegates.OnClearGarbage onClearGarbage)
        {
            //Register the event
            this.onClearGarbage = onClearGarbage;

            //Register the callback in the buttons
            clearButton.Click += (s, e) => { ClearGarbage(); };
        }
    
        public void CalculateThisCacheSize()
        {
            //Inform the task
            instantiatedByWindow.AddTask((thisCacheTasksId + "_garbageCalc"), "Calculating garbage size.");

            //Show a temporary value
            garbageMetter.Content = "";
            clearButton.Visibility = Visibility.Collapsed;
            cleaningGif.Visibility = Visibility.Visible;

            //Start a new thread to do the calcs
            new Thread(() =>
            {
                //Inform to run in background
                Thread.CurrentThread.IsBackground = true;

                //Value to show
                string toShow = "";

                //Wait some time
                Thread.Sleep(3500);

                //Run the script of size calculator and show it
                if (onCalculateGarbage != null)
                    toShow = onCalculateGarbage(myDocumentsPath, instantiatedByWindow);

                //Run on UI
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    //Show the garbage size
                    garbageMetter.Content = toShow;
                    clearButton.Visibility = Visibility.Visible;
                    cleaningGif.Visibility = Visibility.Collapsed;

                    //Remove the task
                    instantiatedByWindow.RemoveTask((thisCacheTasksId + "_garbageCalc"));
                }));
            }).Start();
        }
    
        //Auxiliar methods

        private void ClearGarbage()
        {
            //Inform the task
            instantiatedByWindow.AddTask((thisCacheTasksId + "_garbageClear"), "Clearing garbage.");

            //Show a temporary value
            garbageMetter.Content = "";
            clearButton.Visibility = Visibility.Collapsed;
            cleaningGif.Visibility = Visibility.Visible;

            //Disable the additional button
            addButton.IsEnabled = false;

            //Start a new thread to do the calcs
            new Thread(() =>
            {
                //Inform to run in background
                Thread.CurrentThread.IsBackground = true;

                //Wait some time
                Thread.Sleep(1500);

                //Run the script of clear
                if (onClearGarbage != null)
                    onClearGarbage(myDocumentsPath);

                //Run on UI
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    //Restore the clear button
                    clearButton.Visibility = Visibility.Visible;
                    cleaningGif.Visibility = Visibility.Collapsed;

                    //Remove the task
                    instantiatedByWindow.RemoveTask((thisCacheTasksId + "_garbageClear"));

                    //Enable the additional button
                    addButton.IsEnabled = true;

                    //Recalculate the size
                    CalculateThisCacheSize();
                }));
            }).Start();
        }
    }
}

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

namespace TS3_Dream_Launcher.Controls.ValidableTextBox
{
    /* 
     * This script is responsible by the function of the custom control of "ValidableTextBox"
    */

    public partial class ValidableTextBox : UserControl
    {
        //Classes of script
        public class ClassDelegates
        {
            public delegate string OnTextChangedValidation(string currentValue);
        }

        //Private methods
        private event ClassDelegates.OnTextChangedValidation onTextChangedValidation;

        //Core methods

        public ValidableTextBox()
        {
            //Initialize the component
            InitializeComponent();

            //Inform that is the DataConext of this User Control
            this.DataContext = this;
        }

        //Public methods

        public void RegisterOnTextChangedValidationCallback(ClassDelegates.OnTextChangedValidation onTextChangedValidation)
        {
            //Register the event
            this.onTextChangedValidation = onTextChangedValidation;

            //Instruct to call the event even when the text was changed
            textBox.TextChanged += (s, e) => { CallOnTextChangedValidationCallback(); };
        }

        public void CallOnTextChangedValidationCallback()
        {
            //Prepare the result storage
            string validationResult = "";

            //Execute the callback and catch the result
            if (this.onTextChangedValidation != null)
                validationResult = this.onTextChangedValidation(textBox.Text);

            //If have error
            if (validationResult != "")
            {
                error.Visibility = Visibility.Visible;
                errorTxt.Text = validationResult;
                textBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                textBox.SelectionBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            }

            //If don't have error
            if (validationResult == "")
            {
                error.Visibility = Visibility.Collapsed;
                errorTxt.Text = "";
                textBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 179, 171, 171));
                textBox.SelectionBrush = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
            }
        }

        public bool hasError()
        {
            //Prepare the value to return
            bool toReturn = false;

            //Force the validation code to run
            CallOnTextChangedValidationCallback();

            //If the error is visible, return the response
            if (error.Visibility == Visibility.Visible)
                toReturn = true;

            //Return the value
            return toReturn;
        }

        //Custom Properties Registration

        //*** LabelName Property ***/

        public static readonly DependencyProperty LabelNameProperty = DependencyProperty.Register(
          nameof(LabelName),
          typeof(string),
          typeof(ValidableTextBox),
          new PropertyMetadata("Label", new PropertyChangedCallback(OnChange_LabelNameProperty)));

        public string LabelName
        {
            get { return (string)GetValue(LabelNameProperty); }
            set { SetValue(LabelNameProperty, value); }
        }

        public static void OnChange_LabelNameProperty(DependencyObject dpo, DependencyPropertyChangedEventArgs dpc)
        {
            //Callback called when property was changed
        }
    }
}

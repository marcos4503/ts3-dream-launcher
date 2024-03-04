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

namespace TS3_Dream_Launcher.Controls.BeautyButton
{
    /* 
     * This script is responsible by the function of the custom control of "BeautyButton"
    */

    public partial class BeautyButton : Button
    {
        //Core methods

        public BeautyButton()
        {
            //Initialize the component
            InitializeComponent();
        }

        //Custom Properties Registration

        //*** HoverBackground Property ***/

        public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register(
                nameof(HoverBackground),
                typeof(Brush),
                typeof(BeautyButton),
                new PropertyMetadata((new BrushConverter().ConvertFromString("#FFBEE6FD") as Brush), new PropertyChangedCallback(OnChange_HoverBackgroundProperty))
            );

        public Brush HoverBackground
        {
            get { return (Brush)GetValue(HoverBackgroundProperty); }
            set { SetValue(HoverBackgroundProperty, value); }
        }

        public static void OnChange_HoverBackgroundProperty(DependencyObject dpo, DependencyPropertyChangedEventArgs dpc)
        {
            //Callback called when property was changed
        }
    }
}

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
    /// <summary>
    /// Interação lógica para BeautyButton.xam
    /// </summary>
    public partial class BeautyButton : Button
    {
        //Private static variables
        readonly static Brush defaultHoverBackgroundValue = new BrushConverter().ConvertFromString("#FFBEE6FD") as Brush;

        //Core methods

        public BeautyButton()
        {
            InitializeComponent();
        }

        //Addiction of "HoverBackground" color, custom property for the "Brush" tab of Button

        public Brush HoverBackground
        {
            get { return (Brush)GetValue(HoverBackgroundProperty); }
            set { SetValue(HoverBackgroundProperty, value); }
        }

        public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register("HoverBackground", typeof(Brush), typeof(BeautyButton), new PropertyMetadata(defaultHoverBackgroundValue));
    }
}

// Updated by XamlIntelliSenseFileGenerator 10/07/2024 21:40:33
#pragma checksum "..\..\..\..\..\Controls\ListItems\LinkPatchItem.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "75159386D3AD784166D7F71D9BB8F305C387D2D9"
//------------------------------------------------------------------------------
// <auto-generated>
//     O código foi gerado por uma ferramenta.
//     Versão de Tempo de Execução:4.0.30319.42000
//
//     As alterações ao arquivo poderão causar comportamento incorreto e serão perdidas se
//     o código for gerado novamente.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using TS3_Dream_Launcher.Controls.ListItems;


namespace TS3_Dream_Launcher.Controls.ListItems
{


    /// <summary>
    /// LinkPatchItem
    /// </summary>
    public partial class LinkPatchItem : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector
    {

        private bool _contentLoaded;

        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "6.0.25.0")]
        public void InitializeComponent()
        {
            if (_contentLoaded)
            {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/TS3 Dream Launcher;V1.1.0;component/controls/listitems/linkpatchitem.xaml", System.UriKind.Relative);

#line 1 "..\..\..\..\..\Controls\ListItems\LinkPatchItem.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);

#line default
#line hidden
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "6.0.25.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target)
        {
            this._contentLoaded = true;
        }

        internal System.Windows.Controls.Border icon;
        internal System.Windows.Controls.Border iconStatus;
        internal System.Windows.Controls.TextBlock status;
        internal System.Windows.Controls.TextBlock title;
        internal System.Windows.Controls.TextBlock description;
        internal TS3_Dream_Launcher.Controls.BeautyButton.BeautyButton installButton;
        internal TS3_Dream_Launcher.Controls.BeautyButton.BeautyButton reInstallButton;
        internal TS3_Dream_Launcher.Controls.BeautyButton.BeautyButton noActions;
        internal System.Windows.Controls.Image installingGif;
    }
}

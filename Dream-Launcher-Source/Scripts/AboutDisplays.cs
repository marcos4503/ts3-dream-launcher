using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TS3_Dream_Launcher.Scripts
{
    /*
     * This class creates a object for query informations about displays 
    */

    public class AboutDisplays
    {
        //Enums of script
        public enum DisplaysMode
        {
            GetAllRegistryForAllDisplays,
            GetCurrentDisplaySettingsOnly
        }

        //Classes of script
        [StructLayout(LayoutKind.Sequential)]
        public struct DisplayInfo
        {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        //Private variables
        private List<DisplayInfo> displays = new List<DisplayInfo>();

        //Core methods

        public AboutDisplays(DisplaysMode loadInformationsMode)
        {
            //Prepare the temporary data holder
            DisplayInfo tempDisplayInfo = new DisplayInfo();

            //If is desired all registry settings
            if(loadInformationsMode == DisplaysMode.GetAllRegistryForAllDisplays)
            {
                //Prepare the index
                int i = 0;

                //Start the loop to extract informations
                while (EnumDisplaySettings(null, i, ref tempDisplayInfo))
                {
                    displays.Add(tempDisplayInfo);
                    i += 1;
                }
            }

            //If is desired only the current settings defined for main display
            if (loadInformationsMode == DisplaysMode.GetCurrentDisplaySettingsOnly)
            {
                EnumDisplaySettings(null, -1, ref tempDisplayInfo);
                displays.Add(tempDisplayInfo);
            }
        }

        public DisplayInfo[] GetDisplaysInformations()
        {
            //Return the list of displays informations
            return displays.ToArray();
        }

        //Native methods

        const int ENUM_CURRENT_SETTINGS = -1;
        const int ENUM_REGISTRY_SETTINGS = -2;
        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DisplayInfo devMode);
    }
}

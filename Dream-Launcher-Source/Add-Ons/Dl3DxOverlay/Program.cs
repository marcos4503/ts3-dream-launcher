using System;

namespace Dl3DxOverlay
{
    /*
     * This is the code responsible by the working of the Dream Launcher Directx Overlay on
     * Command Line Interface - CLI.
    */

    public static class Program
    {
        //Private variables
        private static string[] catchedArguments = null;

        //Core methods

        [STAThread]
        public static void Main(string[] arguments)
        {
            //Store all the arguments
            catchedArguments = arguments;

            //If don't have a argument, cancel
            if (arguments.Length < 2)
                return;

            //Get the target process name and overlay position
            string processName = arguments[0];
            int overlayPosition = int.Parse(arguments[1]);

            //Try to get a reference for the process name
            System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessesByName(processName).FirstOrDefault();

            //If not found a process, stop the application
            if (process == null)
                return;

            //Start the DirectX overlay in target process
            DirectXOverlay overlay = new DirectXOverlay(process, overlayPosition);
            overlay.Initialize(null);

            //Try to read the input in the console, to pause the application...
            string result = Console.ReadLine();
        }
    }
}
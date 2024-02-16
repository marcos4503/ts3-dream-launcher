using Microsoft.Diagnostics.Tracing.Session;
using Overlay.NET.Common;
using Overlay.NET.Directx;
using Process.NET;
using Process.NET.Memory;
using Process.NET.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dl3DxOverlay
{
    /*
     * This is the object that makes the overlay work
    */

    public class DirectXOverlay : DirectXOverlayPlugin
    {
        //Constant variables
        //Event Codes (https://github.com/GameTechDev/PresentMon/blob/40ee99f437bc1061a27a2fc16a8993ee8ce4ebb5/PresentData/PresentMonTraceConsumer.cpp)
        private const int EventID_D3D9PresentStart = 1;
        private const int EventID_DxgiPresentStart = 42;
        //ETW provider codes
        private static readonly Guid DXGI_provider = Guid.Parse("{CA11C036-0102-4A2D-A6AD-F03CFED5D3C9}");
        private static readonly Guid D3D9_provider = Guid.Parse("{783ACA0A-790E-4D7F-8451-AA850511C6B9}");

        //Cache variables
        private Thread ramCounterThread = null;
        private string ramString = "";
        private Thread fpsCounterThread = null;
        private TraceEventSession fpsEtwSession = null;
        private Stopwatch fpsStopwatch = null;
        private List<long> fpsTimestampCollection = null;
        private Thread fpsEtwThread = null;
        private Thread fpsEtwOutputThread = null;
        private object fpsThreadsSync = new object();
        private string fpsString = "";

        //Private variables
        private int overlayPosition = -1;
        private System.Diagnostics.Process targetProcess = null;
        private ProcessSharp processSharp = null;
        private Stopwatch stopwatch = null;
        private TickEngine tickEngine = new TickEngine();

        //Draw tools
        private int backgroundBrush = -1;
        private int whiteBrush = -1;
        private int separatorBrush = -1;
        private int font = -1;

        //Core methods

        public DirectXOverlay(System.Diagnostics.Process targetProcess, int overlayPosition)
        {
            //Store reference for target process
            this.overlayPosition = overlayPosition;
            this.targetProcess = targetProcess;
        }

        public override void Initialize(IWindow tiwindow)
        {
            //Initialize process sharp
            processSharp = new ProcessSharp(targetProcess, MemoryType.Remote);

            //Get the target window
            IWindow targetWindow = processSharp.WindowFactory.MainWindow;

            //Set the target window
            base.Initialize(targetWindow);
            OverlayWindow = new DirectXOverlayWindow(targetWindow.Handle, false);

            //Start a new stopwatch
            stopwatch = Stopwatch.StartNew();

            //Register events for tick engine
            tickEngine.PreTick += OnPreTick;
            tickEngine.Tick += OnTick;

            //Prepare the draw tools
            PrepareTheDrawTools();
            //Start RAM and FPS counter threads
            StartRamCounterThread();
            StartFpsCounterThread();

            //Enable the overlay
            Enable();

            //Start the update loop
            while (true)
                Update();
        }

        private void OnPreTick(object sender, EventArgs e) 
        {
            //Check if the target window is activated
            bool isActivated = TargetWindow.IsActivated;

            //If the window is not activated and the overlay is visible, hide it
            if(isActivated == false && OverlayWindow.IsVisible == true)
            {
                stopwatch.Stop();
                ClearScreen();
                OverlayWindow.Hide();
            }

            //If the window is activated  and the overlay is not visible, show it
            if(isActivated == true && OverlayWindow.IsVisible == false)
                OverlayWindow.Show();
        }

        private void OnTick(object sender, EventArgs e)
        {
            //If the overlay is not visible, cancel
            if (OverlayWindow.IsVisible == false)
                return;

            //Update the overlay
            OverlayWindow.Update();
            InternalRender();
        }

        private protected void InternalRender() 
        {
            //If the stopwatch is stopped, resume it
            if (stopwatch.IsRunning == false)
                stopwatch.Start();

            //Start the scene
            OverlayWindow.Graphics.BeginScene();
            OverlayWindow.Graphics.ClearScene();

            //Render the overlay content
            RenderOverlayContent();

            //End the scene
            OverlayWindow.Graphics.EndScene();
        }

        private void ClearScreen()
        {
            OverlayWindow.Graphics.BeginScene();
            OverlayWindow.Graphics.ClearScene();
            OverlayWindow.Graphics.EndScene();
        }

        //Content methods

        private void StartRamCounterThread()
        {
            //Prepare the thread
            ramCounterThread = new Thread(() =>
            {
                //Start a loop
                while (true)
                {
                    //If the process was endend, break the loop
                    if (targetProcess.HasExited == true)
                        break;

                    //Get the RAM usage
                    targetProcess.Refresh();
                    int ramUsageMb = (int)((float)(targetProcess.PrivateMemorySize64) / 1024.0f / 1024.0f);

                    //Save into the string
                    ramString = (ramUsageMb + " MB");

                    //Wait interval
                    Thread.Sleep(5000);
                }

                //Wait interval
                Thread.Sleep(250);
            });

            //Start the thread
            ramCounterThread.IsBackground = true;
            ramCounterThread.Start();
        }

        private void StartFpsCounterThread()
        {
            //Prepare the thread
            fpsCounterThread = new Thread(() =>
            {
                //Create the ETW session
                fpsEtwSession = new TraceEventSession("dl3DxOverlayFpsSession");
                fpsEtwSession.StopOnDispose = true;
                fpsEtwSession.EnableProvider("Microsoft-Windows-D3D9");
                fpsEtwSession.EnableProvider("Microsoft-Windows-DXGI");

                //Install the event handler
                fpsEtwSession.Source.AllEvents += data => 
                {
                    //filter out frame presentation events
                    if (((int)data.ID == EventID_D3D9PresentStart && data.ProviderGuid == D3D9_provider) || ((int)data.ID == EventID_DxgiPresentStart && data.ProviderGuid == DXGI_provider))
                    {
                        //If the process is not the target process, cancel
                        if (data.ProcessID != targetProcess.Id)
                            return;

                        //Lock this block of code to avoid changing "fpsTimestampCollection" while other thread is working on it
                        lock (fpsThreadsSync)
                        {
                            //Add the timestamp of this frame being rendered on the list of frames timestamps
                            fpsTimestampCollection.Add(fpsStopwatch.ElapsedMilliseconds);

                            //If is reached the maximum of 1000, remove the more older to avoid ifinite list size
                            if (fpsTimestampCollection.Count > 1000)
                                fpsTimestampCollection.RemoveAt(0);
                        }
                    }
                };

                //Prepare the timestamp collection list
                fpsTimestampCollection = new List<long>();

                //Create a fps stopwatch
                fpsStopwatch = new Stopwatch();
                fpsStopwatch.Start();

                //Create the ETW thread process
                fpsEtwThread = new Thread(() => 
                {
                    //Start tracing
                    fpsEtwSession.Source.Process();
                });
                fpsEtwThread.IsBackground = true;
                fpsEtwThread.Start();

                //Create the ETW thread output process
                fpsEtwOutputThread = new Thread(() => 
                {
                    //Start the fps monitor loop
                    while (true)
                    {
                        //Prepare the time information
                        long startTime = -1;
                        long endTime = -1;

                        //Lock this block of code to avoid reading "fpsTimestampCollection" while other thread is changing it
                        lock (fpsThreadsSync)
                        {
                            //Calculate the start time and end time to be used as filter to get the frames rendered between this times
                            endTime = fpsStopwatch.ElapsedMilliseconds;
                            startTime = endTime - 2000;

                            //Get the number of frames rendered between the start and end time
                            int count = 0;
                            foreach (var timeStamp in fpsTimestampCollection)
                                if (timeStamp >= startTime && timeStamp <= endTime)
                                    count += 1;

                            //Calculate FPS
                            fpsString = (GetFixedNumberString(((double)count / 2000.0f * 1000.0f).ToString("F0"), 3) + " FPS");
                        }

                        //Wait time before next FPS update
                        Thread.Sleep(200);
                    }
                });
                fpsEtwOutputThread.IsBackground = true;
                fpsEtwOutputThread.Start();
            });

            //Start the thread
            fpsCounterThread.IsBackground = true;
            fpsCounterThread.Start();
        }

        private void PrepareTheDrawTools()
        {
            //Prepare the draw tools
            backgroundBrush = OverlayWindow.Graphics.CreateBrush(Color.FromArgb(135, 0, 0, 0));
            whiteBrush = OverlayWindow.Graphics.CreateBrush(Color.FromArgb(255, 255, 255, 255));
            separatorBrush = OverlayWindow.Graphics.CreateBrush(Color.FromArgb(128, 255, 255, 255));
            font = OverlayWindow.Graphics.CreateFont("Courier New", 12);
        }

        private void RenderOverlayContent()
        {
            //Get the overlay window size
            int width = OverlayWindow.Width;
            int height = OverlayWindow.Height;

            //Define the background size of the overlay
            int bgWidth = 132;
            int bgHeight = 16;

            //Draw a fill rectangle (bottom-right of screen)
            if (overlayPosition == 0)
            {
                OverlayWindow.Graphics.FillRectangle((width - bgWidth - 6), (height - bgHeight - 6), (bgWidth), (bgHeight), backgroundBrush);
                OverlayWindow.Graphics.DrawText(ramString, font, whiteBrush, (width - bgWidth - 6 + 2), (height - bgHeight - 6 + 2));
                OverlayWindow.Graphics.DrawLine((width - (bgWidth / 2) - 6), (height - bgHeight - 6 + 4), (width - (bgWidth / 2) - 6), (height - bgHeight - 6 + 12), 2, separatorBrush);
                OverlayWindow.Graphics.DrawText(fpsString, font, whiteBrush, (width - bgWidth - 6 + 80), (height - bgHeight - 6 + 2));
            }

            //Draw a fill rectangle (top-middle of screen)
            if (overlayPosition == 1)
            {
                OverlayWindow.Graphics.FillRectangle((int)((float)width * 0.5f) - (bgWidth / 2), 6, bgWidth, bgHeight, backgroundBrush);
                OverlayWindow.Graphics.DrawText(ramString, font, whiteBrush, (int)((float)width * 0.5f) - (bgWidth / 2) + 2, 8);
                OverlayWindow.Graphics.DrawLine((int)((float)width * 0.5f), 10, (int)((float)width * 0.5f), 18, 2, separatorBrush);
                OverlayWindow.Graphics.DrawText(fpsString, font, whiteBrush, (int)((float)width * 0.5f) - (bgWidth / 2) + 80, 8);
            }
        }

        //Auxiliar methods

        public override void Enable()
        {
            //Enable the overlay
            tickEngine.Interval = (1000 / 30).Milliseconds();
            tickEngine.IsTicking = true;
            base.Enable();
        }

        public override void Disable()
        {
            //Disable the overlay
            tickEngine.IsTicking = false;
            base.Disable();
        }
    
        public override void Update()
        {
            //Update the tick engine
            tickEngine.Pulse();
        }

        public override void Dispose()
        {
            //Dispose this overlay
            OverlayWindow.Dispose();
            base.Dispose();

            //Stop the threads
            ramCounterThread.Interrupt();
            fpsCounterThread.Interrupt();
            fpsEtwSession.Dispose();
            fpsEtwThread.Interrupt();
            fpsEtwOutputThread.Interrupt();
        }
    
        public string GetFixedNumberString(string stringToFix, int decimalHouses)
        {
            //Prepare the result string
            string toReturn = stringToFix;

            //Add zero if is smaller than the needed
            while (toReturn.Length < decimalHouses)
                toReturn = (" " + toReturn);

            //Return the string
            return toReturn;
        }
    }
}

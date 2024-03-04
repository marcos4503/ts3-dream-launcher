using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MarcosTomaz.ATS
{
    /*
    / This class make possible to do background tasks in other thread
    / in a easy and simplified way.
    */

    public class AsyncTaskSimplified
    {
        //Enums of script
        public enum ExecutionMode
        {
            NewDefaultThread
        }

        //Classes of script
        public class ClassDelegates
        {
            public delegate void OnStartTask_RunMainThread(Window sourceWindow, string[] startParameters);
            public delegate string[] OnExecuteTask_RunBackground(Window sourceWindow, string[] startParameters, ThreadTools threadTools);
            public delegate void OnNewProgress_RunMainThread(Window sourceWindow, string newProgress);
            public delegate void OnDoneTask_RunMainThread(Window sourceWindow, string[] bgTaskResult);
        }
        public class ThreadTools
        {
            //This class will be passed into the event "onExecuteTask_RunBackground" in parameters and will be used to help the user to do useful things

            //Private variables
            private AsyncTaskSimplified parentAsyncTaskSimplified = null;

            //Core methods
            public ThreadTools(AsyncTaskSimplified parentAsyncTaskSimplified)
            {
                //Initialize this class
                this.parentAsyncTaskSimplified = parentAsyncTaskSimplified;
            }

            /// <summary>
            /// Allows the "onExecuteTask_RunBackground" event (which runs on another Background Thread)<br/>
            /// to report progress and then the "onNewProgress_RunMainThread" (which runs on UI/Main Thread) event will be called<br/>
            /// to update something in the UI or current Main Thread.
            /// </summary>
            public void ReportNewProgress(string newProgress)
            {
                //Call the parent async task simplified object to report progress
                parentAsyncTaskSimplified.ReportProgress(newProgress);
            }

            /// <summary>
            /// This method will make the Background Thread that is running the code contained in the "onExecuteTask_RunBackground"<br/>
            /// event sleep for a few milliseconds defined in the parameter.
            /// </summary>
            public void MakeThreadSleep(int timeOfSleepInMs)
            {
                //Make the Thread that called this method sleep
                try { Thread.Sleep(timeOfSleepInMs); } catch (Exception e) { Debug.WriteLine(e.Message); }
            }
        }

        //Private variables
        private bool isAlreadyRunning = false;
        private Window callerWindow = null;
        private string[] startParameters = null;

        //Public variables
        public event ClassDelegates.OnStartTask_RunMainThread onStartTask_RunMainThread;
        public event ClassDelegates.OnExecuteTask_RunBackground onExecuteTask_RunBackground;
        public event ClassDelegates.OnNewProgress_RunMainThread onNewProgress_RunMainThread;
        public event ClassDelegates.OnDoneTask_RunMainThread onDoneTask_RunMainThread;

        //Core methods

        ///<summary>
        /// Initialize the "Async Task Simplified" object to do any background tasks!
        ///
        /// <br/><br/><b>WARN:</b> THIS CLASS MUST ONLY BE STARTED FROM THE UI/MAIN THREAD!<br/><br/>
        /// 
        /// context - The context that this Async Task Simplified is being called.<br/><br/>
        /// 
        /// 
        /// startParameters - The parameters array that will be used by the Task. The parameters will be available for<br/>
        /// "onStartTask_RunMainThread" and "onExecuteTask_RunBackground" events as well.
        /// 
        /// <br/><br/><b>WARN:</b> You must register callbacks for the required events. See the events below...<br/><br/>
        /// 
        /// onStartTask_RunMainThread - The code for this event runs on the UI/Main Thread. The code for<br/>
        /// this event is executed before the Async Task starts. In the parameter of this event you find<br/>
        /// the String array that contains the parameters you provided when starting the Async Task.<br/><br/>
        /// 
        ///  onExecuteTask_RunBackground - The code for this event runs on the Background Thread. In the parameter of<br/>
        ///  this event you find the String array that contains the parameters you provided when starting the Async Task. The code<br/>
        ///  inside this event will be executed in another Thread, in a background Thread that is not the<br/>
        ///  UI/Main Thread.From within this event it is not possible to do anything that would only be<br/>
        ///  possible from the UI/Main Thread, such as controlling the UI, Window, manipulating UI<br/>
        ///  components and so on. Here inside this event is where you should put all the heavy or time<br/>
        ///  consuming code that you would like to run in the background. This event also brings you an object<br/>
        ///  called "ThreadTools" that you can use to report progress. Whenever you call the "ThreadTools"<br/>
        ///  object to report progress, the next method("onNewProgress_RunMainThread") will be called and you<br/>
        ///  can for example update things in the UI and etc. At the end of this event's code, you can return a<br/>
        ///  "string" Array that will be the result of your code runned by this event, in addition, this array<br/>
        ///  response will be sent to the "onDoneTask_RunMainThread" event as a parameter as well.<br/><br/>
        ///  
        /// onNewProgress_RunMainThread - The code for this event runs on the UI/Main Thread. The code inside this<br/>
        /// event will be executed in the UI/Main Thread and this event will always be called when you use the<br/>
        /// "ThreadTools" object contained in the "onExecuteTask_RunBackground" event to call ReportNewProgress() method, that<br/>
        /// will run in the background. You can use this method together with "ThreadTools" to communicate with the UI or<br/>
        /// update it as the background task moves.
        /// 
        /// onDoneTask_RunMainThread - The code for this event runs on the UI/Main Thread. The code inside this<br/>
        /// event will be executed shortly after the code contained in the "onExecuteTask_RunBackground" event<br/>
        /// completes. In addition, this event takes as a parameter a String array resulting from the processing<br/>
        /// done after the execution of the event code "onExecuteTask_RunBackground".
        ///</summary>
        public AsyncTaskSimplified(Window context, string[] startParameters)
        {
            //If is already running, stop this method
            if (isAlreadyRunning == true)
                return;

            //Fill this class
            callerWindow = context;
            this.startParameters = startParameters;
        }

        /// <summary>
        /// This method causes the task to start running immediately based on the chosen mode.<br/><br/>
        /// 
        /// <b>WARN:</b> THIS METHOD MUST ONLY BE RUN FROM THE UI/MAIN THREAD!<br/><br/>
        /// 
        /// executionMode - The mode the Task will be executed.<br/><br/>
        /// 
        /// -- The "NewDefaultThread" will run your task in C# common "Thread"<br/>
        /// class and will not be queued as it will run in parallel with other<br/>
        /// tasks that also use "NewDefaultThread" mode.
        /// </summary>
        public void Execute(ExecutionMode executionMode)
        {
            //If is already running, stop this method
            if (isAlreadyRunning == true)
                return;

            //If was not registered the mandatory callbacks, cancel
            if(onExecuteTask_RunBackground == null || onDoneTask_RunMainThread == null)
            {
                throw new Exception("Error starting Async Task. You must register callbacks for events \"onExecuteTask_RunBackground\" and \"onDoneTask_RunMainThread\".");
                return;
            }

            //Inform that is running now
            isAlreadyRunning = true;

            //Call the "onStartTask_RunMainThread" event on Main Thread
            if(onStartTask_RunMainThread != null)
                onStartTask_RunMainThread(callerWindow, startParameters);

            //If is the execution mode "NewDefaultThread" (Parallel)
            if (executionMode == ExecutionMode.NewDefaultThread)
                new Thread(() => 
                {
                    //Inform to run in background
                    Thread.CurrentThread.IsBackground = true;

                    //Run the background code and get the result
                    string[] bgResult = onExecuteTask_RunBackground(callerWindow, startParameters, new ThreadTools(this));

                    //Run the on done task informing the result (in Main Thread)
                    Application.Current.Dispatcher.Invoke(new Action(() => 
                    { 
                        onDoneTask_RunMainThread(callerWindow, bgResult); 
                    }));
                }).Start();
        }

        /// <summary>
        /// <b>WARN:</b> This method must not be used because it is a method of internal use of the Async Task Simplified class.
        /// </summary>
        public void ReportProgress(string newProgress)
        {
            //If is not running, stop this method
            if (isAlreadyRunning == false)
                return;

            //Run the on new progress callback (in Main Thread)
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                onNewProgress_RunMainThread(callerWindow, newProgress);
            }));
        }
    }
}

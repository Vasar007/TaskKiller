using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;

namespace TaskKiller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Fields

        /// <summary>
        /// Timer for cleaning.
        /// </summary>
        private DispatcherTimer mDispatcherTimerApp;

        /// <summary>
        /// List for exception processes.
        /// </summary>
        private string[] mNotKilledProcessesList;

        #endregion

        #region Private Properties

        /// <summary>
        /// View model for binding with DataGrid in window form.
        /// </summary>
        public ObservableCollection<ProcessInfo> ListProcesses { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Initialize private fields.
            mDispatcherTimerApp = new DispatcherTimer();

            // DispatcherTimer setup.
            mDispatcherTimerApp.Tick += new EventHandler(CleaningProcesses);
            mDispatcherTimerApp.Interval = new TimeSpan(0, 0, 30);

            // Initialize data for DataGrid.
            ListProcesses = new ObservableCollection<ProcessInfo>();
            
            // Add new process info.
            CreateProcessesInfo();

            // Bind this class with DataGrid.
            ProcessViewer.DataContext = this;

            // Display CPU usage on window title.
            DisplayProcessorTotalUsage();
        }

        #endregion

        #region Button Methods

        /// <summary>
        /// Update all information about running processes in current system.
        /// </summary>
        /// <param name="sender">Button.</param>
        /// <param name="e">Click event.</param>
        private void UpdateProcesses(object sender, EventArgs e)
        {
            // Clear all old process.
            ListProcesses.Clear();

            // Add new process info.
            CreateProcessesInfo();
        }

        /// <summary>
        /// Kill process which selected in DataGrid.
        /// </summary>
        /// <param name="sender">Button.</param>
        /// <param name="e">Click event.</param>
        private void KillProcess(object sender, EventArgs e)
        {
            var processInfo = ProcessViewer.SelectedItem as ProcessInfo;

            // Check if user didn't select any item in DataGrid.
            if (processInfo == null)
                return;

            foreach (var process in Process.GetProcesses())
            {
                if ((process.ProcessName == processInfo.ProcessName) && 
                    (process.Id == processInfo.Id))
                {
                    // If program found process, try to kill it...
                    try
                    {
                        process.Kill();
                        MessageBox.Show($"Process {processInfo.ProcessName} was killed",
                            "TaskKiller");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to kill process {processInfo.ProcessName}." +
                            $" {ex.Message}", "TaskKiller");
                    }
                    finally
                    {
                        // Update DataGrid after killing new process.
                        UpdateProcesses(sender, e);
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Start new process on the entered path.
        /// </summary>
        /// <param name="sender">Button.</param>
        /// <param name="e">Click event.</param>
        private void StartProcess(object sender, EventArgs e)
        {
            var desktopWorkingArea = SystemParameters.WorkArea;

            // Show window with fields for writing path to process.
            string path = Interaction.InputBox("Enter process path", "TaskKiller", "" ,
                ((int)desktopWorkingArea.Width - 300) / 2,
                ((int)desktopWorkingArea.Height - 100) / 2);

            // Check if process path is empty.
            if (string.IsNullOrWhiteSpace(path))
                return;

            // Try to start process on current path...
            try
            {
                Process.Start(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start process {path}. {ex.Message}", "TaskKiller");
            }
            finally
            {
                // Update DataGrid after starting new process.
                UpdateProcesses(sender, e);
            }
            
        }

        /// <summary>
        /// Set configuration for cleaning and run/stop timer.
        /// </summary>
        /// <param name="sender">Button.</param>
        /// <param name="e">Click event.</param>
        private void SetConfigForCleaning(object sender, EventArgs e)
        {
            // Toogle timer switch and update text in textbox.
            if (mDispatcherTimerApp.IsEnabled)
            {
                mDispatcherTimerApp.Stop();

                this.SpecialButton.Content = "Start Cleaning";
            }
            else
            {
                var desktopWorkingArea = SystemParameters.WorkArea;

                // Show window with fields for writing names of the processes.
                string input = Interaction.InputBox("Enter names of the processes which won't be" +
                    " killed separated by semicolons", "TaskKiller", "",
                    ((int)desktopWorkingArea.Width - 300) / 2,
                    ((int)desktopWorkingArea.Height - 100) / 2);

                // Excude all not valid processes names.
                mNotKilledProcessesList = input.Split(';').
                    Where(str => !string.IsNullOrWhiteSpace(str)).ToArray();

                mDispatcherTimerApp.Start();

                this.SpecialButton.Content = "Stop Cleaning";
            }
        }

        #endregion

        #region Timer Method

        /// <summary>
        /// Calculate process value and kill garbage processes.
        /// </summary>
        /// <param name="sender">Timer.</param>
        /// <param name="e">Basic System.EventArgs class.</param>
        private void CleaningProcesses(object sender, EventArgs e)
        {
            // Define some variables for calculating.
            double calclulatedValue = 0;
            double tempMax = double.MinValue;
            var garbageProcess = -1;

            // Iterate processes list.
            foreach (var processInfo in ListProcesses)
            {
                // If process has high priority class or it's system process or it's in the speceal 
                // list, skip it.
                if (processInfo.TotalProcessorTime == TimeSpan.Zero ||
                (processInfo.PriorityClass == (ProcessPriorityClass.AboveNormal |
                ProcessPriorityClass.High | ProcessPriorityClass.RealTime)) ||
                mNotKilledProcessesList.Contains(processInfo.ProcessName))
                {
                    continue;
                }


                // Calculate process value.
                calclulatedValue = ((double)(processInfo.PhysicalMemoryUsage) / (1024)) -
                    (processInfo.TotalProcessorTime.TotalMilliseconds /
                    Convert.ToDouble(Environment.ProcessorCount));

                // Find max garbage process.
                if (tempMax < calclulatedValue)
                {
                    tempMax = calclulatedValue;
                    garbageProcess = processInfo.Id;
                }

            }

            // Check if we found nothing.
            if (garbageProcess == -1)
                return;

            // Find process for cleaning in system list.
            foreach (var process in Process.GetProcesses())
            {
                var processName = process.ProcessName;
            
                if (process.Id == garbageProcess)
                {
                    // If program found process, try to kill it...
                    try
                    {
                        process.Kill();
                        MessageBox.Show($"Process {processName} killed", "TaskKiller");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to kill process {processName}." +
                            $" {ex.Message}", "TaskKiller");
                    }
                    finally
                    {
                        // Update DataGrid after killing new process.
                        UpdateProcesses(sender, e);
                    }
                }
            }

            // Forcing the CommandManager to raise the RequerySuggested event.
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Display processor total usage on window title.
        /// </summary>
        private void DisplayProcessorTotalUsage()
        {
            // Create worker with report progress.
            var wrkr = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };

            var curProcess = Process.GetCurrentProcess().ProcessName;

            // Calculate CPU usage.
            wrkr.DoWork += (object sender, DoWorkEventArgs e) => {
                while (true)
                {
                    var message = string.Empty;

                    //Getting the CPU usage via a PerformanceCounter.
                    var cpuCounter = new PerformanceCounter("Processor", "% Processor Time",
                        "_Total");
                    cpuCounter.NextValue();

                    // Wait a second to get a valid reading.
                    Thread.Sleep(1000);
                    var usage = cpuCounter.NextValue();

                    message = String.Format("{0} CPU: {1:0.0} %", curProcess, usage);

                    wrkr.ReportProgress(0, message);
                }
            };

            // Display CPU usage on window title.
            wrkr.ProgressChanged += (object sender, ProgressChangedEventArgs e) => {
                var val = e.UserState as string;

                if (string.IsNullOrEmpty(val))
                    return;

                this.MainWindowApp.Title = val;
            };

            wrkr.RunWorkerAsync();
        }

        /// <summary>
        /// Create <see cref="ProcessInfo"/> from the process inforamtion.
        /// </summary>
        private void CreateProcessesInfo()
        {
            // Process the processes and add into collection.
            foreach (var process in Process.GetProcesses())
            {
                // Create ProcessInfo class and handle exceptions.
                var processInfo = new ProcessInfo();
                try
                {
                    processInfo.ProcessName         = process.ProcessName;
                    processInfo.Id                  = process.Id;
                    processInfo.PhysicalMemoryUsage = process.WorkingSet64;
                    processInfo.BasePriority        = process.BasePriority;

                    // Additional checking for system properties.
                    try
                    {
                        processInfo.PriorityClass           = process.PriorityClass;
                        processInfo.UserProcessorTime       = process.UserProcessorTime;
                        processInfo.PrivilegedProcessorTime = process.PrivilegedProcessorTime;
                        processInfo.TotalProcessorTime      = process.TotalProcessorTime;
                    }
                    catch (Exception)
                    {
                        processInfo.PriorityClass           = ProcessPriorityClass.Normal;
                        processInfo.UserProcessorTime       = TimeSpan.Zero;
                        processInfo.PrivilegedProcessorTime = TimeSpan.Zero;
                        processInfo.TotalProcessorTime      = TimeSpan.Zero;
                    }

                    processInfo.PagedSystemMemorySize64 = process.PagedSystemMemorySize64;
                    processInfo.PagedMemorySize64       = process.PagedMemorySize64;
                    processInfo.PrivateMemorySize64     = process.PrivateMemorySize64;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to get info about {process.ProcessName}." +
                            $" {ex.Message}",
                        "TaskKiller");
                }

                // Add process info to list.
                ListProcesses.Add(processInfo);
            }
        }

        #endregion
    }
}

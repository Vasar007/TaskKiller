using System;
using System.Diagnostics;

namespace TaskKiller
{
    /// <summary>
    /// Сlass that contains all available information about the process.
    /// </summary>
    public class ProcessInfo
    {
        #region Public Properties

        /// <summary>
        /// The name that the system uses to identify the process for the user.
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        /// System-generated unique identifier that is referenced by this instance of the 
        /// <see cref="System.Diagnostics.Process"/>.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The amount of physical memory, in bytes, allocated for the associated process.
        /// </summary>
        public long PhysicalMemoryUsage { get; set; }

        /// <summary>
        /// The base priority, which is computed by the 
        /// <see cref="System.Diagnostics.Process.PriorityClass"/>
        /// the associated process.
        /// </summary>
        public int BasePriority { get; set; }

        /// <summary>
        /// Category of priority for the associated process, from which is calculated 
        /// <see cref="System.Diagnostics.Process.BasePriority"/>.
        /// </summary>
        public ProcessPriorityClass PriorityClass { get; set; }

        /// <summary>
        /// Showing the amount of time that the associated process has spent running code inside
        /// the application (not inside a kernel).
        /// </summary>
        public TimeSpan UserProcessorTime { get; set; }

        /// <summary>
        /// Time that indicates the amount of time the process has spent running code inside
        /// the operating system kernel.
        /// </summary>
        public TimeSpan PrivilegedProcessorTime { get; set; }

        /// <summary>
        /// Time that indicates the amount of time spent by process on CPU. This value is the 
        /// sum of the values of properties 
        /// <see cref="System.Diagnostics.Process.UserProcessorTime"/> and 
        /// <see cref="System.Diagnostics.Process.PrivilegedProcessorTime"/>.
        /// </summary>
        public TimeSpan TotalProcessorTime { get; set; }

        /// <summary>
        /// The amount of system memory, in bytes, allocated for the associated process that 
        /// can be written to the paging file of virtual memory.
        /// </summary>
        public long PagedSystemMemorySize64 { get; set; }

        /// <summary>
        /// The amount of memory in bytes, allocated in the paging file virtual memory for 
        /// the associated process.
        /// </summary>
        public long PagedMemorySize64 { get; set; }

        /// <summary>
        /// The amount of memory in bytes, allocated for the associated process that
        /// cannot be made available to other processes.
        /// </summary>
        public long PrivateMemorySize64 { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ProcessInfo()
        {
        }

        /// <summary>
        /// Constructor with parameters.
        /// </summary>
        public ProcessInfo(string processName, int id, long workingSet64, int basePriority,
            ProcessPriorityClass priorityClass, TimeSpan userProcessorTime, 
            TimeSpan privilegedProcessorTime, TimeSpan totalProcessorTime,
            long pagedSystemMemorySize64, long pagedMemorySize64, long privateMemorySize64)
        {
            this.ProcessName = processName;
            this.Id = id;
            this.PhysicalMemoryUsage = workingSet64;
            this.BasePriority = basePriority;
            this.PriorityClass = priorityClass;
            this.UserProcessorTime = userProcessorTime;
            this.PrivilegedProcessorTime = privilegedProcessorTime;
            this.TotalProcessorTime = totalProcessorTime;
            this.PagedSystemMemorySize64 = pagedSystemMemorySize64;
            this.PagedMemorySize64 = pagedMemorySize64;
            this.PrivateMemorySize64 = privateMemorySize64;
        }

        #endregion
    }
}

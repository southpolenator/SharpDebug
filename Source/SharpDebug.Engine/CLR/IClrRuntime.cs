using System;
using System.Collections.Generic;

namespace SharpDebug.CLR
{
    /// <summary>
    /// CLR code Runtime interface. This is valid only if there is CLR loaded into debugging process.
    /// </summary>
    public interface IClrRuntime
    {
        /// <summary>
        /// Gets the process.
        /// </summary>
        Process Process { get; }

        /// <summary>
        /// Gets the CLR version.
        /// </summary>
        ModuleVersion Version { get; }

        /// <summary>
        /// Gets the array of application domains in the process. Note that System AppDomain and Shared AppDomain are omitted.
        /// </summary>
        IClrAppDomain[] AppDomains { get; }

        /// <summary>
        /// Gets the enumeration of all application domains in the process including System and Shared AppDomain.
        /// </summary>
        IEnumerable<IClrAppDomain> AllAppDomains { get; }

        /// <summary>
        /// Gets the Shared AppDomain.
        /// </summary>
        IClrAppDomain SharedDomain { get; }

        /// <summary>
        /// Gets the System AppDomain.
        /// </summary>
        IClrAppDomain SystemDomain { get; }

        /// <summary>
        /// Gets the array of all modules loaded in the runtime.
        /// </summary>
        IClrModule[] Modules { get; }

        /// <summary>
        /// Gets the array of all managed threads in the process. Only threads which have previously run managed code will be enumerated.
        /// </summary>
        IClrThread[] Threads { get; }

        /// <summary>
        /// Gets the array of GC threads in the runtime.
        /// </summary>
        IClrThread[] GCThreads { get; }

        /// <summary>
        /// Gets the number of logical GC heaps in the process. This is always 1 for a workstation GC,
        /// and usually it's the number of logical processors in a server GC application.
        /// </summary>
        int HeapCount { get; }

        /// <summary>
        /// Gets a value indicating whether the process is running in server GC mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the process is running in server GC mode; otherwise, <c>false</c>.
        /// </value>
        bool ServerGC { get; }

        /// <summary>
        /// Gets the GC heap of the process.
        /// </summary>
        IClrHeap Heap { get; }

        /// <summary>
        /// Gets the AppDomain specified by the name. If multiple AppDomains have same name, it will throw exception.
        /// If there is no such AppDomain, it will return null.
        /// </summary>
        /// <param name="appDomainName">The application domain name.</param>
        IClrAppDomain GetAppDomainByName(string appDomainName);

        /// <summary>
        /// Gets the module by the file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        IClrModule GetModuleByFileName(string fileName);

        /// <summary>
        /// Reads the name of the source file, line and displacement of the specified code address.
        /// </summary>
        Tuple<string, uint, ulong> ReadSourceFileNameAndLine(ulong address);

        /// <summary>
        /// Reads the function name and displacement of the specified code address.
        /// </summary>
        Tuple<string, ulong> ReadFunctionNameAndDisplacement(ulong address);
    }
}

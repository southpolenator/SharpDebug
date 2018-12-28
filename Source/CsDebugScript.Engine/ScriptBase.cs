using System;
using System.Linq;
using System.Dynamic;
using System.Collections.Generic;
using CsDebugScript.Drawing.Interfaces;
using CsDebugScript.Engine;

namespace CsDebugScript
{
    /// <summary>
    /// Base class for all C# scripts.
    /// </summary>
    public class ScriptBase
    {
        /// <summary>
        /// Helper class for making Modules be dynamic object inside the scripts.
        /// </summary>
        private class ModulesDynamicObject : DynamicObject
        {
            /// <summary>
            /// Tries the get member.
            /// </summary>
            /// <param name="binder">The binder.</param>
            /// <param name="result">The result.</param>
            /// <returns><c>true</c> if member is found; <c>false</c> otherwise</returns>
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                Module module = Module.All.FirstOrDefault(m => string.Compare(binder.Name, m.Name, true) == 0);

                if (module != null)
                {
                    result = new ModuleGlobalsDynamicObject(module);
                    return true;
                }

                result = null;
                return false;
            }
        }

        /// <summary>
        /// Helper class for making one Module be dynamic object inside the scripts (after getting it from Modules dynamic object).
        /// </summary>
        private class ModuleGlobalsDynamicObject : DynamicObject
        {
            /// <summary>
            /// The module
            /// </summary>
            private Module module;

            /// <summary>
            /// Initializes a new instance of the <see cref="ModuleGlobalsDynamicObject"/> class.
            /// </summary>
            /// <param name="module">The module.</param>
            public ModuleGlobalsDynamicObject(Module module)
            {
                this.module = module;
            }

            /// <summary>
            /// Performs an implicit conversion from <see cref="ModuleGlobalsDynamicObject"/> to <see cref="Module"/>.
            /// </summary>
            /// <param name="helper">The helper.</param>
            /// <returns>
            /// The result of the conversion.
            /// </returns>
            public static implicit operator Module(ModuleGlobalsDynamicObject helper)
            {
                return helper.module;
            }

            /// <summary>
            /// Tries the get member.
            /// </summary>
            /// <param name="binder">The binder.</param>
            /// <param name="result">The result.</param>
            /// <returns><c>true</c> if member is found; <c>false</c> otherwise</returns>
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                try
                {
                    result = module.GetVariable(binder.Name);
                    return true;
                }
                catch (Exception)
                {
                    var property = module.GetType().GetProperty(binder.Name);

                    if (property != null)
                    {
                        result = property.GetValue(module);
                        return true;
                    }

                    result = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// Helper class for making Globals be dynamic object inside the scripts.
        /// </summary>
        private class GlobalsDynamicObject : DynamicObject
        {
            /// <summary>
            /// Tries the get member.
            /// </summary>
            /// <param name="binder">The binder.</param>
            /// <param name="result">The result.</param>
            /// <returns><c>true</c> if member is found; <c>false</c> otherwise</returns>
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                try
                {
                    result = Process.Current.GetGlobal(binder.Name);
                    return true;
                }
                catch (Exception)
                {
                    result = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// Helper class for making <see cref="VariableCollection"/> be dynamic object inside the scripts.
        /// </summary>
        private abstract class VariableCollectionDynamicObject : DynamicObject
        {
            /// <summary>
            /// Gets the <see cref="VariableCollection"/> object to be represented as dynamic.
            /// </summary>
            protected abstract VariableCollection GetVariableCollection();

            /// <summary>
            /// Provides the implementation for operations that get member values. Classes derived
            /// from the System.Dynamic.DynamicObject class can override this method to specify
            /// dynamic behavior for operations such as getting a value for a property.
            /// </summary>
            /// <param name="binder">Provides information about the object that called the dynamic operation. The
            /// <code>binder.Name</code> property provides the name of the member on which the dynamic operation
            /// is performed. For example, for the <code>Console.WriteLine(sampleObject.SampleProperty)</code>
            /// statement, where sampleObject is an instance of the class derived from the <see cref="System.Dynamic.DynamicObject"/>
            /// class, <code>binder.Name</code> returns "SampleProperty". The <code>binder.IgnoreCase</code> property specifies
            /// whether the member name is case-sensitive.</param>
            /// <param name="result">The result of the get operation. For example, if the method is called for a property,
            /// you can assign the property value to result.</param>
            /// <returns>
            /// <c>true</c> if the operation is successful; otherwise, <c>false</c>. If this method returns
            /// <c>false</c>, the run-time binder of the language determines the behavior. (In most
            /// cases, a run-time exception is thrown.)
            /// </returns>
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                VariableCollection variableCollection = GetVariableCollection();

                if (variableCollection.ContainsName(binder.Name))
                {
                    result = variableCollection[binder.Name];
                    return true;
                }

                result = null;
                return false;
            }

            /// <summary>
            /// Provides the implementation for operations that get a value by index. Classes
            /// derived from the System.Dynamic.DynamicObject class can override this method
            /// to specify dynamic behavior for indexing operations.
            /// </summary>
            /// <param name="binder">Provides information about the operation.</param>
            /// <param name="indexes">The indexes that are used in the operation. For example, for the sampleObject[3]
            /// operation in C# (sampleObject(3) in Visual Basic), where sampleObject is derived
            /// from the DynamicObject class, indexes[0] is equal to 3.</param>
            /// <param name="result">The result of the index operation.</param>
            /// <returns>
            /// <c>true</c> if the operation is successful; otherwise, <c>false</c>. If this method returns
            /// <c>false</c>, the run-time binder of the language determines the behavior. (In most
            /// cases, a run-time exception is thrown.)
            /// </returns>
            public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
            {
                if (indexes.Length == 1 && indexes[0] is string)
                {
                    result = GetVariableCollection()[indexes[0].ToString()];
                    return true;
                }

                return base.TryGetIndex(binder, indexes, out result);
            }

            /// <summary>
            /// Returns the enumeration of all dynamic member names.
            /// </summary>
            /// <returns>A sequence that contains dynamic member names.</returns>
            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return GetVariableCollection().Names;
            }
        }

        /// <summary>
        /// Helper class for making <see cref="Arguments"/> be dynamic object inside the scripts.
        /// </summary>
        private class ArgumentsDynamicObject : VariableCollectionDynamicObject
        {
            /// <summary>
            /// Gets the <see cref="VariableCollection"/> object to be represented as dynamic.
            /// </summary>
            protected override VariableCollection GetVariableCollection()
            {
                return StackFrame.Current.Arguments;
            }
        }

        /// <summary>
        /// Helper class for making <see cref="Locals"/> be dynamic object inside the scripts.
        /// </summary>
        private class LocalsDynamicObject : VariableCollectionDynamicObject
        {
            /// <summary>
            /// Gets the <see cref="VariableCollection"/> object to be represented as dynamic.
            /// </summary>
            protected override VariableCollection GetVariableCollection()
            {
                return StackFrame.Current.Locals;
            }
        }

        /// <summary>
        /// The Modules dynamic object. You can use this dynamic variable to easily access Modules and afterwards global variables.
        /// </summary>
        public dynamic Modules = new ModulesDynamicObject();

        /// <summary>
        /// The Globals dynamic object. You can use this dynamic variable to easily access global variables.
        /// </summary>
        public dynamic Globals = new GlobalsDynamicObject();

        /// <summary>
        /// Gets the array of all processes being debugged.
        /// </summary>
        public static Process[] Processes => Process.All;

        /// <summary>
        /// Gets the array of all threads in the current process.
        /// </summary>
        public static Thread[] Threads => Process.Current.Threads;

        /// <summary>
        /// Gets the array of all frames in the current thread.
        /// </summary>
        public static StackFrame[] Frames => StackTrace.Current.Frames;

        /// <summary>
        /// Gets the variable collection of arguments from current stack frame.
        /// </summary>
        public static dynamic Arguments { get; private set; } = new ArgumentsDynamicObject();

        /// <summary>
        /// Gets the variable collection of local variables from current stack frame.
        /// </summary>
        public static dynamic Locals { get; private set; } = new LocalsDynamicObject();

        /// <summary>
        /// Gets graphics object used for creating drawing objects.
        /// </summary>
        public static IGraphics Graphics => Context.Graphics;

        #region Console helpers
        /// <summary>
        /// Helper function for writing onto console (shorter version for scripts).
        /// </summary>
        /// <param name="obj">The object.</param>
        public static void write(object obj)
        {
            Console.Write(obj);
        }

        /// <summary>
        /// Helper function for writing onto console (shorter version for scripts).
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void write(string format, params object[] args)
        {
            Console.Write(format, args);
        }

        /// <summary>
        /// Helper function for writing onto console (shorter version for scripts).
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="objects">The objects.</param>
        public static void write<T>(IEnumerable<T> objects)
        {
            Console.Write(string.Join(", ", objects));
        }

        /// <summary>
        /// Helper function for writing onto console (shorter version for scripts).
        /// </summary>
        /// <param name="obj">The object.</param>
        public static void Write(object obj)
        {
            Console.Write(obj);
        }

        /// <summary>
        /// Helper function for writing onto console (shorter version for scripts).
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void Write(string format, params object[] args)
        {
            Console.Write(format, args);
        }

        /// <summary>
        /// Helper function for writing onto console (shorter version for scripts).
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="objects">The objects.</param>
        public static void Write<T>(IEnumerable<T> objects)
        {
            Console.Write(string.Join(", ", objects));
        }

        /// <summary>
        /// Helper function for writing line onto console (shorter version for scripts).
        /// </summary>
        /// <param name="obj">The object.</param>
        public static void writeln(object obj)
        {
            Console.WriteLine(obj);
        }

        /// <summary>
        /// Helper function for writing line onto console (shorter version for scripts).
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void writeln(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        /// <summary>
        /// Helper function for writing line onto console (shorter version for scripts).
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="objects">The objects.</param>
        public static void writeln<T>(IEnumerable<T> objects)
        {
            Console.WriteLine(string.Join(", ", objects));
        }

        /// <summary>
        /// Helper function for writing line onto console (shorter version for scripts).
        /// </summary>
        public static void writeln()
        {
            Console.WriteLine();
        }

        /// <summary>
        /// Helper function for writing line onto console (shorter version for scripts).
        /// </summary>
        /// <param name="obj">The object.</param>
        public static void WriteLine(object obj)
        {
            Console.WriteLine(obj);
        }

        /// <summary>
        /// Helper function for writing line onto console (shorter version for scripts).
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        /// <summary>
        /// Helper function for writing line onto console (shorter version for scripts).
        /// </summary>
        /// <typeparam name="T">Enumeration type</typeparam>
        /// <param name="objects">The objects.</param>
        public static void WriteLine<T>(IEnumerable<T> objects)
        {
            Console.WriteLine(string.Join(", ", objects));
        }

        /// <summary>
        /// Helper function for writing line onto console (shorter version for scripts).
        /// </summary>
        public static void WriteLine()
        {
            Console.WriteLine();
        }
        #endregion

        #region Graphics helpers
        /// <summary>
        /// Creates <see cref="IBitmap"/> with pixel type read from data element type.
        /// </summary>
        /// <remarks>
        /// If <paramref name="data"/> is <see cref="Variable"/> of <see cref="CodeType"/> <code>unsigned char*</code>,
        /// then pixel type is read as <code>byte</code>.
        /// </remarks>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="data">Bitmap data.</param>
        /// <param name="channels">Description of the image channels.</param>
        /// <param name="stride">Bitmap stride.</param>
        /// <returns>Instance of the <see cref="IBitmap"/> object.</returns>
        public IBitmap CreateImage(dynamic width, dynamic height, dynamic data, ChannelType[] channels, dynamic stride = null)
        {
            int imageWidth = Convert.ToInt32(width);
            int imageHeight = Convert.ToInt32(width);
            int dataStride = stride == null ? -1 : Convert.ToInt32(stride);
            Variable dataVariable = data as Variable;
            BuiltinType dataType;
            NakedPointer dataPointer;

            if (dataVariable != null)
            {
                dataPointer = new NakedPointer(dataVariable.GetCodeType().Module.Process, dataVariable.GetPointerAddress());
                try
                {
                    dataType = dataVariable.GetCodeType().ElementType.BuiltinType;
                }
                catch
                {
                    dataType = dataVariable.GetCodeType().BuiltinType;
                }
            }
            else
            {
                // Consider data as memory pointer and data type as byte
                dataPointer = new NakedPointer(Convert.ToUInt64(data));
                dataType = BuiltinType.UInt8;
            }

            switch (dataType)
            {
                case BuiltinType.Float32:
                    return Graphics.CreateBitmap(imageWidth, imageHeight, channels, ReadPixels<float>(imageWidth, imageHeight, dataPointer, dataStride, channels.Length));
                case BuiltinType.Float64:
                    return Graphics.CreateBitmap(imageWidth, imageHeight, channels, ReadPixels<double>(imageWidth, imageHeight, dataPointer, dataStride, channels.Length));
                case BuiltinType.Int8:
                    return Graphics.CreateBitmap(imageWidth, imageHeight, channels, ReadPixels<sbyte>(imageWidth, imageHeight, dataPointer, dataStride, channels.Length));
                case BuiltinType.Int16:
                    return Graphics.CreateBitmap(imageWidth, imageHeight, channels, ReadPixels<short>(imageWidth, imageHeight, dataPointer, dataStride, channels.Length));
                case BuiltinType.Int32:
                    return Graphics.CreateBitmap(imageWidth, imageHeight, channels, ReadPixels<int>(imageWidth, imageHeight, dataPointer, dataStride, channels.Length));
                case BuiltinType.NoType:
                case BuiltinType.Char8:
                case BuiltinType.Void:
                case BuiltinType.UInt8:
                    return Graphics.CreateBitmap(imageWidth, imageHeight, channels, ReadPixels<byte>(imageWidth, imageHeight, dataPointer, dataStride, channels.Length));
                case BuiltinType.UInt16:
                    return Graphics.CreateBitmap(imageWidth, imageHeight, channels, ReadPixels<ushort>(imageWidth, imageHeight, dataPointer, dataStride, channels.Length));
                default:
                    throw new NotImplementedException($"Unknown image data type: {dataType}");
            }
        }

        /// <summary>
        /// Creates <see cref="IBitmap"/> with the specified pixel type.
        /// </summary>
        /// <typeparam name="PixelType">Type of the pixel.</typeparam>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="data">Bitmap data.</param>
        /// <param name="channels">Description of the image channels.</param>
        /// <param name="stride">Bitmap stride.</param>
        /// <returns>Instance of the <see cref="IBitmap"/> object.</returns>
        public IBitmap CreateImage<PixelType>(dynamic width, dynamic height, dynamic data, ChannelType[] channels, dynamic stride = null)
        {
            int imageWidth = Convert.ToInt32(width);
            int imageHeight = Convert.ToInt32(width);
            int dataStride = stride == null ? -1 : Convert.ToInt32(stride);
            Variable dataVariable = data as Variable;
            NakedPointer dataPointer;

            if (dataVariable != null)
                dataPointer = new NakedPointer(dataVariable.GetCodeType().Module.Process, dataVariable.GetPointerAddress());
            else
                dataPointer = new NakedPointer(Convert.ToUInt64(data));

            // pixels will be PixelType[], but if we use dynamic, compiler will do the magic to call the right function.
            dynamic pixels = ReadPixels<PixelType>(imageWidth, imageHeight, dataPointer, dataStride, channels.Length);

            return Graphics.CreateBitmap(imageWidth, imageHeight, channels, pixels);
        }

        /// <summary>
        /// Reads pixels data into single array of pixels with new stride equal to <paramref name="width"/>.
        /// </summary>
        /// <typeparam name="T">Type of the pixel.</typeparam>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="data">Pointer to start of bitmap data.</param>
        /// <param name="stride">Row stride in bytes.</param>
        /// <param name="channels">Number of channels in the image.</param>
        /// <returns>Array of image pixels.</returns>
        private static T[] ReadPixels<T>(int width, int height, NakedPointer data, int stride, int channels)
        {
            int pixelByteSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();

            if (stride <= 0 || stride == pixelByteSize * channels * width)
            {
                return new CodeArray<T>(data, width * height * channels).ToArray();
            }
            else
            {
                T[] result = new T[width * height * channels];
                int rowElements = width * channels;

                for (int y = 0, j = 0; y < height; y++)
                {
                    CodeArray<T> array = new CodeArray<T>(data.AdjustPointer(stride * y), rowElements);

                    for (int x = 0; x < rowElements; x++, j++)
                    {
                        result[j] = array[x];
                    }
                }

                return result;
            }
        }
        #endregion
    }
}

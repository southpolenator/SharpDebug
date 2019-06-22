﻿using System;
using System.Collections.Generic;
using System.Linq;
using SharpDebug.Drawing.Interfaces;
using SharpDebug.Engine;
using Microsoft.CodeAnalysis.Scripting;

namespace SharpDebug
{
    /// <summary>
    /// Helper for dumping objects using InteractiveScriptBase.ObjectWriter.
    /// </summary>
    public static class InteractiveScriptBaseExtensions
    {
        /// <summary>
        /// Outputs the specified object using InteractiveScriptBase.ObjectWriter.
        /// </summary>
        /// <param name="obj">The object.</param>
        public static void Dump(this object obj)
        {
            InteractiveScriptBase interactiveScript = InteractiveScriptBase.Current;

            if (interactiveScript == null)
            {
                throw new NotImplementedException("Calling Dump() is only supported while using interactive scripting");
            }

            interactiveScript.Dump(obj);
        }
    }

    /// <summary>
    /// Base class for interactive script commands
    /// </summary>
    public class InteractiveScriptBase : ScriptBase
    {
        /// <summary>
        /// Gets the interactive script base of the script that is currently executing.
        /// </summary>
        public static InteractiveScriptBase Current { get; internal set; }

        /// <summary>
        /// Gets or sets the object writer using in interactive scripting mode.
        /// </summary>
        public IObjectWriter ObjectWriter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether interactive commands/scripts will be executed in STA thread.
        /// </summary>
        /// <value>
        ///   <c>true</c> if force STA thread execution; otherwise, <c>false</c>.
        /// </value>
        public bool ForceStaExecution { get; set; }

        /// <summary>
        /// Gets or sets the internal object writer. It is used for writing objects to host window.
        /// </summary>
        internal IObjectWriter _InternalObjectWriter_ { get; set; }

        /// <summary>
        /// Gets or sets the UI action executor.
        /// </summary>
        internal Action<Action> _UiActionExecutor_ { get; set; }

        /// <summary>
        /// Stops interactive scripting execution. You can use this simply by entering it as command in interactive scripting mode.
        /// </summary>
        public object quit
        {
            get
            {
                throw new ExitRequestedException();
            }
        }

        /// <summary>
        /// Stops interactive scripting execution. You can use this simply by entering it as command in interactive scripting mode.
        /// </summary>
        public object q
        {
            get
            {
                return quit;
            }
        }

        /// <summary>
        /// Stops interactive scripting execution. You can use this simply by entering it as command in interactive scripting mode.
        /// </summary>
        public object exit
        {
            get
            {
                return quit;
            }
        }

        /// <summary>
        /// The interactive script base type for next compile iteration
        /// </summary>
        internal Type _InteractiveScriptBaseType_;

        /// <summary>
        /// The Roslyn script state
        /// </summary>
        internal ScriptState<object> _ScriptState_;

        /// <summary>
        /// The list of CodeGen generated code.
        /// </summary>
        internal List<ImportUserTypeCode> _CodeGenCode_;

        /// <summary>
        /// The list of CodeGen generated assemblies.
        /// </summary>
        internal List<ImportUserTypeAssembly> _CodeGenAssemblies_;

        /// <summary>
        /// The code resolver used for generating code with CodeGen.
        /// </summary>
        internal ScriptExecution.SourceResolver _CodeResolver_;

        /// <summary>
        /// The assembly resolver used for generating assemblies with CodeGen.
        /// </summary>
        internal ScriptExecution.MetadataResolver _AssemblyResolver_;

        /// <summary>
        /// Extracted user type metadata from the running assembly. This is used during '#reset' command.
        /// </summary>
        internal List<UserTypeMetadata> _ExtractedUserTypeMetadata_ = new List<UserTypeMetadata>();

        /// <summary>
        /// Outputs the specified object using ObjectWriter.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Dump(object obj)
        {
            obj = ObjectWriter.Output(obj);
            _InternalObjectWriter_.Output(obj);
        }

        private IEnumerable<string> GetCommands(Type type, System.Reflection.BindingFlags additionalBinding, string nameFilter = "")
        {
            var methods = type.GetMethods(System.Reflection.BindingFlags.Public | additionalBinding | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);

            nameFilter = nameFilter.ToLower();
            foreach (var method in methods)
            {
                if (method.DeclaringType != type || method.IsSpecialName)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(nameFilter) || method.Name.ToLower().Contains(nameFilter))
                {
                    yield return method.ToString();
                }
            }
        }

        /// <summary>
        /// Gets the available commands.
        /// </summary>
        /// <param name="nameFilter">The name filter.</param>
        /// <returns>Enumeration of available commands</returns>
        public IEnumerable<string> GetCommands(string nameFilter = "")
        {
            Type type = GetType();

            while (type != null)
            {
                foreach (var command in GetCommands(type, type == GetType() ? System.Reflection.BindingFlags.NonPublic : System.Reflection.BindingFlags.Default, nameFilter))
                {
                    yield return command;
                }

                type = type.BaseType;
            }
        }

        /// <summary>
        /// Lists the available commands.
        /// </summary>
        /// <param name="nameFilter">The name filter.</param>
        /// <param name="signatureFilter">The signature filter.</param>
        public void ListCommands(string nameFilter = "", string signatureFilter = "")
        {
            var commands = GetCommands(nameFilter);

            if (!string.IsNullOrEmpty(signatureFilter))
            {
                signatureFilter = signatureFilter.ToLower();
                commands = commands.Where(c => c.ToLower().Contains(signatureFilter));
            }

            foreach (var command in commands)
            {
                Console.WriteLine(command);
            }
        }

        /// <summary>
        /// Gets the available commands including all base classes.
        /// </summary>
        /// <param name="nameFilter">The name filter.</param>
        /// <returns>Enumeration of available commands.</returns>
        public IEnumerable<string> GetAllCommands(string nameFilter = "")
        {
            Type type = GetType();

            while (type != typeof(object))
            {
                var methods = type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);

                nameFilter = nameFilter.ToLower();
                foreach (var method in methods)
                {
                    if (method.DeclaringType != type)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(nameFilter) || method.Name.ToLower().Contains(nameFilter))
                    {
                        yield return method.ToString();
                    }
                }

                type = type.BaseType;
            }
        }

        /// <summary>
        /// Lists the available commands.
        /// </summary>
        /// <param name="nameFilter">The name filter.</param>
        /// <param name="signatureFilter">The signature filter.</param>
        public void ListAllCommands(string nameFilter = "", string signatureFilter = "")
        {
            var commands = GetAllCommands(nameFilter);

            if (!string.IsNullOrEmpty(signatureFilter))
            {
                signatureFilter = signatureFilter.ToLower();
                commands = commands.Where(c => c.ToLower().Contains(signatureFilter));
            }

            foreach (var command in commands)
            {
                Console.WriteLine(command);
            }
        }

        /// <summary>
        /// Gets stored variables in current interactive execution.
        /// </summary>
        /// <param name="nameFilter">The name filter.</param>
        public IDictionary<string, object> GetVariables(string nameFilter = "")
        {
            IEnumerable<string> variableNames = _ScriptState_.Variables.Select(v => v.Name).Distinct();

            if (!string.IsNullOrEmpty(nameFilter))
            {
                nameFilter = nameFilter.ToLower();
                variableNames = variableNames.Where(v => v.ToLower() == nameFilter);
            }

            Dictionary<string, object> variables = new Dictionary<string, object>();

            foreach (var variableName in variableNames)
            {
                variables.Add(variableName, _ScriptState_.GetVariable(variableName).Value);
            }

            return variables;
        }

        /// <summary>
        /// Lists stored variables in current interactive execution.
        /// </summary>
        /// <param name="nameFilter">The name filter.</param>
        public void ListVariables(string nameFilter = "")
        {
            foreach (var variable in GetVariables(nameFilter))
            {
                Console.WriteLine("  {0} [{1}] = {2}", variable.Key, variable.Value.GetType(), variable.Value);
            }
        }

        /// <summary>
        /// Changes the base class for interactive scripting.
        /// </summary>
        /// <typeparam name="T">Type of the new base class.</typeparam>
        public void ChangeBaseClass<T>()
            where T : InteractiveScriptBase
        {
            ChangeBaseClass(typeof(T));
        }

        /// <summary>
        /// Changes the base class for interactive scripting.
        /// </summary>
        /// <param name="newBaseClassType">Type of the new base class.</param>
        /// <remarks>Base class type must inherit InteractiveScriptBase.</remarks>
        public void ChangeBaseClass(Type newBaseClassType)
        {
            if (typeof(InteractiveScriptBase).IsAssignableFrom(newBaseClassType))
            {
                _InteractiveScriptBaseType_ = newBaseClassType;
            }
            else
            {
                throw new ArgumentException(nameof(newBaseClassType));
            }
        }

        /// <summary>
        /// Executes the specified action in UI thread.
        /// </summary>
        /// <param name="action">The action.</param>
        public void ExecuteInUiThread(Action action)
        {
            if (_UiActionExecutor_ != null)
            {
                _UiActionExecutor_(action);
            }
            else
            {
                action();
            }
        }

        /// <summary>
        /// Imports user types from modules using the specified importing options.
        /// </summary>
        /// <param name="options">The importing options.</param>
        /// <param name="asAssembly">If set to <c>true</c> user types will be imported as assembly. If set to <c>false</c> user types will be imported as script code.</param>
        public void ImportUserTypes(ImportUserTypeOptions options, bool asAssembly = false)
        {
            if (asAssembly)
            {
                ImportUserTypeAssembly assembly = _AssemblyResolver_.GenerateAssembly(options);

                _CodeGenAssemblies_.Add(assembly);
            }
            else
            {
                ImportUserTypeCode code = _CodeResolver_.GenerateCode(options);

                _CodeGenCode_.Add(code);
            }
        }

        /// <summary>
        /// Imports user types to be available for automatic user type casting.
        /// </summary>
        /// <param name="assembly">The assembly containing user types.</param>
        public void __ImportUserTypes__(System.Reflection.Assembly assembly)
        {
            _ExtractedUserTypeMetadata_.AddRange(ScriptCompiler.ExtractMetadata(assembly));
        }

        #region Graphics helpers
        /// <summary>
        /// Draws image on screen with pixel type read from data element type.
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
        public void DrawImage(dynamic width, dynamic height, dynamic data, ChannelType[] channels, dynamic stride = null)
        {
            Dump(CreateImage(width, height, data, channels, stride));
        }

        /// <summary>
        /// Draws image on screen with the specified pixel type.
        /// </summary>
        /// <typeparam name="PixelType">Type of the pixel.</typeparam>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="data">Bitmap data.</param>
        /// <param name="channels">Description of the image channels.</param>
        /// <param name="stride">Bitmap stride.</param>
        public void DrawImage<PixelType>(dynamic width, dynamic height, dynamic data, ChannelType[] channels, dynamic stride = null)
        {
            Dump(CreateImage<PixelType>(width, height, data, channels, stride));
        }

        /// <summary>
        /// Draws RGB image on screen with pixel type read from data element type.
        /// </summary>
        /// <remarks>
        /// If <paramref name="data"/> is <see cref="Variable"/> of <see cref="CodeType"/> <code>unsigned char*</code>,
        /// then pixel type is read as <code>byte</code>.
        /// </remarks>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="data">Bitmap data.</param>
        /// <param name="stride">Bitmap stride.</param>
        public void DrawRgbImage(dynamic width, dynamic height, dynamic data, dynamic stride = null)
        {
            Dump(CreateImage(width, height, data, Channels.RGB, stride));
        }

        /// <summary>
        /// Draws RGB image on screen with the specified pixel type.
        /// </summary>
        /// <typeparam name="PixelType">Type of the pixel.</typeparam>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="data">Bitmap data.</param>
        /// <param name="stride">Bitmap stride.</param>
        public void DrawRgbImage<PixelType>(dynamic width, dynamic height, dynamic data, dynamic stride = null)
        {
            Dump(CreateImage<PixelType>(width, height, data, Channels.RGB, stride));
        }

        /// <summary>
        /// Draws RGBA image on screen with pixel type read from data element type.
        /// </summary>
        /// <remarks>
        /// If <paramref name="data"/> is <see cref="Variable"/> of <see cref="CodeType"/> <code>unsigned char*</code>,
        /// then pixel type is read as <code>byte</code>.
        /// </remarks>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="data">Bitmap data.</param>
        /// <param name="stride">Bitmap stride.</param>
        public void DrawRgbaImage(dynamic width, dynamic height, dynamic data, dynamic stride = null)
        {
            Dump(CreateImage(width, height, data, Channels.RGBA, stride));
        }

        /// <summary>
        /// Draws RGBA image on screen with the specified pixel type.
        /// </summary>
        /// <typeparam name="PixelType">Type of the pixel.</typeparam>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="data">Bitmap data.</param>
        /// <param name="stride">Bitmap stride.</param>
        public void DrawRgbaImage<PixelType>(dynamic width, dynamic height, dynamic data, dynamic stride = null)
        {
            Dump(CreateImage<PixelType>(width, height, data, Channels.RGBA, stride));
        }

        /// <summary>
        /// Draws BGR image on screen with pixel type read from data element type.
        /// </summary>
        /// <remarks>
        /// If <paramref name="data"/> is <see cref="Variable"/> of <see cref="CodeType"/> <code>unsigned char*</code>,
        /// then pixel type is read as <code>byte</code>.
        /// </remarks>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="data">Bitmap data.</param>
        /// <param name="stride">Bitmap stride.</param>
        public void DrawBgrImage(dynamic width, dynamic height, dynamic data, dynamic stride = null)
        {
            Dump(CreateImage(width, height, data, Channels.BGR, stride));
        }

        /// <summary>
        /// Draws BGR image on screen with the specified pixel type.
        /// </summary>
        /// <typeparam name="PixelType">Type of the pixel.</typeparam>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="data">Bitmap data.</param>
        /// <param name="stride">Bitmap stride.</param>
        public void DrawBgrImage<PixelType>(dynamic width, dynamic height, dynamic data, dynamic stride = null)
        {
            Dump(CreateImage<PixelType>(width, height, data, Channels.BGR, stride));
        }

        /// <summary>
        /// Draws BGRA image on screen with pixel type read from data element type.
        /// </summary>
        /// <remarks>
        /// If <paramref name="data"/> is <see cref="Variable"/> of <see cref="CodeType"/> <code>unsigned char*</code>,
        /// then pixel type is read as <code>byte</code>.
        /// </remarks>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="data">Bitmap data.</param>
        /// <param name="stride">Bitmap stride.</param>
        public void DrawBgraImage(dynamic width, dynamic height, dynamic data, dynamic stride = null)
        {
            Dump(CreateImage(width, height, data, Channels.BGRA, stride));
        }

        /// <summary>
        /// Draws BGRA image on screen with the specified pixel type.
        /// </summary>
        /// <typeparam name="PixelType">Type of the pixel.</typeparam>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="data">Bitmap data.</param>
        /// <param name="stride">Bitmap stride.</param>
        public void DrawBgraImage<PixelType>(dynamic width, dynamic height, dynamic data, dynamic stride = null)
        {
            Dump(CreateImage<PixelType>(width, height, data, Channels.BGRA, stride));
        }

        /// <summary>
        /// Draws CMYK image on screen with pixel type read from data element type.
        /// </summary>
        /// <remarks>
        /// If <paramref name="data"/> is <see cref="Variable"/> of <see cref="CodeType"/> <code>unsigned char*</code>,
        /// then pixel type is read as <code>byte</code>.
        /// </remarks>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="data">Bitmap data.</param>
        /// <param name="stride">Bitmap stride.</param>
        public void DrawCmykImage(dynamic width, dynamic height, dynamic data, dynamic stride = null)
        {
            Dump(CreateImage(width, height, data, Channels.CMYK, stride));
        }

        /// <summary>
        /// Draws CMYK image on screen with the specified pixel type.
        /// </summary>
        /// <typeparam name="PixelType">Type of the pixel.</typeparam>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="data">Bitmap data.</param>
        /// <param name="stride">Bitmap stride.</param>
        public void DrawCmykImage<PixelType>(dynamic width, dynamic height, dynamic data, dynamic stride = null)
        {
            Dump(CreateImage<PixelType>(width, height, data, Channels.CMYK, stride));
        }

        /// <summary>
        /// Draws grayscale image on screen with pixel type read from data element type.
        /// </summary>
        /// <remarks>
        /// If <paramref name="data"/> is <see cref="Variable"/> of <see cref="CodeType"/> <code>unsigned char*</code>,
        /// then pixel type is read as <code>byte</code>.
        /// </remarks>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="data">Bitmap data.</param>
        /// <param name="stride">Bitmap stride.</param>
        public void DrawGrayscaleImage(dynamic width, dynamic height, dynamic data, dynamic stride = null)
        {
            Dump(CreateImage(width, height, data, Channels.Grayscale, stride));
        }

        /// <summary>
        /// Draws grayscale image on screen with the specified pixel type.
        /// </summary>
        /// <typeparam name="PixelType">Type of the pixel.</typeparam>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="data">Bitmap data.</param>
        /// <param name="stride">Bitmap stride.</param>
        public void DrawGrayscaleImage<PixelType>(dynamic width, dynamic height, dynamic data, dynamic stride = null)
        {
            Dump(CreateImage<PixelType>(width, height, data, Channels.Grayscale, stride));
        }
        #endregion
    }
}

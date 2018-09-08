using CsDebugScript.UI.CodeWindow;
using Dynamitey;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CsDebugScript.UI.ResultVisualizers
{
    /// <summary>
    /// Visualizes all types of objects and can be used as a base class.
    /// Usually one would inherit <see cref="CustomObjectResultVisualizer"/> class
    /// and override <see cref="ResultVisualizer.ExpandedChildren"/> and <see cref="ResultVisualizer.GetValue"/>
    /// to implement custom object visualizers.
    /// </summary>
    internal class ObjectResultVisualizer : ResultVisualizer
    {
        /// <summary>
        /// Image that is used as an icon for exceptions.
        /// </summary>
        private static ImageSource ExceptionImage = CompletionData.CreateTextImage("", Brushes.Red);

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectResultVisualizer"/> class.
        /// </summary>
        /// <param name="result">Resulting object that should be visualized.</param>
        /// <param name="resultType">Type of the resulting object that should be visualized.</param>
        /// <param name="name">Name of the variable / property.</param>
        /// <param name="dataType">Data type that will be used to generate icon of the variable / property</param>
        /// <param name="interactiveResultVisualizer">Interactive result visualizer that can be used for creating UI elements.</param>
        public ObjectResultVisualizer(object result, Type resultType, string name, CompletionDataType dataType, InteractiveResultVisualizer interactiveResultVisualizer)
            : base(result, resultType, name, dataType, interactiveResultVisualizer)
        {
        }

        /// <summary>
        /// Checks if this item has child elements and should be expandable.
        /// </summary>
        public override bool IsExpandable
        {
            get
            {
                if (result != null && resultType != null)
                {
                    Type type = result.GetType();

                    return !type.IsEnum && !type.IsPrimitive;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets child elements that will be shown in [Public] group.
        /// It usualy represents public fields and properties.
        /// </summary>
        public override IEnumerable<IResultVisualizer> PublicChildren
        {
            get
            {
                if (result != null)
                {
                    Type type = result.GetType();

                    if (!type.IsPrimitive && !type.IsEnum)
                    {
                        // Non-static properties
                        var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                        foreach (var property in properties)
                        {
                            if (property.CanRead && property.GetIndexParameters().Length == 0)
                            {
                                bool shouldForceDefaultVisualizer = property.GetCustomAttributes(false).OfType<ForceDefaultVisualizerAtttribute>().Any();

                                yield return Create(GetValue(() => property.GetValue(result)), property.PropertyType, property.Name, CompletionDataType.Property, interactiveResultVisualizer, shouldForceDefaultVisualizer);
                            }
                        }

                        // Non-static fields
                        var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                        foreach (var field in fields)
                        {
                            if (!field.IsStatic && !field.Name.EndsWith(">k__BackingField"))
                            {
                                bool shouldForceDefaultVisualizer = field.GetCustomAttributes(false).OfType<ForceDefaultVisualizerAtttribute>().Any();

                                yield return Create(GetValue(() => field.GetValue(result)), field.FieldType, field.Name, CompletionDataType.Variable, interactiveResultVisualizer, shouldForceDefaultVisualizer);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets child elements that will be shown in [Internal] group.
        /// It usualy represents internal/properted/private fields and properties.
        /// </summary>
        public override IEnumerable<IResultVisualizer> NonPublicChildren
        {
            get
            {
                if (result != null)
                {
                    Type type = result.GetType();

                    if (!type.IsPrimitive && !type.IsEnum)
                    {
                        // Non-static properties
                        var properties = type.GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        foreach (var property in properties)
                        {
                            if (property.CanRead && property.GetIndexParameters().Length == 0)
                            {
                                yield return Create(GetValue(() => property.GetValue(result)), property.PropertyType, property.Name, CompletionDataType.Property, interactiveResultVisualizer);
                            }
                        }

                        // Non-static fields
                        var fields = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        foreach (var field in fields)
                        {
                            if (!field.IsStatic && !field.Name.EndsWith(">k__BackingField"))
                            {
                                yield return Create(GetValue(() => field.GetValue(result)), field.FieldType, field.Name, CompletionDataType.Variable, interactiveResultVisualizer);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets child elements that will be shown in [Static] group.
        /// It usualy represents static fields and properties.
        /// </summary>
        public override IEnumerable<IResultVisualizer> StaticChildren
        {
            get
            {
                if (result != null)
                {
                    Type type = result.GetType();

                    if (!type.IsPrimitive && !type.IsEnum)
                    {
                        // Static properties
                        var staticProperties = type.GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                        foreach (var property in staticProperties)
                        {
                            if (property.CanRead && property.GetIndexParameters().Length == 0)
                            {
                                yield return Create(GetValue(() => property.GetValue(result)), property.PropertyType, property.Name, CompletionDataType.StaticProperty, interactiveResultVisualizer);
                            }
                        }

                        // Static fields
                        var staticFields = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                        foreach (var field in staticFields)
                        {
                            if (field.IsStatic && !field.Name.EndsWith(">k__BackingField"))
                            {
                                yield return Create(GetValue(() => field.GetValue(result)), field.FieldType, field.Name, CompletionDataType.StaticVariable, interactiveResultVisualizer);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets child elements that will be shown in [Dynamic] group.
        /// It usualy represents dynamic fields and properties that can be accessed by using dynamic keyword.
        /// </summary>
        public override IEnumerable<IResultVisualizer> DynamicChildren
        {
            get
            {
                if (result != null)
                {
                    if (typeof(DynamicObject).IsAssignableFrom(result.GetType()))
                    {
                        DynamicObject dynamicObject = (DynamicObject)result;

                        foreach (string memberName in dynamicObject.GetDynamicMemberNames())
                        {
                            yield return Create(GetValue(() => Dynamic.InvokeGet(dynamicObject, memberName)), typeof(DynamicObject), memberName, CompletionDataType.Variable, interactiveResultVisualizer);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets child elements that will be shown in [Enumeration] group.
        /// It usualy represents list of items that are available in result if it is enumeration.
        /// </summary>
        public override IEnumerable<IResultVisualizer> EnumerationChildren
        {
            get
            {
                if (result != null)
                {
                    if (typeof(IEnumerable).IsAssignableFrom(result.GetType()))
                    {
                        IEnumerable enumeration = (IEnumerable)result;
                        int index = 0;

                        foreach (object value in enumeration)
                        {
                            yield return Create(value, value?.GetType(), $"[{index++}]", CompletionDataType.Variable, interactiveResultVisualizer);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Safely evaluates getValueFunction.
        /// </summary>
        /// <param name="getValueFunction">Function to be evaluated</param>
        /// <returns>It will return either result of that function or exception that was thrown.</returns>
        protected static object GetValue(Func<object> getValueFunction)
        {
            try
            {
                return getValueFunction();
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        /// <summary>
        /// Gets the string that describes value of the variable / property.
        /// </summary>
        protected override string GetTypeString()
        {
            return resultType != null ? InteractiveExecution.GetCodeName(result?.GetType() ?? resultType) : "";
        }

        /// <summary>
        /// Gets the value of the property that will be visualized.
        /// If it is not <see cref="UIElement"/>, it will be added as a string (<see cref="ResultVisualizer.ValueString"/>).
        /// </summary>
        protected override object GetValue()
        {
            if (result != null && result.GetType() == typeof(ulong))
            {
                return $"0x{result:X} ({result})";
            }
            else if (result != null && typeof(Exception).IsAssignableFrom(result.GetType()))
            {
                return interactiveResultVisualizer.CreateTextWithIcon("Exception", ExceptionImage, result.ToString(), bold: true);
            }

            return result != null ? result : "null";
        }
    }
}

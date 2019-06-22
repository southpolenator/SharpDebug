using System;

namespace SharpDebug
{
    /// <summary>
    /// Interface that implements every constant template agrument generated type by CodeGen.
    /// </summary>
    public interface ITemplateConstant
    {
    }

    /// <summary>
    /// Generic interface that implements every constant template agrument generated type by CodeGen.
    /// </summary>
    /// <typeparam name="T">Type of the constant.</typeparam>
    public interface ITemplateConstant<T> : ITemplateConstant
    {
        /// <summary>
        /// Value of the constant used as template argument.
        /// </summary>
        T Value { get; }
    }

    /// <summary>
    /// Attribute used by CodeGen to describe constant template argument type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TemplateConstantAttribute : Attribute
    {
        /// <summary>
        /// String that was used to parse template constant argument.
        /// </summary>
        public string String { get; set; }

        /// <summary>
        /// Value of the constant used as template argument.
        /// </summary>
        public object Value { get; set; }
    }

    /// <summary>
    /// Helper class to get value of the template constant argument.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TConstant"></typeparam>
    public static class TemplateConstant<TValue, TConstant>
        where TConstant : ITemplateConstant<TValue>, new()
    {
        /// <summary>
        /// Value of the constant used as template argument.
        /// </summary>
        public static readonly TValue Value = new TConstant().Value;
    }
}

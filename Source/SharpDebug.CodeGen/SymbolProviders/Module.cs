using SharpUtilities;
using System;
using System.Collections.Generic;

namespace SharpDebug.CodeGen.SymbolProviders
{
    /// <summary>
    /// Fake symbol that represents template argument constant.
    /// </summary>
    internal class ConstantSymbol : Symbol
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantSymbol"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        public ConstantSymbol(Module module)
            : base(module)
        {
        }

        #region Intentionally not implemented methods
        /// <summary>
        /// Determines whether symbol has virtual table of functions.
        /// </summary>
        public override bool HasVTable()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes the cache.
        /// </summary>
        public override void InitializeCache()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets user type base classes.
        /// </summary>
        protected override IEnumerable<Symbol> GetBaseClasses()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the element type (if symbol is array or pointer).
        /// </summary>
        protected override Symbol GetElementType()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the enumeration values.
        /// </summary>
        protected override IEnumerable<Tuple<string, string>> GetEnumValues()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets user type fields.
        /// </summary>
        protected override IEnumerable<SymbolField> GetFields()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the pointer type to this symbol.
        /// </summary>
        protected override Symbol GetPointerType()
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    /// <summary>
    /// Fake symbol that represents template argument constant as a number.
    /// </summary>
    internal class IntegralConstantSymbol : ConstantSymbol
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegralConstantSymbol"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="value">Constant value.</param>
        /// <param name="constantText">String that this constant was parsed from.</param>
        public IntegralConstantSymbol(Module module, object value, string constantText)
            : base(module)
        {
            Value = value;
            Name = constantText;
            Tag = Engine.CodeTypeTag.TemplateArgumentConstant;
        }

        /// <summary>
        /// Gets the constant value.
        /// </summary>
        public object Value { get; private set; }
    }

    /// <summary>
    /// Fake symbol that represents template argument constant as a enum.
    /// </summary>
    internal class EnumConstantSymbol : IntegralConstantSymbol
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumConstantSymbol"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="value">Constant value.</param>
        /// <param name="constantText">String that this constant was parsed from.</param>
        /// <param name="enumSymbol">Enumeration symbol.</param>
        public EnumConstantSymbol(Module module, object value, string constantText, Symbol enumSymbol)
            : base(module, value, constantText)
        {
            EnumSymbol = enumSymbol;
        }

        /// <summary>
        /// Gets the enumeration symbol.
        /// </summary>
        public Symbol EnumSymbol { get; private set; }
    }

    /// <summary>
    /// Interface represents module (set of symbols) during debugging.
    /// </summary>
    public abstract class Module
    {
        /// <summary>
        /// The public symbols cache
        /// </summary>
        private SimpleCache<HashSet<string>> publicSymbols;

        /// <summary>
        /// Cache of constant symbols.
        /// </summary>
        private DictionaryCache<string, Symbol> constantSymbols;

        /// <summary>
        /// Initializes a new instance of the <see cref="Module"/> class.
        /// </summary>
        public Module()
        {
            publicSymbols = SimpleCache.Create(() => new HashSet<string>(GetPublicSymbols()));
            constantSymbols = new DictionaryCache<string, Symbol>(CreateConstantSymbol);
        }

        /// <summary>
        /// Gets the set of public symbols.
        /// </summary>
        public HashSet<string> PublicSymbols
        {
            get
            {
                return publicSymbols.Value;
            }
        }

        /// <summary>
        /// Gets the module name.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the default namespace.
        /// </summary>
        public string Namespace { get; protected set; }

        /// <summary>
        /// Gets the global scope symbol.
        /// </summary>
        public abstract Symbol GlobalScope { get; }

        /// <summary>
        /// Finds the list of global types specified by the wildcard.
        /// </summary>
        /// <param name="nameWildcard">The type name wildcard.</param>
        public abstract Symbol[] FindGlobalTypeWildcard(string nameWildcard);

        /// <summary>
        /// Gets all types defined in the symbol.
        /// </summary>
        public abstract IEnumerable<Symbol> GetAllTypes();

        /// <summary>
        /// Gets the symbol by name from the cache.
        /// </summary>
        /// <param name="name">The symbol name.</param>
        /// <returns>Symbol if found; otherwise null.</returns>
        public abstract Symbol GetSymbol(string name);

        /// <summary>
        /// Determines whether the specified text is constant.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>
        ///   <c>true</c> if the specified text is constant; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsConstant(string text)
        {
            object value;
            Symbol enumSymbol;

            return ParseConstant(text, out value, out enumSymbol);
        }

        /// <summary>
        /// Gets constant symbol for the specified text. If text doesn't represents constant, exception will be thrown.
        /// </summary>
        /// <param name="text">Text to be parsed.</param>
        /// <returns>Symbol that represents constant expression used as template argument.</returns>
        public virtual Symbol GetConstantSymbol(string text)
        {
            return constantSymbols[text];
        }

        /// <summary>
        /// Creates constant symbol from the specified text.
        /// </summary>
        /// <param name="constantText">Text to be parsed.</param>
        /// <returns>Symbol that represents constant expression used as template argument.</returns>
        protected Symbol CreateConstantSymbol(string constantText)
        {
            object value;
            Symbol enumSymbol;

            if (!ParseConstant(constantText, out value, out enumSymbol))
                throw new NotSupportedException();

            if (enumSymbol != null)
                return new EnumConstantSymbol(this, value, constantText, enumSymbol);
            return new IntegralConstantSymbol(this, value, constantText);
        }

        /// <summary>
        /// Gets the public symbols.
        /// </summary>
        protected abstract IEnumerable<string> GetPublicSymbols();

        /// <summary>
        /// Tries to parse text to extract constant data.
        /// </summary>
        /// <param name="text">Text to be parsed.</param>
        /// <param name="value">Constant value.</param>
        /// <param name="enumSymbol">Enumeration symbol if constant is enum.</param>
        /// <returns><c>true</c> if text is parsed as constant; <c>false</c> otherwise.</returns>
        private bool ParseConstant(string text, out object value, out Symbol enumSymbol)
        {
            value = null;
            enumSymbol = null;

            if (text[0] == '(')
            {
                int index = text.LastIndexOf(')');

                if (index == -1)
                    return false;

                string castingType = text.Substring(1, index - 1);

                enumSymbol = GetSymbol(castingType);
                if (enumSymbol == null)
                    return false;
                text = text.Substring(index + 1);
            }

            if (text.EndsWith("u"))
            {
                text = text.Substring(0, text.Length - 1);

                uint result;
                bool isConstant = uint.TryParse(text, out result);

                value = result;
                return isConstant;
            }
            else if (text.EndsWith("ul"))
            {
                text = text.Substring(0, text.Length - 2);

                uint result;
                bool isConstant = uint.TryParse(text, out result);

                value = result;
                return isConstant;
            }
            else if (text.EndsWith("ull"))
            {
                text = text.Substring(0, text.Length - 3);

                ulong result;
                bool isConstant = ulong.TryParse(text, out result);

                value = result;
                return isConstant;
            }
            else if (text.EndsWith("ll"))
            {
                text = text.Substring(0, text.Length - 2);

                long result;
                bool isConstant = long.TryParse(text, out result);

                value = result;
                return isConstant;
            }

            bool boolConstant;

            if (bool.TryParse(text, out boolConstant))
            {
                value = boolConstant;
                return true;
            }

            int intConstant;

            if (int.TryParse(text, out intConstant))
            {
                value = intConstant;
                return true;
            }

            long longConstant;

            if (long.TryParse(text, out longConstant))
            {
                value = longConstant;
                return true;
            }

            ulong ulongConstant;

            if (ulong.TryParse(text, out ulongConstant))
            {
                value = ulongConstant;
                return true;
            }

            double doubleConstant;

            if (double.TryParse(text, out doubleConstant))
            {
                value = doubleConstant;
                return true;
            }

            return false;
        }
    }
}

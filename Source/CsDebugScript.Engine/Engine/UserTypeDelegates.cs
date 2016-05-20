using CsDebugScript.Engine.Utility;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace CsDebugScript.Engine
{
    /// <summary>
    /// The symbolic constructor delegate. Creates a new user type by calling constructor with signature of this delegate.
    /// </summary>
    /// <param name="variable">The variable.</param>
    /// <returns>New instance of user type</returns>
    internal delegate object SymbolicConstructorDelegate(Variable variable);

    /// <summary>
    /// The symbolic constructor delegate. Creates a new user type by calling constructor with signature of this delegate.
    /// </summary>
    /// <param name="variable">The variable.</param>
    /// <returns>New instance of user type</returns>
    internal delegate T SymbolicConstructorDelegate<T>(Variable variable);

    /// <summary>
    /// The physical constructor delegate. Creates a new user type by calling constructor with signature of this delegate.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="bufferAddress">The buffer address.</param>
    /// <param name="codeType">The variable code type.</param>
    /// <param name="address">The variable address.</param>
    /// <param name="name">The variable name.</param>
    /// <param name="path">The variable path.</param>
    /// <returns>New instance of user type</returns>
    internal delegate object PhysicalConstructorDelegate(MemoryBuffer buffer, int offset, ulong bufferAddress, CodeType codeType, ulong address, string name, string path);

    /// <summary>
    /// The physical constructor delegate. Creates a new user type by calling constructor with signature of this delegate.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="bufferAddress">The buffer address.</param>
    /// <param name="codeType">The variable code type.</param>
    /// <param name="address">The variable address.</param>
    /// <param name="name">The variable name.</param>
    /// <param name="path">The variable path.</param>
    /// <returns>New instance of user type</returns>
    internal delegate T PhysicalConstructorDelegate<T>(MemoryBuffer buffer, int offset, ulong bufferAddress, CodeType codeType, ulong address, string name, string path);

    /// <summary>
    /// Helper delegates for manipulating user types
    /// </summary>
    internal interface IUserTypeDelegates
    {
        /// <summary>
        /// Gets the symbolic constructor, or null if not available.
        /// </summary>
        SymbolicConstructorDelegate SymbolicConstructor { get; }

        /// <summary>
        /// Gets the physical constructor, or null if not available.
        /// </summary>
        PhysicalConstructorDelegate PhysicalConstructor { get; }

        /// <summary>
        /// Gets the derived class attributes.
        /// </summary>
        DerivedClassAttribute[] DerivedClassAttributes { get; }
    }

    /// <summary>
    /// Helper delegates for manipulating user types
    /// </summary>
    /// <typeparam name="T">The user type</typeparam>
    internal interface IUserTypeDelegates<T>
    {
        /// <summary>
        /// Gets the symbolic constructor, or null if not available.
        /// </summary>
        SymbolicConstructorDelegate<T> SymbolicConstructor { get; }

        /// <summary>
        /// Gets the physical constructor, or null if not available.
        /// </summary>
        PhysicalConstructorDelegate<T> PhysicalConstructor { get; }

        /// <summary>
        /// Gets the derived class attributes.
        /// </summary>
        DerivedClassAttribute[] DerivedClassAttributes { get; }
    }

    /// <summary>
    /// Helper class for getting user type delegates interface
    /// </summary>
    internal abstract class UserTypeDelegates
    {
        /// <summary>
        /// The cache of user type delegates
        /// </summary>
        public static DictionaryCache<Type, IUserTypeDelegates> Delegates = new DictionaryCache<Type, IUserTypeDelegates>(Get);

        /// <summary>
        /// Gets the delegates interface for the specified user type.
        /// </summary>
        /// <param name="userType">The user type.</param>
        private static IUserTypeDelegates Get(Type userType)
        {
            Type userTypeDelegatesType = typeof(UserTypeDelegates<>);
            userTypeDelegatesType = userTypeDelegatesType.MakeGenericType(new Type[] { userType });
            FieldInfo singletonField = userTypeDelegatesType.GetField(nameof(UserTypeDelegates<Variable>.Instance));
            return (IUserTypeDelegates)singletonField.GetValue(null);
        }
    }

    /// <summary>
    /// Implementation of helper delegates for manipulating user types
    /// </summary>
    /// <typeparam name="T">The user type</typeparam>
    internal class UserTypeDelegates<T> : IUserTypeDelegates, IUserTypeDelegates<T>
    {
        /// <summary>
        /// The singleton instance of this class
        /// </summary>
        public static IUserTypeDelegates<T> Instance = new UserTypeDelegates<T>();

        /// <summary>
        /// The physical constructor
        /// </summary>
        private PhysicalConstructorDelegate physicalConstructor = null;

        /// <summary>
        /// The typed physical constructor
        /// </summary>
        private PhysicalConstructorDelegate<T> physicalConstructorTyped = null;

        /// <summary>
        /// The symbolic constructor
        /// </summary>
        private SymbolicConstructorDelegate symbolicConstructor = null;

        /// <summary>
        /// The typed symbolic constructor
        /// </summary>
        private SymbolicConstructorDelegate<T> symbolicConstructorTyped = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTypeDelegates{T}"/> class.
        /// </summary>
        internal protected UserTypeDelegates()
        {
            // Find symbolic constructor
            var userType = typeof(T);
            var constructors = userType.GetConstructors();

            foreach (var constructor in constructors)
            {
                if (!constructor.IsPublic)
                {
                    continue;
                }

                var parameters = constructor.GetParameters();

                if (parameters.Length < 1 || parameters.Count(p => !p.HasDefaultValue) > 1)
                {
                    continue;
                }

                if (parameters[0].ParameterType == typeof(Variable))
                {
                    DynamicMethod method = new DynamicMethod("CreateIntance", userType, new Type[] { typeof(Variable) });
                    ILGenerator gen = method.GetILGenerator();

                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Newobj, constructor);
                    gen.Emit(OpCodes.Ret);
                    symbolicConstructor = (SymbolicConstructorDelegate)method.CreateDelegate(typeof(SymbolicConstructorDelegate));
                    symbolicConstructorTyped = (SymbolicConstructorDelegate<T>)method.CreateDelegate(typeof(SymbolicConstructorDelegate<T>));
                    break;
                }
            }

            // Find physical constructor
            foreach (var constructor in constructors)
            {
                if (!constructor.IsPublic)
                {
                    continue;
                }

                var parameters = constructor.GetParameters();

                if (parameters.Length < 7 || parameters.Count(p => !p.HasDefaultValue) > 7)
                {
                    continue;
                }

                if (parameters[0].ParameterType == typeof(MemoryBuffer)
                    && parameters[1].ParameterType == typeof(int)
                    && parameters[2].ParameterType == typeof(ulong)
                    && parameters[3].ParameterType == typeof(CodeType)
                    && parameters[4].ParameterType == typeof(ulong)
                    && parameters[5].ParameterType == typeof(string)
                    && parameters[6].ParameterType == typeof(string))
                {
                    DynamicMethod method = new DynamicMethod("CreateIntance", userType, new Type[] { typeof(MemoryBuffer), typeof(int), typeof(ulong), typeof(CodeType), typeof(ulong), typeof(string), typeof(string) });
                    ILGenerator gen = method.GetILGenerator();

                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Ldarg_1);
                    gen.Emit(OpCodes.Ldarg_2);
                    gen.Emit(OpCodes.Ldarg_3);
                    gen.Emit(OpCodes.Ldarg, (short)4);
                    gen.Emit(OpCodes.Ldarg, (short)5);
                    gen.Emit(OpCodes.Ldarg, (short)6);
                    gen.Emit(OpCodes.Newobj, constructor);
                    gen.Emit(OpCodes.Ret);
                    physicalConstructor = (PhysicalConstructorDelegate)method.CreateDelegate(typeof(PhysicalConstructorDelegate));
                    physicalConstructorTyped = (PhysicalConstructorDelegate<T>)method.CreateDelegate(typeof(PhysicalConstructorDelegate<T>));
                }
            }

            // Get type attributes
            DerivedClassAttributes = userType.GetCustomAttributes<DerivedClassAttribute>(false).ToArray();
        }

        /// <summary>
        /// Gets the derived class attributes.
        /// </summary>
        public DerivedClassAttribute[] DerivedClassAttributes { get; private set; }

        /// <summary>
        /// Gets the physical constructor, or null if not available.
        /// </summary>
        PhysicalConstructorDelegate IUserTypeDelegates.PhysicalConstructor
        {
            get
            {
                return physicalConstructor;
            }
        }

        /// <summary>
        /// Gets the physical constructor, or null if not available.
        /// </summary>
        PhysicalConstructorDelegate<T> IUserTypeDelegates<T>.PhysicalConstructor
        {
            get
            {
                return physicalConstructorTyped;
            }
        }

        /// <summary>
        /// Gets the symbolic constructor, or null if not available.
        /// </summary>
        SymbolicConstructorDelegate IUserTypeDelegates.SymbolicConstructor
        {
            get
            {
                return symbolicConstructor;
            }
        }

        /// <summary>
        /// Gets the symbolic constructor, or null if not available.
        /// </summary>
        SymbolicConstructorDelegate<T> IUserTypeDelegates<T>.SymbolicConstructor
        {
            get
            {
                return symbolicConstructorTyped;
            }
        }
    }
}

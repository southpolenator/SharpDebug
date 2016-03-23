using CsScriptManaged.Utility;
using CsScripts;
using System;
using System.Linq;
using System.Reflection.Emit;

namespace CsScriptManaged
{
    /// <summary>
    /// Interface for type that can be reused (use with caution, it is meant to be used only by physical code gen)
    /// </summary>
    public interface IReuseUserType
    {
        /// <summary>
        /// Reuses this object with new values.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="bufferAddress">The buffer address.</param>
        /// <param name="codeType">The variable code type.</param>
        /// <param name="address">The variable address.</param>
        /// <param name="name">The variable name.</param>
        /// <param name="path">The variable path.</param>
        void ReuseUserType(MemoryBuffer buffer, int offset, ulong bufferAddress, CodeType codeType, ulong address, string name, string path);
    }

    internal class UserTypeDelegates
    {
        /// <summary>
        /// The symbolic constructor delegate. Creates a new user type by calling constructor with signature of this delegate.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>New instance of user type</returns>
        public delegate object SymbolicConstructorDelegate(Variable variable);

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
        public delegate object PhysicalConstructorDelegate(MemoryBuffer buffer, int offset, ulong bufferAddress, CodeType codeType, ulong address, string name, string path);

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTypeDelegates"/> class.
        /// </summary>
        /// <param name="userType">The user type.</param>
        public UserTypeDelegates(Type userType)
        {
            UserType = userType;

            // Find symbolic constructor
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
                    SymbolicConstructor = (SymbolicConstructorDelegate)method.CreateDelegate(typeof(SymbolicConstructorDelegate));
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
                    PhysicalConstructor = (PhysicalConstructorDelegate)method.CreateDelegate(typeof(PhysicalConstructorDelegate));
                }
            }

            // Check if it supports user type reusing
            SupportsUserTypeReuse = UserType.GetInterfaces().Contains(typeof(IReuseUserType));
        }

        /// <summary>
        /// Gets the user type.
        /// </summary>
        public Type UserType { get; private set; }

        public bool SupportsUserTypeReuse { get; private set; }

        /// <summary>
        /// Gets the symbolic constructor.
        /// </summary>
        public SymbolicConstructorDelegate SymbolicConstructor { get; private set; }

        /// <summary>
        /// Gets the physical constructor.
        /// </summary>
        public PhysicalConstructorDelegate PhysicalConstructor { get; private set; }

        /// <summary>
        /// Reuses this object with new values.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="bufferAddress">The buffer address.</param>
        /// <param name="codeType">The variable code type.</param>
        /// <param name="address">The variable address.</param>
        /// <param name="name">The variable name.</param>
        /// <param name="path">The variable path.</param>
        internal object Reuse(object element, MemoryBuffer buffer, int offset, ulong bufferAddress, CodeType codeType, ulong address, string name = Variable.ComputedName, string path = Variable.UnknownPath)
        {
            if (SupportsUserTypeReuse)
            {
                ((IReuseUserType)element).ReuseUserType(buffer, offset, bufferAddress, codeType, address, name, path);
                return element;
            }
            else
            {
                return PhysicalConstructor(buffer, offset, bufferAddress, codeType, address, name, path);
            }
        }
    }

    internal class UserTypeDelegates<T>
    {
        /// <summary>
        /// The symbolic constructor delegate. Creates a new user type by calling constructor with signature of this delegate.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>New instance of user type</returns>
        public delegate T SymbolicConstructorDelegate(Variable variable);

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
        public delegate T PhysicalConstructorDelegate(MemoryBuffer buffer, int offset, ulong bufferAddress, CodeType codeType, ulong address, string name, string path);

        public static UserTypeDelegates<T> Instance = CreateInstance();

        private static UserTypeDelegates<T> CreateInstance()
        {
            var userType = typeof(T);
            var supportsUserTypeReuse = userType.GetInterfaces().Contains(typeof(IReuseUserType));

            if (supportsUserTypeReuse)
            {
                Type userTypeDelegatesType = typeof(UserTypeDelegatesFullReuse<>);
                userTypeDelegatesType = userTypeDelegatesType.MakeGenericType(new Type[] { userType });
                return (UserTypeDelegates<T>)Activator.CreateInstance(userTypeDelegatesType);
            }
            return new UserTypeDelegates<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTypeDelegates"/> class.
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
                    SymbolicConstructor = (SymbolicConstructorDelegate)method.CreateDelegate(typeof(SymbolicConstructorDelegate));
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
                    PhysicalConstructor = (PhysicalConstructorDelegate)method.CreateDelegate(typeof(PhysicalConstructorDelegate));
                }
            }

            // Check if it supports user type reusing
            SupportsUserTypeReuse = userType.GetInterfaces().Contains(typeof(IReuseUserType));
        }

        public bool SupportsUserTypeReuse { get; private set; }

        /// <summary>
        /// Gets the symbolic constructor.
        /// </summary>
        public SymbolicConstructorDelegate SymbolicConstructor { get; private set; }

        /// <summary>
        /// Gets the physical constructor.
        /// </summary>
        public PhysicalConstructorDelegate PhysicalConstructor { get; private set; }

        /// <summary>
        /// Reuses this object with new values.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="bufferAddress">The buffer address.</param>
        /// <param name="codeType">The variable code type.</param>
        /// <param name="address">The variable address.</param>
        /// <param name="name">The variable name.</param>
        /// <param name="path">The variable path.</param>
        internal virtual T Reuse(T element, MemoryBuffer buffer, int offset, ulong bufferAddress, CodeType codeType, ulong address, string name = Variable.ComputedName, string path = Variable.UnknownPath)
        {
            if (SupportsUserTypeReuse)
            {
                ((IReuseUserType)element).ReuseUserType(buffer, offset, bufferAddress, codeType, address, name, path);
                return element;
            }
            else
            {
                return PhysicalConstructor(buffer, offset, bufferAddress, codeType, address, name, path);
            }
        }
    }

    internal class UserTypeDelegatesFullReuse<T> : UserTypeDelegates<T>
        where T : IReuseUserType
    {
        /// <summary>
        /// Reuses this object with new values.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="bufferAddress">The buffer address.</param>
        /// <param name="codeType">The variable code type.</param>
        /// <param name="address">The variable address.</param>
        /// <param name="name">The variable name.</param>
        /// <param name="path">The variable path.</param>
        internal override T Reuse(T element, MemoryBuffer buffer, int offset, ulong bufferAddress, CodeType codeType, ulong address, string name = Variable.ComputedName, string path = Variable.UnknownPath)
        {
            element.ReuseUserType(buffer, offset, bufferAddress, codeType, address, name, path);
            return element;
        }
    }
}

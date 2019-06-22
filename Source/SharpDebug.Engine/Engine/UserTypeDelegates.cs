﻿using SharpUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SharpDebug.Engine
{
    /// <summary>
    /// The symbolic constructor delegate. Creates a new user type by calling constructor with signature of this delegate.
    /// </summary>
    /// <param name="variable">The variable.</param>
    /// <returns>New instance of user type</returns>
    internal delegate object SymbolicConstructorDelegate(Variable variable);

    /// <summary>
    /// The symbolic constructor with data delegate. Creates a new user type by calling constructor with signature of this delegate.
    /// This constructor is being used with common user types that know how to speed up processing with precalculated data for given code type.
    /// </summary>
    /// <param name="variable">The variable.</param>
    /// <param name="data">The data extracted from the <see cref="CodeType"/> of the specified variable.</param>
    /// <returns>New instance of user type</returns>
    internal delegate object SymbolicConstructorWithDataDelegate(Variable variable, object data);

    /// <summary>
    /// The symbolic constructor delegate. Creates a new user type by calling constructor with signature of this delegate.
    /// </summary>
    /// <param name="variable">The variable.</param>
    /// <returns>New instance of user type</returns>
    internal delegate T SymbolicConstructorDelegate<T>(Variable variable);

    /// <summary>
    /// The symbolic constructor with data delegate. Creates a new user type by calling constructor with signature of this delegate.
    /// This constructor is being used with common user types that know how to speed up processing with precalculated data for given code type.
    /// </summary>
    /// <param name="variable">The variable.</param>
    /// <param name="data">The data extracted from the <see cref="CodeType"/> of the specified variable.</param>
    /// <returns>New instance of user type</returns>
    internal delegate T SymbolicConstructorWithDataDelegate<T>(Variable variable, object data);

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
    /// The downcaster delegate. Creates a new user type by calling constructor with signature of this delegate and downcasted interface.
    /// </summary>
    /// <param name="variable">The variable.</param>
    /// <returns>Downcasted instance of user type</returns>
    internal delegate T DowncasterDelegate<T>(Variable variable);

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
        /// Gets the symbolic constructor with data, or null if not available.
        /// </summary>
        SymbolicConstructorWithDataDelegate SymbolicConstructorWithData { get; }

        /// <summary>
        /// Gets the physical constructor, or null if not available.
        /// </summary>
        PhysicalConstructorDelegate PhysicalConstructor { get; }

        /// <summary>
        /// Gets the derived class attributes.
        /// </summary>
        DerivedClassAttribute[] DerivedClassAttributes { get; }

        /// <summary>
        /// Gets the dictionary of derived class attributes by type name.
        /// </summary>
        Dictionary<string, DerivedClassAttribute> DerivedClassAttributesDictionary { get; }

        /// <summary>
        /// Gets the base classes cache for casting. It is array of types that have base classes.
        /// Each element has BaseClassString and array of MciAuxiliaryClassTypes for base classes.
        /// </summary>
        Tuple<string, Type[]>[] BaseClassesCacheForCasting { get; }

        /// <summary>
        /// Gets the virtual methods.
        /// </summary>
        MethodInfo[] VirtualMethods { get; }

        /// <summary>
        /// Gets the multi-class-inheritance auxiliary class type.
        /// </summary>
        /// <typeparam name="TBase">The base type</typeparam>
        Type GetMciAuxiliaryClassType<TBase>();

        /// <summary>
        /// Gets the multi-class-inheritance auxiliary class type.
        /// </summary>
        /// <param name="baseType">The base type.</param>
        Type GetMciAuxiliaryClassType(Type baseType);
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
        /// Gets the symbolic constructor with data, or null if not available.
        /// </summary>
        SymbolicConstructorWithDataDelegate<T> SymbolicConstructorWithData { get; }

        /// <summary>
        /// Gets the physical constructor, or null if not available.
        /// </summary>
        PhysicalConstructorDelegate<T> PhysicalConstructor { get; }

        /// <summary>
        /// Gets the downcaster delegate.
        /// </summary>
        DowncasterDelegate<T> Downcaster { get; }

        /// <summary>
        /// Gets the derived class attributes.
        /// </summary>
        DerivedClassAttribute[] DerivedClassAttributes { get; }

        /// <summary>
        /// Gets the dictionary of derived class attributes by type name.
        /// </summary>
        Dictionary<string, DerivedClassAttribute> DerivedClassAttributesDictionary { get; }

        /// <summary>
        /// Gets the base classes cache for casting. It is array of types that have base classes.
        /// Each element has BaseClassString and array of MciAuxiliaryClassTypes for base classes.
        /// </summary>
        Tuple<string, Type[]>[] BaseClassesCacheForCasting { get; }

        /// <summary>
        /// Gets the virtual methods.
        /// </summary>
        MethodInfo[] VirtualMethods { get; }

        /// <summary>
        /// Gets the multi-class-inheritance auxiliary class type.
        /// </summary>
        /// <typeparam name="TBase">The base type</typeparam>
        Type GetMciAuxiliaryClassType<TBase>();

        /// <summary>
        /// Gets the multi-class-inheritance auxiliary class type.
        /// </summary>
        /// <param name="baseType">The base type.</param>
        Type GetMciAuxiliaryClassType(Type baseType);
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
        /// The number of dynamically created types
        /// </summary>
        internal static int NumberOfDynamicTypes;

        /// <summary>
        /// The cache of module builder used for creating dynamic types
        /// </summary>
        private static SimpleCache<ModuleBuilder> moduleBuilder;

        /// <summary>
        /// Initializes the <see cref="UserTypeDelegates"/> class.
        /// </summary>
        static UserTypeDelegates()
        {
            moduleBuilder = SimpleCache.Create(() =>
            {
                AssemblyName assemblyName = new AssemblyName("SharpDebug.DynamicAssembly");
                AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

                return assemblyBuilder.DefineDynamicModule("SharpDebug.Dynamic");
            });
        }

        /// <summary>
        /// Gets the module builder used for creating dynamic types.
        /// </summary>
        internal static ModuleBuilder ModuleBuilder
        {
            get
            {
                return moduleBuilder.Value;
            }
        }

        /// <summary>
        /// Gets the delegates interface for the specified user type.
        /// </summary>
        /// <param name="userType">The user type.</param>
        private static IUserTypeDelegates Get(Type userType)
        {
            if (userType.ContainsGenericParameters)
            {
                Type[] variableTypes = new Type[userType.GetGenericArguments().Length];
                for (int i = 0; i < variableTypes.Length; i++)
                    variableTypes[i] = typeof(Variable);
                userType = userType.MakeGenericType(variableTypes);
            }

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
        /// The cache of physical constructors
        /// </summary>
        private SimpleCache<Tuple<PhysicalConstructorDelegate, PhysicalConstructorDelegate<T>>> physicalConstructors;

        /// <summary>
        /// The cache of symbolic constructors
        /// </summary>
        private SimpleCache<Tuple<SymbolicConstructorDelegate, SymbolicConstructorDelegate<T>>> symbolicConstructors;

        /// <summary>
        /// The cache of symbolic constructors with data
        /// </summary>
        private SimpleCache<Tuple<SymbolicConstructorWithDataDelegate, SymbolicConstructorWithDataDelegate<T>>> symbolicConstructorsWithData;

        /// <summary>
        /// The cache of downcaster
        /// </summary>
        private SimpleCache<DowncasterDelegate<T>> downcaster;

        /// <summary>
        /// The cache of derived class attributes
        /// </summary>
        private SimpleCache<DerivedClassAttribute[]> derivedClassAttributes;

        /// <summary>
        /// The cache of dictionary of derived class attributes by type name.
        /// </summary>
        private SimpleCache<Dictionary<string, DerivedClassAttribute>> derivedClassAttributesDictionary;

        /// <summary>
        /// The cache of base classes cache for casting.
        /// </summary>
        private SimpleCache<Tuple<string, Type[]>[]> baseClassesCacheForCasting;

        /// <summary>
        /// The cache of virtual methods
        /// </summary>
        private SimpleCache<MethodInfo[]> virtualMethods;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTypeDelegates{T}"/> class.
        /// </summary>
        internal protected UserTypeDelegates()
        {
            Type userType = typeof(T);
            ConstructorInfo[] constructors = userType.GetConstructors();

            symbolicConstructors = SimpleCache.Create(() =>
            {
                foreach (ConstructorInfo constructor in constructors)
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
                        DynamicMethod method = new DynamicMethod("CreateIntance", userType, new Type[] { typeof(Variable) }, UserTypeDelegates.ModuleBuilder);
                        ILGenerator il = method.GetILGenerator();

                        il.PrepareMethodCall(1, parameters);
                        il.Emit(OpCodes.Newobj, constructor);
                        il.Emit(OpCodes.Ret);

                        var symbolicConstructor = (SymbolicConstructorDelegate)method.CreateDelegate(typeof(SymbolicConstructorDelegate));
                        var symbolicConstructorTyped = (SymbolicConstructorDelegate<T>)method.CreateDelegate(typeof(SymbolicConstructorDelegate<T>));

                        return Tuple.Create(symbolicConstructor, symbolicConstructorTyped);
                    }
                }

                return null;
            });

            symbolicConstructorsWithData = SimpleCache.Create(() =>
            {
                foreach (ConstructorInfo constructor in constructors)
                {
                    if (!constructor.IsPublic)
                    {
                        continue;
                    }

                    var parameters = constructor.GetParameters();

                    if (parameters.Length < 2 || parameters.Count(p => !p.HasDefaultValue) > 2)
                    {
                        continue;
                    }

                    if (parameters[0].ParameterType == typeof(Variable) && parameters[1].ParameterType == typeof(object))
                    {
                        DynamicMethod method = new DynamicMethod("CreateIntance", userType, new Type[] { typeof(Variable), typeof(object) }, UserTypeDelegates.ModuleBuilder);
                        ILGenerator il = method.GetILGenerator();

                        il.PrepareMethodCall(2, parameters);
                        il.Emit(OpCodes.Newobj, constructor);
                        il.Emit(OpCodes.Ret);

                        var symbolicConstructorWithData = (SymbolicConstructorWithDataDelegate)method.CreateDelegate(typeof(SymbolicConstructorWithDataDelegate));
                        var symbolicConstructorWithDataTyped = (SymbolicConstructorWithDataDelegate<T>)method.CreateDelegate(typeof(SymbolicConstructorWithDataDelegate<T>));

                        return Tuple.Create(symbolicConstructorWithData, symbolicConstructorWithDataTyped);
                    }
                }

                return null;
            });

            physicalConstructors = SimpleCache.Create(() =>
            {
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
                        DynamicMethod method = new DynamicMethod("CreateIntance", userType, new Type[] { typeof(MemoryBuffer), typeof(int), typeof(ulong), typeof(CodeType), typeof(ulong), typeof(string), typeof(string) }, UserTypeDelegates.ModuleBuilder);
                        ILGenerator il = method.GetILGenerator();

                        il.PrepareMethodCall(7, parameters);
                        il.Emit(OpCodes.Newobj, constructor);
                        il.Emit(OpCodes.Ret);

                        var physicalConstructor = (PhysicalConstructorDelegate)method.CreateDelegate(typeof(PhysicalConstructorDelegate));
                        var physicalConstructorTyped = (PhysicalConstructorDelegate<T>)method.CreateDelegate(typeof(PhysicalConstructorDelegate<T>));

                        return Tuple.Create(physicalConstructor, physicalConstructorTyped);
                    }
                }

                return null;
            });

            downcaster = SimpleCache.Create(() =>
            {
                if (userType.IsSubclassOf(typeof(UserType)) && typeof(ICastableObject).IsAssignableFrom(userType) && DerivedClassAttributes.Length > 0)
                {
                    MethodInfo downcastObjectMethod = typeof(VariableCastExtender).GetMethod(nameof(VariableCastExtender.DowncastObject));
                    MethodInfo downcastObjectGenericMethod = downcastObjectMethod.MakeGenericMethod(typeof(T));
                    MethodInfo castAsMethod = typeof(Variable).GetMethod(nameof(Variable.CastAs), BindingFlags.Static | BindingFlags.Public);
                    MethodInfo castAsGenericMethod = castAsMethod.MakeGenericMethod(typeof(T));
                    DynamicMethod method = new DynamicMethod(nameof(VariableCastExtender.DowncastObject), userType, new Type[] { typeof(Variable) }, UserTypeDelegates.ModuleBuilder);
                    ILGenerator il = method.GetILGenerator();

                    il.ForwardArguments(1);
                    il.Emit(OpCodes.Call, castAsGenericMethod);
                    il.Emit(OpCodes.Call, downcastObjectGenericMethod);
                    il.Emit(OpCodes.Ret);

                    return (DowncasterDelegate<T>)method.CreateDelegate(typeof(DowncasterDelegate<T>));
                }

                return new DowncasterDelegate<T>((value) => value.CastAs<T>());
            });

            derivedClassAttributes = SimpleCache.Create(() => userType.GetCustomAttributes<DerivedClassAttribute>(false).ToArray());
            derivedClassAttributesDictionary = SimpleCache.Create(() => DerivedClassAttributes.ToDictionary(a => a.TypeName));

            baseClassesCacheForCasting = SimpleCache.Create(() =>
            {
                List<Tuple<string, Type[]>> typeArrays = new List<Tuple<string, Type[]>>();
                Type type = userType;

                while (type != null)
                {
                    BaseClassesArrayAttribute attribute = type.GetCustomAttribute<BaseClassesArrayAttribute>(false);

                    if (attribute != null)
                    {
                        FieldInfo typesField = type.GetField(attribute.FieldName, BindingFlags.Public | BindingFlags.Static);
                        Type[] originalTypes = (Type[])typesField.GetValue(null);
                        Type[] convertedTypes = new Type[originalTypes.Length];
                        string ownerCodeTypeString = UserType.GetBaseClassString(type);

                        for (int i = 0; i < originalTypes.Length; i++)
                            if (originalTypes[i] != null)
                                convertedTypes[i] = GetMciAuxiliaryClassType(originalTypes[i]);
                        typeArrays.Add(Tuple.Create(ownerCodeTypeString, convertedTypes));
                    }

                    type = type.BaseType;
                }

                return typeArrays.ToArray();
            });

            virtualMethods = SimpleCache.Create(() => GetVirtualMethods(userType));
        }

        /// <summary>
        /// Gets the derived class attributes.
        /// </summary>
        public DerivedClassAttribute[] DerivedClassAttributes
        {
            get
            {
                return derivedClassAttributes.Value;
            }
        }

        /// <summary>
        /// Gets the dictionary of derived class attributes by type name.
        /// </summary>
        public Dictionary<string, DerivedClassAttribute> DerivedClassAttributesDictionary
        {
            get
            {
                return derivedClassAttributesDictionary.Value;
            }
        }

        /// <summary>
        /// Gets the physical constructor, or null if not available.
        /// </summary>
        PhysicalConstructorDelegate IUserTypeDelegates.PhysicalConstructor
        {
            get
            {
                return physicalConstructors.Value.Item1;
            }
        }

        /// <summary>
        /// Gets the physical constructor, or null if not available.
        /// </summary>
        PhysicalConstructorDelegate<T> IUserTypeDelegates<T>.PhysicalConstructor
        {
            get
            {
                return physicalConstructors.Value.Item2;
            }
        }

        /// <summary>
        /// Gets the symbolic constructor, or null if not available.
        /// </summary>
        SymbolicConstructorDelegate IUserTypeDelegates.SymbolicConstructor
        {
            get
            {
                return symbolicConstructors.Value.Item1;
            }
        }

        /// <summary>
        /// Gets the symbolic constructor, or null if not available.
        /// </summary>
        SymbolicConstructorDelegate<T> IUserTypeDelegates<T>.SymbolicConstructor
        {
            get
            {
                return symbolicConstructors.Value.Item2;
            }
        }

        /// <summary>
        /// Gets the symbolic constructor with data, or null if not available.
        /// </summary>
        SymbolicConstructorWithDataDelegate IUserTypeDelegates.SymbolicConstructorWithData
        {
            get
            {
                return symbolicConstructorsWithData.Value.Item1;
            }
        }

        /// <summary>
        /// Gets the symbolic constructor with data, or null if not available.
        /// </summary>
        SymbolicConstructorWithDataDelegate<T> IUserTypeDelegates<T>.SymbolicConstructorWithData
        {
            get
            {
                return symbolicConstructorsWithData.Value.Item2;
            }
        }

        /// <summary>
        /// Gets the base classes cache for casting. It is array of types that have base classes.
        /// Each element has BaseClassString and array of MciAuxiliaryClassTypes for base classes.
        /// </summary>
        public Tuple<string, Type[]>[] BaseClassesCacheForCasting
        {
            get
            {
                return baseClassesCacheForCasting.Value;
            }
        }

        /// <summary>
        /// Gets the downcaster delegate.
        /// </summary>
        public DowncasterDelegate<T> Downcaster
        {
            get
            {
                return downcaster.Value;
            }
        }

        /// <summary>
        /// Gets the virtual methods.
        /// </summary>
        public MethodInfo[] VirtualMethods
        {
            get
            {
                return virtualMethods.Value;
            }
        }

        /// <summary>
        /// Gets the multi-class-inheritance auxiliary class type.
        /// </summary>
        /// <typeparam name="TBase">The base type</typeparam>
        public Type GetMciAuxiliaryClassType<TBase>()
        {
            return MciAuxiliaryClassTypeCreatetor<TBase>.Type;
        }

        /// <summary>
        /// The cache of multi-class-inheritance auxiliary class types
        /// </summary>
        private static DictionaryCache<Type, Type> mciAuxiliaryClassTypes = new DictionaryCache<Type, Type>(GetMciAuxiliaryClassTypeFromCreator);

        /// <summary>
        /// Gets the multi-class-inheritance auxiliary class type.
        /// </summary>
        /// <param name="baseType">The base type.</param>
        public Type GetMciAuxiliaryClassType(Type baseType)
        {
            return mciAuxiliaryClassTypes[baseType];
        }

        /// <summary>
        /// Gets the multi-class-inheritance auxiliary class type from creator.
        /// </summary>
        /// <param name="baseType">The base type.</param>
        private static Type GetMciAuxiliaryClassTypeFromCreator(Type baseType)
        {
            Type castableTypeCreatorType = typeof(MciAuxiliaryClassTypeCreatetor<>);
            castableTypeCreatorType = castableTypeCreatorType.MakeGenericType(new Type[] { typeof(T), baseType });
            FieldInfo typeField = castableTypeCreatorType.GetField(nameof(MciAuxiliaryClassTypeCreatetor<Variable>.Type));
            return (Type)typeField.GetValue(null);
        }

        /// <summary>
        /// Determines whether the specified methods contains method, by comparing name, parameters, generic arguments and return type.
        /// </summary>
        /// <param name="methods">The list of methods.</param>
        /// <param name="method">The method.</param>
        private static bool ContainsMethod(IEnumerable<MethodInfo> methods, MethodInfo method)
        {
            var parameters = method.GetParameters();
            var genericArguments = method.GetGenericArguments();

            foreach (var m in methods.Where(mm => mm.Name == method.Name))
            {
                // Check return type
                if (m.ReturnType != method.ReturnType)
                    continue;

                // Check parameters
                var p = m.GetParameters();
                bool found = true;

                if (parameters.Length != p.Length)
                    continue;
                for (int i = 0; i < p.Length && found; i++)
                {
                    if (p[i].ParameterType != parameters[i].ParameterType)
                        found = false;
                    if (p[i].IsOut != parameters[i].IsOut)
                        found = false;
                    if (p[i].IsIn != parameters[i].IsIn)
                        found = false;
                }

                if (!found)
                    continue;

                // Check generic arguments
                var g = m.GetGenericArguments();

                if (genericArguments.Length != g.Length)
                    continue;

                // We found it :)
                return true;
            }

            // Nope, it isn't here
            return false;
        }

        /// <summary>
        /// Gets the array of virtual methods for the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        private static MethodInfo[] GetVirtualMethods(Type type)
        {
            List<MethodInfo> result = new List<MethodInfo>();

            while (type != null)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var method in methods.Where(mm => mm.IsVirtual))
                {
                    // Don't forward System.Object virtual functions
                    if (method.DeclaringType == typeof(object))
                        continue;

                    // Don't forward System.Dynamic.DynamicObject virtual functions
                    if (method.DeclaringType == typeof(System.Dynamic.DynamicObject))
                        continue;

                    // Ignore methods that are overrides or final
                    if (method.IsFinal || !method.Attributes.HasFlag(MethodAttributes.NewSlot))
                        continue;

                    // Ignore private methods
                    if (!method.IsFamily && !method.IsPublic)
                        continue;

                    if (!ContainsMethod(result, method))
                        result.Add(method);
                }

                type = type.BaseType;
            }

            return result.ToArray();
        }

        /// <summary>
        /// The multi-class-inheritance auxiliary class type creator.
        /// </summary>
        /// <typeparam name="TBase">The base type.</typeparam>
        private class MciAuxiliaryClassTypeCreatetor<TBase>
        {
            /// <summary>
            /// The multi-class-inheritance auxiliary class type
            /// </summary>
            public static Type Type = CreateType();

            /// <summary>
            /// Creates the multi-class-inheritance auxiliary class type.
            /// </summary>
            private static Type CreateType()
            {
                Type baseType = typeof(TBase);
                Type parentType = typeof(T);
                ConstructorInfo[] constructors = baseType.GetConstructors();
                string typeName = string.Format("DynamicType{0}", System.Threading.Interlocked.Increment(ref UserTypeDelegates.NumberOfDynamicTypes));
                TypeBuilder typeBuilder = UserTypeDelegates.ModuleBuilder.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout, baseType, new Type[] { typeof(IMultiClassInheritance) });

                // Define constructors
                foreach (ConstructorInfo baseConstructor in constructors)
                {
                    ParameterInfo[] baseParameters = baseConstructor.GetParameters();
                    Type[] parameters = baseParameters.Select(p => p.ParameterType).ToArray();
                    ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName, CallingConventions.Any, parameters);

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        ParameterBuilder parameter = constructorBuilder.DefineParameter(i + 1, baseParameters[i].Attributes, baseParameters[i].Name);

                        if (baseParameters[i].HasDefaultValue)
                            parameter.SetConstant(baseParameters[i].DefaultValue);
                    }

                    ILGenerator ilGenerator = constructorBuilder.GetILGenerator();

                    ilGenerator.ForwardArguments(parameters.Length + 1);
                    ilGenerator.Emit(OpCodes.Call, baseConstructor);
                    ilGenerator.Emit(OpCodes.Ret);
                }

                // Add field for storing parent
                string parentFieldName = "parent";
                FieldBuilder parentField = typeBuilder.DefineField(parentFieldName, parentType, FieldAttributes.Private);

                // Implement property for IMultiClassInheritance
                Type propertyType = typeof(UserType);
                string propertyName = nameof(IMultiClassInheritance.DowncastParent);
                PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, propertyType, null);
                MethodBuilder getPropertyMethodBuilder = typeBuilder.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot, propertyType, Type.EmptyTypes);
                ILGenerator il = getPropertyMethodBuilder.GetILGenerator();

                il.ForwardArguments(1);
                il.Emit(OpCodes.Ldfld, parentField);
                il.Emit(OpCodes.Ret);
                propertyBuilder.SetGetMethod(getPropertyMethodBuilder);

                MethodBuilder setPropertyMethodBuilder = typeBuilder.DefineMethod("set_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot, null, new Type[] { propertyType });
                il = setPropertyMethodBuilder.GetILGenerator();

                il.ForwardArguments(2);
                il.Emit(OpCodes.Castclass, parentType);
                il.Emit(OpCodes.Stfld, parentField);
                il.Emit(OpCodes.Ret);
                propertyBuilder.SetSetMethod(setPropertyMethodBuilder);

                // Forward all virtual methods
                MethodInfo[] parentVirtualMethods = Instance.VirtualMethods;
                MethodInfo[] baseVirtualMethods = UserTypeDelegates<TBase>.Instance.VirtualMethods;

                foreach (MethodInfo parentVirtualMethod in parentVirtualMethods)
                {
                    MethodAttributes methodAttributes = MethodAttributes.Virtual | MethodAttributes.HideBySig;

                    methodAttributes |= parentVirtualMethod.IsFamily ? MethodAttributes.Family : MethodAttributes.Public;
                    if (!ContainsMethod(baseVirtualMethods, parentVirtualMethod))
                        methodAttributes |= MethodAttributes.NewSlot;

                    ParameterInfo[] parentParameters = parentVirtualMethod.GetParameters();
                    Type[] parameters = parentParameters.Select(p => p.ParameterType).ToArray();
                    MethodBuilder methodBuilder = typeBuilder.DefineMethod(parentVirtualMethod.Name, methodAttributes, parentVirtualMethod.ReturnType, parameters);
                    Type[] parentVirtualMethodGenericArguments = parentVirtualMethod.GetGenericArguments();

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        ParameterBuilder parameter = methodBuilder.DefineParameter(i + 1, parentParameters[i].Attributes, parentParameters[i].Name);

                        if (parentParameters[i].HasDefaultValue)
                            parameter.SetConstant(parentParameters[i].DefaultValue);
                    }

                    if (parentVirtualMethodGenericArguments.Length > 0)
                    {
                        // TODO: Generic methods are not supported
                        GenericTypeParameterBuilder[] typeParameters = methodBuilder.DefineGenericParameters(parentVirtualMethodGenericArguments.Select(p => p.Name).ToArray());

                        for (int i = 0; i < typeParameters.Length; i++)
                            typeParameters[i].SetGenericParameterAttributes(parentVirtualMethodGenericArguments[i].GenericParameterAttributes);
                        throw new NotSupportedException();
                    }

                    il = methodBuilder.GetILGenerator();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, parentField);
                    il.ForwardArguments(parameters.Length + 1, 1);
                    il.Emit(OpCodes.Callvirt, parentVirtualMethod);
                    il.Emit(OpCodes.Ret);
                }

                // TODO: What about virtual properties?

                // Build type
                return typeBuilder.CreateTypeInfo().AsType();
            }
        }
    }
}

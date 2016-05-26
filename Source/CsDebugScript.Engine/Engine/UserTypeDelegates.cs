using CsDebugScript.Engine.Utility;
using System;
using System.Collections.Generic;
using System.IO;
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
                AssemblyName assemblyName = new AssemblyName("CsDebugScript.DynamicAssembly");
                byte[] bytes;

                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CsDebugScript.Key.snk"))
                {
                    bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                }

                assemblyName.KeyPair = new StrongNameKeyPair(bytes);

                AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

                return assemblyBuilder.DefineDynamicModule("CsDebugScript.Dynamic");
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
                        DynamicMethod method = new DynamicMethod("CreateIntance", userType, new Type[] { typeof(Variable) });
                        ILGenerator gen = method.GetILGenerator();

                        gen.Emit(OpCodes.Ldarg_0);
                        gen.Emit(OpCodes.Newobj, constructor);
                        gen.Emit(OpCodes.Ret);

                        var symbolicConstructor = (SymbolicConstructorDelegate)method.CreateDelegate(typeof(SymbolicConstructorDelegate));
                        var symbolicConstructorTyped = (SymbolicConstructorDelegate<T>)method.CreateDelegate(typeof(SymbolicConstructorDelegate<T>));

                        return Tuple.Create(symbolicConstructor, symbolicConstructorTyped);
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
                    DynamicMethod method = new DynamicMethod(nameof(VariableCastExtender.DowncastObject), userType, new Type[] { typeof(Variable) });
                    ILGenerator gen = method.GetILGenerator();

                    gen.Emit(OpCodes.Ldarg_0);
                    gen.Emit(OpCodes.Call, castAsGenericMethod);
                    gen.Emit(OpCodes.Call, downcastObjectGenericMethod);
                    gen.Emit(OpCodes.Ret);

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
                    Type[] parameters = baseConstructor.GetParameters().Select(p => p.ParameterType).ToArray();
                    ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName, CallingConventions.Any, parameters);
                    ILGenerator ilGenerator = constructorBuilder.GetILGenerator();

                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    if (parameters.Length >= 1)
                        ilGenerator.Emit(OpCodes.Ldarg_1);
                    if (parameters.Length >= 2)
                        ilGenerator.Emit(OpCodes.Ldarg_2);
                    if (parameters.Length >= 3)
                        ilGenerator.Emit(OpCodes.Ldarg_3);
                    for (int i = 4; i <= parameters.Length; i++)
                        ilGenerator.Emit(OpCodes.Ldarg, (short)i);
                    ilGenerator.Emit(OpCodes.Call, baseConstructor);
                    ilGenerator.Emit(OpCodes.Ret);
                }

                // Add field for storing parent
                string fieldName = "parent";
                FieldBuilder fieldBuilder = typeBuilder.DefineField(fieldName, parentType, FieldAttributes.Private);

                // Implement property for IMultiClassInheritance
                Type propertyType = typeof(UserType);
                string propertyName = nameof(IMultiClassInheritance.DowncastParent);
                PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
                MethodBuilder getPropertyMethodBuilder = typeBuilder.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual, propertyType, Type.EmptyTypes);
                ILGenerator il = getPropertyMethodBuilder.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fieldBuilder);
                il.Emit(OpCodes.Ret);
                propertyBuilder.SetGetMethod(getPropertyMethodBuilder);

                MethodBuilder setPropertyMethodBuilder = typeBuilder.DefineMethod("set_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual, null, new Type[] { propertyType });
                il = setPropertyMethodBuilder.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Castclass, parentType);
                il.Emit(OpCodes.Stfld, fieldBuilder);
                il.Emit(OpCodes.Ret);
                propertyBuilder.SetSetMethod(setPropertyMethodBuilder);

                // Build type
                return typeBuilder.CreateType();
            }
        }
    }
}

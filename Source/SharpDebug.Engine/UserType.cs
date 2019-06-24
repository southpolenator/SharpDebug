using SharpDebug.Engine;
using SharpUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SharpDebug
{
    /// <summary>
    /// Extension class providing cast functionality.
    /// </summary>
    public static class VariableCastExtender
    {
        /// <summary>
        /// Safely determines whether this variable is null pointer.
        /// </summary>
        /// <returns><c>true</c> if variable is null pointer;<c>false</c> otherwise</returns>
        public static bool IsNull(this Variable variable)
        {
            return variable == null || variable.IsNullPointer();
        }

        /// <summary>
        /// Does the dynamic cast, cast with type check.
        /// </summary>
        /// <typeparam name="T">New type to cast variable to.</typeparam>
        /// <param name="variable">The variable.</param>
        public static T DynamicCastAs<T>(this Variable variable)
            where T : UserType
        {
            if (variable == null)
            {
                return null;
            }

            CodeType runtimeType = variable.GetRuntimeType();

            if (runtimeType.IsPointer)
            {
                runtimeType = runtimeType.ElementType;
            }

            if (runtimeType.Inherits<T>())
            {
                variable = variable.DowncastInterface();

                // Figure out what is code type we need to cast to.
                CodeType baseClassCodeType = variable.CastAs<T>().GetCodeType();

                if (baseClassCodeType.IsPointer)
                {
                    baseClassCodeType = baseClassCodeType.ElementType;
                }

                // Cast to base class as it is only way to get offset set correctly.
                return variable.GetBaseClass<T>(baseClassCodeType.Name);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Does the safe cast, cast with null check.
        /// </summary>
        /// <typeparam name="T">New type to cast variable to.</typeparam>
        /// <param name="variable">The variable.</param>
        public static T SafeCastAs<T>(this Variable variable)
            where T : UserType
        {
            if (variable == null)
            {
                return null;
            }

            return variable.CastAs<T>();
        }

        /// <summary>
        /// Does the full downcast, looks up the type based on virtual table and shifts variable address if multi-inheritance was involved.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public static Variable DowncastInterface(this Variable variable)
        {
            if (variable == null)
            {
                return null;
            }

            var runtimeTypeAndOffset = variable.runtimeCodeTypeAndOffset.Value;

            if (runtimeTypeAndOffset.Item2 != 0 || variable.GetCodeType() != runtimeTypeAndOffset.Item1)
            {
                return Variable.CreatePointer(runtimeTypeAndOffset.Item1.PointerToType, variable.GetPointerAddress() - (uint)runtimeTypeAndOffset.Item2);
            }

            return variable;
        }

        /// <summary>
        /// Does the full downcast, looks up the type based on virtual table and shifts variable address if multi-inheritance was involved.
        /// Difference from <see cref="VariableCastExtender.DowncastInterface(Variable)"/> is that this function returns downcasted .NET object
        /// if T contains <see cref="DerivedClassAttribute"/>.
        /// </summary>
        /// <typeparam name="T">The base user type which will be downcasted.</typeparam>
        /// <param name="userType">The user type.</param>
        /// <returns>Downcasted .NET object, but upcasted to the original user type.</returns>
        public static T DowncastObject<T>(this T userType)
            where T : UserType, ICastableObject
        {
            if (userType == null)
            {
                return null;
            }

            Dictionary<string, DerivedClassAttribute> attributes = UserTypeDelegates<T>.Instance.DerivedClassAttributesDictionary;

            if (attributes.Count == 0)
            {
                throw new Exception(string.Format("Specified type {0} doesn't contain derived class attributes", typeof(T).Name));
            }

            Variable variable = userType.DowncastInterface();
            CodeType originalCodeType = variable.GetCodeType();
            List<Tuple<CodeType, int>> types = new List<Tuple<CodeType, int>>();

            if (originalCodeType.IsPointer)
            {
                originalCodeType = originalCodeType.ElementType;
            }

            types.Add(Tuple.Create(originalCodeType, 0));
            while (types.Count > 0)
            {
                List<Tuple<CodeType, int>> newTypes = new List<Tuple<CodeType, int>>();

                foreach (Tuple<CodeType, int> tuple in types)
                {
                    int offset = tuple.Item2;
                    CodeType codeType = tuple.Item1;
                    DerivedClassAttribute attribute;

                    if (attributes.TryGetValue(codeType.Name, out attribute))
                    {
                        // Check if we don't have top level code type
                        if (originalCodeType != codeType)
                        {
                            if (offset > 0)
                            {
                                variable = variable.AdjustPointer(offset);
                            }

                            variable = variable.CastAs(codeType);
                        }

                        UserType downcastedObject = (UserType)variable.CastAs(attribute.Type);

                        return AsUpcast<T>(downcastedObject, (int)(userType.GetPointerAddress() - variable.GetPointerAddress()));
                    }

                    // Add base classes
                    foreach (var t in codeType.InheritedClassesSorted)
                    {
                        newTypes.Add(Tuple.Create(t.Item1, offset + t.Item2));
                    }
                }

                // Continue with new types
                types = newTypes;
            }

            return userType;
        }

        /// <summary>
        /// Check if variable runtime type inherits from the specified type.
        /// </summary>
        /// <typeparam name="T">Type to verify inheritance of.</typeparam>
        /// <param name="variable">The variable.</param>
        public static bool Inherits<T>(this Variable variable)
            where T : UserType
        {
            return variable.GetRuntimeType().Inherits<T>();
        }

        /// <summary>
        /// Reinterpret Cast, changes underlaying code type.
        /// </summary>
        /// <remarks>
        /// Requested Type must be a primitive (int, short etc).
        /// </remarks>
        /// <typeparam name="T">Primitive type to cast variable to.</typeparam>
        /// <param name="variable">The variable.</param>
        /// <returns>Return CodePointer to Variable pointer address.</returns>
        public static CodePointer<T> ReinterpretPointerCast<T>(this Variable variable)
            where T : struct
        {
            // Get CodeType from the generic argument.
            //
            string codeTypeName;

            if (typeof (T) == typeof (int))
            {
                codeTypeName = "int";
            }
            else if (typeof (T) == typeof (short))
            {
                codeTypeName = "short";
            }
            else if (typeof (T) == typeof (uint))
            {
                codeTypeName = "unsigned int";
            }
            else if (typeof (T) == typeof (ushort))
            {
                codeTypeName = "unsigned short";
            }
            else
            {
                throw new NotSupportedException("Requested type is not supported.");
            }

            // Return CodePointer<T>
            //
            return new CodePointer<T>(
                Variable.CreatePointer(
                    CodeType.Create(codeTypeName, variable.GetCodeType().Module).PointerToType,
                    variable.GetPointerAddress()));
        }

        /// <summary>
        /// Converts the specified pointer variable to the code pointer.
        /// This helper function is helpful only when variable is pointer and user wants to access more elements.
        /// </summary>
        /// <typeparam name="T">The user type to be converted to code pointer</typeparam>
        /// <param name="pointerVariable">The pointer variable.</param>
        public static CodePointer<T> ToCodePointer<T>(this T pointerVariable)
            where T : UserType
        {
            return new CodePointer<T>(pointerVariable);
        }

        /// <summary>
        /// Converts the specified pointer variable to the code array.
        /// This helper function is helpful only when variable is pointer and user wants to access it as array of elements.
        /// </summary>
        /// <typeparam name="T">The user type to be converted to code array</typeparam>
        /// <param name="pointerVariable">The pointer variable.</param>
        /// <param name="length">The number of elements in array.</param>
        public static CodeArray<T> ToCodeArray<T>(this T pointerVariable, int length)
            where T : UserType
        {
            return new CodeArray<T>(pointerVariable, length);
        }

        /// <summary>
        /// Converts the specified pointer variable to the code array.
        /// This helper function is helpful only when variable is pointer and user wants to access it as array of elements.
        /// </summary>
        /// <typeparam name="T">The user type to be converted to code array</typeparam>
        /// <param name="pointerVariable">The pointer variable.</param>
        /// <param name="length">The number of elements in array.</param>
        public static CodeArray<T> ToCodeArray<T>(this T pointerVariable, uint length)
            where T : UserType
        {
            return new CodeArray<T>(pointerVariable, length);
        }

        /// <summary>
        /// Converts the specified pointer variable to the code array.
        /// This helper function is helpful only when variable is pointer and user wants to access it as array of elements.
        /// </summary>
        /// <typeparam name="T">The user type to be converted to code array</typeparam>
        /// <param name="pointerVariable">The pointer variable.</param>
        /// <param name="length">The number of elements in array.</param>
        public static CodeArray<T> ToCodeArray<T>(this T pointerVariable, long length)
            where T : UserType
        {
            return new CodeArray<T>(pointerVariable, length);
        }

        /// <summary>
        /// Converts the specified pointer variable to the code array.
        /// This helper function is helpful only when variable is pointer and user wants to access it as array of elements.
        /// </summary>
        /// <typeparam name="T">The user type to be converted to code array</typeparam>
        /// <param name="pointerVariable">The pointer variable.</param>
        /// <param name="length">The number of elements in array.</param>
        public static CodeArray<T> ToCodeArray<T>(this T pointerVariable, ulong length)
            where T : UserType
        {
            return new CodeArray<T>(pointerVariable, length);
        }

        /// <summary>
        /// Adjust Pointer and Cast To Type.
        /// </summary>
        /// <typeparam name="T">New type to cast variable to.</typeparam>
        /// <param name="variable">The variable.</param>
        /// <param name="offset">The offset.</param>
        public static T AdjustPointer<T>(this Variable variable, int offset)
            where T : UserType
        {
            return variable.AdjustPointer(offset).CastAs<T>();
        }

        /// <summary>
        /// Does the same work as <code>is</code> operator but just for multi-class-inheritance objects used in scripts.
        /// </summary>
        /// <typeparam name="T">Type to do the check for.</typeparam>
        /// <param name="userType">The user type.</param>
        /// <returns><c>true</c> if object can be casted to the specified type.</returns>
        public static bool Is<T>(this UserType userType)
            where T : UserType
        {
            return As<T>(userType) != null;
        }

        /// <summary>
        /// Does the same work as <code>as</code> operator but just for multi-class-inheritance objects used in scripts.
        /// </summary>
        /// <typeparam name="T">Type to do the cast to.</typeparam>
        /// <param name="userType">The user type.</param>
        /// <returns>Casted object, or null if object cannot be casted to the specified type.</returns>
        public static T As<T>(this UserType userType)
            where T : UserType
        {
            if (userType == null)
            {
                return null;
            }

            // Try to find going down
            T result = AsDowncast<T>(userType);

            // Try to find going up
            if (result == null)
            {
                result = AsUpcast<T>(userType);
            }

            // Try to find going all the way down and then search up
            if (result == null)
            {
                result = AsUpcast<T>(GetDowncast(userType));
            }

            return result;
        }

        /// <summary>
        /// Helper function for doing upcasting.
        /// </summary>
        /// <typeparam name="T">Type to do the cast to.</typeparam>
        /// <param name="userType">The user type to be upcasted.</param>
        /// <param name="offsetToFollow">The offset to follow when doing upcasting. If less than 0, do searches through all base classes.</param>
        private static T AsUpcast<T>(UserType userType, int offsetToFollow = -1)
            where T : UserType
        {
            // Check if this C# inheritance contains requested type
            T result = userType as T;

            if (result != null)
            {
                return result;
            }

            // Get all types that have base classes
            Type type = userType.GetType();
            var userTypeDelegates = UserTypeDelegates.Delegates[type];
            Tuple<string, Type[]>[] typeArrays = userTypeDelegates.BaseClassesCacheForCasting;

            // Do downcasting over types that have base classes
            CodeType codeType = userType.GetCodeType();
            int baseClassOffset = 0;

            if (codeType.IsPointer)
                codeType = codeType.ElementType;

            foreach (Tuple<string, Type[]> typeTuple in typeArrays)
            {
                string ownerCodeTypeString = typeTuple.Item1;
                Type[] mciAuxiliaryClassTypes = typeTuple.Item2;
                CodeType nextCodeType = null;

                // Move code type to the correct user type
                while (!codeType.IsFor(ownerCodeTypeString))
                {
                    var tuple = codeType.InheritedClassesSorted[0];

                    baseClassOffset += tuple.Item2;
                    codeType = tuple.Item1;
                }

                // If we are following the offset, find base class index to follow
                int selectedIndex = -1;
                Tuple<CodeType, int>[] inheritedClasses = codeType.InheritedClassesSorted;

                if (offsetToFollow >= 0)
                {
                    for (selectedIndex = 0; selectedIndex < inheritedClasses.Length; selectedIndex++)
                        if (baseClassOffset + inheritedClasses[selectedIndex].Item2 > offsetToFollow)
                            break;
                    selectedIndex--;
                }

                // Go over base classes and try to find the correct one
                for (int i = 0; i < mciAuxiliaryClassTypes.Length; i++)
                {
                    Type mciAuxiliaryClassType = mciAuxiliaryClassTypes[i];

                    if (mciAuxiliaryClassType != null)
                    {
                        if (offsetToFollow < 0 || selectedIndex == i)
                        {
                            object baseClass = userType.GetBaseClass(i, codeType, baseClassOffset, mciAuxiliaryClassType);

                            result = AsUpcast<T>((UserType)baseClass, offsetToFollow < 0 ? offsetToFollow : offsetToFollow - baseClassOffset - inheritedClasses[i].Item2);
                            if (result != null)
                            {
                                return result;
                            }
                        }
                    }
                    else
                    {
                        var tuple = codeType.InheritedClassesSorted[i];

                        nextCodeType = tuple.Item1;
                        baseClassOffset += tuple.Item2;
                    }
                }

                codeType = nextCodeType;
            }

            return null;
        }

        /// <summary>
        /// Helper function for doing downcasting.
        /// </summary>
        /// <typeparam name="T">Type to do the cast to.</typeparam>
        /// <param name="userType">The user type to be downcasted.</param>
        private static T AsDowncast<T>(UserType userType)
            where T : UserType
        {
            while (userType != null)
            {
                T result = userType as T;

                if (result != null)
                {
                    return result;
                }

                IMultiClassInheritance mc = userType as IMultiClassInheritance;

                userType = mc?.DowncastParent;
            }

            return null;
        }

        /// <summary>
        /// Gets the full downcast object by going through multi-class-inheritance classes.
        /// </summary>
        /// <param name="userType">The user type that has multi-class-inheritance.</param>
        /// <returns>Fully downcasted C# object.</returns>
        public static UserType GetDowncast(this UserType userType)
        {
            IMultiClassInheritance mci = userType as IMultiClassInheritance;

            while (mci != null)
            {
                userType = mci.DowncastParent as UserType;
                mci = userType as IMultiClassInheritance;
            }

            return userType;
        }

        /// <summary>
        /// Gets the type of the full downcast object. See <see cref="GetDowncast(UserType)"/> for more info.
        /// </summary>
        /// <param name="userType">The user type that has multi-class-inheritance.</param>
        /// <returns>Type of the fully downcasted C# object.</returns>
        public static Type GetDowncastType(this UserType userType)
        {
            return GetDowncast(userType)?.GetType();
        }

        /// <summary>
        /// Cast Variable to NakedPointer.
        /// </summary>
        /// <param name="variable">The Variable.</param>
        /// <returns>Returns NakePointer to Variable pointer address.</returns>
        public static NakedPointer ToNakedPointer(this Variable variable)
        {
            if (variable == null)
            {
                return null;
            }

            return new NakedPointer(variable.GetCodeType().Module.Process, variable.GetPointerAddress());
        }
    }

    /// <summary>
    /// Helper class to be used in Generics User Type classes as partial class extension.
    /// It is needed when Generics User Type casts variable to new user type (that is template and possible generics). When user type is generics, engine cannot
    /// deduce CodeType that needs to be used, while generated user type code expects correct one. This class provides the bridge between two worlds.
    /// </summary>
    /// <example>
    /// When you have exported user type that is generics <code>MyGenericsType&lt;T&gt;</code> you should use this class to solve
    /// problems that can arise of having this somewhere in the code:
    /// <code>MyGenericsType&lt;MySecondGenericsType&lt;int&gt;&gt;</code>.
    /// Using generics element caster solves problems by extracting inner user type from the code type and later using it when casting.
    /// </example>
    /// <typeparam name="T">Resulting user type</typeparam>
    public class GenericsElementCaster<T>
    {
        /// <summary>
        /// The element code type
        /// </summary>
        private CodeType elementCodeType;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericsElementCaster{T}"/> class.
        /// </summary>
        /// <param name="thisClass">The thisClass variable in generated UserType.</param>
        /// <param name="argumentNumber">The argument number in original user type.</param>
        public GenericsElementCaster(UserMember<Variable> thisClass, int argumentNumber)
            : this(thisClass.Value, argumentNumber)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericsElementCaster{T}"/> class.
        /// </summary>
        /// <param name="thisClass">The thisClass variable in generated UserType.</param>
        /// <param name="argumentNumber">The argument number in original user type.</param>
        public GenericsElementCaster(Variable thisClass, int argumentNumber)
            : this(thisClass.GetCodeType(), argumentNumber)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericsElementCaster{T}"/> class.
        /// </summary>
        /// <param name="parentCodeType">CodeType of the generics class that will be using this helper class.</param>
        /// <param name="argumentNumber">The argument number in original user type.</param>
        public GenericsElementCaster(CodeType parentCodeType, int argumentNumber)
        {
            try
            {
                elementCodeType = CodeType.Create(parentCodeType.TemplateArgumentsStrings[argumentNumber], parentCodeType.Module);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Casts variable to the new type.
        /// </summary>
        /// <param name="variable">The variable to be casted.</param>
        /// <returns>Computed variable that is of new type.</returns>
        public T CastAs(Variable variable)
        {
            if (elementCodeType != null)
            {
                variable = variable.CastAs(elementCodeType);
            }

            return UserTypeDelegates<T>.Instance.Downcaster(variable);
        }
    }

    /// <summary>
    /// Base class for user defined types in C# scripts
    /// </summary>
    public class UserType : Variable
    {
        /// <summary>
        /// The memory buffer using which user type was initialized
        /// </summary>
        protected MemoryBuffer memoryBuffer;

        /// <summary>
        /// The offset inside the memory buffer using which user type was initialized
        /// </summary>
        protected int memoryBufferOffset;

        /// <summary>
        /// The address of the memory buffer using which user type was initialized
        /// </summary>
        protected ulong memoryBufferAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserType"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public UserType(Variable variable)
            : base(variable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserType"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="buffer">The memory buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="bufferAddress">The buffer address.</param>
        public UserType(Variable variable, MemoryBuffer buffer, int offset, ulong bufferAddress)
            : base(variable)
        {
            memoryBuffer = buffer;
            memoryBufferOffset = offset;
            memoryBufferAddress = bufferAddress;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserType" /> class.
        /// </summary>
        /// <param name="buffer">The memory buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="bufferAddress">The buffer address.</param>
        /// <param name="codeType">The variable code type.</param>
        /// <param name="address">The variable address.</param>
        /// <param name="name">The variable name.</param>
        /// <param name="path">The variable path.</param>
        public UserType(MemoryBuffer buffer, int offset, ulong bufferAddress, CodeType codeType, ulong address, string name = Variable.ComputedName, string path = Variable.UnknownPath)
            : base(codeType, address, name, path)
        {
            memoryBuffer = buffer;
            memoryBufferOffset = offset;
            memoryBufferAddress = bufferAddress;
        }

        /// <summary>
        /// Dictionary of possible conversions from .NET built-in type into C++ compiler generated string.
        /// </summary>
        private static readonly Dictionary<Type, string[]> builtinTypesToStringsMapping = new Dictionary<Type, string[]>()
        {
            { typeof(bool), new[] { "bool" } },
            { typeof(char), new[] { "signed char", "char", "char16_t", "char32_t", "wchar_t" } },
            { typeof(byte), new[] { "unsigned char", "uint8_t", "__uint8", "byte" } },
            { typeof(sbyte), new[] { "char", "int8_t", "__int8" } },
            { typeof(short), new[] { "short", "int16_t", "__int16" } },
            { typeof(ushort), new[] { "unsigned short", "uint16_t", "__uint16", "ushort" } },
            { typeof(int), new[] { "int", "int32_t", "__int32" } },
            { typeof(uint), new[] { "unsigned", "unsigned int", "uint32_t", "__uint32", "uint" } },
            { typeof(long), new[] { "long long", "int64_t", "__int64" } },
            { typeof(ulong), new[] { "unsigned long long", "uint64_t", "__uint64", "unsigned __int64", "ulong" } },
            { typeof(float), new[] { "float" } },
            { typeof(double), new[] { "double" } },
        };

        /// <summary>
        /// Gets <see cref="UserTypeMetadata"/> instance for the specified type instance.
        /// </summary>
        /// <param name="type">Type that has <see cref="UserTypeAttribute"/> with info about <see cref="CodeType"/>.</param>
        private static UserTypeMetadata GetTypeMetadataInstance(Type type)
        {
            IEnumerable<UserTypeMetadata> allMetadata = UserTypeMetadata.ReadFromType(type);

            if (!type.IsGenericType)
            {
                UserTypeMetadata metadata = allMetadata.First();

                return metadata;
            }

            // Convert all arguments to strings
            string[] argumentsAsStrings = new string[type.GenericTypeArguments.Length];

            for (int i = 0; i < argumentsAsStrings.Length; i++)
            {
                Type argumentType = type.GenericTypeArguments[i];

                if (argumentType.GetTypeInfo().IsSubclassOf(typeof(ITemplateConstant)))
                {
                    TemplateConstantAttribute attribute = argumentType.GetCustomAttribute<TemplateConstantAttribute>();

                    argumentsAsStrings[i] = attribute.String;
                }
                else if (argumentType == typeof(Variable))
                    continue;
                else if (argumentType.IsSubclassOf(typeof(Variable)))
                    argumentsAsStrings[i] = GetBaseClassString(argumentType);
                else // Built-in type
                    continue;
            }

            // Find one that matches them all
            foreach (UserTypeMetadata metadata in allMetadata)
            {
                string[] args = NativeCodeType.GetTemplateArgumentsStrings(metadata.TypeName);

                if (args.Length != argumentsAsStrings.Length)
                    continue;

                bool matches = true;

                for (int i = 0; i < args.Length && matches; i++)
                    if (argumentsAsStrings[i] != null)
                        matches = argumentsAsStrings[i] == args[i];
                for (int i = 0; i < args.Length && matches; i++)
                    if (argumentsAsStrings[i] == null)
                    {
                        Type argumentType = type.GenericTypeArguments[i];

                        if (argumentType == typeof(Variable))
                            continue; // Variable matches anything :)
                        else
                        {
                            string[] expectedStrings;

                            if (builtinTypesToStringsMapping.TryGetValue(argumentType, out expectedStrings))
                                if (expectedStrings.Contains(args[i]))
                                    continue;
                        }

                        // TODO: Fallback to searching symbol from module (metadata contains ModuleName, and later try to match code type with .NET type)
                        matches = false;
                    }
                if (matches)
                    return metadata;
            }

            // Not found :(
            return null;
        }

        /// <summary>
        /// Gets the base class string.
        /// </summary>
        /// <param name="baseClassType">Type of the base class.</param>
        public static string GetBaseClassString(Type baseClassType)
        {
            if (!baseClassType.IsSubclassOf(typeof(Variable)))
                throw new Exception("Specified type doesn't inherit Variable class");

            UserTypeMetadata metadata = GetTypeMetadataInstance(baseClassType);

            if (metadata != null)
                return metadata.TypeName;

            // Fallback to old code path: MyType<>
            IEnumerable<UserTypeMetadata> allMetadata = UserTypeMetadata.ReadFromType(baseClassType);
            string typeName = allMetadata.First().TypeName;

            return typeName.Substring(0, typeName.IndexOf('<')) + "<>";
        }

        /// <summary>
        /// Gets the <see cref="CodeType"/> for the specified <see cref="UserType"/> type instance.
        /// </summary>
        /// <param name="userType"><see cref="UserType"/> type instance.</param>
        /// <returns><see cref="CodeType"/> of the specified type instance.</returns>
        public static CodeType GetClassCodeType(Type userType)
        {
            UserTypeMetadata metadata = GetTypeMetadataInstance(userType);

            if (metadata != null && !string.IsNullOrEmpty(metadata.ModuleName) && !string.IsNullOrEmpty(metadata.TypeName))
                return CodeType.Create($"{metadata.ModuleName}!{metadata.TypeName}");
            return null;
        }

        /// <summary>
        /// Returns array of 16-bit unsigned integers converted from two bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static ushort[] ReadUshortArray(MemoryBuffer buffer, int offset, int elements)
        {
            ushort[] array = new ushort[elements];

            for (int i = 0; i < elements; i++, offset += 2)
                array[i] = ReadUshort(buffer, offset);
            return array;
        }


        /// <summary>
        /// Returns array of bool.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static bool[] ReadBoolArray(MemoryBuffer buffer, int offset, int elements)
        {
            bool[] array = new bool[elements];

            for (int i = 0; i < elements; i++, offset += 2)
                array[i] = ReadBool(buffer, offset);
            return array;
        }

        /// <summary>
        /// Returns array of 32-bit unsigned integers converted from four bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static uint[] ReadUintArray(MemoryBuffer buffer, int offset, int elements)
        {
            uint[] array = new uint[elements];

            for (int i = 0; i < elements; i++, offset += 4)
                array[i] = ReadUint(buffer, offset);
            return array;
        }

        /// <summary>
        /// Returns array of 64-bit unsigned integers converted from eight bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static ulong[] ReadUlongArray(MemoryBuffer buffer, int offset, int elements)
        {
            ulong[] array = new ulong[elements];

            for (int i = 0; i < elements; i++, offset += 8)
                array[i] = ReadUlong(buffer, offset);
            return array;
        }

        /// <summary>
        /// Returns array of 16-bit signed integers converted from two bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static short[] ReadShortArray(MemoryBuffer buffer, int offset, int elements)
        {
            short[] array = new short[elements];

            for (int i = 0; i < elements; i++, offset += 2)
                array[i] = ReadShort(buffer, offset);
            return array;
        }

        /// <summary>
        /// Returns array of 32-bit signed integers converted from four bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static int[] ReadIntArray(MemoryBuffer buffer, int offset, int elements)
        {
            int[] array = new int[elements];

            for (int i = 0; i < elements; i++, offset += 4)
                array[i] = ReadInt(buffer, offset);
            return array;
        }

        /// <summary>
        /// Returns array of 64-bit signed integers converted from eight bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static long[] ReadLongArray(MemoryBuffer buffer, int offset, int elements)
        {
            long[] array = new long[elements];

            for (int i = 0; i < elements; i++, offset += 8)
                array[i] = ReadLong(buffer, offset);
            return array;
        }

        /// <summary>
        /// Returns array of 8-bit unsigned integers converted from one byte at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static unsafe byte[] ReadByteArray(MemoryBuffer buffer, int offset, int elements)
        {
            byte[] array = new byte[elements];

            if (buffer.BytePointer != null)
            {
                fixed (byte* destination = array)
                {
                    byte* source = buffer.BytePointer + offset;
                    MemoryBuffer.MemCpy(destination, source, (uint)array.Length);
                }
            }
            else
                Array.Copy(buffer.Bytes, offset, array, 0, elements);
            return array;
        }

        /// <summary>
        /// Returns array of floats.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static float[] ReadFloatArray(MemoryBuffer buffer, int offset, int elements)
        {
            float[] array = new float[elements];

            for (int i = 0; i < elements; i++)
            {
                array[i] = ReadFloat(buffer, offset);
                offset += sizeof(float);
            }

            return array;
        }

        /// <summary>
        /// Returns array of floats.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        public static double[] ReadDoubleArray(MemoryBuffer buffer, int offset, int elements)
        {
            double[] array = new double[elements];

            for (int i = 0; i < elements; i++)
            {
                array[i] = ReadDouble(buffer, offset);
                offset += sizeof(float);
            }

            return array;
        }

        /// <summary>
        /// Returns array of 8/16-bit unsigned integers converted from one/two bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        /// <param name="charSize">Size of the character.</param>
        public static char[] ReadCharArray(MemoryBuffer buffer, int offset, int elements, int charSize)
        {
            char[] array = new char[elements];

            if (charSize == 1)
                for (int i = 0; i < elements; i++)
                    array[i] = (char)ReadByte(buffer, offset + i);
            else if (charSize == 2)
                for (int i = 0; i < elements; i++)
                    array[i] = (char)ReadUshort(buffer, offset + 2 * i);
            else
                throw new ArgumentOutOfRangeException(nameof(charSize), "Unsupported char size: " + charSize);
            return array;
        }

        /// <summary>
        /// Returns array of 64-bit unsigned integers converted from four/eight bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The memory buffer.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="elements">The number of elements to be read.</param>
        /// <param name="pointerSize">Size of the pointer.</param>
        /// <exception cref="System.Exception">Unsupported pointer size</exception>
        public static ulong[] ReadPointerArray(MemoryBuffer buffer, int offset, int elements, uint pointerSize)
        {
            ulong[] array = new ulong[elements];

            if (pointerSize == 4)
                for (int i = 0; i < elements; i++, offset += 4)
                    array[i] = ReadUint(buffer, offset);
            else if (pointerSize == 8)
                for (int i = 0; i < elements; i++, offset += 8)
                    array[i] = ReadUlong(buffer, offset);
            else
                throw new ArgumentOutOfRangeException(nameof(pointerSize), "Unsupported pointer size: " + pointerSize);
            return array;
        }

        /// <summary>
        /// Returns a 16-bit unsigned integer converted from two bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        public static unsafe ushort ReadUshort(MemoryBuffer buffer, int offset, int bits = 16, int bitsOffset = 0)
        {
            ushort value;

            if (buffer.BytePointer != null)
                value = *((ushort*)(buffer.BytePointer + offset));
            else
                value = BitConverter.ToUInt16(buffer.Bytes, offset);

            if (bits != 16 || bitsOffset != 0)
                value = (ushort)((value >> bitsOffset) & ((1 << bits) - 1));
            return value;
        }

        /// <summary>
        /// Returns a 32-bit unsigned integer converted from four bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        public static unsafe uint ReadUint(MemoryBuffer buffer, int offset, int bits = 32, int bitsOffset = 0)
        {
            uint value;

            if (buffer.BytePointer != null)
                value = *((uint*)(buffer.BytePointer + offset));
            else
                value = BitConverter.ToUInt32(buffer.Bytes, offset);

            if (bits != 32 || bitsOffset != 0)
                value = (uint)((value >> bitsOffset) & ((1 << bits) - 1));
            return value;
        }

        /// <summary>
        /// Returns a 64-bit unsigned integer converted from eight bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        public static unsafe ulong ReadUlong(MemoryBuffer buffer, int offset, int bits = 64, int bitsOffset = 0)
        {
            ulong value;

            if (buffer.BytePointer != null)
                value = *((ulong*)(buffer.BytePointer + offset));
            else
                value = BitConverter.ToUInt64(buffer.Bytes, offset);

            if (bits != 64 || bitsOffset != 0)
                value = (ulong)((value >> bitsOffset) & ((1UL << bits) - 1));
            return value;
        }

        /// <summary>
        /// Returns a 16-bit signed integer converted from two bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        public static unsafe short ReadShort(MemoryBuffer buffer, int offset, int bits = 16, int bitsOffset = 0)
        {
            short value;

            if (buffer.BytePointer != null)
                value = *((short*)(buffer.BytePointer + offset));
            else
                value = BitConverter.ToInt16(buffer.Bytes, offset);

            if (bits != 16 || bitsOffset != 0)
                value = (short)((value >> bitsOffset) & ((1 << bits) - 1));
            return value;
        }

        /// <summary>
        /// Returns a 32-bit signed integer converted from four bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        public static unsafe int ReadInt(MemoryBuffer buffer, int offset, int bits = 32, int bitsOffset = 0)
        {
            int value;

            if (buffer.BytePointer != null)
                value = *((int*)(buffer.BytePointer + offset));
            else
                value = BitConverter.ToInt32(buffer.Bytes, offset);

            if (bits != 32 || bitsOffset != 0)
                value = (int)((value >> bitsOffset) & ((1 << bits) - 1));
            return value;
        }

        /// <summary>
        /// Returns a 64-bit signed integer converted from eight bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        public static unsafe long ReadLong(MemoryBuffer buffer, int offset, int bits = 64, int bitsOffset = 0)
        {
            long value;

            if (buffer.BytePointer != null)
                value = *((long*)(buffer.BytePointer + offset));
            else
                value = BitConverter.ToInt64(buffer.Bytes, offset);

            if (bits != 64 || bitsOffset != 0)
                value = (long)((value >> bitsOffset) & ((1L << bits) - 1));
            return value;
        }

        /// <summary>
        /// Returns a single-precision floating point number converted from hour bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        public static unsafe float ReadFloat(MemoryBuffer buffer, int offset)
        {
            if (buffer.BytePointer != null)
                return *((float*)(buffer.BytePointer + offset));
            else
                return BitConverter.ToSingle(buffer.Bytes, offset);
        }

        /// <summary>
        /// Returns a double-precision floating point number converted from eight bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        public static unsafe double ReadDouble(MemoryBuffer buffer, int offset)
        {
            if (buffer.BytePointer != null)
                return *((double*)(buffer.BytePointer + offset));
            else
                return BitConverter.ToDouble(buffer.Bytes, offset);
        }

        /// <summary>
        /// Returns a Boolean value converted from one byte at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        public static unsafe bool ReadBool(MemoryBuffer buffer, int offset, int bits = 8, int bitsOffset = 0)
        {
            byte value;

            if (buffer.BytePointer != null)
                value = *(buffer.BytePointer + offset);
            else
                value = buffer.Bytes[offset];

            if (bits != 8 || bitsOffset != 0)
                value = (byte)((value >> bitsOffset) & ((1 << bits) - 1));
            return value != 0;
        }

        /// <summary>
        /// Returns a 8-bit signed integer converted from one byte at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        public static unsafe byte ReadByte(MemoryBuffer buffer, int offset, int bits = 8, int bitsOffset = 0)
        {
            byte value;

            if (buffer.BytePointer != null)
                value = *(buffer.BytePointer + offset);
            else
                value = buffer.Bytes[offset];

            if (bits != 8 || bitsOffset != 0)
                value = (byte)((value >> bitsOffset) & ((1 << bits) - 1));
            return value;
        }

        /// <summary>
        /// Returns a signed byte.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        public static unsafe sbyte ReadSbyte(MemoryBuffer buffer, int offset)
        {
            if (buffer.BytePointer != null)
                return *((sbyte*)(buffer.BytePointer + offset));
            else
                return (sbyte)buffer.Bytes[offset];
        }

        /// <summary>
        /// Returns a single character.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        public static char ReadChar(MemoryBuffer buffer, int offset)
        {
            return (char)ReadByte(buffer, offset);
        }

        /// <summary>
        /// Returns a single character.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="bits">The number of bits to interpret.</param>
        /// <param name="bitsOffset">The offset in bits.</param>
        public static char ReadChar(MemoryBuffer buffer, int offset, int bits, int bitsOffset)
        {
            return (char)ReadByte(buffer, offset, bits, bitsOffset);
        }

        /// <summary>
        /// Returns a 64-bit unsigned integer converted from four/eight bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="buffer">The array of bytes.</param>
        /// <param name="offset">The starting position within value.</param>
        /// <param name="pointerSize">Size of the pointer.</param>
        /// <exception cref="System.Exception">Unsupported pointer size</exception>
        public static ulong ReadPointer(MemoryBuffer buffer, int offset, int pointerSize)
        {
            if (pointerSize == 4)
                return ReadUint(buffer, offset);
            else if (pointerSize == 8)
                return ReadUlong(buffer, offset);
            else
                throw new ArgumentOutOfRangeException(nameof(pointerSize), "Unsupported pointer size: " + pointerSize);
        }

        /// <summary>
        /// Reads the ANSI/Unicode string from the specified address.
        /// </summary>
        /// <param name="process">The process.</param>
        /// <param name="address">The memory address.</param>
        /// <param name="charSize">Size of the character.</param>
        /// <param name="length">The length. If length is -1, string is null terminated.</param>
        public static string ReadString(Process process, ulong address, int charSize, int length = -1)
        {
            if (address == 0)
                return null;

            return process.ReadString(address, charSize, length);
        }

        /// <summary>
        /// Reads the pointer and casts it to the type.
        /// </summary>
        /// <typeparam name="T">Type to be casted to</typeparam>
        /// <param name="thisClass">Variable that contains UserMember of the this class.</param>
        /// <param name="classFieldName">Name of the class field.</param>
        /// <param name="buffer">The memory buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="pointerSize">Size of the pointer.</param>
        public static T ReadPointer<T>(UserMember<Variable> thisClass, string classFieldName, MemoryBuffer buffer, int offset, int pointerSize)
        {
            ulong pointer = ReadPointer(buffer, offset, pointerSize);

            if (pointer == 0)
            {
                return default(T);
            }

            return Variable.CreatePointerNoCast(thisClass.Value.GetCodeType().GetClassFieldType(classFieldName), pointer, classFieldName).CastAs<T>();
        }

        /// <summary>
        /// Reads the pointer and casts it to the type.
        /// </summary>
        /// <typeparam name="T">Type to be casted to</typeparam>
        /// <param name="classCodeType">The class code type.</param>
        /// <param name="classFieldName">Name of the class field.</param>
        /// <param name="buffer">The memory buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="pointerSize">Size of the pointer.</param>
        public static T ReadPointer<T>(CodeType classCodeType, string classFieldName, MemoryBuffer buffer, int offset, int pointerSize)
        {
            ulong pointer = ReadPointer(buffer, offset, pointerSize);

            if (pointer == 0)
            {
                return default(T);
            }

            return Variable.CreatePointerNoCast(classCodeType.GetClassFieldType(classFieldName), pointer, classFieldName).CastAs<T>();
        }

        /// <summary>
        /// Gets the variable that is casted to base class.
        /// </summary>
        /// <param name="baseClassIndex">Index of the base class by looking at the offset.</param>
        /// <param name="codeType">The current code type.</param>
        /// <param name="offset">The current code type offset.</param>
        /// <param name="mciAuxiliaryClass">The multi-class-inheritance auxiliary class type.</param>
        internal object GetBaseClass(int baseClassIndex, CodeType codeType, int offset, Type mciAuxiliaryClass)
        {
            var tuple = codeType.InheritedClassesSorted[baseClassIndex];
            object instance = GetBaseClass(Tuple.Create(tuple.Item1, tuple.Item2 + offset)).CastAs(mciAuxiliaryClass);

            ((IMultiClassInheritance)instance).DowncastParent = this;
            return instance;
        }
    }
}

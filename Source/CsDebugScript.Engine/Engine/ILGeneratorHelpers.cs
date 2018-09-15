using System;
using System.Reflection;
using System.Reflection.Emit;

namespace CsDebugScript.Engine
{
    /// <summary>
    /// Helper methods for IL generation.
    /// </summary>
    internal static class ILGeneratorHelpers
    {
        /// <summary>
        /// Emits IL code for the specified constant.
        /// </summary>
        /// <param name="il">The IL generator.</param>
        /// <param name="value">The constant value.</param>
        public static void EmitConstant(this ILGenerator il, object value)
        {
            Type type = value.GetType();

            if (type == typeof(bool))
                EmitConstant(il, (bool)value ? 1 : 0);
            else if (type == typeof(int))
                EmitConstant(il, (int)value);
            else if (type == typeof(uint))
                EmitConstant(il, (uint)value);
            else if (type == typeof(long))
                EmitConstant(il, (long)value);
            else if (type == typeof(ulong))
                EmitConstant(il, (ulong)value);
            else if (type == typeof(string))
                il.Emit(OpCodes.Ldstr, (string)value);
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// Emits IL code for the specified constant.
        /// </summary>
        /// <param name="il">The IL generator.</param>
        /// <param name="value">The constant value.</param>
        public static void EmitConstant(this ILGenerator il, int value)
        {
            if (value == 0)
                il.Emit(OpCodes.Ldc_I4_0);
            else if (value == 1)
                il.Emit(OpCodes.Ldc_I4_1);
            else if (value == 2)
                il.Emit(OpCodes.Ldc_I4_2);
            else if (value == 3)
                il.Emit(OpCodes.Ldc_I4_3);
            else if (value == 4)
                il.Emit(OpCodes.Ldc_I4_4);
            else if (value == 5)
                il.Emit(OpCodes.Ldc_I4_5);
            else if (value == 6)
                il.Emit(OpCodes.Ldc_I4_6);
            else if (value == 7)
                il.Emit(OpCodes.Ldc_I4_7);
            else if (value == 8)
                il.Emit(OpCodes.Ldc_I4_8);
            else if (value == -1)
                il.Emit(OpCodes.Ldc_I4_M1);
            else if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
                il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
            else
                il.Emit(OpCodes.Ldc_I4, value);
        }

        /// <summary>
        /// Emits IL code for the specified constant.
        /// </summary>
        /// <param name="il">The IL generator.</param>
        /// <param name="value">The constant value.</param>
        public static void EmitConstant(this ILGenerator il, uint value)
        {
            EmitConstant(il, (int)value);
        }

        /// <summary>
        /// Emits IL code for the specified constant.
        /// </summary>
        /// <param name="il">The IL generator.</param>
        /// <param name="value">The constant value.</param>
        public static void EmitConstant(this ILGenerator il, long value)
        {
            if (value >= int.MinValue && value <= int.MaxValue)
            {
                EmitConstant(il, (int)value);
                il.Emit(OpCodes.Conv_I8);
            }
            else
                il.Emit(OpCodes.Ldc_I8, value);
        }

        /// <summary>
        /// Emits IL code for the specified constant.
        /// </summary>
        /// <param name="il">The IL generator.</param>
        /// <param name="value">The constant value.</param>
        public static void EmitConstant(this ILGenerator il, ulong value)
        {
            EmitConstant(il, (long)value);
        }

        /// <summary>
        /// Loads arguments to the stack.
        /// </summary>
        /// <param name="il">The IL generator.</param>
        /// <param name="argumentsCount">Number of arguments to be loaded.</param>
        /// <param name="skipArguments">Number of arguments to be skiped during load.</param>
        public static void ForwardArguments(this ILGenerator il, int argumentsCount, int skipArguments = 0)
        {
            if (skipArguments <= 0 && argumentsCount > 0)
                il.Emit(OpCodes.Ldarg_0);
            if (skipArguments <= 1 && argumentsCount > 1)
                il.Emit(OpCodes.Ldarg_1);
            if (skipArguments <= 2 && argumentsCount > 2)
                il.Emit(OpCodes.Ldarg_2);
            if (skipArguments <= 3 && argumentsCount > 3)
                il.Emit(OpCodes.Ldarg_3);
            for (int i = Math.Max(4, skipArguments); i < argumentsCount; i++)
                il.Emit(OpCodes.Ldarg_S, (byte)i);
        }

        /// <summary>
        /// Prepares arguments for method call by loading arguments and default values to the stack.
        /// </summary>
        /// <param name="il">The IL generator.</param>
        /// <param name="forwardedArgumentsCount">Number of arguments to be forwarded.</param>
        /// <param name="methodParameters">Parameters of method to be called.</param>
        public static void PrepareMethodCall(this ILGenerator il, int forwardedArgumentsCount, ParameterInfo[] methodParameters)
        {
            il.ForwardArguments(forwardedArgumentsCount);
            for (int i = forwardedArgumentsCount; i < methodParameters.Length; i++)
                il.EmitConstant(methodParameters[i].DefaultValue);
        }
    }
}

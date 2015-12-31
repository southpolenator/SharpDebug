using CsScriptManaged.Marshaling;
using DbgEngManaged;
using System.Text;

namespace CsScriptManaged.Native
{
    /// <summary>
    /// Extensions for debugger interfaces for easier manipulation
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Requests the specified request type.
        /// </summary>
        /// <param name="client">The DbgEng.dll Advanced interface.</param>
        /// <param name="requestType">Type of the request.</param>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public static EXT_TYPED_DATA Request(this IDebugAdvanced3 client, DebugRequest requestType, EXT_TYPED_DATA request)
        {
            using (var requestNative = new MarshalStructure<EXT_TYPED_DATA>())
            {
                uint outSize;

                requestNative.Structure = request;
                Context.Advanced.Request((uint)requestType, requestNative.Pointer, requestNative.USize, requestNative.Pointer, requestNative.USize, out outSize);
                return requestNative.Structure;
            }
        }

        /// <summary>
        /// Requests the specified request type with extended structure.
        /// </summary>
        /// <param name="client">The DbgEng.dll Advanced interface.</param>
        /// <param name="requestType">Type of the request.</param>
        /// <param name="request">The request.</param>
        /// <param name="extended">The extended string.</param>
        public static EXT_TYPED_DATA RequestExtended(this IDebugAdvanced3 client, DebugRequest requestType, EXT_TYPED_DATA request, string extended)
        {
            using (var requestNative = new MarshalStructureExtendedWithAnsiString<EXT_TYPED_DATA>())
            {
                uint outSize;

                requestNative.Extended = extended;
                requestNative.Structure = request;
                Context.Advanced.Request((uint)requestType, requestNative.Pointer, requestNative.USize, requestNative.Pointer, requestNative.USize, out outSize);
                return requestNative.Structure;
            }
        }

        /// <summary>
        /// Gets the name of the current process executable.
        /// </summary>
        /// <param name="systemObjects">The DbgEng.dll System objects interface.</param>
        public static string GetCurrentProcessExecutableName(this IDebugSystemObjects4 systemObjects)
        {
            uint exeSize;
            StringBuilder sb = new StringBuilder(Constants.MaxFileName);

            systemObjects.GetCurrentProcessExecutableNameWide(sb, (uint)sb.Capacity, out exeSize);
            return sb.ToString();
        }
    }
}

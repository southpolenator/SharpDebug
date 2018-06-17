using CsDebugScript.Exceptions;
using System.Linq;

namespace CsDebugScript.CommonUserTypes.NativeTypes.cv
{
    /// <summary>
    /// Implementation of cv::Mat class.
    /// </summary>
    [UserType(TypeName = "cv::Mat")]
    public class Mat : DynamicSelfUserType
    {
        /// <summary>
        /// Magic value used to verify that cv::Mat flags is correct.
        /// </summary>
        public const int MAGIC_VAL = 0x42FF0000;

        /// <summary>
        /// Mask used to extract magic value.
        /// </summary>
        public const uint MAGIC_MASK = 0xFFFF0000;

        /// <summary>
        /// Flag to check if matrix is continuous.
        /// </summary>
        public const int CONTINUOUS_FLAG = 16384;

        /// <summary>
        /// Flag to check if matrix is submatrix.
        /// </summary>
        public const int SUBMATRIX_FLAG = 32768;

        /// <summary>
        /// Mask to extract matrix element type.
        /// </summary>
        public const int TYPE_MASK = 0x00000FFF;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeType" /> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public Mat(Variable variable)
            : base(variable)
        {
            // Verify code type
            if (!VerifyCodeType(variable.GetCodeType()))
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "cv::Mat");
            }
        }

        /// <summary>
        /// Checks whether cv::Mat is initialized correctly.
        /// </summary>
        public bool IsCorrect => ((int)self.flags & MAGIC_MASK) == MAGIC_VAL;

        /// <summary>
        /// The matrix dimensionality, >= 2
        /// </summary>
        public int Dimensions => (int)self.dims;

        /// <summary>
        /// Array of integers specifying an n-dimensional array shape.
        /// </summary>
        public int[] Sizes => ReadIntArray(self.size.p, Dimensions);

        /// <summary>
        /// Gets the per dimension step (or stride) in bytes.
        /// </summary>
        public int[] Steps => ReadIntArray(self.step.p, Dimensions);

        /// <summary>
        /// Gets matrix element type.
        /// </summary>
        public MatType Type => MatType.ConvertNativeType((int)self.flags & TYPE_MASK);

        /// <summary>
        /// Number of bytes needed to store one element of the matrix.
        /// </summary>
        public int ElementSize => Steps.LastOrDefault();

        /// <summary>
        /// Reports whether the matrix is continuous or not.
        /// </summary>
        public bool IsContinuous => ((int)self.flags & CONTINUOUS_FLAG) != 0;

        /// <summary>
        /// <c>true</c> if the matrix is a submatrix of another matrix.
        /// </summary>
        public bool IsSubmatrix => ((int)self.flags & SUBMATRIX_FLAG) != 0;

        /// <summary>
        /// void* pointer where elements are located.
        /// </summary>
        public new NakedPointer Data => new NakedPointer(GetCodeType().Module.Process, self.data.GetPointerAddress());

        /// <summary>
        /// Reads integer array.
        /// </summary>
        /// <param name="array">Variable that represents array.</param>
        /// <param name="length">Lenght of the array</param>
        private static int[] ReadIntArray(Variable array, int length)
        {
            int[] result = new int[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = (int)array.GetArrayElement(i);
            }
            return result;
        }

        /// <summary>
        /// Verifies if the specified code type is correct for this class.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        private bool VerifyCodeType(CodeType codeType)
        {
            // We want to have this kind of hierarchy
            // flags
            // dims
            // size
            // | p
            // step
            // | p
            // data
            CodeType flags, dims, size, sizep, step, stepp, data;

            var fields = codeType.GetFieldTypes();

            if (!fields.TryGetValue("flags", out flags) || !fields.TryGetValue("dims", out dims) || !fields.TryGetValue("size", out size) || !fields.TryGetValue("step", out step) || !fields.TryGetValue("data", out data))
                return false;

            if (!size.GetFieldTypes().TryGetValue("p", out sizep) || !step.GetFieldTypes().TryGetValue("p", out stepp))
                return false;

            return true;
        }
    }
}

using CsDebugScript.Drawing.Interfaces;
using CsDebugScript.Exceptions;

namespace CsDebugScript.CommonUserTypes.NativeTypes.cv
{
    /// <summary>
    /// Implementation of CvMat class.
    /// </summary>
    [UserType(TypeName = "CvMat")]
    public class CvMat : DynamicSelfUserType, IDrawingVisualizerObject
    {
        /// <summary>
        /// Magic value used to verify that CvMat type is correct.
        /// </summary>
        public const int MAGIC_VAL = 0x42420000;

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
        public CvMat(Variable variable)
            : base(variable)
        {
            // Verify code type
            if (!VerifyCodeType(variable.GetCodeType()))
            {
                throw new WrongCodeTypeException(variable, nameof(variable), "CvMat");
            }
        }

        /// <summary>
        /// Checks whether cv::Mat is initialized correctly.
        /// </summary>
        public bool IsCorrect => ((int)self.type & MAGIC_MASK) == MAGIC_VAL;

        /// <summary>
        /// The matrix dimensionality, always returns 2
        /// </summary>
        public int Dimensions => 2;

        /// <summary>
        /// Array of integers specifying an n-dimensional array shape.
        /// </summary>
        public int[] Sizes => new int[] { Rows, Columns };

        /// <summary>
        /// Gets the per dimension step (or stride) in bytes.
        /// </summary>
        public int[] Steps => new int[] { Step, ElementSize };

        /// <summary>
        /// Gets matrix element type.
        /// </summary>
        public MatType Type => MatType.ConvertNativeType((int)self.type & TYPE_MASK);

        /// <summary>
        /// Number of bytes needed to store one element of the matrix.
        /// </summary>
        public int ElementSize => Step / Columns;

        /// <summary>
        /// Reports whether the matrix is continuous or not.
        /// </summary>
        public bool IsContinuous => ((int)self.type & CONTINUOUS_FLAG) != 0;

        /// <summary>
        /// <c>true</c> if the matrix is a submatrix of another matrix.
        /// </summary>
        public bool IsSubmatrix => ((int)self.type & SUBMATRIX_FLAG) != 0;

        /// <summary>
        /// void* pointer where elements are located.
        /// </summary>
        public new NakedPointer Data => new NakedPointer(GetCodeType().Module.Process, self.data.ptr.GetPointerAddress());

        /// <summary>
        /// Number of rows in this matrix.
        /// </summary>
        public int Rows => (int)self.rows;

        /// <summary>
        /// Number of columns in this matrix.
        /// </summary>
        public int Columns => (int)self.cols;

        /// <summary>
        /// Gets the step (or stride) in bytes.
        /// </summary>
        public int Step => (int)self.step;

        /// <summary>
        /// Cheks if data is correct and object can be visualized as a drawing.
        /// </summary>
        /// <returns><c>true</c> if data is correct and object can be visualized as a drawing.</returns>
        public bool CanVisualize()
        {
            return IsCorrect && Rows > 0 && Columns > 0 && !Data.IsNull;
        }

        /// <summary>
        /// Creates drawing that should be visualized.
        /// </summary>
        /// <param name="graphics">Graphics object used to create drawings.</param>
        /// <returns>Drawing object that should be visualized.</returns>
        public IDrawing CreateDrawing(IGraphics graphics)
        {
            return Mat.CreateDrawing(graphics, Columns, Rows, Data, Step, Type);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (!IsCorrect)
                return "[not initialized]";
            if (!CanVisualize())
                return "[empty]";
            return $"{Rows} x {Columns} x {Type}";
        }

        /// <summary>
        /// Verifies if the specified code type is correct for this class.
        /// </summary>
        /// <param name="codeType">The code type.</param>
        private bool VerifyCodeType(CodeType codeType)
        {
            // We want to have this kind of hierarchy
            // type
            // rows
            // cols
            // data
            // | ptr
            // step
            CodeType type, rows, cols, step, data, ptr;

            var fields = codeType.GetFieldTypes();

            if (!fields.TryGetValue("type", out type) || !fields.TryGetValue("rows", out rows) || !fields.TryGetValue("cols", out cols) || !fields.TryGetValue("step", out step) || !fields.TryGetValue("data", out data))
                return false;

            if (!data.GetFieldTypes().TryGetValue("ptr", out ptr))
                return false;

            return true;
        }
    }
}

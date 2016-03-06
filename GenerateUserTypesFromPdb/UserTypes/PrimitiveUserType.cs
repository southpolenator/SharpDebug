namespace GenerateUserTypesFromPdb.UserTypes
{
    class PrimitiveUserType : UserType
    {
        private string typeName;

        public PrimitiveUserType(string typeName, Symbol symbol)
            : base(symbol, null, null, null)
        {
            this.typeName = typeName;
        }

        public override string ClassName
        {
            get
            {
                return typeName;
            }
        }

        public override string FullClassName
        {
            get
            {
                return typeName;
            }
        }
    }
}

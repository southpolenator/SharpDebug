namespace CsDebugScript
{
    /// <summary>
    /// Interface that says that user type has VTable and that it is safe to do downcasting.
    /// It is preferred to let CodeGen generate this attribute for you.
    /// </summary>
    public interface ICastableObject
    {
    }
}

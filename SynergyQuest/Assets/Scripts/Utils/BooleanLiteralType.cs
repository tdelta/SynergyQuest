namespace Utils
{
    public interface BooleanLiteralType
    {
        bool Value { get; }
    }

    public class TrueLiteralType : BooleanLiteralType
    {
        public bool Value => true;
    }
    
    public class FalseLiteralType : BooleanLiteralType
    {
        public bool Value => false;
    }
}
public static class MathfExtensions
{
    public static bool Approximately(float a, float b, float epsilon)
    {
        return a > b - epsilon && a < b + epsilon;
    }
}

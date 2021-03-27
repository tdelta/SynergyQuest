using System;

namespace Utils
{
    /**
     * Extension functions for the C# root <see cref="Object"/> type
     */
    public static class ObjectExtensions
    {
        /**
         * True if the given object reference is equal to null.
         * This is the same as <c>ReferenceEquals(obj, null)</c>
         */
        public static bool IsNull(this Object obj)
        {
            return ReferenceEquals(obj, null);
        }

        /**
         * True if the given object reference is not equal to null.
         * This is the same as <c>!ReferenceEquals(obj, null)</c>
         */
        public static bool IsNotNull(this Object obj)
        {
            return !obj.IsNull();
        }
    }
}
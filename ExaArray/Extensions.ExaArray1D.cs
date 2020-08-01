using System;

namespace Exa
{
    /// <summary>
    /// Extension methods for <c>ExaArray1D</c>.
    /// </summary>
    public static class ExtensionsExaArray1D
    {
        /// <summary>
        /// Creates a new ExaArray1D from this instance. 
        /// </summary>
        /// <remarks>
        /// When <c>T</c> is a value type, data gets copied as values. When <c>T</c> is a reference type, the pointers
        /// to the original objects are copied. Thus, this factory method does not create a deep copy.
        ///
        /// Performance: O(n)
        /// </remarks>
        /// <param name="other">The instance from which the new instance is to be created.</param>
        /// <returns>The new instance</returns>
        public static ExaArray1D<T> Clone<T>(this ExaArray1D<T> other) => ExaArray1D<T>.CreateFrom(other);

        /// <summary>
        /// Creates a new ExaArray1D from this instance, respecting the given range.
        /// </summary>
        /// <remarks>
        /// When <c>T</c> is a value type, data gets copied as values. When <c>T</c> is a reference type, the pointers
        /// to the original objects are copied. Thus, this factory method does not create a deep copy.
        ///
        /// Performance: O(n)
        ///
        /// The indices are inclusive.
        /// </remarks>
        /// <param name="other">The instance from which the new instance is to be created.</param>
        /// <param name="indexFrom">The first source element which should be part of the new array.</param>
        /// <param name="indexTo">The last source element which should be part of the new array.</param>
        /// <returns>The new instance</returns>
        /// <exception cref="IndexOutOfRangeException">Throws, when one or both of the indices are out of range.</exception>
        public static ExaArray1D<T> Clone<T>(this ExaArray1D<T> other, ulong indexFrom, ulong indexTo) => ExaArray1D<T>.CreateFrom(other, indexFrom, indexTo);
    }
}
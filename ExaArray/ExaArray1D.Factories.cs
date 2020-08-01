using System;

namespace Exa
{
    public partial class ExaArray1D<T>
    {
        /// <summary>
        /// Creates a new ExaArray1D from another. 
        /// </summary>
        /// <remarks>
        /// When <c>T</c> is a value type, data gets copied as values. When <c>T</c> is a reference type, the pointers
        /// to the original objects are copied. Thus, this factory method does not create a deep copy.
        ///
        /// Performance: O(n)
        /// </remarks>
        /// <param name="other">The instance from which the new instance is to be created.</param>
        /// <returns>The new instance</returns>
        public static ExaArray1D<T> CreateFrom(ExaArray1D<T> other)
        {
            var next = new ExaArray1D<T>(other.OptimizationStrategy)
            {
                Length = other.Length,
                chunks = new T[other.chunks.Length][]
            };
            
            for (var n = 0; n < other.chunks.Length; n++)
            {
                next.chunks[n] =  new T[other.chunks[n].Length];
                Array.Copy(other.chunks[n], next.chunks[n], other.chunks[n].Length);
            }
            
            return next;
        }
    }
}
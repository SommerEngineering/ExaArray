using System.Collections.Generic;

namespace Exa
{
    /// <summary>
    /// Contains extension methods.
    /// </summary>
    public static class ExtensionsFramework
    {
        /// <summary>
        /// Creates a new ExaArray1D from this collection of items.
        /// </summary>
        /// <remarks>
        /// Performance: O(n)
        /// </remarks>
        /// <param name="collection">The collection to use</param>
        /// <param name="strategy">The optional optimization strategy.</param>
        /// <returns>The desired instance</returns>
        public static ExaArray1D<T> AsExaArray<T>(this ICollection<T> collection, Strategy strategy = Strategy.MAX_PERFORMANCE) => ExaArray1D<T>.CreateFrom(collection, strategy);

        /// <summary>
        /// Creates a new ExaArray1D from this enumerable sequence of items. The number of items in the sequence is __known__.
        /// </summary>
        /// <remarks>
        /// Creates an array with <c>length</c> items. When this sequence contains less elements, the remaining values are <c>default(T)</c>. 
        /// When the sequence contains more elements, these additional elements getting ignored.
        /// 
        /// Performance: O(n)
        /// </remarks>
        /// <param name="sequence">The sequence to consume in order to create the array.</param>
        /// <param name="length">The number of elements in the sequence. When the sequence contains more elements, these additional elements are ignored.</param>
        /// <param name="strategy">The optional optimization strategy.</param>
        /// <returns>The desired instance</returns>
        public static ExaArray1D<T> AsExaArray<T>(this IEnumerable<T> sequence, ulong length, Strategy strategy = Strategy.MAX_PERFORMANCE) => ExaArray1D<T>.CreateFrom(sequence, length, strategy);

        /// <summary>
        /// Creates a new ExaArray1D from this enumerable sequence of items. The number of items in the sequence is __unknown__.
        /// </summary>
        /// <remarks>
        /// This method is slow because the number of items in this sequence is unknown. When you know the
        /// number of items, you should use another factory method, where the number of items can be provided.
        ///
        /// Performance: O(n)
        /// </remarks>
        /// <param name="sequence">The sequence to consume in order to create the array.</param>
        /// <param name="strategy">The optional optimization strategy.</param>
        /// <returns>The desired instance</returns>
        public static ExaArray1D<T> AsExaArray<T>(this IEnumerable<T> sequence, Strategy strategy = Strategy.MAX_PERFORMANCE) => ExaArray1D<T>.CreateFrom(sequence, strategy);
    }
}
using System;

namespace Exa
{
    /// <summary>
    /// The two-dimensional exa-scale array. Can grow up to 18,446,744,073,709,551,615 elements in total.
    /// </summary>
    public sealed class ExaArray2D<T>
    {
        /// <summary>
        /// The total number of possible elements.
        /// </summary>
        public const ulong MAX_NUMBER_ELEMENTS = ulong.MaxValue;

        private ulong sumLengthOrdinates = 0;
        
        // Chunk storage:
        private ExaArray1D<ExaArray1D<T>> chunks = new ExaArray1D<ExaArray1D<T>>(Strategy.MAX_PERFORMANCE);
        
        /// <summary>
        /// Returns the current total number of elements across all dimensions. 
        /// </summary>
        /// <remarks>
        /// Performance: O(1)
        /// </remarks>
        public ulong Length => this.sumLengthOrdinates;

        /// <summary>
        /// Gets or sets an element of the array.
        /// </summary>
        /// <remarks>
        /// Getting a value: When asking for elements which are not yet allocated, returns <c>default(T)</c>. Performance: O(1).
        /// 
        /// Setting a value: The underlying data structure gets extended on demand as necessary. The array can and will grow
        /// on demand per index. For example, the abscissa index 5 might have allocated memory for 15 elements while abscissa
        /// index 16 have allocated memory for 1,000 elements.
        ///
        /// Performance, when the memory is already allocated: O(1)
        /// Performance to extend on demand: O(n)
        ///
        /// On the abscissa, you extend up to 1,152,921,504,606,850,000 entries. Across all dimensions, you can have 18,446,744,073,709,551,615
        /// elements on total.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Throws, when you tried to extend more than 1,152,921,504,606,850,000 elements on the abscissa
        /// or you tried to extend to more than 18,446,744,073,709,551,615 elements in total.</exception>
        public T this[ulong indexAbscissa, ulong indexOrdinate]
        {
            get
            {
                if (this.chunks.Length == 0 || indexAbscissa >= this.chunks.Length || indexOrdinate >= this.chunks[indexAbscissa]?.Length)
                    return default(T);

                return this.chunks[indexAbscissa][indexOrdinate];
            }

            set
            {
                if(this.chunks.Length == 0 || indexAbscissa >= this.chunks.Length)
                    this.chunks.Extend(indexAbscissa - this.chunks.Length + 1);
                
                this.chunks[indexAbscissa] ??= new ExaArray1D<T>(Strategy.MAX_PERFORMANCE);
                if(this.chunks[indexAbscissa].Length == 0 || indexOrdinate >= this.chunks[indexAbscissa].Length - 1)
                {
                    var extendBy = (indexOrdinate - this.chunks[indexAbscissa].Length) + 1;
                    if(extendBy > MAX_NUMBER_ELEMENTS - this.sumLengthOrdinates)
                        throw new ArgumentOutOfRangeException($"It is not possible to extend more than {MAX_NUMBER_ELEMENTS} total elements across all dimensions.");
                    
                    this.chunks[indexAbscissa].Extend(extendBy);
                    this.sumLengthOrdinates += extendBy;
                }

                this.chunks[indexAbscissa][indexOrdinate] = value;
            }
        }
    }
}
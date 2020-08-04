using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Exa
{
    /// <summary>
    /// The two-dimensional exa-scale array. Can grow up to 18,446,744,073,709,551,615 elements in total.
    /// </summary>
    public sealed class ExaArray2D<T> : ISerializable
    {
        /// <summary>
        /// The total number of possible elements.
        /// </summary>
        public const ulong MAX_NUMBER_ELEMENTS = ulong.MaxValue;

        private ulong sumLengthOrdinates = 0;
        
        // Chunk storage:
        private readonly ExaArray1D<ExaArray1D<T>> chunks = new ExaArray1D<ExaArray1D<T>>(Strategy.MAX_PERFORMANCE);

        /// <summary>
        /// Constructs a two-dimensional exa-scale array.
        /// </summary>
        public ExaArray2D()
        {
        }
        
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
        
        #region Store and load

        /// <summary>
        /// Stores the exa array into a stream.
        /// </summary>
        /// <remarks>
        /// This method does not dispose the stream.
        /// </remarks>
        public void Store(Stream outputStream)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(outputStream, this);
        }

        /// <summary>
        /// Restores an exa array from the given stream.
        /// </summary>
        /// <remarks>
        /// This method does not dispose the stream.
        /// </remarks>
        public static ExaArray2D<T> Restore(Stream inputStream)
        {
            var formatter = new BinaryFormatter();
            return formatter.Deserialize(inputStream) as ExaArray2D<T>;
        }

        #endregion
        
        #region Serialization

        /// <summary>
        /// This method serves for the serialization process. Do not call it manually. 
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", "v1");
            info.AddValue("length", this.Length);
            info.AddValue("chunks", this.chunks, typeof(ExaArray1D<ExaArray1D<T>>));
        }
        
        private ExaArray2D(SerializationInfo info, StreamingContext context)
        {
            switch (info.GetString("version"))
            {
                case "v1":
                    this.sumLengthOrdinates = info.GetUInt64("length");
                    this.chunks = info.GetValue("chunks", typeof(ExaArray1D<ExaArray1D<T>>)) as ExaArray1D<ExaArray1D<T>>;
                    break;

                default:
                    break;
            }
        }

        #endregion
    }
}
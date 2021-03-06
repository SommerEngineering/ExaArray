using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Exa
{
    /// <summary>
    /// This class represents an one-dimensional array with the ability to grow up
    /// to 4.6 quintillion (4,607,183,514,018,780,000) or 4.6 exa elements. 
    /// </summary>
    /// <typeparam name="T">The desired type to use, e.g. byte, int, etc.</typeparam>
    [Serializable]
    public sealed partial class ExaArray1D<T> : ISerializable
    {
        /// <summary>
        /// Unfortunately, this seems to be the maximal number of entries an
        /// C# array can hold e.g. of uint. Due to this limitation, this is
        /// also the maximum number of possible chunks.
        /// </summary>
        private const uint MAX_CAPACITY_ARRAY = 2_146_435_071; // Thanks @Frassle on Github for the exact number: https://github.com/dotnet/runtime/issues/12221#issuecomment-665553557

        /// <summary>
        /// This is the maximal number of elements per array when we use a number within power of two.
        /// Then, the divisions are faster.
        /// </summary>
        private const uint MAX_CAPACITY_ARRAY_PERFORMANCE = 1_073_741_824; // 2^30... Thanks @jkotas on Github: https://github.com/dotnet/runtime/issues/12221#issuecomment-665644665

        /// <summary>
        /// The total number of possible elements, when using the optimization strategy for max. elements.
        /// </summary>
        public const ulong MAX_NUMBER_ELEMENTS = 4_607_183_514_018_780_000;

        /// <summary>
        /// The total number of possible elements, when using the optimization strategy for max. performance. This is the default.
        /// </summary>
        public const ulong MAX_NUMBER_ELEMENTS_PERFORMANCE = 1_152_921_504_606_850_000;
        
        /// <summary>
        /// Gets the configured optimization strategy.
        /// </summary>
        public Strategy OptimizationStrategy { get; }
        
        // Chunk storage:
        private T[][] chunks = new T[1][];
        
        // Max. array and chunk capacity:
        private readonly uint maxArrayCapacity;
        
        // Max. number elements:
        private readonly ulong maxElements;

        /// <summary>
        /// Creates an empty one-dimensional exa-scale array.
        /// </summary>
        public ExaArray1D(Strategy strategy = Strategy.MAX_PERFORMANCE)
        {
            this.chunks[0] = new T[0];
            this.OptimizationStrategy = strategy;
            this.maxElements = this.OptimizationStrategy == Strategy.MAX_PERFORMANCE ? MAX_NUMBER_ELEMENTS_PERFORMANCE : MAX_NUMBER_ELEMENTS;
            this.maxArrayCapacity = this.OptimizationStrategy == Strategy.MAX_PERFORMANCE ? MAX_CAPACITY_ARRAY_PERFORMANCE : MAX_CAPACITY_ARRAY;
        }

        /// <summary>
        /// Extends the array's capacity by some extend.
        /// </summary>
        /// <remarks>
        /// Please ensure, that neither the <c>extendBy</c> parameter nor the total number of
        /// elements can exceed 4,607,183,514,018,780,000. Otherwise, an <c>ArgumentOutOfRangeException</c>
        /// will be thrown. You can use the <see cref="MAX_NUMBER_ELEMENTS"/> constant to perform checks.
        ///
        /// Performance: O(n) where n is the new total number of elements
        /// Memory: O(n+m) where <c>n</c> is the necessary memory for the previously elements, and <c>m</c>
        /// is the memory needed for the desired new capacity.
        /// </remarks>
        /// <param name="extendBy">Extend this array by this number of elements. Cannot exceed 4,607,183,514,018,780,000.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws, if either the total number of elements or the
        /// <c>extendBy</c> argument exceeds the limit of 4,607,183,514,018,780,000 elements.</exception>
        public void Extend(ulong extendBy = 1)
        {
            if (extendBy > this.maxElements || this.Length + extendBy >= this.maxElements)
                throw new ArgumentOutOfRangeException($"It is not possible to extend more than {this.maxElements} elements.");

            this.Length += extendBy;
            var availableInCurrentChunk = this.maxArrayCapacity - (ulong) (this.chunks[^1]?.Length ?? 0);
            if (extendBy >= (ulong)availableInCurrentChunk)
            {
                // Extend the current chunk to its max:
                var extendedInner = new T[this.maxArrayCapacity];
                Array.Copy(this.chunks[^1], extendedInner, this.chunks[^1].Length);
                this.chunks[^1] = extendedInner;

                //
                // Now, adding as much new chunks as necessary:
                //
                ulong leftOver = extendBy - (ulong) availableInCurrentChunk;
                if(leftOver == 0)
                    return;

                do
                {
                    ulong allocating = leftOver >= this.maxArrayCapacity ? this.maxArrayCapacity : leftOver;
                    leftOver -= allocating;
                    
                    // First, we allocate space for the new chunk:
                    var extendedOuter = new T[this.chunks.Length + 1][];
                    Array.Copy(this.chunks, extendedOuter, this.chunks.Length);
                    this.chunks = extendedOuter;
                    
                    // Now, we allocate the inner array i.e. the new chunk itself:
                    this.chunks[^1] = new T[allocating];

                } while (leftOver > 0);
                
                return;
            }
            
            // Extend the current chunk as necessary:
            var extendedChunk = new T[(this.chunks[^1]?.Length ?? 0) + (int)extendBy];
            Array.Copy(this.chunks[^1], extendedChunk, this.chunks[^1].Length);
            this.chunks[^1] = extendedChunk;
        }

        /// <summary>
        /// Gets the currently available number of possible values i.e. the capacity of the array.
        /// </summary>
        /// <remarks>
        /// Performance: O(1)
        /// </remarks>
        public ulong Length { get; private set; } = 0;
        
        /// <summary>
        /// Gets or sets a value at a certain position.
        /// </summary>
        /// <remarks>
        /// Performance: O(1)
        /// </remarks>
        /// <param name="index">The desired position in the exa-scale array. The index is zero-based.</param>
        /// <exception cref="IndexOutOfRangeException">Throws, if the index exceeds <see cref="MAX_NUMBER_ELEMENTS"/> or
        /// the desired index is not available due to missing extending.</exception>
        public T this[ulong index]
        {
            get
            {
                if(index >= this.maxElements)
                    throw new IndexOutOfRangeException();

                int chunkIndex = -1;
                int elementIndex = -1;
                switch (this.OptimizationStrategy)
                {
                    case Strategy.MAX_PERFORMANCE:
                        chunkIndex = (int) (index / MAX_CAPACITY_ARRAY_PERFORMANCE);
                        elementIndex = (int) (index - (ulong) chunkIndex * MAX_CAPACITY_ARRAY_PERFORMANCE);
                        break;
                    case Strategy.MAX_ELEMENTS:
                        chunkIndex = (int) (index / MAX_CAPACITY_ARRAY);
                        elementIndex = (int) (index - (ulong) chunkIndex * MAX_CAPACITY_ARRAY);
                        break;
                }
                
                if (chunkIndex >= this.chunks.Length || elementIndex >= this.chunks[chunkIndex].Length)
                    throw new IndexOutOfRangeException();
                
                return this.chunks[chunkIndex][elementIndex];
            }

            set
            {
                if(index >= this.maxElements)
                    throw new IndexOutOfRangeException();
                
                int chunkIndex = -1;
                int elementIndex = -1;
                switch (this.OptimizationStrategy)
                {
                    case Strategy.MAX_PERFORMANCE:
                        chunkIndex = (int) (index / MAX_CAPACITY_ARRAY_PERFORMANCE);
                        elementIndex = (int) (index - (ulong) chunkIndex * MAX_CAPACITY_ARRAY_PERFORMANCE);
                        break;
                    case Strategy.MAX_ELEMENTS:
                        chunkIndex = (int) (index / MAX_CAPACITY_ARRAY);
                        elementIndex = (int) (index - (ulong) chunkIndex * MAX_CAPACITY_ARRAY);
                        break;
                }
                
                if (chunkIndex >= this.chunks.Length || elementIndex >= this.chunks[chunkIndex].Length)
                    throw new IndexOutOfRangeException();

                this.chunks[chunkIndex][elementIndex] = value;
            }
        }

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
        /// <param name="indexFrom">The first source element which should be part of the new array.</param>
        /// <param name="indexTo">The last source element which should be part of the new array.</param>
        /// <returns>The new instance</returns>
        /// <exception cref="IndexOutOfRangeException">Throws, when one or both of the indices are out of range.</exception>
        public ExaArray1D<T> this[ulong indexFrom, ulong indexTo] => ExaArray1D<T>.CreateFrom(this, indexFrom, indexTo);

        /// <summary>
        /// Yields an enumerator across all elements.
        /// </summary>
        /// <remarks>
        /// Performance: O(n)
        /// Memory: O(1)
        /// </remarks>
        /// <returns>An enumerator across all elements.</returns>
        public IEnumerable<T> Items()
        {
            for (ulong n = 0; n < this.Length; n++)
                yield return this[n];
        }

        #region Store and load

        /// <summary>
        /// Stores the exa array into a stream. <b>Please read the remarks regarding security issues.</b>
        /// </summary>
        /// <remarks>
        /// The data stored in this way should never be part of a public API. Serializing and
        /// deserializing is not secure: an attacker can manipulate the data in a targeted
        /// manner to compromise the API server, etc.
        ///
        /// This method does not dispose the stream.
        /// </remarks>
        public void Store(Stream outputStream)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(outputStream, this);
        }

        /// <summary>
        /// Restores an exa array from the given stream. <b>Please read the remarks regarding security issues.</b>
        /// </summary>
        /// <remarks>
        /// The data loaded in this way should never be part of a public API. Serializing and
        /// deserializing is not secure: an attacker can manipulate the data in a targeted
        /// manner to compromise the API server, etc.
        /// 
        /// This method does not dispose the stream.
        /// </remarks>
        public static ExaArray1D<T> Restore(Stream inputStream)
        {
            var formatter = new BinaryFormatter();
            return formatter.Deserialize(inputStream) as ExaArray1D<T>;
        }

        #endregion

        #region Serialization

        /// <summary>
        /// This method serves for the serialization process. Do not call it manually. 
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", "v1");
            info.AddValue("strategy", this.OptimizationStrategy, typeof(Strategy));
            info.AddValue("length", this.Length);
            info.AddValue("chunks", this.chunks, typeof(T[][]));
        }
        
        private ExaArray1D(SerializationInfo info, StreamingContext context)
        {
            switch (info.GetString("version"))
            {
                case "v1":
                    this.Length = info.GetUInt64("length");
                    this.chunks = info.GetValue("chunks", typeof(T[][])) as T[][];
                    this.OptimizationStrategy = (Strategy) info.GetValue("strategy", typeof(Strategy));
                    break;
            }

            this.chunks[0] ??= new T[0];
            this.maxElements = this.OptimizationStrategy == Strategy.MAX_PERFORMANCE ? MAX_NUMBER_ELEMENTS_PERFORMANCE : MAX_NUMBER_ELEMENTS;
            this.maxArrayCapacity = this.OptimizationStrategy == Strategy.MAX_PERFORMANCE ? MAX_CAPACITY_ARRAY_PERFORMANCE : MAX_CAPACITY_ARRAY;
        }

        #endregion
    }
}
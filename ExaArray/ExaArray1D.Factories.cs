using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Creates a new ExaArray1D from another, respecting the given range.
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
        public static ExaArray1D<T> CreateFrom(ExaArray1D<T> other, ulong indexFrom, ulong indexTo)
        {
            if (indexTo < indexFrom)
                throw new IndexOutOfRangeException("Index to must be greater than index from.");
            
            if (indexTo >= other.Length || indexFrom >= other.Length)
                throw new IndexOutOfRangeException("Index to must be within the range of the source.");
            
            //
            // Determine the source start chunk and index.
            //
            int sourceChunkIndexTo = -1;
            int sourceElementIndexTo = -1;
            switch (other.OptimizationStrategy)
            {
                case Strategy.MAX_PERFORMANCE:
                    sourceChunkIndexTo = (int) (indexTo / MAX_CAPACITY_ARRAY_PERFORMANCE);
                    sourceElementIndexTo = (int) (indexTo - (ulong) sourceChunkIndexTo * MAX_CAPACITY_ARRAY_PERFORMANCE);
                    break;
                case Strategy.MAX_ELEMENTS:
                    sourceChunkIndexTo = (int) (indexTo / MAX_CAPACITY_ARRAY);
                    sourceElementIndexTo = (int) (indexTo - (ulong) sourceChunkIndexTo * MAX_CAPACITY_ARRAY);
                    break;
            }
            
            //
            // Determine the source end chunk and index.
            //
            int sourceChunkIndexFrom = -1;
            int sourceElementIndexFrom = -1;
            switch (other.OptimizationStrategy)
            {
                case Strategy.MAX_PERFORMANCE:
                    sourceChunkIndexFrom = (int) (indexFrom / MAX_CAPACITY_ARRAY_PERFORMANCE);
                    sourceElementIndexFrom = (int) (indexFrom - (ulong) sourceChunkIndexFrom * MAX_CAPACITY_ARRAY_PERFORMANCE);
                    break;
                case Strategy.MAX_ELEMENTS:
                    sourceChunkIndexFrom = (int) (indexFrom / MAX_CAPACITY_ARRAY);
                    sourceElementIndexFrom = (int) (indexFrom - (ulong) sourceChunkIndexFrom * MAX_CAPACITY_ARRAY);
                    break;
            }
            
            // How many element we have to copy?
            ulong newRange = indexTo - indexFrom + 1;
            
            //
            // Determine, how many total chunks we need for the copy.
            //
            int destChunks = -1;
            switch (other.OptimizationStrategy)
            {
                case Strategy.MAX_PERFORMANCE:
                    destChunks = (int) ((newRange - 1) / MAX_CAPACITY_ARRAY_PERFORMANCE) + 1;
                    break;
                case Strategy.MAX_ELEMENTS:
                    destChunks = (int) ((newRange - 1) / MAX_CAPACITY_ARRAY) + 1;
                    break;
            }

            // Create the copy and allocate the needed number of outer chunk.
            var next = new ExaArray1D<T>(other.OptimizationStrategy)
            {
                Length = newRange,
                chunks = new T[destChunks][],
            };
            
            //
            // Variables for the copy process.
            //
            
            int sourceChunkIndex = sourceChunkIndexFrom;
            int destinationChunkIndex = 0;
            int sourceElementIndex = sourceElementIndexFrom;
            int destinationElementIndex = 0;
            ulong leftOverTotal = newRange;

            do
            {
                //
                // Determine how many elements we copy next.
                //

                uint numberToCopy = 0;
                
                // Case: small number of elements from first chunk only
                if (sourceChunkIndexFrom == sourceChunkIndexTo)
                    numberToCopy = (uint) (sourceElementIndexTo - sourceElementIndexFrom + 1);
                
                // Case: this is first chunk of multiple chunks + start somewhere in the middle
                else if (sourceElementIndex > 0)
                    numberToCopy = (uint) (other.maxArrayCapacity - sourceElementIndex);
                    
                // Case: multiple chunks + we are in the middle of huge copy process + next chunk does __not__ exist
                else if (next.chunks[destinationChunkIndex] == null && sourceElementIndex == 0 && leftOverTotal >= other.maxArrayCapacity)
                    numberToCopy = other.maxArrayCapacity;
                
                // Case: multiple chunks + we are in the middle of huge copy process + next chunk does exist
                else if (next.chunks[destinationChunkIndex] != null && sourceElementIndex == 0 && leftOverTotal >= other.maxArrayCapacity)
                    numberToCopy = (uint) (other.maxArrayCapacity - next.chunks[destinationChunkIndex].Length);
                
                // Case: multiple chunks + this seems to be the last chunk
                else if (sourceElementIndex == 0 && leftOverTotal < other.maxArrayCapacity)
                    numberToCopy = (uint) leftOverTotal;
                
                //
                // Can we allocate an entire chunk or do we have to fill up the existing
                // chunk first?
                //
                if(next.chunks[destinationChunkIndex] == null)
                    next.chunks[destinationChunkIndex] = new T[numberToCopy];
                else
                {
                    var extended = new T[next.chunks[destinationChunkIndex].Length + numberToCopy];
                    Array.Copy(next.chunks[destinationChunkIndex], extended, next.chunks[destinationChunkIndex].Length);
                    next.chunks[destinationChunkIndex] = extended;
                }
                
                // Copy the data:
                Array.Copy(other.chunks[sourceChunkIndex], sourceElementIndex, next.chunks[destinationChunkIndex], destinationElementIndex, numberToCopy);

                //
                // Update the state machine.
                //
                var needNewDestinationChunk = next.chunks[destinationChunkIndex].Length == next.maxArrayCapacity;
                var readNextSourceChunk = sourceElementIndex + numberToCopy == other.maxArrayCapacity;
                var leftOverCurrentSourceChunk = (int) (readNextSourceChunk ? 0 : other.maxArrayCapacity - sourceElementIndex - numberToCopy);
                
                leftOverTotal -= numberToCopy;
                sourceChunkIndex = readNextSourceChunk ? sourceChunkIndex + 1 : sourceChunkIndex;
                destinationChunkIndex = needNewDestinationChunk ? destinationChunkIndex + 1 : destinationChunkIndex;
                sourceElementIndex = leftOverCurrentSourceChunk;
                destinationElementIndex = (int) (needNewDestinationChunk ? 0 : numberToCopy);

            } while (leftOverTotal > 0);
            return next;
        }

        /// <summary>
        /// Creates a new ExaArray1D from an enumerable sequence of items. The number of items in the sequence is __unknown__.
        /// </summary>
        /// <remarks>
        /// This factory method is slow because the number of items in the sequence is unknown. When you know the
        /// number of items, you should use another factory method, where the number of items can be provided.
        ///
        /// Performance: O(n)
        /// </remarks>
        /// <param name="sequence">The sequence to consume in order to create the array.</param>
        /// <param name="strategy">The optional optimization strategy.</param>
        /// <returns>The desired instance</returns>
        public static ExaArray1D<T> CreateFrom(IEnumerable<T> sequence, Strategy strategy = Strategy.MAX_PERFORMANCE)
        {
            var inst = new ExaArray1D<T>(strategy);
            ulong position = 0;
            foreach (var element in sequence)
            {
                inst.Extend();
                inst[position++] = element;
            }

            return inst;
        }

        /// <summary>
        /// Creates a new ExaArray1D from an enumerable sequence of items. The number of items in the sequence is __known__.
        /// </summary>
        /// <remarks>
        /// Creates an array with <c>length</c> items. When the sequence contains less elements, the remaining values are <c>default(T)</c>. 
        /// When the sequence contains more elements, these additional elements getting ignored.
        /// 
        /// Performance: O(n)
        /// </remarks>
        /// <param name="sequence">The sequence to consume in order to create the array.</param>
        /// <param name="length">The number of elements in the sequence. When the sequence contains more elements, these additional elements are ignored.</param>
        /// <param name="strategy">The optional optimization strategy.</param>
        /// <returns>The desired instance</returns>
        public static ExaArray1D<T> CreateFrom(IEnumerable<T> sequence, ulong length, Strategy strategy = Strategy.MAX_PERFORMANCE)
        {
            var inst = new ExaArray1D<T>(strategy);
            inst.Extend(length);
            
            ulong position = 0;
            foreach (var element in sequence)
            {
                if(position == length)
                    break;

                inst[position++] = element;
            }

            return inst;
        }

        /// <summary>
        /// Creates a new ExaArray1D from a collection of items.
        /// </summary>
        /// <remarks>
        /// Performance: O(n)
        /// </remarks>
        /// <param name="collection">The collection to use</param>
        /// <param name="strategy">The optional optimization strategy.</param>
        /// <returns>The desired instance</returns>
        public static ExaArray1D<T> CreateFrom(ICollection<T> collection, Strategy strategy = Strategy.MAX_PERFORMANCE)
        {
            var inst = new ExaArray1D<T>(strategy);
            inst.Extend((ulong) collection.Count);
            
            ulong position = 0;
            foreach (var element in collection)
                inst[position++] = element;

            return inst;
        }
    }
}
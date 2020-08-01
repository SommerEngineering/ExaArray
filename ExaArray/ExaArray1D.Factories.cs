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

        public static ExaArray1D<T> CreateFrom(ExaArray1D<T> other, ulong indexFrom, ulong indexTo)
        {
            if (indexTo < indexFrom)
                throw new IndexOutOfRangeException("Index to must be greater than index from.");
            
            if (indexTo >= other.Length || indexFrom >= other.Length)
                throw new IndexOutOfRangeException("Index to must be greater than index from.");
            
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
            
            ulong newRange = indexTo - indexFrom + 1;
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

            var next = new ExaArray1D<T>(other.OptimizationStrategy)
            {
                Length = newRange,
                chunks = new T[destChunks][],
            };

            int sourceChunkIndex = sourceChunkIndexFrom;
            int destinChunkIndex = 0;
            int sourceElementIndex = sourceElementIndexFrom;
            int destinElementIndex = 0;
            ulong leftOver = newRange;
            
            do
            {
                uint numberToCopy = 0;
                
                // Case: small number of elements from first chunk only
                if (sourceChunkIndexFrom == sourceChunkIndexTo)
                    numberToCopy = (uint) (sourceElementIndexTo - sourceElementIndexFrom + 1);
                
                // Case: this is first chunk of multiple chunks + start somewhere in the middle
                else if (sourceElementIndex > 0)
                    numberToCopy = (uint) (other.maxArrayCapacity - sourceElementIndex);
                    
                // Case: multiple chunks + we are in the middle of huge copy process + next chunk does __not__ exist
                else if (next.chunks[destinChunkIndex] == null && sourceElementIndex == 0 && leftOver >= other.maxArrayCapacity)
                    numberToCopy = other.maxArrayCapacity;
                
                // Case: multiple chunks + we are in the middle of huge copy process + next chunk does exist
                else if (next.chunks[destinChunkIndex] != null && sourceElementIndex == 0 && leftOver >= other.maxArrayCapacity)
                    numberToCopy = (uint) (other.maxArrayCapacity - next.chunks[destinChunkIndex].Length);
                
                // Case: multiple chunks + this seems to be the last chunk
                else if (sourceElementIndex == 0 && leftOver < other.maxArrayCapacity)
                    numberToCopy = (uint) leftOver;
                
                if(next.chunks[destinChunkIndex] == null)
                    next.chunks[destinChunkIndex] = new T[numberToCopy];
                else
                {
                    var extended = new T[next.chunks[destinChunkIndex].Length + numberToCopy];
                    Array.Copy(next.chunks[destinChunkIndex], extended, next.chunks[destinChunkIndex].Length);
                    next.chunks[destinChunkIndex] = extended;
                }
                
                Array.Copy(other.chunks[sourceChunkIndex], sourceElementIndex, next.chunks[destinChunkIndex], destinElementIndex, numberToCopy);

                var needNewChunk = next.chunks[destinChunkIndex].Length == next.maxArrayCapacity;
                leftOver -= numberToCopy;
                sourceChunkIndex++;
                destinChunkIndex = needNewChunk ? destinChunkIndex + 1 : destinChunkIndex;
                sourceElementIndex = 0;
                destinElementIndex = (int) (needNewChunk ? 0 : numberToCopy);

            } while (leftOver > 0);
            return next;
        }
    }
}
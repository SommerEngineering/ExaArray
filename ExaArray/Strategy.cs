namespace Exa
{
    /// <summary>
    /// Different strategies for memory vs. performance handling.
    /// </summary>
    public enum Strategy
    {
        /// <summary>
        /// Maximizes the performance. Thus, only 1.1 quintillion elements can be stored.
        /// </summary>
        MAX_PERFORMANCE = 1,
        
        /// <summary>
        /// Maximizes the number of available elements. Thus, the full 4.6 quintillion elements are available.
        /// </summary>
        MAX_ELEMENTS = 2,
    }
}
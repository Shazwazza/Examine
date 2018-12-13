namespace Examine.SearchCriteria
{
    /// <summary>
    /// Used during a sort operation to specify how the field should be sorted
    /// </summary>
    public enum SortType
    {
        /// <summary>
        /// Sort by document score (relevancy).  Sort values are Float and higher
        ///             values are at the front.
        /// 
        /// </summary>
        Score,
        /// <summary>
        /// Sort by document number (index order).  Sort values are Integer and lower
        ///             values are at the front.
        /// 
        /// </summary>
        DocumentOrder,
        /// <summary>
        /// Sort using term values as Strings.  Sort values are String and lower
        ///             values are at the front.
        /// 
        /// </summary>
        String,
        /// <summary>
        /// Sort using term values as encoded Integers.  Sort values are Integer and
        ///             lower values are at the front.
        /// 
        /// </summary>
        Int,
        /// <summary>
        /// Sort using term values as encoded Floats.  Sort values are Float and
        ///             lower values are at the front.
        /// 
        /// </summary>
        Float,
        /// <summary>
        /// Sort using term values as encoded Longs.  Sort values are Long and
        ///             lower values are at the front.
        /// 
        /// </summary>
        Long,
        /// <summary>
        /// Sort using term values as encoded Doubles.  Sort values are Double and
        ///             lower values are at the front.
        /// 
        /// </summary>
        Double,
        /// <summary>
        /// Sort using term values as encoded Shorts.  Sort values are Short and
        ///             lower values are at the front.
        /// 
        /// </summary>
        Short,
        /// <summary>
        /// Sort using term values as encoded Bytes.  Sort values are Byte and
        ///             lower values are at the front.
        /// 
        /// </summary>
        Byte
    }
}
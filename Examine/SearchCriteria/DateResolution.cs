
namespace Examine.SearchCriteria
{
    /// <summary>
    /// The precision of a date
    /// </summary>
    public enum DateResolution
    {
        /// <summary>
        /// Date is the Year component
        /// </summary>
        Year,
        /// <summary>
        /// Date is just the Year and Month component
        /// </summary>
        Month,
        /// <summary>
        /// Date is just the Year, Month and Day component
        /// </summary>
        Day,
        /// <summary>
        /// Date is just the Year, Month, Day and Hour component
        /// </summary>
        Hour,
        /// <summary>
        /// Date is just the Year, Month, Day, Hour and Minute component
        /// </summary>
        Minute,
        /// <summary>
        /// Date is just the Year, Month, Day, Hour, Minute and Second component
        /// </summary>
        Second,
        /// <summary>
        /// Date is just the Year, Month, Day, Hour, Minute, Second and Millisecond component
        /// </summary>
        Millisecond
    }
}

namespace Examine
{
    public enum ValueSetValidationStatus
    {
        /// <summary>
        /// If the result is valid
        /// </summary>
        Valid,

        /// <summary>
        /// If validation failed, the value set will not be included in the index
        /// </summary>
        Failed,

        /// <summary>
        /// If validation passed but the value set was filtered
        /// </summary>
        Filtered
    }
}

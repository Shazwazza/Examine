namespace Examine
{
    /// <summary>
    /// Base for Relevance Scorer Functions
    /// </summary>
    public abstract class RelevanceScorerFunctionBaseDefintion
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName">Name of the field for the function</param>
        /// <param name="boost">Boost for the function</param>
        public RelevanceScorerFunctionBaseDefintion(string fieldName, float boost)
        {
            FieldName = fieldName;
            Boost = boost;
        }

        /// <summary>
        /// Name of the field for the function
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// Boost for the function
        /// </summary>
        public float Boost { get; }
    }
}

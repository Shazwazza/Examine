
namespace Examine.Search
{
    /// <summary>
    /// Represents a value used in a query like <see cref="IQuery"/>
    /// </summary>
    public interface IExamineValue
    {
        /// <summary>
        /// Different ways to match terms
        /// </summary>
        public Examineness Examineness { get; }

        /// <summary>
        /// The level
        /// </summary>
        public float Level { get; }

        /// <summary>
        /// The value
        /// </summary>
        public string Value { get; }
    }
}

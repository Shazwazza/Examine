
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
        Examineness Examineness { get; }

        /// <summary>
        /// The level
        /// </summary>
        float Level { get; }

        /// <summary>
        /// The value
        /// </summary>
        string Value { get; }
    }
}


namespace Examine.Search
{
    /// <summary>
    /// A boosted representation of an <see cref="IExamineValue"/>
    /// </summary>
    public interface IExamineValueBoosted : IExamineValue
    {
        /// <summary>
        /// Gets the boost factor applied to the current item.
        /// </summary>
        public float Boost { get; }
    }
}

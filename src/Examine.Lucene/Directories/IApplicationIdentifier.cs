namespace Examine.Lucene.Directories
{
    /// <summary>
    /// Represents a class that can return a unique identifier for the application
    /// </summary>
    public interface IApplicationIdentifier
    {
        /// <summary>
        /// Gets the unique identifier for the application
        /// </summary>
        /// <returns></returns>
        string GetApplicationUniqueIdentifier();
    }
}

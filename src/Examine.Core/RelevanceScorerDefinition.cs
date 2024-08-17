using System.Collections.Generic;

namespace Examine
{
    /// <summary>
    /// Defines how to score a document to affect it's relevance
    /// </summary>
    public class RelevanceScorerDefinition
    {
        public RelevanceScorerDefinition(string name,
                                         IEnumerable<RelevanceScorerFunctionBaseDefintion> functionScorerDefintions)
        {
            Name = name;
            FunctionScorerDefintions = functionScorerDefintions;
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Field Boosting Function Defintions
        /// </summary>
        public IEnumerable<RelevanceScorerFunctionBaseDefintion> FunctionScorerDefintions { get; }
    }
}

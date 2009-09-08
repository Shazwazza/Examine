using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UmbracoExamine
{
    public class SearchResult
    {

        public int Id { get; set; }
        public float Score { get; set; }
        public Dictionary<string, string> Fields { get; set; }

        /// <summary>
        /// Override this method so that the Distinct() operator works
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var result = (SearchResult)obj;

            return Id.Equals(result.Id);
        }

        /// <summary>
        /// Override this method so that the Distinct() operator works
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

    }
}

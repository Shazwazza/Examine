using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace UmbracoExamine.DataServices
{
    public class NamedService : INamedService
    {
        #region INamedService Members

        public virtual XDocument GetAllData(string indexType)
        {
            switch (indexType)
            {
                case "member":
                    //Lookup the member data
                    break;

                default:
                    return UnhandledIndexType(indexType);
            }

            throw new NotImplementedException();
        }

        protected virtual XDocument UnhandledIndexType(string indexType)
        {
            return null;
        }

        #endregion
    }
}

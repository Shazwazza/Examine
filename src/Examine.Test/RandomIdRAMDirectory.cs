using System;
using Lucene.Net.Store;

namespace Examine.Test
{
    public class RandomIdRAMDirectory : RAMDirectory
    {
        private readonly string _lockId = Guid.NewGuid().ToString();
        public override string GetLockId()
        {
            return _lockId;
        }
    }
}
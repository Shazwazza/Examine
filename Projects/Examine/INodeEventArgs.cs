using System;
using System.ComponentModel;

namespace Examine
{
    [Obsolete("This is no longer required and will be removed in future versions")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface INodeEventArgs
    {
        int NodeId { get; }
    }
}

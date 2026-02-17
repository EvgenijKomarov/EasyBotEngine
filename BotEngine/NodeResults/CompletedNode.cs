using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.NodeResults
{
    internal class CompletedNode<TBuffer>(TBuffer obj) : INodeResult<TBuffer>
    {
        public Type? NextNode { get; } = null;
        public TBuffer Object => obj;
    }
}

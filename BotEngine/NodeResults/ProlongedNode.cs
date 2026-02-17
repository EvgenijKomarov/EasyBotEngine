using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.NodeResults
{
    internal class ProlongedNode<TBuffer>(Type nextNode, TBuffer obj) : INodeResult<TBuffer>
    {
        public Type NextNode { get; } = nextNode;
        public TBuffer Object => obj;
    }
}

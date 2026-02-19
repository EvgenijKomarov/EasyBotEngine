using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.NodeResults
{
    internal class ProlongedNode<TBuffer, TOutput>(Type nextNode, TBuffer obj) : NodeResult<TBuffer, TOutput>
    {
        public Type NextNode { get; } = nextNode;
        public TOutput Output { get; } = default(TOutput);
        public TBuffer Object => obj;
    }
}

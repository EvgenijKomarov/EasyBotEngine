using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.NodeResults
{
    internal class CompletedNode<TBuffer, TOutput>(TOutput obj) : NodeResult<TBuffer, TOutput>
    {
        public Type? NextNode { get; } = null;
        public TOutput Output => obj;
        public TBuffer Object { get; } = default(TBuffer);
    }
}

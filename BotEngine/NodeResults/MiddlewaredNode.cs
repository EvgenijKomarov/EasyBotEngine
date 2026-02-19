using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.NodeResults
{
    internal class MiddlewaredNode<TBuffer, TOutput>(TBuffer obj) : NodeResult<TBuffer, TOutput>
    {
        public Type? NextNode { get; } = null;
        public TOutput Output { get; } = default(TOutput);
        public TBuffer Object => obj;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotEngine.NodeResults
{
    internal class CompletedNode<TBuffer>(TBuffer obj) : INodeResult<TBuffer>
    {
        public string NextNode { get; } = null;
        public TBuffer Object => obj;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotEngine.NodeResults
{
    internal class ProlongedNode<TBuffer>(string nextNode, TBuffer obj) : INodeResult<TBuffer>
    {
        public string NextNode { get; } = nextNode;
        public TBuffer Object => obj;
    }
}

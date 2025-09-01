using BotEngine.Domain;
using BotEngine.Nodes;
using BotEngine.Technical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Test.TestNodes
{
    internal class EndNode: Node
    {
        public override string[] GetIdentificators() => ["End"];
        public override async Task<INodeInvokeResult> Invoke(MessageInput message)
        {
            return CompleteProcess("Goodbye");
        }
    }
}

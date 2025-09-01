using BotEngine.Domain;
using BotEngine.Nodes;
using BotEngine.Technical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.TestNodes
{
    internal class StartNode: Node
    {
        public override string[] GetIdentificators() => ["Text"];
        public override async Task<INodeInvokeResult> Invoke(MessageInput message)
        {
            if (message.GetData()[0] == "End") 
            {
                return RedirectToAnotherNode("End");
            }
            else
            {
                return CompleteProcess("Welcome");
            }
        }
    }
}

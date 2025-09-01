using BotEngine.Domain;
using BotEngine.Nodes;
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
        public override Task Invoke(MessageInput message)
        {
            if (message.GetData()[0] == "End") 
            {
                NextIdentificator = "End";
            }
            else
            {
                Text = "Welcome";
            }

            return Task.CompletedTask;
        }
    }
}

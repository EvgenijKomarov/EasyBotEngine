using BotEngine.Domain;
using BotEngine.Technical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotEngine.Nodes
{
    public class DefaultNode: Node
    {
        public override string[] GetIdentificators() => [];
        public override async Task<INodeInvokeResult> Invoke(MessageInput input)
        {
            return CompleteProcess($"An error occured during procession of BotEngine. {input.GetData()}");
        }
    }
}

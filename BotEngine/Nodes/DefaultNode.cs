using BotEngine.Domain;
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
        public override Task Invoke(MessageInput input)
        {
            Text = $"An error occured during procession of BotEngine. {input.GetData()}";
            return Task.CompletedTask;
        }
    }
}

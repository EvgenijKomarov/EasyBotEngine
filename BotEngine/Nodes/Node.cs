using BotEngine.Domain;
using BotEngine.Nodes.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotEngine.Nodes
{
    public abstract class Node
    {
        public List<Button> Buttons { get; private set; }
        public string Text { get; private set; }

        public abstract string[] GetValidators();
        public abstract Task Invoke(MessageInput input);
    }
}

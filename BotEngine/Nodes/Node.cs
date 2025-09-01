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
        public List<Button> Buttons { get; protected set; }
        public string Text { get; protected set; }

        public string NextValidator { get; protected set; } = string.Empty;
        public string[] NextData { get; protected set; }

        public bool isNeedProlongedIteration => NextValidator == string.Empty;
        public abstract string[] GetValidators();
        public abstract Task Invoke(MessageInput input);
    }
}

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
        public List<Button> Buttons { get; protected set; } = new List<Button>();
        public string Text { get; protected set; } = string.Empty;
        public int? MessageId { get; protected set; }

        public string NextIdentificator { get; protected set; } = string.Empty;
        public string[] NextData { get; protected set; }

        public bool IsNeedProlongedIteration => NextIdentificator != string.Empty;
        public abstract string[] GetIdentificators();
        public abstract Task Invoke(MessageInput input);
    }
}

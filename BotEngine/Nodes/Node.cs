using BotEngine.Domain;
using BotEngine.Nodes.Buttons;
using BotEngine.Technical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotEngine.Nodes
{
    public abstract class Node
    {
        internal List<Button> Buttons { get; set; } = new List<Button>();
        internal string Text { get; set; } = string.Empty;
        internal int? MessageId { get; set; }

        internal string NextIdentificator { get; set; } = string.Empty;
        internal string[] NextData { get; set; }

        protected INodeInvokeResult RedirectToAnotherNode(string identificator, string[] data = null, int? messageId = null)
        {
            NextIdentificator = identificator;
            NextData = data ?? [];
            MessageId = messageId ?? null;

            return new ProlongedNode();
        }

        protected INodeInvokeResult CompleteProcess(string text, List<Button> buttons = null, int? messageId = null)
        {
            Text = text;
            Buttons = buttons ?? new List<Button>();
            MessageId = messageId ?? null;

            return new CompletedNode();
        }

        protected INodeInvokeResult InterruptProcess()
        {
            return new InterruptedNode();
        }

        public abstract string[] GetIdentificators();
        public abstract Task<INodeInvokeResult> Invoke(MessageInput input);
    }
}

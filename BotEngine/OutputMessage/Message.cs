using BotEngine.Nodes.Buttons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotEngine.OutputMessage
{
    public class Message
    {
        public List<Button> Buttons { get; internal set; }
        public string Text { get; internal set; }
        public int? MessageId { get; internal set; }
    }
}

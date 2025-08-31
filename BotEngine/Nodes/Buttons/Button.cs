using BotEngine.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotEngine.Nodes.Buttons
{
    public class Button
    {
        public string Text { get; }

        public string CallBackQuery { get; }

        public Button(string text, CallBackQuery query)
        {
            Text = text;
            CallBackQuery = string.Join("_", [query.QueryCommand, .. query.Args]);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotEngine.Domain
{
    public class TextMessage: MessageInput
    {
        public string Text { get; set; }

        public TextMessage(int userId, string text) {
            UserId = userId;
            Text = text;
        }

        public override string GetIdentificator() => "Text";
        public override string[] GetData() => [Text];
    }
}

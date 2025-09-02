using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotEngine.Domain
{
    public class CallBackQuery: MessageInput
    {
        public string QueryCommand { get; }

        public string[] Args { get; }

        public CallBackQuery(int userId, int messageId, string queryText) 
        {
            string[] args = queryText.Split("_");

            UserId = userId;
            QueryCommand = args[0];
            MessageId = messageId;
            Args = args.Skip(1).ToArray();
        }

        public override string GetIdentificator() => QueryCommand;

        public override string[] GetData() => Args;
    }
}

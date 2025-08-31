using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotEngine.Domain
{
    public class CommandMessage: MessageInput
    {
        public string Command { get; set; }

        private string[] Args;

        public CommandMessage(int userId, string commandText)
        {
            string[] args = commandText.Split(" ");

            UserId = userId;
            Command = args[0].Skip(1).ToString();
            Args = args.Skip(1).ToArray();
        }

        public override string GetValidator() => Command;

        public override string[] GetData() => Args;
    }
}

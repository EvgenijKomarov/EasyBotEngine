using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotEngine.Domain
{
    public abstract class MessageInput
    {
        public int? UserId { get; set; }

        public int? MessageId { get; set; }

        public abstract string GetValidator();

        public abstract string[] GetData();
    }
}

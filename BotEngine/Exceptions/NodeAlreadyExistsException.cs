using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotEngine.Exceptions
{
    public class NodeAlreadyExistsException : Exception
    {
        public NodeAlreadyExistsException() : base("Node was alreasy added to this engine") { }
    }
}

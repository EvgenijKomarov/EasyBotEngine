using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotEngine.Exceptions
{
    public class NodeNotFoundException : Exception
    {
        public NodeNotFoundException() : base("Node not found or not registered") { }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Exceptions
{
    public class NodeNotFoundException : Exception
    {
        public NodeNotFoundException(Type nodeType) : base($"Node {nodeType.ToString()} not found or not registered") { }
    }
}

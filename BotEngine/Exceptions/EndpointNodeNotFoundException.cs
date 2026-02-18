using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Exceptions
{
    public class EndpointNodeNotFoundException: Exception
    {
        public EndpointNodeNotFoundException(Type nodeType) : base($"EndpointNode {nodeType.Name} not found or not registered") { }
    }
}

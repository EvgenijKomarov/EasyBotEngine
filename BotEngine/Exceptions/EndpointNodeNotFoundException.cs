using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Exceptions
{
    public class EndpointNodeNotFoundException: Exception
    {
        public EndpointNodeNotFoundException(string nodeTypeId) : base($"EndpointNode {nodeTypeId} not found or not registered") { }
    }
}

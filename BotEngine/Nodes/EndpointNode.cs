using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Nodes
{
    /// <summary>
    /// Endpoint of engine
    /// </summary>
    /// <typeparam name="TBuffer">Data buffer</typeparam>
    /// <typeparam name="TOutput">Output that can be returned by engine during process</typeparam>
    public abstract class EndpointNode<TBuffer, TOutput>(string endpointId) : Node<TBuffer, TOutput>
        where TBuffer : notnull
        where TOutput : notnull
    {
        public string EndpointId = endpointId;
    }
}

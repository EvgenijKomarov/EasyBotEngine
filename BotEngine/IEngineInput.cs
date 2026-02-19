using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    /// <summary>
    /// Entity that used in EasyBotEngine
    /// </summary>
    /// <typeparam name="TBuffer"></typeparam>
    public interface IEngineInput<TBuffer> where TBuffer : notnull
    {
        /// <summary>
        /// Target endpoint node
        /// </summary>
        string EndpointNodeId {  get; }
        /// <summary>
        /// Data buffer
        /// </summary>
        TBuffer Object { get; }
    }
}

using Engine.NodeResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Engine
{
    /// <summary>
    /// Entities that crawled by engine
    /// </summary>
    /// <typeparam name="TBuffer">Data that crawls between nodes</typeparam>
    public abstract class Node<TBuffer> where TBuffer : notnull
    {
        /// <summary>
        /// Return this to get to the next node
        /// </summary>
        /// <typeparam name="TNextNode">Next node type</typeparam>
        /// <param name="obj">Buffer</param>
        /// <returns></returns>
        protected INodeResult<TBuffer> Next<TNextNode>(TBuffer obj) => new ProlongedNode<TBuffer>(typeof(TNextNode), obj);

        /// <summary>
        /// Return this to complete process
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected INodeResult<TBuffer> Complete(TBuffer obj) => new CompletedNode<TBuffer>(obj);

        /// <summary>
        /// Operation that invoked inside node to process data buffer
        /// </summary>
        /// <param name="input">Data that uses in node</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public abstract Task<INodeResult<TBuffer>> Invoke(TBuffer input, CancellationToken? token = null);
    }
}

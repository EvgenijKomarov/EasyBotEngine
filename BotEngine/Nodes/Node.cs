using Engine.NodeResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Engine.Nodes
{
    /// <summary>
    /// Node that can be crawled by engine.
    /// </summary>
    /// <typeparam name="TBuffer">Data that crawls between nodes</typeparam>
    /// <typeparam name="TOutput">Data that can be returned by node after finish</typeparam>
    public abstract class Node<TBuffer, TOutput> 
        where TBuffer : notnull
        where TOutput : notnull
    {
        /// <summary>
        /// Return this to crawl to the next node
        /// </summary>
        /// <typeparam name="TNextNode">Next node type</typeparam>
        /// <param name="obj">Buffer</param>
        /// <returns></returns>
        protected INodeResult<TBuffer, TOutput> Next<TNextNode>(TBuffer obj) where TNextNode : Node<TBuffer, TOutput>
            => new ProlongedNode<TBuffer, TOutput>(typeof(TNextNode), obj);

        /// <summary>
        /// Return this to complete process
        /// </summary>
        /// <param name="obj">Result of the process</param>
        /// <returns></returns>
        protected INodeResult<TBuffer, TOutput> Complete(TOutput obj) => new CompletedNode<TBuffer, TOutput>(obj);

        /// <summary>
        /// Operation that invoked inside node to process data buffer
        /// </summary>
        /// <param name="input">Data that uses in node</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public abstract Task<INodeResult<TBuffer, TOutput>> Invoke(TBuffer input, CancellationToken? token = null);
    }
}

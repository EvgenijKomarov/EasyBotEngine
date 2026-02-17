using Engine.NodeResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public abstract class Middleware<TBuffer> where TBuffer : notnull
    {
        /// <summary>
        /// Return this to get to the node
        /// </summary>
        /// <typeparam name="TNextNode">Next node type</typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected INodeResult<TBuffer> RedirectToNode<TNextNode>(TBuffer obj) where TNextNode : Node<TBuffer>
            => new ProlongedNode<TBuffer>(typeof(TNextNode), obj);

        /// <summary>
        /// Return this to complete middleware
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected INodeResult<TBuffer> Complete(TBuffer obj) => new CompletedNode<TBuffer>(obj);

        /// <summary>
        /// Conditions of middleware
        /// </summary>
        /// <returns></returns>
        public virtual Task<bool> GetCondition(TBuffer data, CancellationToken? token = null) => Task.FromResult(true);

        /// <summary>
        /// Operation that invoked inside middleware to process data buffer
        /// </summary>
        /// <param name="input">Data that uses in middleware</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public abstract Task<INodeResult<TBuffer>> Invoke(TBuffer input, CancellationToken? token = null);
    }
}

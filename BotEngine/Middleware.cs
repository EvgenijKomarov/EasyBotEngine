using Engine.NodeResults;
using Engine.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    /// <summary>
    /// Middleware that invoked between start of the process and reaching first node
    /// </summary>
    /// <typeparam name="TBuffer">Data buffer</typeparam>
    /// <typeparam name="TOutput">This entity exists only to ensure that engine can be injectet into engine</typeparam>
    public abstract class Middleware<TBuffer, TOutput> 
        where TBuffer : notnull
        where TOutput : notnull
    {
        /// <summary>
        /// Return this to get to the node
        /// </summary>
        /// <typeparam name="TNextNode">Next node type</typeparam>
        /// <param name="obj">Buffer</param>
        /// <returns></returns>
        protected INodeResult<TBuffer, TOutput> RedirectToNode<TNextNode>(TBuffer obj) 
            where TNextNode : Node<TBuffer, TOutput>
            => new ProlongedNode<TBuffer, TOutput>(typeof(TNextNode), obj);

        /// <summary>
        /// Return this to complete middleware
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected INodeResult<TBuffer, TOutput> Complete(TBuffer obj) => new MiddlewaredNode<TBuffer, TOutput>(obj);

        /// <summary>
        /// Finish the whole processing with a final output from middleware
        /// </summary>
        /// <param name="output">Final output</param>
        /// <returns></returns>
        protected INodeResult<TBuffer, TOutput> Finish(TOutput output) => new CompletedNode<TBuffer, TOutput>(output);

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
        public abstract Task<INodeResult<TBuffer, TOutput>> Invoke(TBuffer input, CancellationToken? token = null);
    }
}

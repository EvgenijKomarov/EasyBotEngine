using BotEngine.NodeResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotEngine
{
    public abstract class Middleware<TBuffer> where TBuffer : notnull
    {
        /// <summary>
        /// Return this to get to the node
        /// </summary>
        /// <param name="nextNode"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected INodeResult<TBuffer> MoveToNode(string nextNode, TBuffer obj) => new ProlongedNode<TBuffer>(nextNode, obj);

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

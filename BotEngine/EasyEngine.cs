using BotEngine.Exceptions;
using BotEngine.NodeResults;

namespace BotEngine
{
    /// <summary>
    /// Business logic engine
    /// </summary>
    /// <typeparam name="TInput">Input data</typeparam>
    /// <typeparam name="TBuffer">Data that crawls between nodes</typeparam>
    /// <typeparam name="TOutput">Result of the process</typeparam>
    public class EasyEngine<TInput, TBuffer, TOutput>
        where TInput : notnull 
        where TBuffer : notnull
        where TOutput : notnull
    {
        private Func<TInput, TBuffer> inputToBuffer;
        private Func<TBuffer, TOutput> bufferToOutput;

        private Dictionary<string, Node<TBuffer>> _nodes = new Dictionary<string, Node<TBuffer>>();
        private List<Middleware<TBuffer>> _middlewares = new List<Middleware<TBuffer>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapInputToBuffer">How to map input data into data buffer</param>
        /// <param name="mapBufferToOutput">How to map data buffer into output data</param>
        public EasyEngine(Func<TInput, TBuffer> mapInputToBuffer, Func<TBuffer, TOutput> mapBufferToOutput)
        {
            inputToBuffer = mapInputToBuffer;
            bufferToOutput = mapBufferToOutput;
        }

        /// <summary>
        /// Use this method to add nodes to engine
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <exception cref="NodeAlreadyExistsException">Node with the same id already exists</exception>
        public EasyEngine<TInput, TBuffer, TOutput> AddNode(Node<TBuffer> node)
        {
            foreach(string id in node.GetIdentificators())
            {
                if(!_nodes.TryAdd(id, node)) { throw new NodeAlreadyExistsException(); }
            }
            return this;
        }

        /// <summary>
        /// Use this method to add middlewares to engine
        /// </summary>
        /// <param name="middleware"></param>
        /// <returns></returns>
        public EasyEngine<TInput, TBuffer, TOutput> AddMiddleware(Middleware<TBuffer> middleware)
        {
            _middlewares.Add(middleware);
            return this;
        }

        /// <summary>
        /// Use this method to process data
        /// </summary>
        /// <param name="nodeIndex">Index of node</param>
        /// <param name="input">Input data</param>
        /// <param name="token"></param>
        /// <returns>Output data</returns>
        public async Task<TOutput> Process(string nodeIndex, TInput input, CancellationToken? token = null)
        {
            TBuffer dataResult = await Crawl(nodeIndex, inputToBuffer(input), token);
            return bufferToOutput(dataResult);
        }

        private async Task<TBuffer> Crawl(string index, TBuffer input, CancellationToken? token = null)
        {
            INodeResult<TBuffer> current = new CompletedNode<TBuffer>(input);
            foreach (var mid in _middlewares)
            {
                if (await mid.GetCondition(current.Object))
                {
                    current = await mid.Invoke(current.Object, token);
                    if (current.NextNode != null) { break; }
                }
            }

            if(current.NextNode == null) { current = new ProlongedNode<TBuffer>(index, current.Object); }

            while (current.NextNode != null)
            {
                if(_nodes.TryGetValue(current.NextNode, out Node<TBuffer> node))
                {
                    current = await node.Invoke(current.Object, token);
                }
                else { throw new NodeNotFoundException(); }
            }
            return current.Object;
        }
    }
}

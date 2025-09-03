using Engine.Exceptions;
using Engine.NodeResults;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace Engine
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
        private readonly ILogger<EasyEngine<TInput, TBuffer, TOutput>>? _logger;

        private Dictionary<string, Node<TBuffer>> _nodes = new Dictionary<string, Node<TBuffer>>();
        private List<Middleware<TBuffer>> _middlewares = new List<Middleware<TBuffer>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapInputToBuffer">How to map input data into data buffer</param>
        /// <param name="mapBufferToOutput">How to map data buffer into output data</param>
        /// <param name="logger">Logger instance (optional)</param>
        public EasyEngine(
            Func<TInput, TBuffer> mapInputToBuffer,
            Func<TBuffer, TOutput> mapBufferToOutput,
            ILogger<EasyEngine<TInput, TBuffer, TOutput>>? logger = null)
        {
            inputToBuffer = mapInputToBuffer;
            bufferToOutput = mapBufferToOutput;
            _logger = logger;
        }

        /// <summary>
        /// Use this method to add nodes to engine
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <exception cref="NodeAlreadyExistsException">Node with the same id already exists</exception>
        public EasyEngine<TInput, TBuffer, TOutput> AddNode(Node<TBuffer> node)
        {
            foreach (string id in node.GetIdentificators())
            {
                if (!_nodes.TryAdd(id, node)) { throw new NodeAlreadyExistsException(); }
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
        public async Task<TOutput?> Process(string nodeIndex, TInput input, CancellationToken? token = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var executionChain = new List<string>();
            var result = default(TOutput);
            Exception? exception = null;

            try
            {
                _logger?.LogDebug("Starting process with node: {NodeIndex}", nodeIndex);

                TBuffer dataResult = await Crawl(nodeIndex, inputToBuffer(input), token, executionChain);
                result = bufferToOutput(dataResult);
            }
            catch (Exception ex)
            {
                exception = ex;
                _logger?.LogError(ex, "Process failed with error: {ErrorMessage}", ex.Message);
            }
            finally
            {
                stopwatch.Stop();
                LogExecutionSummary(nodeIndex, stopwatch.Elapsed, executionChain, exception);
            }

            return result;
        }

        private async Task<TBuffer> Crawl(string index, TBuffer input, CancellationToken? token, List<string> executionChain)
        {
            INodeResult<TBuffer> current = new CompletedNode<TBuffer>(input);

            // Обработка middleware
            foreach (var mid in _middlewares)
            {
                if (await mid.GetCondition(current.Object, token ?? CancellationToken.None))
                {
                    current = await mid.Invoke(current.Object, token);

                    _logger?.LogDebug("Middleware executed: {MiddlewareType}", mid.GetType().Name);
                    executionChain.Add($"Middleware: {mid.GetType().Name}");

                    if (current.NextNode != null)
                    {
                        _logger?.LogDebug("Middleware redirected to node: {NextNode}", current.NextNode);
                        break;
                    }
                }
            }

            if (current.NextNode == null)
            {
                current = new ProlongedNode<TBuffer>(index, current.Object);
            }

            // Обработка узлов
            while (current.NextNode != null)
            {
                if (_nodes.TryGetValue(current.NextNode, out Node<TBuffer> node))
                {
                    _logger?.LogDebug("Executing node: {NodeType} ({NodeId})", node.GetType().Name, current.NextNode);
                    executionChain.Add($"Node: {node.GetType().Name}");

                    current = await node.Invoke(current.Object, token);

                    _logger?.LogDebug("Node {NodeType} completed. Next: {NextNode}",
                        node.GetType().Name, current.NextNode ?? "COMPLETED");
                }
                else
                {
                    _logger?.LogError("Node not found: {NodeId}", current.NextNode);
                    throw new NodeNotFoundException();
                }
            }

            return current.Object;
        }

        private void LogExecutionSummary(
            string startNode,
            TimeSpan duration,
            List<string> executionChain,
            Exception? exception)
        {
            if (_logger == null) return;

            var sb = new StringBuilder();
            sb.AppendLine("=== Execution Summary ===");
            sb.AppendLine($"Start Node: {startNode}");
            sb.AppendLine($"Duration: {duration.TotalMilliseconds}ms");
            sb.AppendLine($"Result Type: {typeof(TOutput).Name}");
            sb.AppendLine($"Success: {exception == null}");

            if (exception != null)
            {
                sb.AppendLine($"Error: {exception.Message}");
            }

            sb.AppendLine();
            sb.AppendLine("Execution Chain:");

            if (executionChain.Count == 0)
            {
                sb.AppendLine("  No nodes/middleware executed");
            }
            else
            {
                for (int i = 0; i < executionChain.Count; i++)
                {
                    sb.AppendLine($"  {i + 1}. {executionChain[i]}");
                }
            }

            sb.AppendLine("========================");

            if (exception == null)
            {
                _logger.LogInformation(sb.ToString());
            }
            else
            {
                _logger.LogError(exception, sb.ToString());
            }
        }
    }
}

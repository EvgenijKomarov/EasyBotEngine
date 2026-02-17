using Engine.Exceptions;
using Engine.NodeResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

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

        private HashSet<Type> _middlewareTypes = new HashSet<Type>();
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider">Service provider for DI</param>
        /// <param name="mapInputToBuffer">How to map input data into data buffer</param>
        /// <param name="mapBufferToOutput">How to map data buffer into output data</param>
        /// <param name="logger">Logger instance (optional)</param>
        public EasyEngine(
            IServiceProvider serviceProvider,
            Func<TInput, TBuffer> mapInputToBuffer,
            Func<TBuffer, TOutput> mapBufferToOutput,
            ILogger<EasyEngine<TInput, TBuffer, TOutput>>? logger = null)
        {
            _serviceProvider = serviceProvider;
            inputToBuffer = mapInputToBuffer;
            bufferToOutput = mapBufferToOutput;
            _logger = logger;
        }

        /// <summary>
        /// Use this method to add middlewares to engine
        /// </summary>
        /// <typeparam name="TMiddleware"></typeparam>
        /// <returns></returns>
        public EasyEngine<TInput, TBuffer, TOutput> AddMiddleware<TMiddleware>() where TMiddleware : Middleware<TBuffer>
        {
            _middlewareTypes.Add(typeof(TMiddleware));
            return this;
        }

        /// <summary>
        /// Use this method to process data
        /// </summary>
        /// <param name="endpointNode">Entry node</param>
        /// <param name="input">Input data</param>
        /// <param name="token"></param>
        /// <returns>Output data</returns>
        public async Task<TOutput?> Process(Type endpointNode, TInput input, CancellationToken? token = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var executionChain = new List<string>();
            var result = default(TOutput);
            Exception? exception = null;

            try
            {
                _logger?.LogDebug("Starting process with node: {NodeIndex}", endpointNode);

                TBuffer dataResult = await Crawl(endpointNode, inputToBuffer(input), token, executionChain);
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
                LogExecutionSummary(endpointNode, stopwatch.Elapsed, executionChain, exception);
            }

            return result;
        }

        private async Task<TBuffer> Crawl(Type endpointNode, TBuffer input, CancellationToken? token, List<string> executionChain)
        {
            INodeResult<TBuffer> current = new CompletedNode<TBuffer>(input);

            // Обработка middleware
            foreach (var middlewareType in _middlewareTypes)
            {
                var middleware =
                    (Middleware<TBuffer>)
                    _serviceProvider.GetRequiredService(middlewareType);

                if (await middleware.GetCondition(current.Object, token))
                {
                    current =
                        await middleware.Invoke(current.Object, token);

                    executionChain.Add(
                        $"Middleware: {middlewareType.Name}");

                    if (current.NextNode != null)
                        break;
                }
            }

            if (current.NextNode == null)
            {
                current = new ProlongedNode<TBuffer>(endpointNode, current.Object);
            }

            // Обработка узлов
            while (current.NextNode != null)
            {
                Node<TBuffer> node = GetNode(current.NextNode);

                current =
                    await node.Invoke(
                        current.Object,
                        token);
            }

            return current.Object;
        }

        private Node<TBuffer> GetNode(Type nodeType)
        {
            return (Node<TBuffer>)_serviceProvider.GetRequiredService(nodeType);
        }

        private void LogExecutionSummary(
            Type startNode,
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

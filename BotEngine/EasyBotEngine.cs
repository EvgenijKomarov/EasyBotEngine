using Engine.Exceptions;
using Engine.NodeResults;
using Engine.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Engine
{
    /// <summary>
    /// Business logic engine
    /// </summary>
    /// <typeparam name="TInput">Input data. Inherit of interface IEngineInput</typeparam>
    /// <typeparam name="TBuffer">Data that crawls between nodes</typeparam>
    /// <typeparam name="TOutput">Result of the process</typeparam>
    public class EasyBotEngine<TInput, TBuffer, TOutput>
    where TInput : IEngineInput<TBuffer>
    where TBuffer : notnull
    where TOutput : notnull
    {
        private readonly ILogger<EasyBotEngine<TInput, TBuffer, TOutput>>? _logger;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider">Service provider for DI. It should contains all nodes and middlewares. 
        /// All nodes and middlewares should be registered as Transient or Scoped Registration example:
        /// services.AddTransient&lt;MyNode&gt;();
        /// services.AddTransient&lt;MyEndpointNode&gt;();
        /// services.AddTransient&lt;MyMiddleware&gt;();</param>
        /// <param name="logger">Logger instance (optional)</param>
        public EasyBotEngine(
            IServiceProvider serviceProvider,
            ILogger<EasyBotEngine<TInput, TBuffer, TOutput>>? logger = null)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Get all node types from engine
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<Type> GetAllNodeTypes()
        {
            return GetAllNodes().Select(x => x.GetType()).ToList();
        }

        /// <summary>
        /// Get all endpoint node types from engine
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Type> GetEndpointNodeTypes()
        {
            return GetEndpointNodes()
				.ToDictionary(
			    x => x.EndpointId,
			    x => x.GetType()
		    );
		}

        /// <summary>
        /// Get all middleware types from engine
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<Type> GetMiddlewareTypes()
        {
            return GetMiddlewares().Select(x => x.GetType()).ToList();
        }

        /// <summary>
        /// Use this method to process data
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="token"></param>
        /// <returns>Output data</returns>
        public async Task<TOutput?> Process(TInput input, CancellationToken? token = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var executionChain = new List<string>();
            TOutput? result = default(TOutput);
            Exception? exception = null;

            try
            {
                var endpointNode = GetEndpointNode(input.EndpointNodeId);
                _logger?.LogDebug("Starting process with node: {NodeIndex}", input.EndpointNodeId);

                result = await Crawl(endpointNode.GetType(), input.Object, token, executionChain);
            }
            catch (Exception ex)
            {
                // For specific exceptions we should let caller handle them (tests expect exceptions to be thrown)
                if (ex is EndpointNodeNotFoundException || ex is NodeNotFoundException)
                    throw;

                if (ex is OperationCanceledException || ex is TaskCanceledException)
                    throw new TaskCanceledException("Cancellation", ex);

                exception = ex;
                _logger?.LogError(ex, "Process failed with error: {ErrorMessage}", ex.Message);
            }
            finally
            {
                stopwatch.Stop();
                LogExecutionSummary(input.EndpointNodeId, stopwatch.Elapsed, executionChain, exception);
            }

            return result;
        }

        private async Task<TOutput> Crawl(Type endpointNode, TBuffer input, CancellationToken? token, List<string> executionChain)
        {
            INodeResult<TBuffer, TOutput> current = new MiddlewaredNode<TBuffer, TOutput>(input);

            // Обработка middleware
            foreach (var middleware in GetMiddlewares())
            {
                if (await middleware.GetCondition(current.Object, token))
                {
                    current =
                        await middleware.Invoke(current.Object, token);

                    executionChain.Add(
                        $"[Middleware]: {middleware.GetType().Name}");

                    if (current.NextNode != null)
                        break;
                }
            }

            // If after middleware processing we already have a final output -> return it
            var defaultOutput = default(TOutput);
            if (!EqualityComparer<TOutput>.Default.Equals(current.Output, defaultOutput))
            {
                return current.Output;
            }

            if (current.NextNode == null)
            {
                current = new ProlongedNode<TBuffer, TOutput>(endpointNode, current.Object);
            }

            // Обработка nodes
            while (current.NextNode != null)
            {
                Node<TBuffer, TOutput> node = GetNode(current.NextNode);

                current =
                    await node.Invoke(
                        current.Object,
                        token);

                executionChain.Add(
                        $"[Node]: {node.GetType().Name}");
            }

            return current.Output;
        }

        /// <summary>
        /// Get all node types from engine
        /// </summary>
        /// <returns></returns>
        private IReadOnlyList<Node<TBuffer, TOutput>> GetAllNodes()
        {
            return _serviceProvider.GetServices<Node<TBuffer, TOutput>>().ToList();
        }

        /// <summary>
        /// Get all endpoints nodes from engine
        /// </summary>
        /// <returns></returns>
        private IReadOnlyList<EndpointNode<TBuffer, TOutput>> GetEndpointNodes()
        {
            return _serviceProvider.GetServices<EndpointNode<TBuffer, TOutput>>().ToList();
        }

        /// <summary>
        /// Get all middlewares from engine
        /// </summary>
        /// <returns></returns>
        private IReadOnlyList<Middleware<TBuffer, TOutput>> GetMiddlewares()
        {
            return _serviceProvider.GetServices<Middleware<TBuffer, TOutput>>().ToList();
        }

        private EndpointNode<TBuffer, TOutput> GetEndpointNode(string nodeId)
        {
            if (GetEndpointNodeTypes().TryGetValue(nodeId, out Type node)) 
            {
				return GetEndpointNodes().First(x => x.GetType() == node);
			}
            throw new EndpointNodeNotFoundException(nodeId);
        }

        private Node<TBuffer, TOutput> GetNode(Type nodeType)
        {
            var node = GetAllNodes().FirstOrDefault(x => x.GetType() == nodeType);
            if (node == null) throw new NodeNotFoundException(nodeType);
            return node;
        }

        private void LogExecutionSummary(
            string startNodeId,
            TimeSpan duration,
            List<string> executionChain,
            Exception? exception)
        {
            if (_logger == null) return;

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("=== Execution Summary ===");
            sb.AppendLine($"Start Node: {startNodeId}");
            sb.AppendLine($"Duration: {duration.TotalMilliseconds}ms");
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

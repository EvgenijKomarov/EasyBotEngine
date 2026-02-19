using Engine.Nodes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a Node type
        /// </summary>
        /// <typeparam name="TNode">Node type to register</typeparam>
        /// <typeparam name="TBuffer">Buffer type</typeparam>
        /// <typeparam name="TOutput">Output type</typeparam>
        /// <param name="services">Service collection</param>
        /// <param name="lifetime">Service lifetime (default: Transient)</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddEngineNode<TNode, TBuffer, TOutput>(
            this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TNode : notnull, INode<TBuffer, TOutput>
            where TBuffer : notnull
            where TOutput : notnull
        {
            var nodeType = typeof(TNode);
            var baseNodeType = typeof(INode<TBuffer, TOutput>);

            services.Add(new ServiceDescriptor(nodeType, nodeType, lifetime));
            services.Add(new ServiceDescriptor(baseNodeType, sp => sp.GetRequiredService(nodeType), lifetime));

            return services;
        }

        /// <summary>
        /// Registers an EndpointNode type
        /// </summary>
        /// <typeparam name="TNode">EndpointNode type to register</typeparam>
        /// <typeparam name="TBuffer">Buffer type</typeparam>
        /// <typeparam name="TOutput">Output type</typeparam>
        /// <param name="services">Service collection</param>
        /// <param name="lifetime">Service lifetime (default: Transient)</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddEngineEndpointNode<TNode, TBuffer, TOutput>(
            this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TNode : notnull, IEndpointNode<TBuffer, TOutput>
            where TBuffer : notnull
            where TOutput : notnull
        {
            var nodeType = typeof(TNode);
            var baseNodeType = typeof(INode<TBuffer, TOutput>);
            var baseEndpointNodeType = typeof(IEndpointNode<TBuffer, TOutput>);

            services.Add(new ServiceDescriptor(nodeType, nodeType, lifetime));
            services.Add(new ServiceDescriptor(baseEndpointNodeType, sp => sp.GetRequiredService(nodeType), lifetime));
            services.Add(new ServiceDescriptor(baseNodeType, sp => sp.GetRequiredService(nodeType), lifetime));

            return services;
        }

        /// <summary>
        /// Registers a Middleware type
        /// </summary>
        /// <typeparam name="TMiddleware">Middleware type to register</typeparam>
        /// <typeparam name="TBuffer">Buffer type</typeparam>
        /// <typeparam name="TOutput">Output type</typeparam>
        /// <param name="services">Service collection</param>
        /// <param name="lifetime">Service lifetime (default: Transient)</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddEngineMiddleware<TMiddleware, TBuffer, TOutput>(
            this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TMiddleware : notnull, IMiddleware<TBuffer, TOutput>
            where TBuffer: notnull
            where TOutput : notnull
        {
            var middlewareType = typeof(TMiddleware);
            var baseMiddlewareType = typeof(IMiddleware<TBuffer, TOutput>);

            services.Add(new ServiceDescriptor(middlewareType, middlewareType, lifetime));
            services.Add(new ServiceDescriptor(baseMiddlewareType, sp => sp.GetRequiredService(middlewareType), lifetime));

            return services;
        }
    }
}

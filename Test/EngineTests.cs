using Engine;
using Engine.Exceptions;
using Engine.Extensions;
using Engine.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace EngineTests;

#region Test Models

public class TestBuffer
{
    public int Value { get; set; }
    public List<string> History { get; } = new();
}

// Результат обработки
public class TestOutput
{
    public int FinalValue { get; set; }
    public string Status { get; set; } = "Success";
}

// Входные данные
public class TestInput : IEngineInput<TestBuffer>
{
    public required Type EndpointNode { get; init; }
    public required TestBuffer Object { get; init; }
}

#endregion


#region Test Nodes


public class StartNode : EndpointNode<TestBuffer, TestOutput>
{
    public override Task<INodeResult<TestBuffer, TestOutput>> Invoke(
        TestBuffer input,
        CancellationToken? token = null)
    {
        input.History.Add("StartNode");
        input.Value += 10;
        return Task.FromResult(Next<ProcessingNode>(input));
    }
}

// Обычная нода
public class ProcessingNode : Node<TestBuffer, TestOutput>
{
    public override Task<INodeResult<TestBuffer, TestOutput>> Invoke(
        TestBuffer input,
        CancellationToken? token = null)
    {
        input.History.Add("ProcessingNode");
        input.Value *= 2;
        return Task.FromResult(Next<FinalNode>(input));
    }
}

// Финальная нода
public class FinalNode : Node<TestBuffer, TestOutput>
{
    public override Task<INodeResult<TestBuffer, TestOutput>> Invoke(
        TestBuffer input,
        CancellationToken? token = null)
    {
        input.History.Add("FinalNode");
        return Task.FromResult(Complete(new TestOutput
        {
            FinalValue = input.Value,
            Status = "Completed"
        }));
    }
}

// Нода с ветвлением
public class BranchingNode : Node<TestBuffer, TestOutput>
{
    public override Task<INodeResult<TestBuffer, TestOutput>> Invoke(
        TestBuffer input,
        CancellationToken? token = null)
    {
        input.History.Add("BranchingNode");
        return input.Value > 50
            ? Task.FromResult(Next<HighValueNode>(input))
            : Task.FromResult(Next<LowValueNode>(input));
    }
}

public class HighValueNode : Node<TestBuffer, TestOutput>
{
    public override Task<INodeResult<TestBuffer, TestOutput>> Invoke(
        TestBuffer input,
        CancellationToken? token = null)
    {
        input.History.Add("HighValueNode");
        input.Value += 100;
        return Task.FromResult(Complete(new TestOutput
        {
            FinalValue = input.Value,
            Status = "High"
        }));
    }
}

public class LowValueNode : Node<TestBuffer, TestOutput>
{
    public override Task<INodeResult<TestBuffer, TestOutput>> Invoke(
        TestBuffer input,
        CancellationToken? token = null)
    {
        input.History.Add("LowValueNode");
        input.Value -= 10;
        return Task.FromResult(Complete(new TestOutput
        {
            FinalValue = input.Value,
            Status = "Low"
        }));
    }
}

// Middleware с условием
public class ConditionalMiddleware : Middleware<TestBuffer, TestOutput>
{
    private readonly int _threshold;

    public ConditionalMiddleware(int threshold = 30)
    {
        _threshold = threshold;
    }

    public override Task<bool> GetCondition(
        TestBuffer input,
        CancellationToken? token = null)
    {
        return Task.FromResult(input.Value > _threshold);
    }

    public override Task<INodeResult<TestBuffer, TestOutput>> Invoke(
        TestBuffer input,
        CancellationToken? token = null)
    {
        input.History.Add($"[MW] ConditionalMiddleware (threshold={_threshold})");
        input.Value += 5;
        return Task.FromResult(Complete(input));
    }
}

// Middleware без условия (всегда применяется)
public class AlwaysOnMiddleware : Middleware<TestBuffer, TestOutput>
{
    public override Task<bool> GetCondition(
        TestBuffer input,
        CancellationToken? token = null)
    {
        return Task.FromResult(true);
    }

    public override Task<INodeResult<TestBuffer, TestOutput>> Invoke(
        TestBuffer input,
        CancellationToken? token = null)
    {
        input.History.Add("[MW] AlwaysOnMiddleware");
        input.Value *= 3;
        return Task.FromResult(Complete(input));
    }
}

// Middleware, перехватывающий выполнение
public class ShortCircuitMiddleware : Middleware<TestBuffer, TestOutput>
{
    public override Task<bool> GetCondition(
        TestBuffer input,
        CancellationToken? token = null)
    {
        return Task.FromResult(input.Value % 2 == 0);
    }

    public override Task<INodeResult<TestBuffer, TestOutput>> Invoke(
        TestBuffer input,
        CancellationToken? token = null)
    {
        input.History.Add("[MW] ShortCircuitMiddleware");
            // Short-circuit and return a final output
            var output = new TestOutput
            {
                FinalValue = input.Value * 10,
                Status = "ShortCircuited"
            };

            return Task.FromResult(Finish(output));
    }
}

// Нода, бросающая исключение
public class FailingNode : Node<TestBuffer, TestOutput>
{
    public override Task<INodeResult<TestBuffer, TestOutput>> Invoke(
        TestBuffer input,
        CancellationToken? token = null)
    {
        input.History.Add("FailingNode");
        throw new InvalidOperationException("Simulated node failure");
    }
}

// Нода, реагирующая на отмену
public class CancellableNode : Node<TestBuffer, TestOutput>
{
    public override async Task<INodeResult<TestBuffer, TestOutput>> Invoke(
        TestBuffer input,
        CancellationToken? token = null)
    {
        input.History.Add("CancellableNode");
        token?.ThrowIfCancellationRequested();
        await Task.Delay(10, token ?? CancellationToken.None);
        input.Value += 100;
        return Complete(new TestOutput { FinalValue = input.Value });
    }
}

class CountingNode : EndpointNode<TestBuffer, TestOutput>
{
    public static int InstanceCount { get; private set; }

    public CountingNode() => InstanceCount++;

    public override Task<INodeResult<TestBuffer, TestOutput>> Invoke(
        TestBuffer input,
        CancellationToken? token = null)
    {
        input.History.Add($"CountingNode_Instance_{InstanceCount}");
        return Task.FromResult(Complete(new TestOutput { FinalValue = input.Value }));
    }
}

#endregion

public abstract class EngineTestBase
{
    protected IServiceProvider CreateServiceProvider(
        Action<ServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();

        // Регистрация движка
        services.AddSingleton<EasyBotEngine<TestInput, TestBuffer, TestOutput>>();

        // Базовая регистрация нод и мидлваров
        services.AddEngineNode<StartNode, TestBuffer, TestOutput>();
        services.AddEngineNode<ProcessingNode, TestBuffer, TestOutput>();
        services.AddEngineNode<FinalNode, TestBuffer, TestOutput>();
        services.AddEngineNode<BranchingNode, TestBuffer, TestOutput>();
        services.AddEngineNode<HighValueNode, TestBuffer, TestOutput>();
        services.AddEngineNode<LowValueNode, TestBuffer, TestOutput>();
        services.AddEngineNode<FailingNode, TestBuffer, TestOutput>();
        services.AddEngineNode<CancellableNode, TestBuffer, TestOutput>();

        services.AddEngineEndpointNode<StartNode, TestBuffer, TestOutput>();


        // Дополнительная конфигурация из теста
        configure?.Invoke(services);

        return services.BuildServiceProvider();
    }
}


[TestFixture]
public class EasyBotEngineTests : EngineTestBase
{
    [Test]
    public async Task Process_ShouldExecuteNodeChainCorrectly()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var engine = provider.GetRequiredService<EasyBotEngine<TestInput, TestBuffer, TestOutput>>();

        var input = new TestInput
        {
            EndpointNode = typeof(StartNode),
            Object = new TestBuffer { Value = 5 }
        };

        // Act
        var result = await engine.Process(input);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.FinalValue, Is.EqualTo((5 + 10) * 2)); // StartNode(+10) -> ProcessingNode(*2) -> FinalNode
        Assert.That(result.Status, Is.EqualTo("Completed"));
        CollectionAssert.AreEqual(
            new[] { "StartNode", "ProcessingNode", "FinalNode" },
            input.Object.History
        );
    }

    [Test]
    public async Task Process_ShouldApplyMiddlewareWhenConditionMet()
    {
        // Arrange
        var provider = CreateServiceProvider(services =>
        {
            services.AddTransient<ConditionalMiddleware>();
            services.AddTransient<IMiddleware<TestBuffer, TestOutput>, ConditionalMiddleware>();
        });

        var engine = provider.GetRequiredService<EasyBotEngine<TestInput, TestBuffer, TestOutput>>();

        var input = new TestInput
        {
            EndpointNode = typeof(StartNode),
            Object = new TestBuffer { Value = 40 } // > threshold (30)
        };

        // Act
        var result = await engine.Process(input);

        // Assert
        Assert.That(result.FinalValue, Is.EqualTo((40 + 5 + 10) * 2)); // MW(+5) -> StartNode(+10) -> ProcessingNode(*2)
        StringAssert.Contains("ConditionalMiddleware", string.Join(", ", input.Object.History));
    }

    [Test]
    public async Task Process_ShouldSkipMiddlewareWhenConditionNotMet()
    {
        // Arrange
        var provider = CreateServiceProvider(services =>
        {
            services.AddTransient<ConditionalMiddleware>();
            services.AddTransient<IMiddleware<TestBuffer, TestOutput>, ConditionalMiddleware>();
        });

        var engine = provider.GetRequiredService<EasyBotEngine<TestInput, TestBuffer, TestOutput>>();

        var input = new TestInput
        {
            EndpointNode = typeof(StartNode),
            Object = new TestBuffer { Value = 20 } // < threshold (30)
        };

        // Act
        var result = await engine.Process(input);

        // Assert
        Assert.That(result.FinalValue, Is.EqualTo((20 + 10) * 2)); // Без мидлвара
        CollectionAssert.DoesNotContain(input.Object.History,
            "ConditionalMiddleware");
    }

    [Test]
    public async Task Process_ShouldSupportShortCircuitFromMiddleware()
    {
        // Arrange
        var provider = CreateServiceProvider(services =>
        {
            services.AddTransient<ShortCircuitMiddleware>();
            services.AddTransient<IMiddleware<TestBuffer, TestOutput>, ShortCircuitMiddleware>();
        });

        var engine = provider.GetRequiredService<EasyBotEngine<TestInput, TestBuffer, TestOutput>>();

        var input = new TestInput
        {
            EndpointNode = typeof(StartNode),
            Object = new TestBuffer { Value = 8 } // Чётное → триггерит шорт-сёркит
        };

        // Act
        var result = await engine.Process(input);

        // Assert
        Assert.That(result.Status, Is.EqualTo("ShortCircuited"));
        Assert.That(result.FinalValue, Is.EqualTo(8 * 10)); // Мидлвар вернул результат напрямую
        CollectionAssert.Contains(input.Object.History, "[MW] ShortCircuitMiddleware");
        CollectionAssert.DoesNotContain(input.Object.History, "StartNode"); // Ноды не вызывались
    }

    [Test]
    public async Task Process_ShouldHandleBranchingLogicCorrectly()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var engine = provider.GetRequiredService<EasyBotEngine<TestInput, TestBuffer, TestOutput>>();

        var inputHigh = new TestInput
        {
            EndpointNode = typeof(BranchingNode),
            Object = new TestBuffer { Value = 60 }
        };

        var inputLow = new TestInput
        {
            EndpointNode = typeof(BranchingNode),
            Object = new TestBuffer { Value = 30 }
        };

        // Act
        var resultHigh = await engine.Process(inputHigh);
        var resultLow = await engine.Process(inputLow);

        // Assert
        Assert.That(resultHigh.Status, Is.EqualTo("High"));
        Assert.That(resultHigh.FinalValue, Is.EqualTo(60 + 100));

        Assert.That(resultLow.Status, Is.EqualTo("Low"));
        Assert.That(resultLow.FinalValue, Is.EqualTo(30 - 10));

        CollectionAssert.Contains(inputHigh.Object.History, "HighValueNode");
        CollectionAssert.Contains(inputLow.Object.History, "LowValueNode");
    }

    [Test]
    public void Process_ShouldThrowWhenEndpointNodeNotFound()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var engine = provider.GetRequiredService<EasyBotEngine<TestInput, TestBuffer, TestOutput>>();

        var input = new TestInput
        {
            EndpointNode = typeof(object), // Невалидный тип
            Object = new TestBuffer { Value = 5 }
        };

        // Act & Assert
        Assert.ThrowsAsync<EndpointNodeNotFoundException>(async () =>
            await engine.Process(input));
    }

    [Test]
    public void Process_ShouldThrowWhenNodeNotFoundInChain()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var engine = provider.GetRequiredService<EasyBotEngine<TestInput, TestBuffer, TestOutput>>();

        // Создаём ноду, которая ссылается на незарегистрированный тип
        var invalidNode = new InvalidNextNode();
        var input = new TestInput
        {
            EndpointNode = typeof(InvalidNextNode),
            Object = new TestBuffer { Value = 5 }
        };

        // Act & Assert
        Assert.ThrowsAsync<NodeNotFoundException>(async () =>
            await engine.Process(input));
    }

    // Вспомогательная нода для теста выше
    private class InvalidNextNode : IEndpointNode<TestBuffer, TestOutput>
    {
        public override Task<INodeResult<TestBuffer, TestOutput>> Invoke(
            TestBuffer input,
            CancellationToken? token = null)
        {
            // Ссылается на незарегистрированный тип
            return Task.FromResult(Next<NonExistentNode>(input));
        }
    }

    private class NonExistentNode : INode<TestBuffer, TestOutput>
    {
        public override Task<INodeResult<TestBuffer, TestOutput>> Invoke(
            TestBuffer input,
            CancellationToken? token = null) => throw new NotImplementedException();
    }

    [Test]
    public async Task Process_ShouldHandleNodeExceptionsGracefully()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var engine = provider.GetRequiredService<EasyBotEngine<TestInput, TestBuffer, TestOutput>>();

        var input = new TestInput
        {
            EndpointNode = typeof(FailingNode),
            Object = new TestBuffer { Value = 5 }
        };

        // Act & Assert
        var result = await engine.Process(input);
        Assert.That(result, Is.Null); // В текущей реализации возвращается default(TOutput)
        // Логгирование ошибки проверяется через мок логгера в отдельном тесте
    }

    [Test]
    public async Task Process_ShouldRespectCancellationToken()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var engine = provider.GetRequiredService<EasyBotEngine<TestInput, TestBuffer, TestOutput>>();

        var input = new TestInput
        {
            EndpointNode = typeof(CancellableNode),
            Object = new TestBuffer { Value = 5 }
        };

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Отменяем немедленно

        // Act & Assert
        var ex = Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await engine.Process(input, cts.Token));

        StringAssert.Contains("Cancellation", ex.Message);
    }

    [Test]
    public async Task Process_ShouldCreateNewInstancesForEachCall_TransientLifetime()
    {
        // Arrange
        var callCount = 0;
        var provider = CreateServiceProvider(services =>
        {
            services.AddTransient<CountingNode>();
            services.AddTransient<INode<TestBuffer, TestOutput>, CountingNode>();
        });

        var engine = provider.GetRequiredService<EasyBotEngine<TestInput, TestBuffer, TestOutput>>();

        var input1 = new TestInput
        {
            EndpointNode = typeof(CountingNode),
            Object = new TestBuffer { Value = 1 }
        };

        var input2 = new TestInput
        {
            EndpointNode = typeof(CountingNode),
            Object = new TestBuffer { Value = 2 }
        };

        // Act
        await engine.Process(input1);
        await engine.Process(input2);

        // Assert
        Assert.That(CountingNode.InstanceCount, Is.GreaterThanOrEqualTo(2),
            "Должно быть создано минимум 2 экземпляра (Transient lifetime)");
    }

    [Test]
    public async Task Process_ShouldHandleMiddlewareOrderCorrectly()
    {
        // Arrange
        var provider = CreateServiceProvider(services =>
        {
            // Регистрируем мидлвары в определённом порядке
            services.AddTransient<FirstMiddleware>();
            services.AddTransient<IMiddleware<TestBuffer, TestOutput>, FirstMiddleware>();

            services.AddTransient<SecondMiddleware>();
            services.AddTransient<IMiddleware<TestBuffer, TestOutput>, SecondMiddleware>();
        });

        var engine = provider.GetRequiredService<EasyBotEngine<TestInput, TestBuffer, TestOutput>>();

        var input = new TestInput
        {
            EndpointNode = typeof(StartNode),
            Object = new TestBuffer { Value = 1 }
        };

        // Act
        var result = await engine.Process(input);

        // Assert
        // Порядок в DI определяется порядком регистрации
        var history = string.Join(" -> ", input.Object.History);
        StringAssert.Contains("[MW] FirstMiddleware", history);
        StringAssert.Contains("[MW] SecondMiddleware", history);
        Assert.That(input.Object.History.IndexOf("[MW] FirstMiddleware"),
            Is.LessThan(input.Object.History.IndexOf("[MW] SecondMiddleware")));
    }

    class FirstMiddleware : IMiddleware<TestBuffer, TestOutput>
    {
        public Task<bool> GetCondition(TestBuffer input, CancellationToken? token) => Task.FromResult(true);
        public Task<INodeResult<TestBuffer, TestOutput>> Invoke(TestBuffer input, CancellationToken? token)
        {
             input.History.Add("[MW] FirstMiddleware");
             input.Value += 10;
             return Task.FromResult(Complete(input));
        }
    }

    class SecondMiddleware : IMiddleware<TestBuffer, TestOutput>
    {
        public Task<bool> GetCondition(TestBuffer input, CancellationToken? token) => Task.FromResult(true);
        public Task<INodeResult<TestBuffer, TestOutput>> Invoke(TestBuffer input, CancellationToken? token)
        {
             input.History.Add("[MW] SecondMiddleware");
             input.Value += 20;
             return Task.FromResult(Complete(input));
        }
     }
}
using Engine;
using Engine.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace EngineTests;

[TestFixture]
public class EasyEngineTests
{
    private ServiceProvider _provider;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddTransient<StartNode>();
        services.AddTransient<SecondNode>();
        services.AddTransient<FinalNode>();

        services.AddTransient<TestMiddleware>();

        _provider = services.BuildServiceProvider();
    }

    private EasyEngine<string, TestBuffer, string> CreateEngine()
    {
        return new EasyEngine<string, TestBuffer, string>(
            _provider,
            input => new TestBuffer { Value = input },
            buffer => buffer.Value,
            NullLogger<EasyEngine<string, TestBuffer, string>>.Instance);
    }

    // =============================
    // SUCCESS FLOW
    // =============================

    [Test]
    public async Task Process_Should_Execute_All_Nodes()
    {
        var engine = CreateEngine();

        engine
            .AddNode<StartNode>()
            .AddNode<SecondNode>()
            .AddNode<FinalNode>();

        var result =
            await engine.Process(
                typeof(StartNode),
                "start");

        Assert.That(
            result,
            Is.EqualTo(
                "start->Start->Second->Final"));
    }

    // =============================
    // MIDDLEWARE TEST
    // =============================

    [Test]
    public async Task Middleware_Should_Modify_Buffer()
    {
        var engine = CreateEngine();

        engine
            .AddNode<StartNode>()
            .AddNode<SecondNode>()
            .AddNode<FinalNode>()
            .AddMiddleware<TestMiddleware>();

        var result =
            await engine.Process(
                typeof(StartNode),
                "start");

        Assert.That(
            result,
            Is.EqualTo(
                "start->Middleware->Start->Second->Final"));
    }

    // =============================
    // COMPLETE EARLY
    // =============================

    [Test]
    public async Task Node_Should_Complete_Process()
    {
        var services = new ServiceCollection();

        services.AddTransient<EarlyCompleteNode>();

        var provider =
            services.BuildServiceProvider();

        var engine =
            new EasyEngine<string, TestBuffer, string>(
                provider,
                x => new TestBuffer { Value = x },
                x => x.Value);

        engine.AddNode<EarlyCompleteNode>();

        var result =
            await engine.Process(
                typeof(EarlyCompleteNode),
                "start");

        Assert.That(
            result,
            Is.EqualTo(
                "start->Completed"));
    }

    // =============================
    // DI CREATES NEW INSTANCES
    // =============================

    [Test]
    public async Task Should_Create_New_Node_Instance_Per_Run()
    {
        var counter = new Counter();

        var services = new ServiceCollection();

        services.AddSingleton(counter);

        services.AddTransient<CounterNode>();

        var provider =
            services.BuildServiceProvider();

        var engine =
            new EasyEngine<string, TestBuffer, string>(
                provider,
                x => new TestBuffer { Value = x },
                x => x.Value);

        engine.AddNode<CounterNode>();

        await engine.Process(
            typeof(CounterNode),
            "test");

        await engine.Process(
            typeof(CounterNode),
            "test");

        Assert.That(counter.Count, Is.EqualTo(2));
    }
}

//
// TEST BUFFER
//

public class TestBuffer
{
    public string Value { get; set; } = "";
}

//
// TEST NODES
//

public class StartNode : Node<TestBuffer>
{
    public override Task<INodeResult<TestBuffer>> Invoke(
        TestBuffer input,
        CancellationToken? token = null)
    {
        input.Value += "->Start";

        return Task.FromResult(
            Next<SecondNode>(input));
    }
}

public class SecondNode : Node<TestBuffer>
{
    public override Task<INodeResult<TestBuffer>> Invoke(
        TestBuffer input,
        CancellationToken? token = null)
    {
        input.Value += "->Second";

        return Task.FromResult(
            Next<FinalNode>(input));
    }
}

public class FinalNode : Node<TestBuffer>
{
    public override Task<INodeResult<TestBuffer>> Invoke(
        TestBuffer input,
        CancellationToken? token = null)
    {
        input.Value += "->Final";

        return Task.FromResult(
            Complete(input));
    }
}

public class EarlyCompleteNode : Node<TestBuffer>
{
    public override Task<INodeResult<TestBuffer>> Invoke(
        TestBuffer input,
        CancellationToken? token = null)
    {
        input.Value += "->Completed";

        return Task.FromResult(
            Complete(input));
    }
}

//
// MIDDLEWARE
//

public class TestMiddleware : Middleware<TestBuffer>
{
    public override Task<INodeResult<TestBuffer>> Invoke(
        TestBuffer input,
        CancellationToken? token = null)
    {
        input.Value += "->Middleware";

        return Task.FromResult(
            Complete(input));
    }
}

//
// COUNTER TEST
//

public class Counter
{
    public int Count;
}

public class CounterNode : Node<TestBuffer>
{
    private readonly Counter _counter;

    public CounterNode(Counter counter)
    {
        _counter = counter;
    }

    public override Task<INodeResult<TestBuffer>> Invoke(
        TestBuffer input,
        CancellationToken? token = null)
    {
        _counter.Count++;

        return Task.FromResult(
            Complete(input));
    }
}

using BotEngine;
using BotEngine.Domain;
using Test.TestNodes;

namespace Test
{
    public class Tests
    {
        EasyBotEngine engine;

        [SetUp]
        public void Setup()
        {
            engine = new EasyBotEngine();
            engine.AddNode<StartNode>();
            engine.AddNode<EndNode>();
        }

        [Test]
        public async Task Test1()
        {
            var mes = await engine.ProcessMessage(new TextMessage(1, "Null"));

            Assert.That(mes.Text == "Welcome");
        }

        [Test]
        public async Task Test2()
        {
            var mes = await engine.ProcessMessage(new TextMessage(1, "End"));

            Assert.That(mes.Text == "Goodbye");
        }
    }
}
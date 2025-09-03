using Engine;
using Microsoft.Extensions.Logging;
using Test.TestNodes;
using Test.TestNodes.Datas;

namespace Test
{
    public class Tests
    {
        EasyEngine<InputData, DataBuffer, OutputData> engine;

        [SetUp]
        public void Setup()
        {
            engine = new EasyEngine<InputData, DataBuffer, OutputData>(
                x => new DataBuffer { 
                    MessageId = x.MessageId.ToString(),
                    Text = x.Text,
                    UserId = x.UserId
                },
                x => new OutputData
                {
                    MessageId = int.TryParse(x.MessageId, out int mesId) ? mesId : 0,
                    Text = x.Text,
                    UserId = int.TryParse(x.UserId, out int usId) ? usId : 0
                });
            engine.AddNode(new StartNode());
            engine.AddNode(new EndNode());
            engine.AddMiddleware(new DefaultMiddleware());
        }

        [Test]
        public async Task Test1()
        {
            var input = new InputData
            {
                MessageId = 1,
                Text = "Hello",
                UserId = "3"
            };
            var mes = await engine.Process("Text", input);

            Assert.That(mes.Text == "Welcome" && mes.UserId.ToString() == input.UserId && mes.MessageId == input.MessageId);
        }

        [Test]
        public async Task Test2()
        {
            var input = new InputData
            {
                MessageId = 1,
                Text = "End",
                UserId = "3"
            };
            var mes = await engine.Process("Text", input);

            Assert.That(mes.Text == "Goodbye" && mes.UserId.ToString() == input.UserId && mes.MessageId == input.MessageId);
        }

        [Test]
        public async Task Test3()
        {
            var input = new InputData
            {
                MessageId = 1,
                Text = "Hello",
                UserId = string.Empty
            };
            var mes = await engine.Process("Text", input);

            Assert.That(mes.Text == "Goodbye" && mes.MessageId == input.MessageId);
        }
    }
}
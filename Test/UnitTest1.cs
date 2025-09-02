using BotEngine;
using Test.TestNodes;
using Test.TestNodes.Datas;

namespace Test
{
    public class Tests
    {
        EasyBotEngine<InputData, DataBuffer, OutputData> engine;

        [SetUp]
        public void Setup()
        {
            engine = new EasyBotEngine<InputData, DataBuffer, OutputData>(
                x => new DataBuffer { 
                    MessageId = x.MessageId.ToString(),
                    Text = x.Text,
                    UserId = x.UserId
                },
                x => new OutputData
                {
                    MessageId = int.Parse(x.MessageId),
                    Text = x.Text,
                    UserId = int.Parse(x.UserId)
                });
            engine.AddNode(new StartNode());
            engine.AddNode(new EndNode());
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
    }
}
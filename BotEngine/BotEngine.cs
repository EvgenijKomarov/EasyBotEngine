using BotEngine.Domain;
using BotEngine.InputMessages;
using BotEngine.Nodes;
using BotEngine.OutputMessage;

namespace BotEngine
{
    public class BotEngine
    {
        private Dictionary<Type, string[]> _nodeValidators = new Dictionary<Type, string[]>();
        protected Type defaultNodeType = typeof(DefaultNode);

        public BotEngine(Type defaultNodeType)
        {
            this.defaultNodeType = defaultNodeType ?? throw new ArgumentNullException(nameof(defaultNodeType));

            if (!typeof(Node).IsAssignableFrom(defaultNodeType))
                throw new ArgumentException($"Type {defaultNodeType.Name} must inherit from Node");
        }
        public BotEngine() { }

        public BotEngine AddNode<T>() where T : Node, new()
        {
            var nodeType = typeof(T);
            var tempNode = new T();
            _nodeValidators[nodeType] = tempNode.GetIdentificators();
            return this;
        }

        public async Task<Message?> ProcessMessage(MessageInput message)
        {
            var node = await GetNode(message);
            return node != null ? new Message
            {
                Text = node.Text,
                Buttons = node.Buttons,
                MessageId = node.MessageId
            } : null;
        }

        private async Task<Node?> GetNode(MessageInput message)
        {
            Type nodeType = null;

            foreach (var kvp in _nodeValidators)
            {
                if (kvp.Value.Contains(message.GetIdentificator()))
                {
                    nodeType = kvp.Key;
                    break;
                }
            }

            nodeType = nodeType ?? defaultNodeType;

            if (Activator.CreateInstance(nodeType) is Node node)
            {
                await node.Invoke(message);
                if (node.IsNeedProlongedIteration)
                {
                    return await GetNode(new NextMessageContext(node.NextIdentificator, node.NextData));
                }
                return node;
            }

            return null;
        }
    }
}

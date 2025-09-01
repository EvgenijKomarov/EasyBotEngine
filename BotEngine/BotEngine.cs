using BotEngine.Domain;
using BotEngine.InputMessages;
using BotEngine.Nodes;
using BotEngine.OutputMessage;

namespace BotEngine
{
    public class BotEngine
    {
        private Dictionary<Type, string[]> _nodeValidators = new Dictionary<Type, string[]>();
        protected Type defaultNodeType;

        public BotEngine(Type defaultNodeType)
        {
            if (defaultNodeType == null)
                throw new ArgumentNullException(nameof(defaultNodeType));

            if (!typeof(Node).IsAssignableFrom(defaultNodeType))
                throw new ArgumentException($"Type {defaultNodeType.Name} must inherit from Node");

            this.defaultNodeType = defaultNodeType;
        }

        public BotEngine AddNode<T>() where T : Node, new()
        {
            var nodeType = typeof(T);
            var tempNode = new T();
            _nodeValidators[nodeType] = tempNode.GetValidators();
            return this;
        }

        public async Task<Message?> ProcessMessage(MessageInput message)
        {
            var node = await GetNode(message);
            if (node != null) 
            {
                return new Message
                {
                    Text = node.Text,
                    Buttons = node.Buttons
                };
            }
            return null;
        }

        private async Task<Node?> GetNode(MessageInput message)
        {
            Type nodeType = null;

            foreach (var kvp in _nodeValidators)
            {
                if (kvp.Value.Contains(message.GetValidator()))
                {
                    nodeType = kvp.Key;
                    break;
                }
            }

            nodeType = nodeType ?? defaultNodeType;

            if (nodeType == null) return null;

            if (Activator.CreateInstance(nodeType) is Node node)
            {
                await node.Invoke(message);
                if (node.isNeedProlongedIteration)
                {
                    return await GetNode(new NextMessageContext(node.NextValidator, node.NextData));
                }
                return node;
            }

            return null;
        }
    }
}

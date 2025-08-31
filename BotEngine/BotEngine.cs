using BotEngine.Domain;
using BotEngine.Nodes;

namespace BotEngine
{
    public class BotEngine
    {
        private Dictionary<Type, string[]> _nodeValidators = new Dictionary<Type, string[]>();
        protected Type defaultNodeType;

        public Node? GetNode(MessageInput message)
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
                node.Invoke(message);
                return node;
            }

            return null;
        }

        public BotEngine AddNode<T>() where T : Node, new()
        {
            var nodeType = typeof(T);
            var tempNode = new T();
            _nodeValidators[nodeType] = tempNode.GetValidators();
            return this;
        }

        public BotEngine SetDefaultNode<T>() where T : Node
        {
            defaultNodeType = typeof(T);
            return this;
        }
    }
}

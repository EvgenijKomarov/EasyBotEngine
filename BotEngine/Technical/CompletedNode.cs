using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotEngine.Technical
{
    internal class CompletedNode : INodeInvokeResult
    {
        public string Result { get; } = "Completed";
    }
}

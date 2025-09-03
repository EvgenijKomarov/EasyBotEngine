using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.TestNodes.Datas;
using static System.Net.Mime.MediaTypeNames;

namespace Test.TestNodes
{
    internal class EndNode: Node<DataBuffer>
    {
        public override string[] GetIdentificators() => ["End"];
        public override async Task<INodeResult<DataBuffer>> Invoke(DataBuffer input, CancellationToken? token)
        {
            input.Text = "Goodbye";
            return Complete(input);
        }
    }
}

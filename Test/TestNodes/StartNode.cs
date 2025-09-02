using BotEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.TestNodes.Datas;

namespace Test.TestNodes
{
    internal class StartNode: Node<DataBuffer>
    {
        public override string[] GetIdentificators() => ["Text"];
        public override async Task<INodeResult<DataBuffer>> Invoke(DataBuffer input, CancellationToken? token)
        {
            if (input.Text == "End") 
            {
                return Next("End", input);
            }
            else
            {
                input.Text = "Welcome";
                return Complete(input);
            }
        }
    }
}

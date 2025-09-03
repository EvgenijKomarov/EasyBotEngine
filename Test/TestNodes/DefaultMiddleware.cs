using BotEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.TestNodes.Datas;

namespace Test.TestNodes
{
    internal class DefaultMiddleware: Middleware<DataBuffer>
    {
        public async override Task<bool> GetCondition(DataBuffer data, CancellationToken? token = null)
        {
            return data.UserId == string.Empty;
        }

        public async override Task<INodeResult<DataBuffer>> Invoke(DataBuffer input, CancellationToken? token = null)
        {
            return MoveToNode("End", input);
        }
    }
}

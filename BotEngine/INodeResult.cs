using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public interface NodeResult<TBuffer, TOutput>
    {
        public TBuffer Object { get; }
        public TOutput Output { get; }
        public Type? NextNode { get; }
    }
}

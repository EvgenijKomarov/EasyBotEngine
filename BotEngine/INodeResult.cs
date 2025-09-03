﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public interface INodeResult<TBuffer>
    {
        public TBuffer Object { get; }
        public string NextNode { get; }
    }
}

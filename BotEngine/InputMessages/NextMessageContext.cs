using BotEngine.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace BotEngine.InputMessages
{
    internal class NextMessageContext : MessageInput
    {
        private string Identificator;
        private string[] Data;

        public NextMessageContext(string validator, string[] data) 
        {
            Identificator = validator;
            Data = data;
        }

        public override string GetIdentificator() => Identificator;
        public override string[] GetData() => Data;
    }
}

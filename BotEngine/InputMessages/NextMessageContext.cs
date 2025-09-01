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
        private string Validator;
        private string[] Data;

        public NextMessageContext(string validator, string[] data) 
        {
            Validator = validator;
            Data = data;
        }

        public override string GetValidator() => Validator;
        public override string[] GetData() => Data;
    }
}

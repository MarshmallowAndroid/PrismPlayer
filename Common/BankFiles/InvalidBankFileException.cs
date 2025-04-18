using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.BankFiles
{
    public class InvalidBankFileException : Exception
    {
        public InvalidBankFileException() { }
        public InvalidBankFileException(string message) : base(message) { }
        public InvalidBankFileException(string message, Exception inner) : base(message, inner) { }
    }
}

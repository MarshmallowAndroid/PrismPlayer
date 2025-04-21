namespace Common.BankFiles
{
    public class InvalidBankFileException : Exception
    {
        public InvalidBankFileException() { }
        public InvalidBankFileException(string message) : base(message) { }
        public InvalidBankFileException(string message, Exception inner) : base(message, inner) { }
    }
}

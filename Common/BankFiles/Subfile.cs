namespace Common.BankFiles
{
    public record Subfile(string Name, long Offset, long Length)
    {
        public override string ToString()
        {
            return Name;
        }
    }
}
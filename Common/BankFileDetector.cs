using Common.BankFiles;
using Common.BankFiles.Katana;
using Common.BankFiles.Yawaraka;

namespace Common
{
    public static class BankFileDetector
    {
        /// <summary>
        /// Quickly detects the bank file type from the first 4 bytes.
        /// </summary>
        /// <param name="bankFileStream">The bank file stream.</param>
        /// <returns>The appropriate <see cref="BankFile"/> implementation. Otherwise null.</returns>
        public static BankFile? DetectBankFile(Stream bankFileStream)
        {
            BinaryReader reader = new(bankFileStream);
            uint magic = reader.ReadUInt32();

            bankFileStream.Position = 0;

            return magic switch
            {
                // _L1G
                0x47314c5f => new YawarakaBankFile(bankFileStream),
                // IDRK
                0x4b524449 => new KatanaBankFile(bankFileStream),
                _ => null,
            };
        }
    }
}

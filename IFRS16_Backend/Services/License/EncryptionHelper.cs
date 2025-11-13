using System.Text;
using System.Security.Cryptography;


namespace IFRS16_Backend.Services.License
{
    public class EncryptionHelper: IEncryptionHelper
    {
        private readonly byte[] _key; // 32 bytes AES key

        public EncryptionHelper(string secretKey)
        {
            if (string.IsNullOrEmpty(secretKey)) throw new ArgumentNullException(nameof(secretKey));
            _key = SHA256.HashData(Encoding.UTF8.GetBytes(secretKey)); // derive 256-bit key
        }

        public string EncryptBase64(string plainBase64)
        {
            var plainBytes = Convert.FromBase64String(plainBase64);
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var cipher = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(aes.IV) + ":" + Convert.ToBase64String(cipher);
        }

        public string DecryptToBase64(string cipherComposite)
        {
            var parts = cipherComposite.Split(':', 2);
            var iv = Convert.FromBase64String(parts[0]);
            var cipher = Convert.FromBase64String(parts[1]);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            return Convert.ToBase64String(plain);
        }
    }
}

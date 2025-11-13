using System.Security.Cryptography;

namespace IFRS16_Backend.Services.License
{
    public static class KeyGenerator
    {
        public static (string PublicKeyBase64, string PrivateKeyBase64) GenerateRsaKeys(int keySize = 2048)
        {
            using var rsa = RSA.Create(keySize);
            var pub = rsa.ExportRSAPublicKey();
            var priv = rsa.ExportRSAPrivateKey();
            return (Convert.ToBase64String(pub), Convert.ToBase64String(priv));
        }
    }
}

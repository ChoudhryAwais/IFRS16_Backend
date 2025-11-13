namespace IFRS16_Backend.Services.License
{
    public interface IEncryptionHelper
    {
        string EncryptBase64(string plainBase64);
        string DecryptToBase64(string cipherComposite);
    }
}

using System.Security.Cryptography;
using System.Text;
using Modules.Crypto.Shared.Interfaces;

namespace Modules.Crypto.Application.Services;

internal sealed class CryptoService : ICryptoService
{
    private readonly byte[] _key;

    public CryptoService()
    {
        var keySource = Environment.GetEnvironmentVariable("CRYPTO_KEY") ?? 
                        throw new InvalidOperationException("CRYPTO_KEY Env is missing");
        _key = ParseKey(keySource);

        if (_key.Length is not (16 or 24 or 32))
            throw new InvalidOperationException("Invalid key length. Use 16/24/32 bytes (AES-128/192/256).");
    }

    private static byte[] ParseKey(string s)
    {
        if (Convert.TryFromBase64String(s, new Span<byte>(new byte[s.Length]), out var _))
            return Convert.FromBase64String(s);

        if (s.Length % 2 != 0) 
            return Encoding.UTF8.GetBytes(s);
        
        try
        {
            return Enumerable.Range(0, s.Length / 2)
                .Select(i => Convert.ToByte(s.Substring(i * 2, 2), 16))
                .ToArray();
        }
        catch { /* ignore */ }

        return Encoding.UTF8.GetBytes(s);
    }

    public string? Encrypt(string? plain)
    {
        if (string.IsNullOrWhiteSpace(plain))
            return null;
        
        using var aes = CreateAes();
        aes.GenerateIV();

        var plainBytes = Encoding.UTF8.GetBytes(plain);
        var cipherBytes = aes.CreateEncryptor().TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string? Decrypt(string? cipherTextBase64)
    {
        if (string.IsNullOrWhiteSpace(cipherTextBase64))
            return null;
        
        var allBytes = Convert.FromBase64String(cipherTextBase64);

        using var aes = CreateAes();
        var ivLength = aes.BlockSize / 8;
        var iv = new byte[ivLength];
        var cipherBytes = new byte[allBytes.Length - ivLength];

        Buffer.BlockCopy(allBytes, 0, iv, 0, ivLength);
        Buffer.BlockCopy(allBytes, ivLength, cipherBytes, 0, cipherBytes.Length);

        aes.IV = iv;

        var plainBytes = aes.CreateDecryptor().TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }

    private Aes CreateAes()
    {
        var aes = Aes.Create();
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        return aes;
    }
}

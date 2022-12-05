using System.Security.Cryptography;
using StrikeArmy.Database.Model;

namespace StrikeArmy.Services;

public static class BoltCard
{
    public static uint CheckBoltCard(BoltCardConfig config, string p, string c)
    {
        if (string.IsNullOrEmpty(p) || string.IsNullOrEmpty(c))
        {
            throw new InvalidOperationException("Invalid bolt card request");
        }

        var pBytes = Convert.FromHexString(p);
        if (pBytes.Length != 16)
        {
            throw new ArgumentException("p must be 16 bytes");
        }

        var cBytes = Convert.FromHexString(c);
        if (cBytes.Length != 8)
        {
            throw new ArgumentException("c must be 8 bytes");
        }

        var aes = Aes.Create();
        aes.Key = config.K1.ToByteArray();

        var pDecrypted = aes.DecryptCbc(pBytes, new byte[16], PaddingMode.None);

        if (!CheckCmac(pDecrypted, config.K2.ToByteArray(), cBytes))
        {
            throw new InvalidOperationException("CMAC verify failed");
        }

        var ctr = (uint)(pDecrypted[10] << 16) | (uint)(pDecrypted[9] << 8) | pDecrypted[8];
        if (config.Counter >= ctr)
        {
            throw new InvalidOperationException("Counter check failed");
        }

        return ctr;
    }

    public static bool CheckCmac(byte[] payload, byte[] readKey, byte[] c)
    {
        // PICCDataTag
        // | UID LEN | RFU | Ctr | UID |
        // | 1 1 0 0 | 0 1 | 1   | 1   |
        if (payload[0] != 0xc7)
        {
            // PICCDataTag does not match expected, maybe card is formatted incorrectly or with different settings
            throw new ArgumentException("decrypted data does not start with 0xC7");
        }

        var svc = new byte[16];
        // MACing payload 
        Array.Copy(new byte[] {0x3c, 0xc3, 0x00, 0x01, 0x00, 0x80}, svc, 6);
        // UID + CTR
        Array.Copy(payload, 1, svc, 6, 10);

        var ks = AesCmac(readKey, svc);
        var cm = AesCmac(ks, Array.Empty<byte>());
        var truncC = new[]
        {
            cm[1], cm[3], cm[5], cm[7], cm[9], cm[11], cm[13], cm[15]
        };

        return truncC.SequenceEqual(c);
    }

    public static byte[] AesCmac(byte[] key, byte[] payload)
    {
        const int BlockSize = 16;
        var last = new byte[BlockSize];
        var x = new byte[BlockSize];
        var y = new byte[BlockSize];
        var (k1, k2) = CreateSubKey(key);
        if (payload.Length % BlockSize == 0 && payload.Length > 0)
        {
            var offset = payload.Length - BlockSize;
            for (var j = 0; j < BlockSize; j++)
            {
                last[j] = (byte)(payload[offset + j] ^ k1[j]);
            }
        }
        else
        {
            var r = payload.Length % BlockSize;
            var l = payload.Length - r;
            Array.Copy(payload, l, last, 0, r);
            last[r] = 0x80;
            for (var j = 0; j < BlockSize; j++)
            {
                last[j] ^= k2[j];
            }
        }


        using var aes = Aes.Create();
        aes.BlockSize = BlockSize * 8;
        aes.Key = key;
        aes.IV = new byte[BlockSize];
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.Zeros;

        var enc = aes.CreateEncryptor();
        for (var z = 0; z < Math.Floor(payload.Length / (double)BlockSize); z++)
        {
            var offset = BlockSize * z;
            for (var f = 0; f < BlockSize; f++)
            {
                y[f] = (byte)(payload[offset + f] ^ x[f]);
            }

            enc.TransformBlock(y, 0, BlockSize, x, 0);
        }

        for (var f = 0; f < BlockSize; f++)
        {
            y[f] = (byte)(last[f] ^ x[f]);
        }

        var t = enc.TransformFinalBlock(y, 0, BlockSize);
        return t[..BlockSize];
    }

    public static (byte[] K1, byte[] K2) CreateSubKey(byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;

        var l = aes.EncryptCbc(new byte[16], new byte[16], PaddingMode.None);

        var k1 = Rol(l);
        if ((l[0] & 0x80) == 0x80)
        {
            k1[15] ^= 0x87;
        }

        var k2 = Rol(k1);
        if ((k1[0] & 0x80) == 0x80)
        {
            k2[15] ^= 0x87;
        }

        return (k1, k2);
    }

    private static byte[] Rol(byte[] b)
    {
        var r = new byte[b.Length];
        var carry = (byte)0x00;

        for (var i = b.Length - 1; i >= 0; i--)
        {
            var u = (ushort)(b[i] << 1);
            r[i] = (byte)((u & 0xff) + carry);
            carry = (byte)((u & 0xff00) >> 8);
        }

        return r;
    }
}

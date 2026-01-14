using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PorownywarkaSzyfrow
{
    // MUSI być public, żeby Form1 go widział
    public enum AlgoId : byte
    {
        Vigenere = 1,
        DES = 2,
        RC2 = 3
    }

    // Cały "silnik" z twojego programu konsolowego
    public static class CryptoCore
    {
        // Prosty format pliku: MAGIC(4) + VER(1) + ALG(1) + SALTLEN(1) + IVLEN(1) + SALT + IV + DATA...
        private static readonly byte[] Magic = Encoding.ASCII.GetBytes("TSZ1");
        private const byte Version = 1;

        // === PUBLICZNE METODY, których użyje Form1 ===

        public static void EncryptFile(string inputPath, string outputPath, string password, AlgoId algo)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Plik wejściowy nie istnieje.", inputPath);

            using var inFs = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var outFs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);

            if (algo == AlgoId.DES)
            {
                byte[] iv = RandomBytes(8);

                byte[] salt;
                byte[] preDerivedKey;
                (salt, preDerivedKey) = DeriveSafeDesKeyByResalting(password, saltLen: 16, iterations: 100_000);

                WriteHeader(outFs, algo, salt, iv);
                EncryptWithSymmetricKey(inFs, outFs, CreateDes, preDerivedKey, iv);
                return;
            }

            if (algo == AlgoId.RC2)
            {
                byte[] iv = RandomBytes(8);
                byte[] salt = RandomBytes(16);

                WriteHeader(outFs, algo, salt, iv);
                EncryptWithSymmetricPassword(inFs, outFs, password, salt, iv, CreateRc2, 16);
                return;
            }

            // Vigenère
            WriteHeader(outFs, AlgoId.Vigenere, Array.Empty<byte>(), Array.Empty<byte>());
            EncryptVigenereStream(inFs, outFs, password);
        }

        // ZWRACA algorytm z nagłówka, żeby GUI mogło go pokazać w logu
        public static AlgoId DecryptFile(string inputPath, string outputPath, string password)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Plik wejściowy nie istnieje.", inputPath);

            using var inFs = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var outFs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);

            var header = ReadHeader(inFs);
            var algo = header.Algo;

            switch (algo)
            {
                case AlgoId.Vigenere:
                    DecryptVigenereStream(inFs, outFs, password);
                    break;

                case AlgoId.DES:
                    var desKey = DeriveKey(password, header.Salt, iterations: 100_000, keyBytes: 8);
                    RejectWeakDesKey(desKey);
                    DecryptWithSymmetricKey(inFs, outFs, createAlgo: CreateDes, key: desKey, iv: header.IV);
                    break;

                case AlgoId.RC2:
                    DecryptWithSymmetricPassword(inFs, outFs, password, header.Salt, header.IV, createAlgo: CreateRc2, keyBytes: 16);
                    break;

                default:
                    throw new InvalidOperationException("Nieobsługiwany algorytm w nagłówku.");
            }

            return algo;
        }

        // === PONIŻEJ: reszta prywatnych metod z twojego Program.cs ===
        // Vigenere, DES/RC2, nagłówek, KDF itp.

        private static void EncryptVigenereStream(Stream input, Stream output, string password)
            => VigenereTransformStream(input, output, password, encrypt: true);

        private static void DecryptVigenereStream(Stream input, Stream output, string password)
            => VigenereTransformStream(input, output, password, encrypt: false);

        private static void VigenereTransformStream(Stream input, Stream output, string password, bool encrypt)
        {
            byte[] key = Encoding.UTF8.GetBytes(password);
            if (key.Length == 0)
                throw new InvalidOperationException("Hasło nie może być puste.");

            byte[] buffer = new byte[1024 * 1024];
            long i = 0;

            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int j = 0; j < read; j++)
                {
                    byte k = key[(int)(i % key.Length)];
                    buffer[j] = encrypt
                        ? (byte)((buffer[j] + k) % 256)
                        : (byte)((256 + buffer[j] - k) % 256);
                    i++;
                }
                output.Write(buffer, 0, read);
            }
        }

        private static SymmetricAlgorithm CreateDes()
        {
            var des = DES.Create();
            des.Mode = CipherMode.CBC;
            des.Padding = PaddingMode.PKCS7;
            des.KeySize = 64;
            return des;
        }

        private static SymmetricAlgorithm CreateRc2()
        {
            var rc2 = RC2.Create();
            rc2.Mode = CipherMode.CBC;
            rc2.Padding = PaddingMode.PKCS7;
            rc2.KeySize = 128;
            rc2.EffectiveKeySize = 128;
            return rc2;
        }

        private static void EncryptWithSymmetricPassword(
            Stream input,
            Stream output,
            string password,
            byte[] salt,
            byte[] iv,
            Func<SymmetricAlgorithm> createAlgo,
            int keyBytes)
        {
            using var algo = createAlgo();
            algo.Key = DeriveKey(password, salt, iterations: 100_000, keyBytes: keyBytes);
            algo.IV = iv;

            using var crypto = new CryptoStream(output, algo.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true);
            CopyStream(input, crypto);
            crypto.FlushFinalBlock();
        }

        private static void DecryptWithSymmetricPassword(
            Stream input,
            Stream output,
            string password,
            byte[] salt,
            byte[] iv,
            Func<SymmetricAlgorithm> createAlgo,
            int keyBytes)
        {
            using var algo = createAlgo();
            algo.Key = DeriveKey(password, salt, iterations: 100_000, keyBytes: keyBytes);
            algo.IV = iv;

            using var crypto = new CryptoStream(input, algo.CreateDecryptor(), CryptoStreamMode.Read, leaveOpen: true);
            CopyStream(crypto, output);
        }

        private static void EncryptWithSymmetricKey(
            Stream input,
            Stream output,
            Func<SymmetricAlgorithm> createAlgo,
            byte[] key,
            byte[] iv)
        {
            using var algo = createAlgo();
            algo.Key = key;
            algo.IV = iv;

            using var crypto = new CryptoStream(output, algo.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true);
            CopyStream(input, crypto);
            crypto.FlushFinalBlock();
        }

        private static void DecryptWithSymmetricKey(
            Stream input,
            Stream output,
            Func<SymmetricAlgorithm> createAlgo,
            byte[] key,
            byte[] iv)
        {
            using var algo = createAlgo();
            algo.Key = key;
            algo.IV = iv;

            using var crypto = new CryptoStream(input, algo.CreateDecryptor(), CryptoStreamMode.Read, leaveOpen: true);
            CopyStream(crypto, output);
        }

        private static void CopyStream(Stream from, Stream to)
        {
            byte[] buffer = new byte[1024 * 1024];
            int read;
            while ((read = from.Read(buffer, 0, buffer.Length)) > 0)
                to.Write(buffer, 0, read);
        }

        private static (byte[] Salt, byte[] Key) DeriveSafeDesKeyByResalting(string password, int saltLen, int iterations)
        {
            for (int attempt = 0; attempt < 10_000; attempt++)
            {
                byte[] salt = RandomBytes(saltLen);
                byte[] key = DeriveKey(password, salt, iterations, keyBytes: 8);

                if (!DES.IsWeakKey(key) && !DES.IsSemiWeakKey(key))
                    return (salt, key);
            }

            throw new InvalidOperationException("Nie udało się wygenerować bezpiecznego (nie-weak) klucza DES. Zmień hasło.");
        }

        private static void RejectWeakDesKey(byte[] key)
        {
            if (DES.IsWeakKey(key) || DES.IsSemiWeakKey(key))
            {
                throw new InvalidOperationException(
                    "Wyprowadzony klucz DES jest weak/semi-weak. " +
                    "To może oznaczać niepoprawne hasło lub plik zaszyfrowany w sposób nieakceptowany (weak key).");
            }
        }

        private static byte[] DeriveKey(string password, byte[] salt, int iterations, int keyBytes)
        {
            using var kdf = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            return kdf.GetBytes(keyBytes);
        }

        private static void WriteHeader(Stream output, AlgoId algo, byte[] salt, byte[] iv)
        {
            if (salt.Length > 255 || iv.Length > 255)
                throw new InvalidOperationException("Za długie SALT/IV.");

            output.Write(Magic, 0, Magic.Length);
            output.WriteByte(Version);
            output.WriteByte((byte)algo);
            output.WriteByte((byte)salt.Length);
            output.WriteByte((byte)iv.Length);

            if (salt.Length > 0) output.Write(salt, 0, salt.Length);
            if (iv.Length > 0) output.Write(iv, 0, iv.Length);
        }

        private readonly struct Header
        {
            public Header(AlgoId algo, byte[] salt, byte[] iv)
            {
                Algo = algo;
                Salt = salt;
                IV = iv;
            }
            public AlgoId Algo { get; }
            public byte[] Salt { get; }
            public byte[] IV { get; }
        }

        private static Header ReadHeader(Stream input)
        {
            Span<byte> magic = stackalloc byte[4];
            if (input.Read(magic) != 4 || !magic.SequenceEqual(Magic))
                throw new InvalidOperationException("Nieprawidłowy format pliku (MAGIC).");

            int ver = input.ReadByte();
            if (ver != Version)
                throw new InvalidOperationException($"Nieobsługiwana wersja formatu: {ver}.");

            int algoByte = input.ReadByte();
            if (algoByte < 0)
                throw new InvalidOperationException("Nie można odczytać algorytmu.");

            int saltLen = input.ReadByte();
            int ivLen = input.ReadByte();
            if (saltLen < 0 || ivLen < 0)
                throw new InvalidOperationException("Nie można odczytać długości SALT/IV.");

            byte[] salt = new byte[saltLen];
            byte[] iv = new byte[ivLen];

            ReadExactly(input, salt);
            ReadExactly(input, iv);

            return new Header((AlgoId)algoByte, salt, iv);
        }

        private static void ReadExactly(Stream input, byte[] buffer)
        {
            int offset = 0;
            while (offset < buffer.Length)
            {
                int read = input.Read(buffer, offset, buffer.Length - offset);
                if (read <= 0) throw new EndOfStreamException("Nieoczekiwany koniec pliku.");
                offset += read;
            }
        }

        private static byte[] RandomBytes(int length)
        {
            byte[] b = new byte[length];
            RandomNumberGenerator.Fill(b);
            return b;
        }
    }
}

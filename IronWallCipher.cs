using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public class IronWallCipher
{
    // --- HELPER BIT OPERATIONS ---

    // Rotates bits to the left (For Diffusion effect)
    private static byte RotateLeft(byte value, int count)
    {
        return (byte)((value << count) | (value >> (8 - count)));
    }

    // Rotates bits to the right (For Decryption)
    private static byte RotateRight(byte value, int count)
    {
        return (byte)((value >> count) | (value << (8 - count)));
    }

    // Derives a strong key from Password and Salt
    private static byte[] DeriveKey(string password, byte[] salt)
    {
        using (var sha256 = SHA256.Create())
        {
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var combined = new byte[passwordBytes.Length + salt.Length];
            Buffer.BlockCopy(passwordBytes, 0, combined, 0, passwordBytes.Length);
            Buffer.BlockCopy(salt, 0, combined, passwordBytes.Length, salt.Length);
            return sha256.ComputeHash(combined);
        }
    }

    // --- ENCRYPTION ---
    public static byte[] Encrypt(byte[] data, string password)
    {
        // 1. Generate a unique random 'Salt' for each file
        byte[] salt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // 2. Derive the key
        byte[] key = DeriveKey(password, salt);
        List<byte> result = new List<byte>();

        // Prepend the Salt to the file (Needed for decryption)
        result.AddRange(salt);

        byte currentKeyByte = key[0];

        for (int i = 0; i < data.Length; i++)
        {
            byte b = data[i];

            // LAYER 1: XOR (Confusion)
            b = (byte)(b ^ currentKeyByte);

            // LAYER 2: Bit Rotation (Avalanche Effect)
            b = RotateLeft(b, 3);

            // LAYER 3: Modular Addition (To break linearity)
            b = (byte)(b + key[i % key.Length]);

            // LAYER 4: Chaining (Cipher Block Chaining - CBC style)
            // The previous encrypted byte affects the current one.
            // Even a 1-bit change at the start changes the entire file.
            if (i > 0)
            {
                // Get the last added element from Result list (Account for Salt length)
                byte previousEncryptedByte = result[result.Count - 1];
                b = (byte)(b ^ previousEncryptedByte);
            }

            result.Add(b);

            // Update the key at every step (Rolling Key)
            currentKeyByte = (byte)((currentKeyByte + 11) % 256);
        }

        return result.ToArray();
    }

    // --- DECRYPTION ---
    public static byte[] Decrypt(byte[] encryptedData, string password)
    {
        if (encryptedData.Length < 16) throw new Exception("Invalid or corrupted file format.");

        // 1. Extract the Salt from the beginning
        byte[] salt = encryptedData.Take(16).ToArray();
        byte[] actualData = encryptedData.Skip(16).ToArray();

        // 2. Re-derive the key
        byte[] key = DeriveKey(password, salt);

        byte[] decrypted = new byte[actualData.Length];
        byte currentKeyByte = key[0];

        for (int i = 0; i < actualData.Length; i++)
        {
            byte b = actualData[i];

            // We need the previous encrypted data to reverse the chaining
            byte xorMask = 0;
            if (i > 0)
            {
                xorMask = actualData[i - 1];
            }

            // PERFORM OPERATIONS IN REVERSE ORDER (Reverse Engineering)

            // 4. Reverse Chaining
            if (i > 0)
            {
                b = (byte)(b ^ xorMask);
            }

            // 3. Modular Subtraction
            b = (byte)(b - key[i % key.Length]);

            // 2. Right Bit Rotation
            b = RotateRight(b, 3);

            // 1. Reverse XOR
            b = (byte)(b ^ currentKeyByte);

            decrypted[i] = b;

            // Update key (Same logic as encryption)
            currentKeyByte = (byte)((currentKeyByte + 11) % 256);
        }

        return decrypted;
    }
}
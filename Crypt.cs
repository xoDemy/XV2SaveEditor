using System;
using System.Runtime.InteropServices;

namespace XV2SaveEditor
{
    public static class Crypt
    {
        public static byte[] DecryptV30(byte[] bytes)
        {
            if (bytes.Length != 1221280)
                throw new ArgumentException(
                    $"Expected encrypted save size 1,221,280 bytes, got {bytes.Length:N0}."
                );

            UIntPtr arraySize = new UIntPtr(1221280u);

            IntPtr source = SaveDecrypt(bytes, arraySize);

            if (source == IntPtr.Zero)
                throw new InvalidOperationException("SaveDecrypt returned a null pointer.");

            byte[] decrypted = new byte[1221112];

            Marshal.Copy(
                source,
                decrypted,
                0,
                decrypted.Length
            );

            return decrypted;
        }

        public static byte[] EncryptV30(byte[] bytes)
        {
            if (bytes.Length != 1221112)
                throw new ArgumentException(
                    $"Expected decrypted save size 1,221,112 bytes, got {bytes.Length:N0}."
                );

            UIntPtr arraySize = new UIntPtr(1221112u);

            IntPtr source = SaveEncrypt(bytes, arraySize);

            if (source == IntPtr.Zero)
                throw new InvalidOperationException("SaveEncrypt returned a null pointer.");

            byte[] encrypted = new byte[1221280];

            Marshal.Copy(
                source,
                encrypted,
                0,
                encrypted.Length
            );

            return encrypted;
        }

        [DllImport(
            "AesCtrLibrary.dll",
            CallingConvention = CallingConvention.Cdecl
        )]
        private static extern IntPtr SaveDecrypt(
            byte[] file,
            UIntPtr arraySize
        );

        [DllImport(
            "AesCtrLibrary.dll",
            CallingConvention = CallingConvention.Cdecl
        )]
        private static extern IntPtr SaveEncrypt(
            byte[] file,
            UIntPtr arraySize
        );
    }
}
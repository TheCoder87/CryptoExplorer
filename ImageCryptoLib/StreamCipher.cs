﻿using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CryptoExplorer.ImageCryptoLib
{
    public static class StreamCipher
    {
        private static byte[] _encryptedBytes = null;
        private static int _bytes;
        private static byte[] _iv = DeriveKey();
        public static Bitmap EncryptImage(this string filePath, string keyStreamFilePath, Operation bitwiseOperation)
        {
            try
            {
                byte[] iv=GetKeyStreamBytes(keyStreamFilePath);
                Bitmap bmp = new Bitmap(Bitmap.FromFile(filePath)); // Pick image from file path

                // Lock the bitmap's bits.  
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                System.Drawing.Imaging.BitmapData bmpData =
                    bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    bmp.PixelFormat);

                // Get the address of the first line.
                IntPtr ptr = bmpData.Scan0;

                // Declare an array to hold the bytes of the bitmap.
                int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
                byte[] contentBytes = new byte[bytes];

                // Copy the content bytes into the array (This does not copy the image header!).
                System.Runtime.InteropServices.Marshal.Copy(ptr, contentBytes, 0, bytes);

                byte[] modifiedBytes = new byte[contentBytes.Length];
                // Modify the bytes

                //for (int i = 0; i < contentBytes.Length; i++) {
                //    modifiedBytes[i] = (byte) (contentBytes[i] ^ _iv[i]);
                //}
                modifiedBytes = BitwiseOperation.Compute(contentBytes, iv, bitwiseOperation);

                _encryptedBytes = modifiedBytes; // Store the encrypted bytes in memory, for later use in decryption routine.
                _bytes = bytes;

                // Copy the modified values back to the bitmap
                System.Runtime.InteropServices.Marshal.Copy(modifiedBytes, 0, ptr, bytes);

                // Unlock the bits.
                bmp.UnlockBits(bmpData);
                return bmp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //public delegate byte[] BitwiseOperation(byte[] operand1, byte[] operand2);

        private static byte[] GetKeyStreamBytes(string keyStreamFilePath) {
            Bitmap bmp = new Bitmap(Bitmap.FromFile(keyStreamFilePath)); // Pick image from file path

            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] contentBytes = new byte[bytes];

            // Copy the content bytes into the array (This does not copy the image header!).
            System.Runtime.InteropServices.Marshal.Copy(ptr, contentBytes, 0, bytes);
            return contentBytes;
        }
        private static byte[] DeriveKey()
        {
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[3200000];
                rng.GetBytes(tokenData);
                return tokenData;
            }
        }
    }
}

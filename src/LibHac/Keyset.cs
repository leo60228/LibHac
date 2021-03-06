﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using LibHac.Fs;
using LibHac.FsService;
using LibHac.Spl;
using Aes = LibHac.Crypto.Aes;

namespace LibHac
{
    public class Keyset
    {
        /// <summary>
        /// The number of keyblobs that were used for &lt; 6.2.0 crypto
        /// </summary>
        private const int UsedKeyblobCount = 6;

        private const int SdCardKeyIdCount = 3;

        public byte[][] KeyblobKeys { get; } = Utilities.CreateJaggedByteArray(0x20, 0x10);
        public byte[][] KeyblobMacKeys { get; } = Utilities.CreateJaggedByteArray(0x20, 0x10);
        public byte[][] EncryptedKeyblobs { get; } = Utilities.CreateJaggedByteArray(0x20, 0xB0);
        public byte[][] Keyblobs { get; } = Utilities.CreateJaggedByteArray(0x20, 0x90);
        public byte[][] KeyblobKeySources { get; } = Utilities.CreateJaggedByteArray(0x20, 0x10);
        public byte[] KeyblobMacKeySource { get; } = new byte[0x10];
        public byte[][] TsecRootKeys { get; } = Utilities.CreateJaggedByteArray(0x20, 0x10);
        public byte[][] MasterKekSources { get; } = Utilities.CreateJaggedByteArray(0x20, 0x10);
        public byte[][] MasterKeks { get; } = Utilities.CreateJaggedByteArray(0x20, 0x10);
        public byte[] MasterKeySource { get; } = new byte[0x10];
        public byte[][] MasterKeys { get; } = Utilities.CreateJaggedByteArray(0x20, 0x10);
        public byte[][] Package1Keys { get; } = Utilities.CreateJaggedByteArray(0x20, 0x10);
        public byte[][] Package2Keys { get; } = Utilities.CreateJaggedByteArray(0x20, 0x10);
        public byte[] Package2KeySource { get; } = new byte[0x10];
        public byte[] AesKekGenerationSource { get; } = new byte[0x10];
        public byte[] AesKeyGenerationSource { get; } = new byte[0x10];
        public byte[] KeyAreaKeyApplicationSource { get; } = new byte[0x10];
        public byte[] KeyAreaKeyOceanSource { get; } = new byte[0x10];
        public byte[] KeyAreaKeySystemSource { get; } = new byte[0x10];
        public byte[] SaveMacKekSource { get; } = new byte[0x10];
        public byte[] SaveMacSdCardKekSource { get; } = new byte[0x10];
        public byte[] SaveMacKeySource { get; } = new byte[0x10];
        public byte[] SaveMacSdCardKeySource { get; } = new byte[0x10];
        public byte[] TitleKekSource { get; } = new byte[0x10];
        public byte[] HeaderKekSource { get; } = new byte[0x10];
        public byte[] SdCardKekSource { get; } = new byte[0x10];
        public byte[][] SdCardKeySources { get; } = Utilities.CreateJaggedByteArray(SdCardKeyIdCount, 0x20);
        public byte[] HeaderKeySource { get; } = new byte[0x20];
        public byte[] HeaderKey { get; } = new byte[0x20];
        public byte[] XciHeaderKey { get; } = new byte[0x10];
        public byte[][] TitleKeks { get; } = Utilities.CreateJaggedByteArray(0x20, 0x10);
        public byte[][][] KeyAreaKeys { get; } = Utilities.CreateJaggedByteArray(0x20, 3, 0x10);
        public byte[] EticketRsaKek { get; } = new byte[0x10];
        public byte[] RetailSpecificAesKeySource { get; } = new byte[0x10];
        public byte[] PerConsoleKeySource { get; } = new byte[0x10];
        public byte[] BisKekSource { get; } = new byte[0x10];
        public byte[][] BisKeySource { get; } = Utilities.CreateJaggedByteArray(4, 0x20);
        public byte[] SslRsaKek { get; } = new byte[0x10];

        // Device-specific keys
        public byte[] SecureBootKey { get; } = new byte[0x10];
        public byte[] TsecKey { get; } = new byte[0x10];
        public byte[] DeviceKey { get; } = new byte[0x10];
        public byte[][] BisKeys { get; } = Utilities.CreateJaggedByteArray(4, 0x20);
        public byte[] SaveMacKey { get; } = new byte[0x10];
        public byte[] SaveMacSdCardKey { get; } = new byte[0x10];
        public byte[] SdSeed { get; } = new byte[0x10];
        public byte[][] SdCardKeySourcesSpecific { get; } = Utilities.CreateJaggedByteArray(SdCardKeyIdCount, 0x20);
        public byte[][] SdCardKeys { get; } = Utilities.CreateJaggedByteArray(SdCardKeyIdCount, 0x20);

        public RSAParameters EticketExtKeyRsa { get; set; }

        public bool KeysetForDev;
        public byte[] NcaHdrFixedKeyModulus
        {
            get
            {
                if (KeysetForDev)
                {
                    return NcaHdrFixedKeyModulusDev;
                }
                else
                {
                    return NcaHdrFixedKeyModulusProd;
                }
            }
        }

        public byte[] AcidFixedKeyModulus
        {
            get
            {
                if (KeysetForDev)
                {
                    return AcidFixedKeyModulusDev;
                }
                else
                {
                    return AcidFixedKeyModulusProd;
                }
            }
        }

        public byte[] Package2FixedKeyModulus
        {
            get
            {
                if (KeysetForDev)
                {
                    return Package2FixedKeyModulusDev;
                }
                else
                {
                    return Package2FixedKeyModulusProd;
                }
            }
        }

        private static readonly byte[] NcaHdrFixedKeyModulusProd =
        {
            0xBF, 0xBE, 0x40, 0x6C, 0xF4, 0xA7, 0x80, 0xE9, 0xF0, 0x7D, 0x0C, 0x99, 0x61, 0x1D, 0x77, 0x2F,
            0x96, 0xBC, 0x4B, 0x9E, 0x58, 0x38, 0x1B, 0x03, 0xAB, 0xB1, 0x75, 0x49, 0x9F, 0x2B, 0x4D, 0x58,
            0x34, 0xB0, 0x05, 0xA3, 0x75, 0x22, 0xBE, 0x1A, 0x3F, 0x03, 0x73, 0xAC, 0x70, 0x68, 0xD1, 0x16,
            0xB9, 0x04, 0x46, 0x5E, 0xB7, 0x07, 0x91, 0x2F, 0x07, 0x8B, 0x26, 0xDE, 0xF6, 0x00, 0x07, 0xB2,
            0xB4, 0x51, 0xF8, 0x0D, 0x0A, 0x5E, 0x58, 0xAD, 0xEB, 0xBC, 0x9A, 0xD6, 0x49, 0xB9, 0x64, 0xEF,
            0xA7, 0x82, 0xB5, 0xCF, 0x6D, 0x70, 0x13, 0xB0, 0x0F, 0x85, 0xF6, 0xA9, 0x08, 0xAA, 0x4D, 0x67,
            0x66, 0x87, 0xFA, 0x89, 0xFF, 0x75, 0x90, 0x18, 0x1E, 0x6B, 0x3D, 0xE9, 0x8A, 0x68, 0xC9, 0x26,
            0x04, 0xD9, 0x80, 0xCE, 0x3F, 0x5E, 0x92, 0xCE, 0x01, 0xFF, 0x06, 0x3B, 0xF2, 0xC1, 0xA9, 0x0C,
            0xCE, 0x02, 0x6F, 0x16, 0xBC, 0x92, 0x42, 0x0A, 0x41, 0x64, 0xCD, 0x52, 0xB6, 0x34, 0x4D, 0xAE,
            0xC0, 0x2E, 0xDE, 0xA4, 0xDF, 0x27, 0x68, 0x3C, 0xC1, 0xA0, 0x60, 0xAD, 0x43, 0xF3, 0xFC, 0x86,
            0xC1, 0x3E, 0x6C, 0x46, 0xF7, 0x7C, 0x29, 0x9F, 0xFA, 0xFD, 0xF0, 0xE3, 0xCE, 0x64, 0xE7, 0x35,
            0xF2, 0xF6, 0x56, 0x56, 0x6F, 0x6D, 0xF1, 0xE2, 0x42, 0xB0, 0x83, 0x40, 0xA5, 0xC3, 0x20, 0x2B,
            0xCC, 0x9A, 0xAE, 0xCA, 0xED, 0x4D, 0x70, 0x30, 0xA8, 0x70, 0x1C, 0x70, 0xFD, 0x13, 0x63, 0x29,
            0x02, 0x79, 0xEA, 0xD2, 0xA7, 0xAF, 0x35, 0x28, 0x32, 0x1C, 0x7B, 0xE6, 0x2F, 0x1A, 0xAA, 0x40,
            0x7E, 0x32, 0x8C, 0x27, 0x42, 0xFE, 0x82, 0x78, 0xEC, 0x0D, 0xEB, 0xE6, 0x83, 0x4B, 0x6D, 0x81,
            0x04, 0x40, 0x1A, 0x9E, 0x9A, 0x67, 0xF6, 0x72, 0x29, 0xFA, 0x04, 0xF0, 0x9D, 0xE4, 0xF4, 0x03
        };

        private static readonly byte[] AcidFixedKeyModulusProd =
        {
            0xDD, 0xC8, 0xDD, 0xF2, 0x4E, 0x6D, 0xF0, 0xCA, 0x9E, 0xC7, 0x5D, 0xC7, 0x7B, 0xAD, 0xFE, 0x7D,
            0x23, 0x89, 0x69, 0xB6, 0xF2, 0x06, 0xA2, 0x02, 0x88, 0xE1, 0x55, 0x91, 0xAB, 0xCB, 0x4D, 0x50,
            0x2E, 0xFC, 0x9D, 0x94, 0x76, 0xD6, 0x4C, 0xD8, 0xFF, 0x10, 0xFA, 0x5E, 0x93, 0x0A, 0xB4, 0x57,
            0xAC, 0x51, 0xC7, 0x16, 0x66, 0xF4, 0x1A, 0x54, 0xC2, 0xC5, 0x04, 0x3D, 0x1B, 0xFE, 0x30, 0x20,
            0x8A, 0xAC, 0x6F, 0x6F, 0xF5, 0xC7, 0xB6, 0x68, 0xB8, 0xC9, 0x40, 0x6B, 0x42, 0xAD, 0x11, 0x21,
            0xE7, 0x8B, 0xE9, 0x75, 0x01, 0x86, 0xE4, 0x48, 0x9B, 0x0A, 0x0A, 0xF8, 0x7F, 0xE8, 0x87, 0xF2,
            0x82, 0x01, 0xE6, 0xA3, 0x0F, 0xE4, 0x66, 0xAE, 0x83, 0x3F, 0x4E, 0x9F, 0x5E, 0x01, 0x30, 0xA4,
            0x00, 0xB9, 0x9A, 0xAE, 0x5F, 0x03, 0xCC, 0x18, 0x60, 0xE5, 0xEF, 0x3B, 0x5E, 0x15, 0x16, 0xFE,
            0x1C, 0x82, 0x78, 0xB5, 0x2F, 0x47, 0x7C, 0x06, 0x66, 0x88, 0x5D, 0x35, 0xA2, 0x67, 0x20, 0x10,
            0xE7, 0x6C, 0x43, 0x68, 0xD3, 0xE4, 0x5A, 0x68, 0x2A, 0x5A, 0xE2, 0x6D, 0x73, 0xB0, 0x31, 0x53,
            0x1C, 0x20, 0x09, 0x44, 0xF5, 0x1A, 0x9D, 0x22, 0xBE, 0x12, 0xA1, 0x77, 0x11, 0xE2, 0xA1, 0xCD,
            0x40, 0x9A, 0xA2, 0x8B, 0x60, 0x9B, 0xEF, 0xA0, 0xD3, 0x48, 0x63, 0xA2, 0xF8, 0xA3, 0x2C, 0x08,
            0x56, 0x52, 0x2E, 0x60, 0x19, 0x67, 0x5A, 0xA7, 0x9F, 0xDC, 0x3F, 0x3F, 0x69, 0x2B, 0x31, 0x6A,
            0xB7, 0x88, 0x4A, 0x14, 0x84, 0x80, 0x33, 0x3C, 0x9D, 0x44, 0xB7, 0x3F, 0x4C, 0xE1, 0x75, 0xEA,
            0x37, 0xEA, 0xE8, 0x1E, 0x7C, 0x77, 0xB7, 0xC6, 0x1A, 0xA2, 0xF0, 0x9F, 0x10, 0x61, 0xCD, 0x7B,
            0x5B, 0x32, 0x4C, 0x37, 0xEF, 0xB1, 0x71, 0x68, 0x53, 0x0A, 0xED, 0x51, 0x7D, 0x35, 0x22, 0xFD
        };

        private static readonly byte[] Package2FixedKeyModulusProd =
        {
            0x8D, 0x13, 0xA7, 0x77, 0x6A, 0xE5, 0xDC, 0xC0, 0x3B, 0x25, 0xD0, 0x58, 0xE4, 0x20, 0x69, 0x59,
            0x55, 0x4B, 0xAB, 0x70, 0x40, 0x08, 0x28, 0x07, 0xA8, 0xA7, 0xFD, 0x0F, 0x31, 0x2E, 0x11, 0xFE,
            0x47, 0xA0, 0xF9, 0x9D, 0xDF, 0x80, 0xDB, 0x86, 0x5A, 0x27, 0x89, 0xCD, 0x97, 0x6C, 0x85, 0xC5,
            0x6C, 0x39, 0x7F, 0x41, 0xF2, 0xFF, 0x24, 0x20, 0xC3, 0x95, 0xA6, 0xF7, 0x9D, 0x4A, 0x45, 0x74,
            0x8B, 0x5D, 0x28, 0x8A, 0xC6, 0x99, 0x35, 0x68, 0x85, 0xA5, 0x64, 0x32, 0x80, 0x9F, 0xD3, 0x48,
            0x39, 0xA2, 0x1D, 0x24, 0x67, 0x69, 0xDF, 0x75, 0xAC, 0x12, 0xB5, 0xBD, 0xC3, 0x29, 0x90, 0xBE,
            0x37, 0xE4, 0xA0, 0x80, 0x9A, 0xBE, 0x36, 0xBF, 0x1F, 0x2C, 0xAB, 0x2B, 0xAD, 0xF5, 0x97, 0x32,
            0x9A, 0x42, 0x9D, 0x09, 0x8B, 0x08, 0xF0, 0x63, 0x47, 0xA3, 0xE9, 0x1B, 0x36, 0xD8, 0x2D, 0x8A,
            0xD7, 0xE1, 0x54, 0x11, 0x95, 0xE4, 0x45, 0x88, 0x69, 0x8A, 0x2B, 0x35, 0xCE, 0xD0, 0xA5, 0x0B,
            0xD5, 0x5D, 0xAC, 0xDB, 0xAF, 0x11, 0x4D, 0xCA, 0xB8, 0x1E, 0xE7, 0x01, 0x9E, 0xF4, 0x46, 0xA3,
            0x8A, 0x94, 0x6D, 0x76, 0xBD, 0x8A, 0xC8, 0x3B, 0xD2, 0x31, 0x58, 0x0C, 0x79, 0xA8, 0x26, 0xE9,
            0xD1, 0x79, 0x9C, 0xCB, 0xD4, 0x2B, 0x6A, 0x4F, 0xC6, 0xCC, 0xCF, 0x90, 0xA7, 0xB9, 0x98, 0x47,
            0xFD, 0xFA, 0x4C, 0x6C, 0x6F, 0x81, 0x87, 0x3B, 0xCA, 0xB8, 0x50, 0xF6, 0x3E, 0x39, 0x5D, 0x4D,
            0x97, 0x3F, 0x0F, 0x35, 0x39, 0x53, 0xFB, 0xFA, 0xCD, 0xAB, 0xA8, 0x7A, 0x62, 0x9A, 0x3F, 0xF2,
            0x09, 0x27, 0x96, 0x3F, 0x07, 0x9A, 0x91, 0xF7, 0x16, 0xBF, 0xC6, 0x3A, 0x82, 0x5A, 0x4B, 0xCF,
            0x49, 0x50, 0x95, 0x8C, 0x55, 0x80, 0x7E, 0x39, 0xB1, 0x48, 0x05, 0x1E, 0x21, 0xC7, 0x24, 0x4F
        };

        private static readonly byte[] NcaHdrFixedKeyModulusDev =
        {
            0xD8, 0xF1, 0x18, 0xEF, 0x32, 0x72, 0x4C, 0xA7, 0x47, 0x4C, 0xB9, 0xEA, 0xB3, 0x04, 0xA8, 0xA4,
            0xAC, 0x99, 0x08, 0x08, 0x04, 0xBF, 0x68, 0x57, 0xB8, 0x43, 0x94, 0x2B, 0xC7, 0xB9, 0x66, 0x49,
            0x85, 0xE5, 0x8A, 0x9B, 0xC1, 0x00, 0x9A, 0x6A, 0x8D, 0xD0, 0xEF, 0xCE, 0xFF, 0x86, 0xC8, 0x5C,
            0x5D, 0xE9, 0x53, 0x7B, 0x19, 0x2A, 0xA8, 0xC0, 0x22, 0xD1, 0xF3, 0x22, 0x0A, 0x50, 0xF2, 0x2B,
            0x65, 0x05, 0x1B, 0x9E, 0xEC, 0x61, 0xB5, 0x63, 0xA3, 0x6F, 0x3B, 0xBA, 0x63, 0x3A, 0x53, 0xF4,
            0x49, 0x2F, 0xCF, 0x03, 0xCC, 0xD7, 0x50, 0x82, 0x1B, 0x29, 0x4F, 0x08, 0xDE, 0x1B, 0x6D, 0x47,
            0x4F, 0xA8, 0xB6, 0x6A, 0x26, 0xA0, 0x83, 0x3F, 0x1A, 0xAF, 0x83, 0x8F, 0x0E, 0x17, 0x3F, 0xFE,
            0x44, 0x1C, 0x56, 0x94, 0x2E, 0x49, 0x83, 0x83, 0x03, 0xE9, 0xB6, 0xAD, 0xD5, 0xDE, 0xE3, 0x2D,
            0xA1, 0xD9, 0x66, 0x20, 0x5D, 0x1F, 0x5E, 0x96, 0x5D, 0x5B, 0x55, 0x0D, 0xD4, 0xB4, 0x77, 0x6E,
            0xAE, 0x1B, 0x69, 0xF3, 0xA6, 0x61, 0x0E, 0x51, 0x62, 0x39, 0x28, 0x63, 0x75, 0x76, 0xBF, 0xB0,
            0xD2, 0x22, 0xEF, 0x98, 0x25, 0x02, 0x05, 0xC0, 0xD7, 0x6A, 0x06, 0x2C, 0xA5, 0xD8, 0x5A, 0x9D,
            0x7A, 0xA4, 0x21, 0x55, 0x9F, 0xF9, 0x3E, 0xBF, 0x16, 0xF6, 0x07, 0xC2, 0xB9, 0x6E, 0x87, 0x9E,
            0xB5, 0x1C, 0xBE, 0x97, 0xFA, 0x82, 0x7E, 0xED, 0x30, 0xD4, 0x66, 0x3F, 0xDE, 0xD8, 0x1B, 0x4B,
            0x15, 0xD9, 0xFB, 0x2F, 0x50, 0xF0, 0x9D, 0x1D, 0x52, 0x4C, 0x1C, 0x4D, 0x8D, 0xAE, 0x85, 0x1E,
            0xEA, 0x7F, 0x86, 0xF3, 0x0B, 0x7B, 0x87, 0x81, 0x98, 0x23, 0x80, 0x63, 0x4F, 0x2F, 0xB0, 0x62,
            0xCC, 0x6E, 0xD2, 0x46, 0x13, 0x65, 0x2B, 0xD6, 0x44, 0x33, 0x59, 0xB5, 0x8F, 0xB9, 0x4A, 0xA9
        };

        private static readonly byte[] AcidFixedKeyModulusDev =
        {
            0xD6, 0x34, 0xA5, 0x78, 0x6C, 0x68, 0xCE, 0x5A, 0xC2, 0x37, 0x17, 0xF3, 0x82, 0x45, 0xC6, 0x89,
            0xE1, 0x2D, 0x06, 0x67, 0xBF, 0xB4, 0x06, 0x19, 0x55, 0x6B, 0x27, 0x66, 0x0C, 0xA4, 0xB5, 0x87,
            0x81, 0x25, 0xF4, 0x30, 0xBC, 0x53, 0x08, 0x68, 0xA2, 0x48, 0x49, 0x8C, 0x3F, 0x38, 0x40, 0x9C,
            0xC4, 0x26, 0xF4, 0x79, 0xE2, 0xA1, 0x85, 0xF5, 0x5C, 0x7F, 0x58, 0xBA, 0xA6, 0x1C, 0xA0, 0x8B,
            0x84, 0x16, 0x14, 0x6F, 0x85, 0xD9, 0x7C, 0xE1, 0x3C, 0x67, 0x22, 0x1E, 0xFB, 0xD8, 0xA7, 0xA5,
            0x9A, 0xBF, 0xEC, 0x0E, 0xCF, 0x96, 0x7E, 0x85, 0xC2, 0x1D, 0x49, 0x5D, 0x54, 0x26, 0xCB, 0x32,
            0x7C, 0xF6, 0xBB, 0x58, 0x03, 0x80, 0x2B, 0x5D, 0xF7, 0xFB, 0xD1, 0x9D, 0xC7, 0xC6, 0x2E, 0x53,
            0xC0, 0x6F, 0x39, 0x2C, 0x1F, 0xA9, 0x92, 0xF2, 0x4D, 0x7D, 0x4E, 0x74, 0xFF, 0xE4, 0xEF, 0xE4,
            0x7C, 0x3D, 0x34, 0x2A, 0x71, 0xA4, 0x97, 0x59, 0xFF, 0x4F, 0xA2, 0xF4, 0x66, 0x78, 0xD8, 0xBA,
            0x99, 0xE3, 0xE6, 0xDB, 0x54, 0xB9, 0xE9, 0x54, 0xA1, 0x70, 0xFC, 0x05, 0x1F, 0x11, 0x67, 0x4B,
            0x26, 0x8C, 0x0C, 0x3E, 0x03, 0xD2, 0xA3, 0x55, 0x5C, 0x7D, 0xC0, 0x5D, 0x9D, 0xFF, 0x13, 0x2F,
            0xFD, 0x19, 0xBF, 0xED, 0x44, 0xC3, 0x8C, 0xA7, 0x28, 0xCB, 0xE5, 0xE0, 0xB1, 0xA7, 0x9C, 0x33,
            0x8D, 0xB8, 0x6E, 0xDE, 0x87, 0x18, 0x22, 0x60, 0xC4, 0xAE, 0xF2, 0x87, 0x9F, 0xCE, 0x09, 0x5C,
            0xB5, 0x99, 0xA5, 0x9F, 0x49, 0xF2, 0xD7, 0x58, 0xFA, 0xF9, 0xC0, 0x25, 0x7D, 0xD6, 0xCB, 0xF3,
            0xD8, 0x6C, 0xA2, 0x69, 0x91, 0x68, 0x73, 0xB1, 0x94, 0x6F, 0xA3, 0xF3, 0xB9, 0x7D, 0xF8, 0xE0,
            0x72, 0x9E, 0x93, 0x7B, 0x7A, 0xA2, 0x57, 0x60, 0xB7, 0x5B, 0xA9, 0x84, 0xAE, 0x64, 0x88, 0x69
        };

        private static readonly byte[] Package2FixedKeyModulusDev =
        {
            0xB3, 0x65, 0x54, 0xFB, 0x0A, 0xB0, 0x1E, 0x85, 0xA7, 0xF6, 0xCF, 0x91, 0x8E, 0xBA, 0x96, 0x99,
            0x0D, 0x8B, 0x91, 0x69, 0x2A, 0xEE, 0x01, 0x20, 0x4F, 0x34, 0x5C, 0x2C, 0x4F, 0x4E, 0x37, 0xC7,
            0xF1, 0x0B, 0xD4, 0xCD, 0xA1, 0x7F, 0x93, 0xF1, 0x33, 0x59, 0xCE, 0xB1, 0xE9, 0xDD, 0x26, 0xE6,
            0xF3, 0xBB, 0x77, 0x87, 0x46, 0x7A, 0xD6, 0x4E, 0x47, 0x4A, 0xD1, 0x41, 0xB7, 0x79, 0x4A, 0x38,
            0x06, 0x6E, 0xCF, 0x61, 0x8F, 0xCD, 0xC1, 0x40, 0x0B, 0xFA, 0x26, 0xDC, 0xC0, 0x34, 0x51, 0x83,
            0xD9, 0x3B, 0x11, 0x54, 0x3B, 0x96, 0x27, 0x32, 0x9A, 0x95, 0xBE, 0x1E, 0x68, 0x11, 0x50, 0xA0,
            0x6B, 0x10, 0xA8, 0x83, 0x8B, 0xF5, 0xFC, 0xBC, 0x90, 0x84, 0x7A, 0x5A, 0x5C, 0x43, 0x52, 0xE6,
            0xC8, 0x26, 0xE9, 0xFE, 0x06, 0xA0, 0x8B, 0x53, 0x0F, 0xAF, 0x1E, 0xC4, 0x1C, 0x0B, 0xCF, 0x50,
            0x1A, 0xA4, 0xF3, 0x5C, 0xFB, 0xF0, 0x97, 0xE4, 0xDE, 0x32, 0x0A, 0x9F, 0xE3, 0x5A, 0xAA, 0xB7,
            0x44, 0x7F, 0x5C, 0x33, 0x60, 0xB9, 0x0F, 0x22, 0x2D, 0x33, 0x2A, 0xE9, 0x69, 0x79, 0x31, 0x42,
            0x8F, 0xE4, 0x3A, 0x13, 0x8B, 0xE7, 0x26, 0xBD, 0x08, 0x87, 0x6C, 0xA6, 0xF2, 0x73, 0xF6, 0x8E,
            0xA7, 0xF2, 0xFE, 0xFB, 0x6C, 0x28, 0x66, 0x0D, 0xBD, 0xD7, 0xEB, 0x42, 0xA8, 0x78, 0xE6, 0xB8,
            0x6B, 0xAE, 0xC7, 0xA9, 0xE2, 0x40, 0x6E, 0x89, 0x20, 0x82, 0x25, 0x8E, 0x3C, 0x6A, 0x60, 0xD7,
            0xF3, 0x56, 0x8E, 0xEC, 0x8D, 0x51, 0x8A, 0x63, 0x3C, 0x04, 0x78, 0x23, 0x0E, 0x90, 0x0C, 0xB4,
            0xE7, 0x86, 0x3B, 0x4F, 0x8E, 0x13, 0x09, 0x47, 0x32, 0x0E, 0x04, 0xB8, 0x4D, 0x5B, 0xB0, 0x46,
            0x71, 0xB0, 0x5C, 0xF4, 0xAD, 0x63, 0x4F, 0xC5, 0xE2, 0xAC, 0x1E, 0xC4, 0x33, 0x96, 0x09, 0x7B
        };

        public ExternalKeySet ExternalKeySet { get; } = new ExternalKeySet();

        public void SetSdSeed(byte[] sdseed)
        {
            Array.Copy(sdseed, SdSeed, SdSeed.Length);
            DeriveSdCardKeys();
        }

        public void DeriveKeys(IProgressReport logger = null)
        {
            DeriveKeyblobKeys();
            DecryptKeyblobs(logger);
            ReadKeyblobs();

            Derive620MasterKeks();
            DeriveMasterKeys();

            DerivePerConsoleKeys();
            DerivePerFirmwareKeys();
            DeriveNcaHeaderKey();
            DeriveSdCardKeys();
        }

        private void DeriveKeyblobKeys()
        {
            if (SecureBootKey.IsEmpty() || TsecKey.IsEmpty()) return;

            bool haveKeyblobMacKeySource = !MasterKeySource.IsEmpty();
            var temp = new byte[0x10];

            for (int i = 0; i < UsedKeyblobCount; i++)
            {
                if (KeyblobKeySources[i].IsEmpty()) continue;

                Aes.DecryptEcb128(KeyblobKeySources[i], temp, TsecKey);
                Aes.DecryptEcb128(temp, KeyblobKeys[i], SecureBootKey);

                if (!haveKeyblobMacKeySource) continue;

                Aes.DecryptEcb128(KeyblobMacKeySource, KeyblobMacKeys[i], KeyblobKeys[i]);
            }
        }

        private void DecryptKeyblobs(IProgressReport logger = null)
        {
            var cmac = new byte[0x10];
            var expectedCmac = new byte[0x10];
            var counter = new byte[0x10];

            for (int i = 0; i < UsedKeyblobCount; i++)
            {
                if (KeyblobKeys[i].IsEmpty() || KeyblobMacKeys[i].IsEmpty() || EncryptedKeyblobs[i].IsEmpty())
                {
                    continue;
                }

                Array.Copy(EncryptedKeyblobs[i], expectedCmac, 0x10);
                CryptoOld.CalculateAesCmac(KeyblobMacKeys[i], EncryptedKeyblobs[i], 0x10, cmac, 0, 0xa0);

                if (!Utilities.ArraysEqual(cmac, expectedCmac))
                {
                    logger?.LogMessage($"Warning: Keyblob MAC {i:x2} is invalid. Are SBK/TSEC key correct?");
                }

                Array.Copy(EncryptedKeyblobs[i], 0x10, counter, 0, 0x10);

                Aes.DecryptCtr128(EncryptedKeyblobs[i].AsSpan(0x20), Keyblobs[i], KeyblobKeys[i], counter);
            }
        }

        private void ReadKeyblobs()
        {
            for (int i = 0; i < UsedKeyblobCount; i++)
            {
                if (Keyblobs[i].IsEmpty()) continue;

                Array.Copy(Keyblobs[i], 0x80, Package1Keys[i], 0, 0x10);
                Array.Copy(Keyblobs[i], MasterKeks[i], 0x10);
            }
        }

        private void Derive620MasterKeks()
        {
            for (int i = UsedKeyblobCount; i < 0x20; i++)
            {
                if (TsecRootKeys[i - UsedKeyblobCount].IsEmpty() || MasterKekSources[i].IsEmpty()) continue;

                Aes.DecryptEcb128(MasterKekSources[i], MasterKeks[i], TsecRootKeys[i - UsedKeyblobCount]);
            }
        }

        private void DeriveMasterKeys()
        {
            if (MasterKeySource.IsEmpty()) return;

            for (int i = 0; i < 0x20; i++)
            {
                if (MasterKeks[i].IsEmpty()) continue;

                Aes.DecryptEcb128(MasterKeySource, MasterKeys[i], MasterKeks[i]);
            }
        }

        private void DerivePerConsoleKeys()
        {
            var kek = new byte[0x10];

            // Derive the device key
            if (!PerConsoleKeySource.IsEmpty() && !KeyblobKeys[0].IsEmpty())
            {
                Aes.DecryptEcb128(PerConsoleKeySource, DeviceKey, KeyblobKeys[0]);
            }

            // Derive save key
            if (!SaveMacKekSource.IsEmpty() && !SaveMacKeySource.IsEmpty() && !DeviceKey.IsEmpty())
            {
                GenerateKek(DeviceKey, SaveMacKekSource, kek, AesKekGenerationSource, null);
                Aes.DecryptEcb128(SaveMacKeySource, SaveMacKey, kek);
            }

            // Derive BIS keys
            if (DeviceKey.IsEmpty()
                || BisKekSource.IsEmpty()
                || AesKekGenerationSource.IsEmpty()
                || AesKeyGenerationSource.IsEmpty()
                || RetailSpecificAesKeySource.IsEmpty())
            {
                return;
            }

            // If the user doesn't provide bis_key_source_03 we can assume it's the same as bis_key_source_02
            if (BisKeySource[3].IsEmpty() && !BisKeySource[2].IsEmpty())
            {
                Array.Copy(BisKeySource[2], BisKeySource[3], 0x20);
            }

            Aes.DecryptEcb128(RetailSpecificAesKeySource, kek, DeviceKey);
            if (!BisKeySource[0].IsEmpty()) Aes.DecryptEcb128(BisKeySource[0], BisKeys[0], kek);

            GenerateKek(DeviceKey, BisKekSource, kek, AesKekGenerationSource, AesKeyGenerationSource);

            for (int i = 1; i < 4; i++)
            {
                if (!BisKeySource[i].IsEmpty()) Aes.DecryptEcb128(BisKeySource[i], BisKeys[i], kek);
            }
        }

        private void DerivePerFirmwareKeys()
        {
            bool haveKakSource0 = !KeyAreaKeyApplicationSource.IsEmpty();
            bool haveKakSource1 = !KeyAreaKeyOceanSource.IsEmpty();
            bool haveKakSource2 = !KeyAreaKeySystemSource.IsEmpty();
            bool haveTitleKekSource = !TitleKekSource.IsEmpty();
            bool havePackage2KeySource = !Package2KeySource.IsEmpty();

            for (int i = 0; i < 0x20; i++)
            {
                if (MasterKeys[i].IsEmpty())
                {
                    continue;
                }

                if (haveKakSource0)
                {
                    GenerateKek(MasterKeys[i], KeyAreaKeyApplicationSource, KeyAreaKeys[i][0],
                        AesKekGenerationSource, AesKeyGenerationSource);
                }

                if (haveKakSource1)
                {
                    GenerateKek(MasterKeys[i], KeyAreaKeyOceanSource, KeyAreaKeys[i][1],
                        AesKekGenerationSource, AesKeyGenerationSource);
                }

                if (haveKakSource2)
                {
                    GenerateKek(MasterKeys[i], KeyAreaKeySystemSource, KeyAreaKeys[i][2],
                        AesKekGenerationSource, AesKeyGenerationSource);
                }

                if (haveTitleKekSource)
                {
                    Aes.DecryptEcb128(TitleKekSource, TitleKeks[i], MasterKeys[i]);
                }

                if (havePackage2KeySource)
                {
                    Aes.DecryptEcb128(Package2KeySource, Package2Keys[i], MasterKeys[i]);
                }
            }
        }

        private void DeriveNcaHeaderKey()
        {
            if (HeaderKekSource.IsEmpty() || HeaderKeySource.IsEmpty() || MasterKeys[0].IsEmpty()) return;

            var headerKek = new byte[0x10];

            GenerateKek(MasterKeys[0], HeaderKekSource, headerKek, AesKekGenerationSource,
                AesKeyGenerationSource);
            Aes.DecryptEcb128(HeaderKeySource, HeaderKey, headerKek);
        }

        public void DeriveSdCardKeys()
        {
            var sdKek = new byte[0x10];
            GenerateKek(MasterKeys[0], SdCardKekSource, sdKek, AesKekGenerationSource, AesKeyGenerationSource);

            for (int k = 0; k < SdCardKeyIdCount; k++)
            {
                for (int i = 0; i < 0x20; i++)
                {
                    SdCardKeySourcesSpecific[k][i] = (byte)(SdCardKeySources[k][i] ^ SdSeed[i & 0xF]);
                }
            }

            for (int k = 0; k < SdCardKeyIdCount; k++)
            {
                Aes.DecryptEcb128(SdCardKeySourcesSpecific[k], SdCardKeys[k], sdKek);
            }

            // Derive sd card save key
            if (!SaveMacSdCardKekSource.IsEmpty() && !SaveMacSdCardKeySource.IsEmpty())
            {
                var keySource = new byte[0x10];

                for (int i = 0; i < 0x10; i++)
                {
                    keySource[i] = (byte)(SaveMacSdCardKeySource[i] ^ SdSeed[i]);
                }

                GenerateKek(MasterKeys[0], SaveMacSdCardKekSource, sdKek, AesKekGenerationSource, null);
                Aes.DecryptEcb128(keySource, SaveMacSdCardKey, sdKek);
            }
        }

        internal static readonly string[] KakNames = { "application", "ocean", "system" };

        public static int GetMasterKeyRevisionFromKeyGeneration(int keyGeneration)
        {
            if (keyGeneration == 0) return 0;

            return keyGeneration - 1;
        }

        private static void GenerateKek(ReadOnlySpan<byte> key, ReadOnlySpan<byte> src, Span<byte> dest, ReadOnlySpan<byte> kekSeed, ReadOnlySpan<byte> keySeed)
        {
            Span<byte> kek = stackalloc byte[0x10];
            Span<byte> srcKek = stackalloc byte[0x10];

            Aes.DecryptEcb128(kekSeed, kek, key);
            Aes.DecryptEcb128(src, srcKek, kek);

            if (!keySeed.IsEmpty)
            {
                Aes.DecryptEcb128(keySeed, dest, srcKek);
            }
            else
            {
                srcKek.CopyTo(dest);
            }
        }
    }

    public static class ExternalKeyReader
    {
        private const int TitleKeySize = 0x10;

        public static void ReadKeyFile(Keyset keyset, string filename, string titleKeysFilename = null, string consoleKeysFilename = null, IProgressReport logger = null)
        {
            Dictionary<string, KeyValue> keyDictionary = CreateFullKeyDictionary();

            if (filename != null) ReadMainKeys(keyset, filename, keyDictionary, logger);
            if (consoleKeysFilename != null) ReadMainKeys(keyset, consoleKeysFilename, keyDictionary, logger);
            if (titleKeysFilename != null) ReadTitleKeys(keyset, titleKeysFilename, logger);

            keyset.ExternalKeySet.TrimExcess();
            keyset.DeriveKeys(logger);
        }

        public static Keyset ReadKeyFile(string filename, string titleKeysFilename = null, string consoleKeysFilename = null, IProgressReport logger = null, bool dev = false)
        {
            var keyset = new Keyset();
            keyset.KeysetForDev = dev;
            ReadKeyFile(keyset, filename, titleKeysFilename, consoleKeysFilename, logger);

            return keyset;
        }

        public static void LoadConsoleKeys(this Keyset keyset, string filename, IProgressReport logger = null)
        {
            Dictionary<string, KeyValue> uniqueKeyDictionary = CreateUniqueKeyDictionary();

            foreach (KeyValue key in uniqueKeyDictionary.Values)
            {
                byte[] keyBytes = key.GetKey(keyset);
                Array.Clear(keyBytes, 0, keyBytes.Length);
            }

            ReadMainKeys(keyset, filename, uniqueKeyDictionary, logger);
            keyset.DeriveKeys();
        }

        private static void ReadMainKeys(Keyset keyset, string filename, Dictionary<string, KeyValue> keyDict, IProgressReport logger = null)
        {
            if (filename == null) return;

            using (var reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] a = line.Split(',', '=');
                    if (a.Length != 2) continue;

                    string key = a[0].Trim();
                    string valueStr = a[1].Trim();

                    if (!keyDict.TryGetValue(key, out KeyValue kv))
                    {
                        logger?.LogMessage($"Failed to match key {key}");
                        continue;
                    }

                    byte[] value = valueStr.ToBytes();
                    if (value.Length != kv.Size)
                    {
                        logger?.LogMessage($"Key {key} had incorrect size {value.Length}. (Expected {kv.Size})");
                        continue;
                    }

                    byte[] dest = kv.GetKey(keyset);
                    Array.Copy(value, dest, value.Length);
                }
            }
        }

        private static void ReadTitleKeys(Keyset keyset, string filename, IProgressReport progress = null)
        {
            if (filename == null) return;

            using (var reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] splitLine;

                    // Some people use pipes as delimiters
                    if (line.Contains('|'))
                    {
                        splitLine = line.Split('|');
                    }
                    else
                    {
                        splitLine = line.Split(',', '=');
                    }

                    if (splitLine.Length < 2) continue;

                    if (!splitLine[0].Trim().TryToBytes(out byte[] rightsId))
                    {
                        progress?.LogMessage($"Invalid rights ID \"{splitLine[0].Trim()}\" in title key file");
                        continue;
                    }

                    if (!splitLine[1].Trim().TryToBytes(out byte[] titleKey))
                    {
                        progress?.LogMessage($"Invalid title key \"{splitLine[1].Trim()}\" in title key file");
                        continue;
                    }

                    if (rightsId.Length != TitleKeySize)
                    {
                        progress?.LogMessage($"Rights ID {rightsId.ToHexString()} had incorrect size {rightsId.Length}. (Expected {TitleKeySize})");
                        continue;
                    }

                    if (titleKey.Length != TitleKeySize)
                    {
                        progress?.LogMessage($"Title key {titleKey.ToHexString()} had incorrect size {titleKey.Length}. (Expected {TitleKeySize})");
                        continue;
                    }

                    keyset.ExternalKeySet.Add(new RightsId(rightsId), new AccessKey(titleKey)).ThrowIfFailure();
                }
            }
        }

        public static string PrintKeys(Keyset keyset, Dictionary<string, KeyValue> dict)
        {
            if (dict.Count == 0) return string.Empty;

            var sb = new StringBuilder();
            int maxNameLength = dict.Values.Max(x => x.Name.Length);
            int currentGroup = 0;

            foreach (KeyValue keySlot in dict.Values.Where(x => x.Group >= 0).OrderBy(x => x.Group).ThenBy(x => x.Name))
            {
                byte[] key = keySlot.GetKey(keyset);
                if (key.IsEmpty()) continue;

                if (keySlot.Group > currentGroup)
                {
                    if (currentGroup > 0) sb.AppendLine();
                    currentGroup = keySlot.Group;
                }

                string line = $"{keySlot.Name.PadRight(maxNameLength)} = {key.ToHexString()}";
                sb.AppendLine(line);
            }

            return sb.ToString();
        }

        public static string PrintCommonKeys(Keyset keyset)
        {
            return PrintKeys(keyset, CreateCommonKeyDictionary());
        }

        public static string PrintUniqueKeys(Keyset keyset)
        {
            return PrintKeys(keyset, CreateUniqueKeyDictionary());
        }

        public static string PrintAllKeys(Keyset keyset)
        {
            return PrintKeys(keyset, CreateFullKeyDictionary());
        }

        public static string PrintTitleKeys(Keyset keyset)
        {
            var sb = new StringBuilder();

            foreach ((RightsId rightsId, AccessKey key) kv in keyset.ExternalKeySet.ToList().OrderBy(x => x.rightsId.ToString()))
            {
                string line = $"{kv.rightsId} = {kv.key}";
                sb.AppendLine(line);
            }

            return sb.ToString();
        }

        public static Dictionary<string, KeyValue> CreateCommonKeyDictionary()
        {
            return CreateCommonKeyList().ToDictionary(k => k.Name, k => k, StringComparer.OrdinalIgnoreCase);
        }

        public static Dictionary<string, KeyValue> CreateUniqueKeyDictionary()
        {
            return CreateUniqueKeyList().ToDictionary(k => k.Name, k => k, StringComparer.OrdinalIgnoreCase);
        }

        public static Dictionary<string, KeyValue> CreateFullKeyDictionary()
        {
            List<KeyValue> commonKeys = CreateCommonKeyList();
            List<KeyValue> uniqueKeys = CreateUniqueKeyList();

            return uniqueKeys.Concat(commonKeys).ToDictionary(k => k.Name, k => k, StringComparer.OrdinalIgnoreCase);
        }

        private static List<KeyValue> CreateCommonKeyList()
        {
            var keys = new List<KeyValue>
            {
                new KeyValue("keyblob_mac_key_source", 0x10, 0, set => set.KeyblobMacKeySource),

                new KeyValue("master_key_source", 0x10, 60, set => set.MasterKeySource),
                new KeyValue("package2_key_source", 0x10, 60, set => set.Package2KeySource),

                new KeyValue("aes_kek_generation_source", 0x10, 70, set => set.AesKekGenerationSource),
                new KeyValue("aes_key_generation_source", 0x10, 70, set => set.AesKeyGenerationSource),

                new KeyValue("bis_kek_source", 0x10, 80, set => set.BisKekSource),

                new KeyValue("retail_specific_aes_key_source", 0x10, 90, set => set.RetailSpecificAesKeySource),
                new KeyValue("per_console_key_source", 0x10, 90, set => set.PerConsoleKeySource),

                new KeyValue("header_kek_source", 0x10, 100, set => set.HeaderKekSource),
                new KeyValue("header_key_source", 0x20, 100, set => set.HeaderKeySource),
                new KeyValue("key_area_key_application_source", 0x10, 100, set => set.KeyAreaKeyApplicationSource),
                new KeyValue("key_area_key_ocean_source", 0x10, 100, set => set.KeyAreaKeyOceanSource),
                new KeyValue("key_area_key_system_source", 0x10, 100, set => set.KeyAreaKeySystemSource),
                new KeyValue("titlekek_source", 0x10, 100, set => set.TitleKekSource),

                new KeyValue("save_mac_kek_source", 0x10, 110, set => set.SaveMacKekSource),
                new KeyValue("save_mac_sd_card_kek_source", 0x10, 110, set => set.SaveMacSdCardKekSource),
                new KeyValue("save_mac_key_source", 0x10, 110, set => set.SaveMacKeySource),
                new KeyValue("save_mac_sd_card_key_source", 0x10, 110, set => set.SaveMacSdCardKeySource),
                new KeyValue("sd_card_kek_source", 0x10, 110, set => set.SdCardKekSource),
                new KeyValue("sd_card_save_key_source", 0x20, 110, set => set.SdCardKeySources[0]),
                new KeyValue("sd_card_nca_key_source", 0x20, 110, set => set.SdCardKeySources[1]),
                new KeyValue("sd_card_custom_storage_key_source", 0x20, 110, set => set.SdCardKeySources[2]),

                new KeyValue("eticket_rsa_kek", 0x10, 120, set => set.EticketRsaKek),
                new KeyValue("ssl_rsa_kek", 0x10, 120, set => set.SslRsaKek),
                new KeyValue("xci_header_key", 0x10, 130, set => set.XciHeaderKey),

                new KeyValue("header_key", 0x20, 220, set => set.HeaderKey)
            };

            for (int slot = 0; slot < 0x20; slot++)
            {
                int i = slot;
                keys.Add(new KeyValue($"keyblob_key_source_{i:x2}", 0x10, 0, set => set.KeyblobKeySources[i]));
                keys.Add(new KeyValue($"keyblob_{i:x2}", 0x90, 10, set => set.Keyblobs[i]));
                keys.Add(new KeyValue($"tsec_root_key_{i:x2}", 0x10, 20, set => set.TsecRootKeys[i]));
                keys.Add(new KeyValue($"master_kek_source_{i:x2}", 0x10, 30, set => set.MasterKekSources[i]));
                keys.Add(new KeyValue($"master_kek_{i:x2}", 0x10, 40, set => set.MasterKeks[i]));
                keys.Add(new KeyValue($"package1_key_{i:x2}", 0x10, 50, set => set.Package1Keys[i]));

                keys.Add(new KeyValue($"master_key_{i:x2}", 0x10, 200, set => set.MasterKeys[i]));
                keys.Add(new KeyValue($"package2_key_{i:x2}", 0x10, 210, set => set.Package2Keys[i]));
                keys.Add(new KeyValue($"titlekek_{i:x2}", 0x10, 230, set => set.TitleKeks[i]));
                keys.Add(new KeyValue($"key_area_key_application_{i:x2}", 0x10, 240, set => set.KeyAreaKeys[i][0]));
                keys.Add(new KeyValue($"key_area_key_ocean_{i:x2}", 0x10, 250, set => set.KeyAreaKeys[i][1]));
                keys.Add(new KeyValue($"key_area_key_system_{i:x2}", 0x10, 260, set => set.KeyAreaKeys[i][2]));
            }

            for (int slot = 0; slot < 4; slot++)
            {
                int i = slot;
                keys.Add(new KeyValue($"bis_key_source_{i:x2}", 0x20, 80, set => set.BisKeySource[i]));
            }

            return keys;
        }

        private static List<KeyValue> CreateUniqueKeyList()
        {
            var keys = new List<KeyValue>
            {
                new KeyValue("secure_boot_key", 0x10, 0, set => set.SecureBootKey),
                new KeyValue("tsec_key", 0x10, 0, set => set.TsecKey),
                new KeyValue("sd_seed", 0x10, 10, set => set.SdSeed),

                new KeyValue("device_key", 0x10, 40, set => set.DeviceKey),
                new KeyValue("save_mac_key", 0x10, 60, set => set.SaveMacKey),
                new KeyValue("save_mac_sd_card_key", 0x10, 60, set => set.SaveMacSdCardKey)
            };

            for (int slot = 0; slot < 0x20; slot++)
            {
                int i = slot;
                keys.Add(new KeyValue($"keyblob_mac_key_{i:x2}", 0x10, 20, set => set.KeyblobMacKeys[i]));
                keys.Add(new KeyValue($"keyblob_key_{i:x2}", 0x10, 30, set => set.KeyblobKeys[i]));
                keys.Add(new KeyValue($"encrypted_keyblob_{i:x2}", 0xB0, 100, set => set.EncryptedKeyblobs[i]));
            }

            for (int slot = 0; slot < 4; slot++)
            {
                int i = slot;
                keys.Add(new KeyValue($"bis_key_{i:x2}", 0x20, 50, set => set.BisKeys[i]));
            }

            return keys;
        }

        public class KeyValue
        {
            public readonly string Name;
            public readonly int Size;
            public readonly int Group;
            public readonly Func<Keyset, byte[]> GetKey;

            public KeyValue(string name, int size, int group, Func<Keyset, byte[]> retrieveFunc)
            {
                Name = name;
                Size = size;
                Group = group;
                GetKey = retrieveFunc;
            }
        }
    }

    public enum KeyType
    {
        None,
        Common,
        Unique,
        Title
    }
}

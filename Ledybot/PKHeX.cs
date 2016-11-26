﻿
/// I do not own the code in this class. 
/// All rights and credits for the code in this class belong to Kaphotics.
/// All code within this class is taken from PKHeX https://github.com/kwsch/PKHeX


using System;
using System.Linq;
using System.Text;

namespace Ledybot
{
    public class PKHeX
    {

        public static uint LCRNG(uint seed)
        {
            const uint a = 0x41C64E6D;
            const uint c = 0x00006073;

            return seed * a + c;
        }

        public static uint LCRNG(ref uint seed)
        {
            const uint a = 0x41C64E6D;
            const uint c = 0x00006073;

            return seed = seed * a + c;
        }

        public static readonly byte[][] blockPosition =
{
            new byte[] {0, 0, 0, 0, 0, 0, 1, 1, 2, 3, 2, 3, 1, 1, 2, 3, 2, 3, 1, 1, 2, 3, 2, 3},
            new byte[] {1, 1, 2, 3, 2, 3, 0, 0, 0, 0, 0, 0, 2, 3, 1, 1, 3, 2, 2, 3, 1, 1, 3, 2},
            new byte[] {2, 3, 1, 1, 3, 2, 2, 3, 1, 1, 3, 2, 0, 0, 0, 0, 0, 0, 3, 2, 3, 2, 1, 1},
            new byte[] {3, 2, 3, 2, 1, 1, 3, 2, 3, 2, 1, 1, 3, 2, 3, 2, 1, 1, 0, 0, 0, 0, 0, 0},
        };

        public static readonly byte[] blockPositionInvert =
        {
            0, 1, 2, 4, 3, 5, 6, 7, 12, 18, 13, 19, 8, 10, 14, 20, 16, 22, 9, 11, 15, 21, 17, 23
        };

        public static byte[] shuffleArray(byte[] data, uint sv)
        {
            byte[] sdata = new byte[data.Length];
            Array.Copy(data, sdata, 8); // Copy unshuffled bytes

            // Shuffle Away!
            for (int block = 0; block < 4; block++)
                Array.Copy(data, 8 + 56 * blockPosition[block][sv], sdata, 8 + 56 * block, 56);

            // Fill the Battle Stats back
            if (data.Length > 232)
                Array.Copy(data, 232, sdata, 232, 28);

            return sdata;
        }

        public static byte[] decryptArray(byte[] ekx)
        {
            byte[] pkx = (byte[])ekx.Clone();

            uint pv = BitConverter.ToUInt32(pkx, 0);
            uint sv = (pv >> 0xD & 0x1F) % 24;

            uint seed = pv;

            // Decrypt Blocks with RNG Seed
            for (int i = 8; i < 232; i += 2)
                BitConverter.GetBytes((ushort)(BitConverter.ToUInt16(pkx, i) ^ LCRNG(ref seed) >> 16)).CopyTo(pkx, i);

            // Deshuffle
            pkx = shuffleArray(pkx, sv);

            // Decrypt the Party Stats
            seed = pv;
            if (pkx.Length <= 232) return pkx;
            for (int i = 232; i < 260; i += 2)
                BitConverter.GetBytes((ushort)(BitConverter.ToUInt16(pkx, i) ^ LCRNG(ref seed) >> 16)).CopyTo(pkx, i);

            return pkx;
        }
        public static byte[] encryptArray(byte[] pkx)
        {
            // Shuffle
            uint pv = BitConverter.ToUInt32(pkx, 0);
            uint sv = (pv >> 0xD & 0x1F) % 24;

            byte[] ekx = (byte[])pkx.Clone();

            ekx = shuffleArray(ekx, blockPositionInvert[sv]);

            uint seed = pv;

            // Encrypt Blocks with RNG Seed
            for (int i = 8; i < 232; i += 2)
                BitConverter.GetBytes((ushort)(BitConverter.ToUInt16(ekx, i) ^ LCRNG(ref seed) >> 16)).CopyTo(ekx, i);

            // If no party stats, return.
            if (ekx.Length <= 232) return ekx;

            // Encrypt the Party Stats
            seed = pv;
            for (int i = 232; i < 260; i += 2)
                BitConverter.GetBytes((ushort)(BitConverter.ToUInt16(ekx, i) ^ LCRNG(ref seed) >> 16)).CopyTo(ekx, i);

            // Done
            return ekx;
        }

        public static ushort getCHK(byte[] data)
        {
            ushort chk = 0;
            for (int i = 8; i < 232; i += 2) // Loop through the entire PKX
                chk += BitConverter.ToUInt16(data, i);

            return chk;
        }

        public static readonly int[,] hpivs =
        {
            { 1, 1, 0, 0, 0, 0 }, // Fighting
            { 0, 0, 0, 0, 0, 1 }, // Flying
            { 1, 1, 0, 0, 0, 1 }, // Poison
            { 1, 1, 1, 0, 0, 1 }, // Ground
            { 1, 1, 0, 1, 0, 0 }, // Rock
            { 1, 0, 0, 1, 0, 1 }, // Bug
            { 1, 0, 1, 1, 0, 1 }, // Ghost
            { 1, 1, 1, 1, 0, 1 }, // Steel
            { 1, 0, 1, 0, 1, 0 }, // Fire
            { 1, 0, 0, 0, 1, 1 }, // Water
            { 1, 0, 1, 0, 1, 1 }, // Grass
            { 1, 1, 1, 0, 1, 1 }, // Electric
            { 1, 0, 1, 1, 1, 0 }, // Psychic
            { 1, 0, 0, 1, 1, 1 }, // Ice
            { 1, 0, 1, 1, 1, 1 }, // Dragon
            { 1, 1, 1, 1, 1, 1 }, // Dark
        };

        public byte[] Data { get; set; }
        
    }
}
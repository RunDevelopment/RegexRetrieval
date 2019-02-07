using System;
using System.Collections.Generic;

namespace RegexRetrieval.Compressed
{
    internal static class VarInt
    {
        public static void Read(ushort[] data, ref int index, out ulong value)
        {
            ushort u;
            ulong v = 0;

            while ((u = data[index]) > 0x7FFF)
            {
                v = (v << 15) | (u & 0x7FFFu);
                index++;
            }

            v = (v << 15) | u;

            value = v;
        }
        public static void Write(List<ushort> data, ushort[] tempBuffer, ulong value)
        {
#if DEBUG
            if (tempBuffer.Length < 5)
                throw new ArgumentException(nameof(tempBuffer));
#endif

            ulong v = value;

            if (v == 0)
            {
                data.Add(0);
                return;
            }

            int parts = 0;
            while (v != 0)
            {
                tempBuffer[parts++] = (ushort) (v & 0x7FFF);
                v = v >> 15;
            }

            for (var i = parts - 1; i > 0; i--)
                data.Add((ushort) (tempBuffer[i] | 0x8000u));
            data.Add(tempBuffer[0]);
        }


        public static void Read(byte[] data, ref int index, out ulong value)
        {
            byte u;
            ulong v = 0;

            while ((u = data[index]) > 0x7F)
            {
                v = (v << 7) | (u & 0x7Fu);
                index++;
            }

            v = (v << 7) | u;

            value = v;
        }
        public static void Write(List<byte> data, byte[] tempBuffer, ulong value)
        {
#if DEBUG
            if (tempBuffer.Length < 10)
                throw new ArgumentException(nameof(tempBuffer));
#endif

            ulong v = value;

            if (v == 0)
            {
                data.Add(0);
                return;
            }

            int parts = 0;
            while (v != 0)
            {
                tempBuffer[parts++] = (byte) (v & 0x7F);
                v = v >> 7;
            }

            for (var i = parts - 1; i > 0; i--)
                data.Add((byte) (tempBuffer[i] | 0x80u));
            data.Add(tempBuffer[0]);
        }
    }
}

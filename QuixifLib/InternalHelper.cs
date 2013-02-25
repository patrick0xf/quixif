#region MIT License

// Copyright (c) 2013 Patrick Fournier
// patrick0xf@thunderground.net
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// Except as contained in this notice, the name(s) of the above copyright
// holders shall not be used in advertising or otherwise to promote the
// sale, use or other dealings in this Software without prior written authorization.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#endregion

using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace QuixifLib
{
    internal static class InternalHelper
    {
        internal static string ToHexNumber(this byte[] ba)
        {
            return ba != null ? string.Concat(ConvertAll(ba, x => x.ToString("X2"))) : null;
        }

        internal static string ToHexNumber(this byte b)
        {
            return b.ToString("X2");
        }

        internal static int ToInteger(this byte[] ba)
        {
            return ba != null ? Int32.Parse(ba.ToHexNumber(), NumberStyles.HexNumber) : default(int);
        }

        internal static short ToShort(this byte[] ba)
        {
            return ba != null ? Int16.Parse(ba.ToHexNumber(), NumberStyles.HexNumber) : default(short);
        }

        internal static long ToLong(this byte[] ba)
        {
            return ba != null ? Int64.Parse(ba.ToHexNumber(), NumberStyles.HexNumber) : default(long);
        }

        internal static string ToAsciiString(this byte[] ba)
        {
            return ba != null ? Encoding.UTF8.GetString(ba, 0, ba.Length) : default(string);
        }

        internal static byte[] ToSubByteArray(this byte[] ba, int offset, int count)
        {
            if (ba == null) return null;

            if (offset + count > ba.Length)
            {
                return new byte[] {};
            }
            var sba = new byte[count];
            Array.Copy(ba, offset, sba, 0, count);
            return sba;
        }

        internal static byte[] GetAlignedData(byte[] data, bool isIntelAlign)
        {
            if (data == null) return null;
            return isIntelAlign ? data.Reverse().ToArray() : data;
        }

        internal static TOutput[] ConvertAll<TInput, TOutput>(TInput[] array, Converter<TInput, TOutput> converter)
        {
            return array == null ? null : (from item in array select converter(item)).ToArray();
        }
    }
}
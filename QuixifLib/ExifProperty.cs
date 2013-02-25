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
using System.Linq;
using System.Text;

namespace QuixifLib
{
    internal class ExifProperty
    {
        internal string TagName { get; private set; }
        internal object Value { get; private set; }
        internal string TagId { get; private set; }
        internal string Format { get; private set; }
        internal bool IsArray { get; private set; }
        internal bool IsOffset { get; private set; }
        internal string OffsetName { get; private set; }
        internal bool IsPadding { get; private set; }

        internal ExifProperty(byte[] tag, byte[][] rawValues, ImageFileDirectoryEntry.FormatType format, TagsMap tagsMap, bool isIntelAlignment)
        {
            TagId = tag.ToHexNumber();
            Format = format.ToString();
            var matchingTag = tagsMap.Tags == null ? null : tagsMap.Tags.FirstOrDefault(t => t.Id == TagId);

            TagName = matchingTag != null ? matchingTag.Value : String.Format("[{2} (Unknown {0}{1})]", rawValues.Length == 1 ? String.Empty : String.Format("array of {0} ", rawValues.Length), format, TagId);
            IsOffset = matchingTag != null && matchingTag.IsOffset;
            OffsetName = matchingTag != null ? matchingTag.OffsetName : String.Empty;
            IsPadding = matchingTag != null && matchingTag.IsPadding;

            //Manual convertions
            object convertedValues;

            switch (format)
            {
                default:
                    convertedValues = null;
                    break;

                case ImageFileDirectoryEntry.FormatType.UByte:
                    var tempUByte = InternalHelper.ConvertAll(rawValues, RawToUByte);
                    convertedValues = rawValues.Length == 1 ? tempUByte[0] : (object) tempUByte;
                    break;

                case ImageFileDirectoryEntry.FormatType.AsciiString:
                    convertedValues = String.Concat(InternalHelper.ConvertAll(rawValues, RawToAsciiString)).TrimEnd('\0');
                    break;

                case ImageFileDirectoryEntry.FormatType.UShort:
                    var tempUShort = InternalHelper.ConvertAll(rawValues, RawToUShort);
                    convertedValues = rawValues.Length == 1 ? tempUShort[0] : (object) tempUShort;
                    break;

                case ImageFileDirectoryEntry.FormatType.ULong:
                    var tempULong = InternalHelper.ConvertAll(rawValues, RawToULong);
                    convertedValues = rawValues.Length == 1 ? tempULong[0] : (object) tempULong;
                    break;

                case ImageFileDirectoryEntry.FormatType.URational:
                    var alignURational = isIntelAlignment ? InternalHelper.ConvertAll(rawValues, IntelAlignRational) : rawValues;
                    var tempURational = InternalHelper.ConvertAll(alignURational, RawToURational);
                    convertedValues = rawValues.Length == 1 ? tempURational[0] : (object) tempURational;
                    break;
            }

            Value = rawValues == null ? null : convertedValues;
            IsArray = rawValues != null && format != ImageFileDirectoryEntry.FormatType.AsciiString && rawValues.Length > 1;
        }

        private static byte RawToUByte(byte[] ba)
        {
            return ba != null ? ba[0] : default(byte);
        }

        private static string RawToAsciiString(byte[] ba)
        {
            return ba != null ? Encoding.UTF8.GetString(ba, 0, ba.Length) : default(string);
        }

        private static short RawToUShort(byte[] ba)
        {
            return ba != null ? ba.ToShort() : default(short);
        }

        private static long RawToULong(byte[] ba)
        {
            return ba != null ? ba.ToLong() : default(long);
        }

        private static byte[] IntelAlignRational(byte[] ba)
        {
            return ba != null ? new[] {ba[4], ba[5], ba[6], ba[7], ba[0], ba[1], ba[2], ba[3]} : null;
        }

        private static float RawToURational(byte[] ba)
        {
            return ba != null ? ((float) (new[] {ba[0], ba[1], ba[2], ba[3]}.ToLong())/new[] {ba[4], ba[5], ba[6], ba[7]}.ToLong()) : default(float);
        }
    }
}
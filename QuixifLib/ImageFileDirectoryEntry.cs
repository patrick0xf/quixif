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

namespace QuixifLib
{
    public class ImageFileDirectoryEntry
    {
        private const int TIFFHEADER_OFFSET = 8;
        private const int TAG_OFFSET = 0;
        private const int TAG_FIELDLENGTH = 2;
        private const int FORMAT_OFFSET = 2;
        private const int FORMAT_FIELDLENGTH = 2;
        private const int ARRAYLENGTH_OFFSET = 4;
        private const int ARRAYLENGTH_FIELDLENGTH = 4;
        private const int DATA_OFFSET = 8;
        private const int DATA_FIELDLENGTH = 4;

        private readonly int[] _itemLengthOf = new[] {0, 1, 1, 2, 4, 8, 1, 1, 2, 4, 8, 4, 8};
        private readonly ExifProperty _property;

        public enum FormatType
        {
            Unused,
            UByte,
            AsciiString,
            UShort,
            ULong,
            URational,
            SByte,
            Undefined,
            SShort,
            SLong,
            SRational,
            SFloat,
            DFloat
        };

        public string Format
        {
            get { return _property.Format; }
        }

        public string TagId
        {
            get { return _property.TagId; }
        }

        public string TagName
        {
            get { return _property.TagName; }
        }

        public object Value
        {
            get { return _property.Value; }
        }

        public bool IsArray
        {
            get { return _property.IsArray; }
        }

        public bool IsOffset
        {
            get { return _property.IsOffset; }
        }

        public string OffsetName
        {
            get { return _property.OffsetName; }
        }

        public bool IsPadding
        {
            get { return _property.IsPadding; }
        }

        internal ImageFileDirectoryEntry(byte[] entryData, byte[] exifData, bool isIntelAlign, TagsMap tagsMap)
        {
            var tag = InternalHelper.GetAlignedData(entryData.ToSubByteArray(TAG_OFFSET, TAG_FIELDLENGTH), isIntelAlign);
            var format = InternalHelper.GetAlignedData(entryData.ToSubByteArray(FORMAT_OFFSET, FORMAT_FIELDLENGTH), isIntelAlign).ToInteger();
            var arrayLength = InternalHelper.GetAlignedData(entryData.ToSubByteArray(ARRAYLENGTH_OFFSET, ARRAYLENGTH_FIELDLENGTH), isIntelAlign).ToInteger();
            var itemLength = _itemLengthOf[format];
            var rawValues = GetRawValueArray(arrayLength, itemLength, entryData, exifData, isIntelAlign);

            _property = new ExifProperty(tag, rawValues, (FormatType) format, tagsMap, isIntelAlign);
        }

        private static byte[][] GetRawValueArray(int arrayLength, int itemLength, byte[] entryData, byte[] exifData, bool isIntelAlign)
        {
            var rawValues = Array.CreateInstance(typeof (byte[]), arrayLength);

            var totalLength = arrayLength*itemLength;

            if (totalLength <= 4)
            {
                //Data is inside of entry
                for (var valueIndex = 0; valueIndex < arrayLength; valueIndex++)
                {
                    rawValues.SetValue(InternalHelper.GetAlignedData(entryData.ToSubByteArray(DATA_OFFSET + (valueIndex*itemLength), itemLength), isIntelAlign), valueIndex);
                }
            }
            else
            {
                //Data is outside of entry
                var dataChunkOffset = InternalHelper.GetAlignedData(entryData.ToSubByteArray(DATA_OFFSET, DATA_FIELDLENGTH), isIntelAlign).ToInteger() + TIFFHEADER_OFFSET;
                for (var valueIndex = 0; valueIndex < arrayLength; valueIndex++)
                {
                    rawValues.SetValue(InternalHelper.GetAlignedData(exifData.ToSubByteArray(dataChunkOffset + (valueIndex*itemLength), itemLength), isIntelAlign), valueIndex);
                }
            }

            return (byte[][]) rawValues;
        }
    }
}
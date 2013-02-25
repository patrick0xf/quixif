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
using System.Collections.Generic;
using System.Linq;

namespace QuixifLib
{
    public partial class Exif
    {
        public byte[] RawData { get; private set; }
        public bool IsValid { get; private set; }
        public List<ImageFileDirectory> ImageFileDirectories { get; private set; }

        private const int TIFFHEADER_OFFSET = 8;
        private const string EXIFHEADER_VALUE = "Exif\0\0";
        private const string TAGMARK_HEX = "002A";
        private const string INTELALIGN_HEX = "4949";
        private const int SIZE_OFFSET = 0;
        private const int SIZE_LENGTH = 2;
        private const int EXIFHEADER_OFFSET = 2;
        private const int EXIFHEADER_LENGTH = 6;
        private const int INTELALIGN_OFFSET = 8;
        private const int INTELALIGN_LENGTH = 2;
        private const int TAGMARK_OFFSET = 10;
        private const int TAGMARK_LENGTH = 2;
        private const int IFDOFFSET_OFFSET = 12;
        private const int IFDOFFSET_LENGTH = 4;

        private readonly bool _alignment;

        private static readonly TagsMap IfdTagsMap = TagsMap.GetTagsMapByName("IFD");

        /// <summary>
        /// Creates an Exif object containing the Exif Data entries for a given data segment
        /// </summary>
        /// <param name="data">A complete Exif data segment</param>
        /// <param name="readThubmails">Specifies whether the ThumbnailData property should be populated while reading the stream</param>
        public Exif(byte[] data, bool readThubmails = true)
        {
            if (data == null) return;
            ImageFileDirectories = new List<ImageFileDirectory>();

            try
            {
                RawData = data;

                if (GetSize() != RawData.Length) return; // Invalid Size
                if (GetHeader() != EXIFHEADER_VALUE) return; // Invalid Header
                _alignment = IsIntelAlign();
                if (GetTagMark().ToHexNumber() != TAGMARK_HEX) return; // Invalid alignment

                var currentIfdOffset = GetFirstIfdOffset();
                var currentIfd = 0;

                while (currentIfdOffset != 0)
                {
                    var imageFileDirectory = new ImageFileDirectory(RawData, currentIfdOffset + TIFFHEADER_OFFSET, _alignment, IfdTagsMap, readThubmails, currentIfd++);

                    ImageFileDirectories.Add(imageFileDirectory);
                    ProcessSubDirectories(imageFileDirectory, readThubmails);

                    currentIfdOffset = imageFileDirectory.NextIfdOffset;
                }

                IsValid = true;
            }
            catch (Exception)
            {
                //Eventually get rid if this catch
                IsValid = false;
            }
        }

        private void ProcessSubDirectories(ImageFileDirectory imageFileDirectory, bool readThubmails)
        {
            foreach (var offsetTag in imageFileDirectory.Entries.Where(offsetTag => offsetTag.IsOffset))
            {
                int offset;
                if (!Int32.TryParse(offsetTag.Value.ToString(), out offset)) continue;
                if (offset == 0) continue;

                var offsetName = offsetTag.OffsetName;
                var tagsMap = TagsMap.GetTagsMapByName(offsetName);
                var offsetImageFileDirectory = new ImageFileDirectory(RawData, offset + TIFFHEADER_OFFSET, _alignment, tagsMap, readThubmails);

                ImageFileDirectories.Add(offsetImageFileDirectory);
                ProcessSubDirectories(offsetImageFileDirectory, readThubmails);
            }
        }

        private int GetSize()
        {
            return RawData.ToSubByteArray(SIZE_OFFSET, SIZE_LENGTH).ToInteger();
        }

        private string GetHeader()
        {
            return RawData.ToSubByteArray(EXIFHEADER_OFFSET, EXIFHEADER_LENGTH).ToAsciiString();
        }

        private bool IsIntelAlign()
        {
            return RawData.ToSubByteArray(INTELALIGN_OFFSET, INTELALIGN_LENGTH).ToHexNumber() == INTELALIGN_HEX;
        }

        private byte[] GetTagMark()
        {
            return InternalHelper.GetAlignedData(RawData.ToSubByteArray(TAGMARK_OFFSET, TAGMARK_LENGTH), _alignment);
        }

        private int GetFirstIfdOffset()
        {
            return InternalHelper.GetAlignedData(RawData.ToSubByteArray(IFDOFFSET_OFFSET, IFDOFFSET_LENGTH), _alignment).ToInteger();
        }
    }
}
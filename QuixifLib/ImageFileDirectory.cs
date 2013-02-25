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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace QuixifLib
{
    public class ImageFileDirectory : IEnumerable
    {
        private const string COMPRESSION_HEX = "0103";
        private const string THUMBNAIL_OFFSET = "0201";
        private const string THUMBNAIL_LENGTH = "0202";
        private const int JPEG_COMPRESSION = 6;
        private const int TIFFHEADER_OFFSET = 8;
        private const int DIRECTORYENTRY_FIELDLENGTH = 12;
        private const int COUNTENTRY_FIELDLENGTH = 2;
        private const int NEXTENTRY_FIELDLENGTH = 4;

        internal int NextIfdOffset { get; private set; }

        public List<ImageFileDirectoryEntry> Entries { get; private set; }
        public byte[] ThumbnailData { get; private set; }
        public string Name { get; private set; }

        public ImageFileDirectoryEntry this[int index]
        {
            get { return Entries[index]; }
        }

        internal ImageFileDirectory(byte[] exifData, int offset, bool isIntelAlign, TagsMap tagsMap, bool readThumbnails, int? ifd = null)
        {
            Name = String.Format("{0}{1}", tagsMap.Name, ifd);

            var entries = new List<ImageFileDirectoryEntry>();

            var entryCount = InternalHelper.GetAlignedData(exifData.ToSubByteArray(offset, COUNTENTRY_FIELDLENGTH), isIntelAlign).ToInteger();

            for (var entryIndex = 0; entryIndex < entryCount; entryIndex++)
            {
                entries.Add(new ImageFileDirectoryEntry(exifData.ToSubByteArray((offset + COUNTENTRY_FIELDLENGTH) + (DIRECTORYENTRY_FIELDLENGTH*entryIndex), DIRECTORYENTRY_FIELDLENGTH), exifData, isIntelAlign, tagsMap));
            }

            NextIfdOffset = InternalHelper.GetAlignedData(exifData.ToSubByteArray((offset + COUNTENTRY_FIELDLENGTH) + (DIRECTORYENTRY_FIELDLENGTH*entryCount), NEXTENTRY_FIELDLENGTH), isIntelAlign).ToInteger();
            Entries = entries;

            if (readThumbnails) PopulateThumbnailData(exifData);
        }

        private void SetThumbnailData(byte[] thumbnailData)
        {
            ThumbnailData = thumbnailData;
        }

        private void PopulateThumbnailData(byte[] exifData)
        {
            var thumbnailCompression = Entries.FirstOrDefault(e => e.TagId == COMPRESSION_HEX);

            if (thumbnailCompression == null || Convert.ToInt32(thumbnailCompression.Value) != JPEG_COMPRESSION) return;

            var thumbnailOffset = Entries.FirstOrDefault(e => e.TagId == THUMBNAIL_OFFSET);
            var thumbnailLength = Entries.FirstOrDefault(e => e.TagId == THUMBNAIL_LENGTH);

            if (thumbnailOffset == null || thumbnailLength == null) return;

            var jpegData = exifData.ToSubByteArray(Convert.ToInt32(thumbnailOffset.Value) + TIFFHEADER_OFFSET, Convert.ToInt32(thumbnailLength.Value));
            SetThumbnailData(jpegData);
        }

        public IEnumerator GetEnumerator()
        {
            return Entries.GetEnumerator();
        }
    }
}
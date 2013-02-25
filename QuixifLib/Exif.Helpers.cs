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
using System.IO;

namespace QuixifLib
{
    public partial class Exif
    {
        /// <summary>
        /// Reads the content of a file's header, up to (and including) the Exif data segment
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <param name="maximumHeaderLength">Stop reading after the reader's position has reached this value, and no Exif data segment was found</param>
        /// <returns></returns>
        public static FileHeaderContent ReadStreamUpToExifData(Stream stream, int maximumHeaderLength = Int32.MaxValue)
        {
            var alreadyReadBytes = new Stack<byte>();
            byte[] markerData = null;

            if (stream != null)
            {
                maximumHeaderLength = Math.Max(maximumHeaderLength, 2); // shouldn't be less than 2

                var b = new byte[2];
                var bytesReadCount = stream.Read(b, 0, 2);
                if (b[0].Equals(Markers.MARKER_) && b[1].Equals(Markers.MARKER_SOI))
                {
                    while (bytesReadCount == 2 && markerData == null && alreadyReadBytes.Count <= maximumHeaderLength)
                    {
                        if (b[0].Equals(Markers.MARKER_) && b[1].Equals(Markers.MARKER_EXIF))
                        {
                            var rawDataSize = new byte[2];
                            stream.Read(rawDataSize, 0, rawDataSize.Length);
                            markerData = new byte[rawDataSize.ToInteger()];
                            Array.Copy(rawDataSize, markerData, 2);
                            stream.Read(markerData, 2, markerData.Length - 2);
                        }
                        else
                        {
                            alreadyReadBytes.Push(b[0]);
                            alreadyReadBytes.Push(b[1]);
                            bytesReadCount = stream.Read(b, 0, 2);
                        }
                    }
                }
                else
                {
                    alreadyReadBytes.Push(b[0]);
                    alreadyReadBytes.Push(b[1]);
                }

                //Handle stray byte
                if (bytesReadCount == 1) alreadyReadBytes.Push(b[0]);
            }
            var retArray = alreadyReadBytes.ToArray();
            Array.Reverse(retArray);
            return new FileHeaderContent(retArray, markerData);
        }

        /// <summary>
        /// Returns a formatted value from the entry's value suitable for display
        /// </summary>
        /// <param name="imageFileDirectoryEntry"></param>
        /// <returns></returns>
        public static string GetDisplayValue(ImageFileDirectoryEntry imageFileDirectoryEntry)
        {
            var displayValue = imageFileDirectoryEntry.Value;

            if (imageFileDirectoryEntry.IsArray && displayValue != null)
            {
                var values = new string[((Array) displayValue).Length];
                for (var index = 0; index < values.Length; index++)
                {
                    values[index] = ((Array) displayValue).GetValue(index).ToString();
                }
                displayValue = String.Join(",", values);
            }

            return displayValue != null ? displayValue.ToString() : String.Empty;
        }
    }
}
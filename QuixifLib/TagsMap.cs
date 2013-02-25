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
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;

namespace QuixifLib
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "root")]
    public class TagsMapRoot
    {
        [XmlElement("tags")]
        public TagsMap TagsMap { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class TagsMap
    {
        [XmlElement("tag")]
        public Tag[] Tags { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        public TagsMap()
        {
        }

        public TagsMap(Stream stream)
        {
            var xmlSerializer = new XmlSerializer(typeof (TagsMapRoot));
            TagsMap tags;
            using (var reader = XmlReader.Create(stream))
            {
                var root = (TagsMapRoot) xmlSerializer.Deserialize(reader);
                tags = root.TagsMap;
            }
            Tags = tags.Tags;
            Name = tags.Name;
        }

        public static TagsMap GetTagsMapByName(string name)
        {
            var tagsMapResourceLocator = String.Format("{0}.TagsMaps.{1}.xml", GetLibraryName(), name);
            return !Assembly.GetExecutingAssembly().GetManifestResourceNames().Contains(tagsMapResourceLocator) ?
                       new TagsMap() :
                       new TagsMap(Assembly.GetExecutingAssembly().GetManifestResourceStream(tagsMapResourceLocator));
        }

        public static string GetLibraryName()
        {
            return "QuixifLib";
        }
    }

    [XmlType(AnonymousType = true)]
    public class Tag
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlAttribute(AttributeName = "isoffset")]
        public bool IsOffset { get; set; }

        [XmlAttribute(AttributeName = "ispadding")]
        public bool IsPadding { get; set; }

        [XmlAttribute(AttributeName = "offsetname")]
        public string OffsetName { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}
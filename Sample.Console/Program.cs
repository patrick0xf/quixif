using System.Net;
using QuixifLib;
using System;
using System.Globalization;
using System.IO;

namespace Sample.Console
{
    public partial class Program
    {
        static void Main()
        {
            System.Console.WriteLine("Example usage of the Quixif Fast Exif reader library");

            while (true)
            {
                bool? isRemote = null;
                while (!isRemote.HasValue)
                {
                    System.Console.WriteLine();
                    System.Console.WriteLine("Use image from [L]ocal Machine or [R]emote Website?");
                    var readKey = System.Console.ReadKey();
                    if (readKey.Key == ConsoleKey.L) isRemote = false;
                    if (readKey.Key == ConsoleKey.R) isRemote = true;
                }

                var resourceLocation = (bool)isRemote ? GetDefaultWebSite() : GetDefaultSampleImage();
                System.Console.WriteLine();
                System.Console.WriteLine("Specify a location, or skip to use default");
                System.Console.WriteLine("Default is {0}", resourceLocation);
                var specificLocation = System.Console.ReadLine();

                if (!String.IsNullOrEmpty(specificLocation)) resourceLocation = specificLocation;

                //Initialize the file header content
                FileHeaderContent fileHeaderContent = null;

                while (fileHeaderContent == null)
                {
                    try
                    {
                        System.Console.WriteLine("Reading from {0}", resourceLocation);

                        //Open a stream. Any stream will do, here we demonstrate with either a WebRequest or a local File
                        using (var stream = (bool) isRemote
                                                ? WebRequest.Create(resourceLocation ?? String.Empty).GetResponse().GetResponseStream()
                                                : File.OpenRead(resourceLocation))
                        {
                            //Read the file header from the stream
                            fileHeaderContent = Exif.ReadStreamUpToExifData(stream);
                        }
                    }
                    catch (Exception ex)
                    {
                        //Catch exceptions and simply retry with a new resource
                        System.Console.WriteLine(ex.Message);
                        if ((bool) isRemote)
                        {
                            System.Console.WriteLine("Check your internet connection or enter a new remote location");
                        }
                        else
                        {
                            System.Console.WriteLine("Enter a new file name and try again");
                        }
                        resourceLocation = System.Console.ReadLine();
                    }
                }

                //Verify that the file header content contains exif data
                if (fileHeaderContent.ExifDataBytes != null && fileHeaderContent.ExifDataBytes.Length > 0)
                {
                    //Create a Exif object from the file's data
                    var exifData = new Exif(fileHeaderContent.ExifDataBytes);
                    //
                    DisplayExifData(exifData);
                }
                else
                {
                    System.Console.WriteLine("Exif data markers not found");

                }

                System.Console.WriteLine();
                System.Console.WriteLine("Press any key to process another file, or Q to exit");
                if (System.Console.ReadKey().Key == ConsoleKey.Q) return;
            }
        }

        private static void DisplayExifData(Exif exifData)
        {
            //Verify that the exif data holds valid information
            if (exifData.IsValid)
            {
                System.Console.WriteLine("Exif data found:");

                //Iterate through each Image File Directory. Commonly used are IDF0 and IDF1
                foreach (var ifd in exifData.ImageFileDirectories)
                {
                    //Display the index of the IDF, whether a thumbnail was found (and its length), and the number of data entries
                    System.Console.WriteLine();
                    System.Console.WriteLine("{0}:", ifd.Name);
                    System.Console.WriteLine("Contains a thumbnail of length: {0}", ifd.ThumbnailData == null ? "N/A" : ifd.ThumbnailData.Length.ToString(CultureInfo.InvariantCulture));
                    System.Console.WriteLine("Contains the following {0} tag{1}", ifd.Entries.Count, ifd.Entries.Count == 1 ? String.Empty : "s");
                    System.Console.WriteLine();

                    //Display all the data entries in the IDF
                    foreach (var entry in ifd.Entries.FindAll(entry => true))
                    {
                        System.Console.WriteLine("  Tag Name:{0}  *  Value ({1}): {2})", entry.TagName, entry.Format, Exif.GetDisplayValue(entry));
                    }
                }
            }
            else
            {
                System.Console.WriteLine("Invalid Exif data block");
                System.Console.WriteLine();
            }
        }
    }
}

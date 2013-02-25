using System;
using System.IO;

namespace Sample.Console
{
    public partial class Program
    {
        private static string GetDefaultSampleImage()
        {
            var version = String.Format("{0}.{1}", Environment.OSVersion.Version.Major, Environment.OSVersion.Version.Minor);

            switch (version)
            {
                case "6.1": // Windows 7
                    var publicFolder = Environment.GetEnvironmentVariable("PUBLIC");

                    if (!String.IsNullOrEmpty(publicFolder))
                        return Path.Combine(publicFolder, @"Pictures\Sample Pictures\Penguins.jpg");
                    break;

                default: // Just don't set anything then...
                    return String.Empty;
            }

            return String.Empty;
        }

        private static string GetDefaultWebSite()
        {
            return @"http://www.exif.org/samples/canon-ixus.jpg";
        }
    }
}
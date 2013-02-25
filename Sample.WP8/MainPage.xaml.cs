using Microsoft.Phone.Tasks;
using QuixifLib;
using System;
using System.Linq;
using System.Text;
using System.Windows;

namespace Sample.WP8
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var photoChooserTask = new PhotoChooserTask();
            photoChooserTask.Completed += PhotoChooserTaskCompleted;
            photoChooserTask.Show();
        }

        private static void PhotoChooserTaskCompleted(object sender, PhotoResult photoResult)
        {
            //Load up exif data from the chosen photo's stream
            var exif = new Exif(Exif.ReadStreamUpToExifData(photoResult.ChosenPhoto, 4096).ExifDataBytes);

            //Iterate through the IFDs and entries, and display the value
            if (exif.IsValid)
            {
                var sb = new StringBuilder();
                foreach (var ifd in exif.ImageFileDirectories.Where(ifd => ifd.Entries.Any(entry => !entry.IsOffset && !entry.IsPadding)))
                {
                    sb.AppendLine(ifd.Name);
                    sb.AppendLine(new string('-', ifd.Name.Length));
                    foreach (var entry in ifd.Entries.Where(entry => !(entry.IsOffset || entry.IsPadding)))
                    {
                        sb.AppendLine(String.Format("{0} : {1}", entry.TagName, Exif.GetDisplayValue(entry)));
                    }
                    sb.AppendLine();
                }
                MessageBox.Show(sb.ToString(), "Quikxif", MessageBoxButton.OK);
            }

            else
            {
                MessageBox.Show("No Exif Data", "Quikxif", MessageBoxButton.OK);
            }
        }
    }
}
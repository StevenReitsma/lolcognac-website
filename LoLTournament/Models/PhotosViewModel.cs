using System.Collections.Generic;
using System.IO;
using System.Web;

namespace LoLTournament.Models
{
    public class PhotosViewModel
    {
        private const string PhotoDirectory = "/Content/Images/Photos";

        public List<Photo> Photos { get; set; }
        public PhotosViewModel()
        {
            Photos = new List<Photo>();

            var files = Directory.EnumerateFiles(HttpRuntime.AppDomainAppPath + PhotoDirectory, "*.jpg", SearchOption.TopDirectoryOnly);

            foreach (var f in files)
            {
                string imageName = Path.GetFileNameWithoutExtension(f);
                string path = PhotoDirectory + '/' + Path.GetFileName(f);
                string thumb = PhotoDirectory + "/Thumb/" + Path.GetFileName(f);

                Photos.Add(new Photo { Name = imageName, Path = path, ThumbnailPath = thumb});
            }
        }
    }
}
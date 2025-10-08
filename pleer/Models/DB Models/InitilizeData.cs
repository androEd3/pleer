using pleer.Models.Media;
using pleer.Models.Users;
using System.IO;

namespace pleer.Models.CONTEXT
{
    public static class InitilizeData
    {
        public static void SeedData(DBContext context)
        {
            //Seed covers
            if (!context.AlbumCovers.Any())
            {
                var covers = new List<AlbumCover>()
                {
                    { new() { FilePath = GetDefaultCoverPath() } },
                };
                context.AddRange(covers);
                context.SaveChanges();
            }

            if (!context.PlaylistCovers.Any())
            {
                var pCovers = new List<PlaylistCover>()
                {
                    { new() { FilePath = GetDefaultCoverPath() } },
                    { new() { FilePath = GetFavoritesCoverPath() } },
                };
                context.AddRange(pCovers);
                context.SaveChanges();
            }

            if (!context.ProfilePictures.Any())
            {
                var pPictures = new List<ProfilePicture>()
                {
                    { new() { FilePath = GetDefaultProfilePicturePath() } },
                };
                context.AddRange(pPictures);
                context.SaveChanges();
            }
        }
        
        public static string GetDefaultCoverPath()
        {
            return "pack://application:,,,/Resources/ServiceImages/DefaultCover.png";
        }

        public static string GetDefaultProfilePicturePath()
        {
            return "pack://application:,,,/Resources/ServiceImages/DefaultPicture.png";
        }

        public static string GetFavoritesCoverPath()
        {
            return "pack://application:,,,/Resources/ServiceImages/FavoritesCover.png";
        }
    }

    public class PictureService
    {
        private string _projectProfilePicturesPath;
        private string _projectAlbumCoversPath;
        private string _projectPlaylistCoversPath;

        public PictureService()
        {
            _projectProfilePicturesPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                "pleer",
                "ServiceImages",
                "ProfilePictures");

            _projectAlbumCoversPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                "pleer",
                "ServiceImages",
                "AlbumCovers");

            _projectPlaylistCoversPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                "pleer",
                "ServiceImages",
                "PlaylistCovers");

            Directory.CreateDirectory(_projectProfilePicturesPath);
            Directory.CreateDirectory(_projectAlbumCoversPath);
            Directory.CreateDirectory(_projectPlaylistCoversPath);
        }

        public ProfilePicture SaveProfilePicture(string sourceImagePath, Artist artist, Listener listener)
        {
            try
            {
                if (Uri.TryCreate(sourceImagePath, UriKind.Absolute, out var uri))
                {
                    sourceImagePath = uri.LocalPath;
                }

                string extension = Path.GetExtension(sourceImagePath);
                string fileName = string.Empty;
                if (listener != null)
                    fileName = $"listener_{listener.Id}_{DateTime.Now:yyyyMMdd}{extension}";
                if (artist != null)
                    fileName = $"listener_{artist.Id}_{DateTime.Now:yyyyMMdd}{extension}";
                string destinationPath = Path.Combine(_projectProfilePicturesPath, fileName);

                File.Copy(sourceImagePath, destinationPath, overwrite: true);

                var profilePicture = new ProfilePicture
                {
                    FilePath = destinationPath,
                };

                return profilePicture;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при сохранении изображения: {ex.Message}");
            }
        }

        public AlbumCover SaveAlbumCover(string sourceImagePath, int albumId)
        {
            try
            {
                string extension = Path.GetExtension(sourceImagePath);
                string fileName = $"album_{albumId}{extension}";
                string destinationPath = Path.Combine(_projectAlbumCoversPath, fileName);

                File.Copy(sourceImagePath, destinationPath, overwrite: true);

                var albumCover = new AlbumCover
                {
                    FilePath = destinationPath,
                };

                return albumCover;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при сохранении изображения: {ex.Message}");
            }
        }

        public PlaylistCover SavePlaylistCover(string sourceImagePath, int playlistId)
        {
            try
            {
                string extension = Path.GetExtension(sourceImagePath);
                string fileName = $"playlist_{playlistId}{extension}";
                string destinationPath = Path.Combine(_projectPlaylistCoversPath, fileName);

                File.Copy(sourceImagePath, destinationPath, overwrite: true);

                var playlistCover = new PlaylistCover
                {
                    FilePath = destinationPath,
                };

                return playlistCover;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при сохранении изображения: {ex.Message}");
            }
        }
    }
}

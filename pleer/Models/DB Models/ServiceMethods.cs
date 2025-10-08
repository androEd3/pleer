using Microsoft.EntityFrameworkCore;
using pleer.Models.CONTEXT;
using pleer.Models.Media;
using pleer.Models.Users;
using System.Diagnostics;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace pleer.Models.DB_Models
{
    // Сервисный класс для работы с плейлистами
    public class ServiceMethods
    {
        public static void AddPlaylistWithLink(Listener listener)
        {
            var context = new DBContext();

            try
            {
                var playlistsCount = context.Playlists
                    .Where(p => p.CreatorId == listener.Id)
                    .Count();

                PlaylistCover cover;

                string playlistTitle = "";

                if (playlistsCount == 0)
                {
                    cover = context.PlaylistCovers
                        .First(pc => pc.FilePath == InitilizeData.GetFavoritesCoverPath());
                    playlistTitle = "Избранное";
                }
                else
                {
                    cover = context.PlaylistCovers
                        .First(pc => pc.FilePath == InitilizeData.GetDefaultCoverPath());
                    playlistTitle = $"Плейлист {playlistsCount + 1}";
                }

                if (cover == null)
                {
                    MessageBox.Show("Обложка для плейлиста не найдена", "Медиатека",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var playlist = new Playlist()
                {
                    Title = playlistTitle,
                    CreationDate = DateOnly.FromDateTime(DateTime.Now),
                    CoverId = cover.Id,
                    CreatorId = listener.Id
                };

                context.Playlists.Add(playlist);
                context.SaveChanges();

                var link = new ListenerPlaylistsLink()
                {
                    ListenerId = listener.Id,
                    PlaylistId = playlist.Id
                };

                context.ListenerPlaylistsLinks.Add(link);
                context.SaveChangesAsync();

                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании плейлиста: {ex.Message}", "Медиатека",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static string GetSha256Hash(string input)
        {
            byte[] data = SHA256.HashData(Encoding.UTF8.GetBytes(input));

            StringBuilder sBuilder = new();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        //Input fields proverochki
        public static string IsPasswordsSame(string password, string repeatedPassword)
        {
            if (password != repeatedPassword)
                return "Пароли не совпадают";

            return password;
        }

        public static string IsPasswordsValidOutput(string password)
        {
            if (password.Length < 6)
                return "Пароль должен содержать минимум 6 символов";

            if (password.Length > 32)
                return "Пароль не должен превышать 32 символа";

            bool hasDigit = password.Any(char.IsDigit);
            bool hasLetter = password.Any(char.IsLetter);

            if (!hasDigit)
                return "Пароль должен содержать хотя бы одну цифру";

            if (!hasLetter)
                return "Пароль должен содержать хотя бы одну букву";

            return password;
        }

        public static string IsValidEmailOutput(string email)
        {
            var trimmedEmail = email.Trim();

            try
            {
                var mailAddress = new MailAddress(trimmedEmail);

                if (mailAddress.Address == trimmedEmail &&
                    !trimmedEmail.EndsWith(".") &&
                    trimmedEmail.Contains("@") &&
                    trimmedEmail.IndexOf('@') > 0 &&
                    trimmedEmail.IndexOf('@') < trimmedEmail.Length - 1)
                {
                    return mailAddress.Address;
                }
            }
            catch { }

            return "Неверный формат почты";
        }

        //Site
        public static void OpenLoginBrowser()
        {
            var authUrl = "https://localhost:7021/Home/Index";
            Process.Start(new ProcessStartInfo
            {
                FileName = authUrl,
                UseShellExecute = true
            });
        }
    }
}

using pleer.Models.CONTEXT;
using pleer.Models.Media;
using pleer.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace pleer.Models.DB_Models
{
    // Сервисный класс для работы с плейлистами
    public class ServiceMethods
    {
        public static void AddPlaylistWithLink(User user, DBContext context, bool isDefaultPlaylist)
        {
            if (isDefaultPlaylist)
            {
                try
                {
                    var playlist = new Playlist()
                    {
                        Title = "Избранное",
                        CreationDate = DateOnly.FromDateTime(DateTime.Now),
                        AlbumCoverId = 1, //favorite
                        CreatorId = user.Id
                    };
                    context.Playlists.Add(playlist);
                    context.SaveChanges();

                    var link = new UserPlaylistsLink()
                    {
                        UserId = user.Id,
                        PlaylistId = playlist.Id
                    };
                    context.UserPlaylistsLinks.Add(link);
                    context.SaveChanges();

                    return;
                }
                catch { }
            }
            else
            {
                try
                {
                    var playlistCount = context.Playlists.Count(p => p.CreatorId == user.Id);

                    var playlist = new Playlist()
                    {
                        Title = $"Плейлист {playlistCount}",
                        CreatorId = user.Id,
                        AlbumCoverId = 2, // nomedia
                        CreationDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    };
                    context.Playlists.Add(playlist);
                    context.SaveChanges();

                    var link = new UserPlaylistsLink()
                    {
                        UserId = user.Id,
                        PlaylistId = playlist.Id
                    };
                    context.UserPlaylistsLinks.Add(link);
                    context.SaveChanges();

                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании плейлиста: {ex.Message}", "Медиатека",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
        public static string IsPasswordsValidOutput(string password, string repeatedPassword)
        {
            if (password != repeatedPassword)
                return "Пароли не совпадают";

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

            return "";
        }

        public static string IsValidEmailOutput(string email)
        {
            var trimmedEmail = email.Trim();

            if (!trimmedEmail.EndsWith("."))
            {
                string address = new MailAddress(email).Address;

                if (address == trimmedEmail)
                    return address;
            }

            return "";
        }
    }
}

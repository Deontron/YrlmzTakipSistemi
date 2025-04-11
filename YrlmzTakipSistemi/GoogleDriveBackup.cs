using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System.IO;
using System.Windows;

namespace YrlmzTakipSistemi
{
    class GoogleDriveBackup
    {
        static string[] Scopes = { DriveService.Scope.DriveFile };
        static string ApplicationName = "WPFBackupApp";

        public static void UploadBackup()
        {
            try
            {
                UserCredential credential;
                using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(stream))
                {
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(reader.BaseStream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None).Result;
                }

                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                DeleteOldBackups(service);

                string databasePath = "company_tracking_system.db";
                string backupFileName = "yedek_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".db";

                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = backupFileName,
                    MimeType = "application/x-sqlite3"
                };

                using (var stream = new FileStream(databasePath, FileMode.Open))
                {
                    var request = service.Files.Create(fileMetadata, stream, "application/x-sqlite3");
                    request.Fields = "id";
                    request.Upload();
                }

                MessageBox.Show("Yedekleme tamamlandı!", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void DeleteOldBackups(DriveService service)
        {
            try
            {
                var request = service.Files.List();
                request.Q = "name contains 'yedek_' and mimeType = 'application/x-sqlite3'";
                request.Fields = "files(id, name, modifiedTime)";
                var result = request.Execute();
                List<Google.Apis.Drive.v3.Data.File> files = result.Files.ToList();

                DateTime now = DateTime.UtcNow;
                foreach (var file in files)
                {
                    if (file.ModifiedTimeDateTimeOffset != null)
                    {
                        DateTime fileDate = file.ModifiedTimeDateTimeOffset.Value.UtcDateTime;
                        if ((now - fileDate).TotalDays > 10)
                        {
                            service.Files.Delete(file.Id).Execute();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eski yedekleri silerken hata oluştu:\n" + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}

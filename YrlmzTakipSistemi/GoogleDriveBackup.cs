using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using System;
using System.IO;
using System.Threading;
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
                {
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None).Result;
                }

                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                string databasePath = "company_tracking_system.db"; 
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = "backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".sqlite",
                    MimeType = "application/x-sqlite3"
                };

                using (var stream = new FileStream(databasePath, FileMode.Open))
                {
                    var request = service.Files.Create(fileMetadata, stream, "application/x-sqlite3");
                    request.Fields = "id";
                    request.Upload();
                }

                MessageBox.Show("Yedekleme tamamlandı!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
    }
}

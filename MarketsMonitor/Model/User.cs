using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using MarketsSystem.Annotations;
using MarketsSystem.MarketsDbContext;
using Limilabs.Client.Authentication.Google;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MarketsSystem.Model
{
    public class User: INotifyPropertyChanged
    {
        private string clientID = "308692977850-6atd1a5so718hsb3a14s3dh3ob4ai1sd.apps.googleusercontent.com";
        private string clientSecret = "hCIqiTJiYPdAbKhjMbFQCHc7";


        private TokenResponse _token;
        private DateTime _lastUpdate;
        private string _email;
        private string _emailToSent;

        public string Email
        {
            get => _email;
            set
            {
                if (value == _email) return;
                _email = value;
                OnPropertyChanged();
            }

        }
        public string EmailToSent
        {
            get => _emailToSent;
            set
            {
                if (value == _emailToSent) return;
                _emailToSent = value;
                OnPropertyChanged();
            }

        }
        public DateTime LastUpdate
        {
            get => _lastUpdate;
            set
            {
                if (value == _lastUpdate) return;
                _lastUpdate = value;
                OnPropertyChanged();
            }

        }


        public TokenResponse Token
        {
            get => _token;
            set
            {
                if (value == _token) return;
                _token = value;
                OnPropertyChanged();
            }

        }

        public async void GetAccessToken()
        {
            string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, User.GetRandomUnusedPort());

            var clientSecrets = new ClientSecrets
            {
                ClientId = clientID,
                ClientSecret = clientSecret
            };
            //Create User credential 
            var credential = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = clientSecrets,
                Scopes = new[] { GoogleScope.ImapAndSmtp.Name, GoogleScope.EmailScope.Name }
            });
            var http = new HttpListener();
          
            http.Prefixes.Add(redirectURI);
            http.Start();

            // Creates the OAuth 2.0 authorization request.
            AuthorizationCodeRequestUrl url = credential.CreateAuthorizationCodeRequest(redirectURI);

            // Opens request in the browser.
           System.Diagnostics.Process.Start(url.Build().ToString());
            
            // Waits for the OAuth authorization response.
            var context = await http.GetContextAsync();

            var response = context.Response;

            // extracts the code
            var code = context.Request.QueryString.Get("code");
        
            string responseString = string.Format("<html><head><meta http-equiv='refresh' content='5';url=https://google.com'></head><body><Authorization was successful></body></html>");
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                responseOutput.Close();
                http.Stop();
          
            });
            MainWindow.main.Activate();
        
            string authCode = code;
      
            TokenResponse token = await credential.ExchangeCodeForTokenAsync("", code, redirectURI, CancellationToken.None);
            TokenResponse refreshed = await credential.RefreshTokenAsync("", token.RefreshToken, CancellationToken.None);
           
            //save  refresh Token toSettings
            Properties.Settings.Default.Token = refreshed.AccessToken;
            Properties.Settings.Default.Save();
            //Get user email
            GoogleApi api = new GoogleApi(Properties.Settings.Default.Token);
            Email = api.GetEmailPlus();

        }
   
        public void SetExcelDocuments (MonitorContext monitorContext)
        {
            IQueryable<AboutProducts> productsExport = monitorContext.Product.Select(product => new AboutProducts
            {
                ProductsName = product.Products.ProductsName,
                NameInMarket = product.ProductName,
                Price = product.ProductPrice,
                Shop = product.Markets.Name,
                Href = product.ProductHref

            });
            OfficeOpenXml.ExcelPackage excel = new OfficeOpenXml.ExcelPackage();
            excel.Workbook.Worksheets.Add("Worksheet1");


            var headerRow = new List<string[]>()
            {
                new string[] { "Name","Name in Market", "Market name", "Price", "Href" }
            };

            // Determine the header range 
            string headerRange = "A1:" + Char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

            // Target a worksheet
            var worksheet = excel.Workbook.Worksheets["Worksheet1"];

            // Popular header row data
            worksheet.Cells[headerRange].LoadFromArrays(headerRow);
            // Load result to Excel book
            worksheet.Cells[2, 1].LoadFromCollection(productsExport);


            FileInfo excelFile = new FileInfo("test.xlsx");
            excel.SaveAs(excelFile);
        }
        public void SendEmailwithGoogle(  )
        {

            string accessToken = Properties.Settings.Default.Token;
            GoogleApi api = new GoogleApi(accessToken);
            string user = api.GetEmailPlus();
            MailMessage mail = new MailMessage();
            SaslMechanismOAuth2 oauth2=null;
            MailKit.Net.Smtp.SmtpClient client;
            try {
                
                client = new MailKit.Net.Smtp.SmtpClient();
                client.Connect("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect);
                //Authenticate user with Access Token
                oauth2 = new SaslMechanismOAuth2(user, accessToken);
                client.Authenticate(oauth2);
                    }
            catch
            {
                MessageBox.Show("Authorization Error. Log in to your account.");
                return;
            }
          
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress( user));
            message.To.Add(new MailboxAddress( this._emailToSent));
            message.Subject = "Market Monitoring result";

            // create our message text, just like before
            var body = new TextPart("plain")
            {
                Text = @""
            };

            // create an image attachment for the file located at path
            var attachment = new MimePart()
            {
                Content = new MimeContent(File.OpenRead("test.xlsx"), ContentEncoding.Default),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = Path.GetFileName("test.xlsx")
            };

            //create the multipart container to hold the message text 
           
            var multipart = new Multipart("mixed");
            multipart.Add(body);
            multipart.Add(attachment);
            
            message.Body = multipart;



            client.Send(message);


        }
        public static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        internal class AboutProducts
        {
            public string ProductsName { get; set; }
            public string NameInMarket { get; set; }
            public string Shop { get; set; }
            public int Price { get; set; }
            public string Href { get; set; }

        }
        public event PropertyChangedEventHandler PropertyChanged;
      
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

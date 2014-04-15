using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
namespace AirStream
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CameraCapture cameraCapture;
        private StorageFile file;
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.
            cameraCapture = new CameraCapture();
            captureField.Source = await cameraCapture.Initialize();
            await cameraCapture.StartPreview();
            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private async void captureButton_Click(object sender, RoutedEventArgs e)
        {
            if (cameraCapture != null)
            {
                file = await cameraCapture.CapturePhoto();
                IRandomAccessStream imageStream = (await file.OpenStreamForReadAsync()).AsRandomAccessStream();
                BitmapImage image = new BitmapImage();
                image.SetSource(imageStream);
                previewField.Source = image;
                await cameraCapture.StopPreview();
                captureField.Source = null;
                cameraCapture.Dispose();
            }
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            if (file != null)
            {
                ShowPhoto(file.Path);
            }
        }

        private async void ShowPhoto(String filePath)
        {
            Stream stream = await file.OpenStreamForReadAsync();
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] bStream = new byte[1024];
                int len = -1;
                while ((len = stream.Read(bStream, 0, 1024)) > 0)
                {
                    ms.Write(bStream, 0, len);
                } 
                ms.Position = 0;
                ShowPhoto(ms.ToArray());
            }
        }

        private async void ShowPhoto(byte[] photo)
        {
            var appleTvLink = @"http://192.168.1.118:7000/photo";
            HttpClient client = new HttpClient();
            client.MaxResponseContentBufferSize = 256000;
            client.DefaultRequestHeaders.Add("User-Agent", "MediaControl/1.0");
            client.DefaultRequestHeaders.Add("X-Apple-Session-ID", (new Guid()).ToString());

            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, appleTvLink);
            req.Content = new ByteArrayContent(photo);
            var result = await client.SendAsync(req);
        }
    }
}

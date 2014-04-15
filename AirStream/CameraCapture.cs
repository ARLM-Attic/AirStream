using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Storage;

namespace AirStream
{
    class CameraCapture : IDisposable
    {
        MediaCapture mediaCapture;
        ImageEncodingProperties imgEncodingProperties;
        MediaEncodingProfile videoEncodingProperties;

        public VideoDeviceController VideoDeviceController
        {
            get { return mediaCapture.VideoDeviceController; }
        }
        public async Task<MediaCapture> Initialize(CaptureUse primaryUse = CaptureUse.Photo)
        {
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            mediaCapture = new MediaCapture();
            if (devices.Count() > 0)
            {
                await this.mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings { VideoDeviceId = devices.ElementAt(1).Id, PhotoCaptureSource = Windows.Media.Capture.PhotoCaptureSource.VideoPreview });
            }  
            mediaCapture.VideoDeviceController.PrimaryUse = primaryUse;
            mediaCapture.SetPreviewRotation(VideoRotation.Clockwise90Degrees);

            imgEncodingProperties = ImageEncodingProperties.CreateJpeg();
            imgEncodingProperties.Width = 640;
            imgEncodingProperties.Height = 480;

            videoEncodingProperties = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto); 

            return mediaCapture;
        }

        public async Task StartPreview()
        {
            await mediaCapture.StartPreviewAsync();
        }

        public async Task StopPreview()
        {
            await mediaCapture.StopPreviewAsync();
        }

        public async Task<StorageFile> CapturePhoto(string desiredName = "photo.jpg")
        {
            var photoStorageFile = await KnownFolders.PicturesLibrary.CreateFileAsync(desiredName, CreationCollisionOption.GenerateUniqueName);
            await mediaCapture.CapturePhotoToStorageFileAsync(imgEncodingProperties, photoStorageFile);
            return photoStorageFile;
        }

        public void Dispose()
        {
            if (mediaCapture != null)
            {
                mediaCapture.Dispose();
                mediaCapture = null;
            }
        }
    }
}

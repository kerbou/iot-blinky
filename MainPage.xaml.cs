// Copyright (c) Microsoft. All rights reserved.

using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.Devices.Gpio;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Blinky
{
    public sealed partial class MainPage : Page
    {        
        public MainPage()
        {
            InitializeComponent();
            DispatcherTimer t = new DispatcherTimer();
            t.Interval = TimeSpan.FromSeconds(60 * 1);
            t.Tick += Timer_Tick;
            t.Start();                 
        }

        private void Timer_Tick(object sender, object e)
        {
            TakePhoto();
        }

        private async void TakePhoto()
        {
            try
            {
                //var filename = DateTime.Now.ToString();
                var filename = DateTime.Now.ToString("o").Replace(":", "-") + ".jpg";
                var photoFile = await KnownFolders.PicturesLibrary.CreateFileAsync(
                                filename, CreationCollisionOption.GenerateUniqueName);
                ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();

                var mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();
                await mediaCapture.CapturePhotoToStorageFileAsync(imageProperties, photoFile);

                var sampleFile = await Package.Current.InstalledLocation.GetFileAsync("Assets\\dropbox.secret");
                var secret = await Windows.Storage.FileIO.ReadTextAsync(sampleFile);

                using (IRandomAccessStream photoStream = await photoFile.OpenReadAsync())
                {
                    using (var dbx = new DropboxClient(secret))
                    { 
                        var updated = await dbx.Files.UploadAsync(
                            "/" + filename,
                            WriteMode.Overwrite.Instance,
                            body: photoStream.AsStream());

                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        
    }
}

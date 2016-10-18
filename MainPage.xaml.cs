// Copyright (c) Microsoft. All rights reserved.

using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.IO;
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
        private const int LED_PIN = 5;
        private GpioPin _pin;
        private GpioPinValue _pinValue;
        private DispatcherTimer _timer;
        private SolidColorBrush _redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush _grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);

        public MainPage()
        {
            InitializeComponent();


            //InitGPIO();
            takePhoto();
                   
        }

        private async void takePhoto()
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

                using (IRandomAccessStream photoStream = await photoFile.OpenReadAsync())
                {
                    using (var dbx = new DropboxClient("foobar"))
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

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                _pin = null;
                GpioStatus.Text = "There is no GPIO controller on this device.";
                return;
            }

            _pin = gpio.OpenPin(LED_PIN);
            _pinValue = GpioPinValue.High;
            _pin.Write(_pinValue);
            _pin.SetDriveMode(GpioPinDriveMode.Output);

            GpioStatus.Text = "GPIO pin initialized correctly.";

        }

   




        private void Timer_Tick(object sender, object e)
        {
            takePhoto();

            if (_pinValue == GpioPinValue.High)
            {
                _pinValue = GpioPinValue.Low;
                _pin.Write(_pinValue);
                LED.Fill = _redBrush;
            }
            else
            {
                _pinValue = GpioPinValue.High;
                _pin.Write(_pinValue);
                LED.Fill = _grayBrush;
            }


        }
             

    }
}

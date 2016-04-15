using Lesson_203;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IoTSmartGarden
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        BMP280 BMP280;
        public Timer timer;
        DeviceClient deviceClient = null;

        public MainPage()
        {
            this.InitializeComponent();
            InitializeDevice();
        }

        private void InitializeDevice()
        {
            deviceClient = DeviceClient.CreateFromConnectionString("HostName=dw-iothub-dev-s1.azure-devices.net;DeviceId=MyNewDevice;SharedAccessKey=WFyyVdkUvtA2hMupKwcG89NU2CvJ1CtPpF91vvrjE7Q=", TransportType.Http1);
            //await timerCallback(null);

        }

        private async Task SendDataToAzure(float temperature, float pressure)
        {
            var telemetryDataPoint = new
            {
                deviceId = "1",
                //windSpeed = currentWindSpeed,
                DateTime.Now,
                temperature,
                //Humidity,
                pressure
            };
            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            await deviceClient.SendEventAsync(message);
            //await deviceClient.SendEventAsync(msg);
        }

        public async static Task ReceiveDataFromAzure()
        {
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString("<replace>", TransportType.Http1);

            Message receivedMessage;
            string messageData;

            while (true)
            {
                receivedMessage = await deviceClient.ReceiveAsync();

                if (receivedMessage != null)
                {
                    messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                    await deviceClient.CompleteAsync(receivedMessage);
                }
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            BMP280 = new BMP280();
            //Initialize the sensor
            await BMP280.Initialize();

            timer = new Timer(timerCallback, this, 0, 15000);
            //await timerCallback(null);

        }

        private async void timerCallback(object state)
        {
            try
            {
                //Create a new object for our barometric sensor class
                

                //Create variables to store the sensor data: temperature, pressure and altitude. 
                //Initialize them to 0.
                float temp = 0;
                float pressure = 0;
                float altitude = 0;

                //Create a constant for pressure at sea level. 
                //This is based on your local sea level pressure (Unit: Hectopascal)
                const float seaLevelPressure = 1013.25f;
                    
                //Read 10 samples of the data
                //for (int i = 0; i < 5; i++)
                {
                    temp = await BMP280.ReadTemperature();
                    pressure = await BMP280.ReadPreasure();
                    altitude = await BMP280.ReadAltitude(seaLevelPressure);

                    //Write the values to your debug console
                    //Debug.WriteLine("Temperature: " + temp.ToString() + " deg C");
                    //Debug.WriteLine("Pressure: " + pressure.ToString() + " Pa");
                    //Debug.WriteLine("Altitude: " + altitude.ToString() + " m");
                }
                
                await SendDataToAzure(temp, pressure);
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(ex.Message);
            }
        }
    }
}

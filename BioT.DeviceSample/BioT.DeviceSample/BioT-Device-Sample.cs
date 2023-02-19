using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace BioT.DeviceSample
{
    public class DeviceSample
    {
        public async void Play()
        {
            var environmentType = BioT.DeviceSample.configuration.AppConfig.ENVIRONMENT_TYPE;

            var mqttEndpoint = BioT.DeviceSample.configuration.AppConfig.MQTT_ENDPOINT;
            var httpBaseEndpoint = BioT.DeviceSample.configuration.AppConfig.HTTP_BASE_ENDPOINT;
            var mqttPort = BioT.DeviceSample.configuration.AppConfig.MQTT_PORT;
            var clientId = BioT.DeviceSample.configuration.AppConfig.CLIENT_ID;
            var certPass = BioT.DeviceSample.configuration.AppConfig.CERT_PASS;
            var certSubFolder = BioT.DeviceSample.configuration.AppConfig.CERT_SUBFOLDER;
            var caCertificateName = BioT.DeviceSample.configuration.AppConfig.CA_CERTIFICATE_NAME;
            var clientCertificateName = BioT.DeviceSample.configuration.AppConfig.CLIENT_CERTIFICATE_NAME;
            var clientCertificatePfxName = BioT.DeviceSample.configuration.AppConfig.CLIENT_CERTIFICATE_PFX_NAME;
            var clientPrivateKeyName = BioT.DeviceSample.configuration.AppConfig.CLIENT_PRIVATE_KEY_NAME;
            
            var assetsSubFolder = BioT.DeviceSample.configuration.AppConfig.ASSETS_SUBFOLDER;
            var assetsSampleFile = BioT.DeviceSample.configuration.AppConfig.ASSETS_SAMPLE_FILE;

            var baseEndpoint = BioT.DeviceSample.configuration.AppConfig.HTTP_BASE_ENDPOINT;

            //certificates Path
            var certificatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, certSubFolder);
            var caCertPath = Path.Combine(certificatesPath, caCertificateName);
            var caCert = X509Certificate.CreateFromCertFile(caCertPath);
            var deviceCertPath = Path.Combine(certificatesPath, clientCertificatePfxName);
            var deviceCert = new X509Certificate(deviceCertPath, certPass);

            // Create a new BioT MQTT client
            BiotMqttClient mqttClient = new BiotMqttClient(mqttEndpoint, mqttPort, true, caCert, deviceCert, clientId);            
            mqttClient.Connect();

            // Getting Remote Configuration 
            // https://docs.biot-med.com/docs/remote-configuration     
            mqttClient.SubscribeToConfigurationChanges();
            
            // Getting a device token
            //https://docs.biot-med.com/docs/device-api-access
            mqttClient.SubscribeToGetDeviceToken();
            mqttClient.PublishToGetToken();

            // Example how to publish BootTime when device connects
            // https://docs.biot-med.com/docs/sending-statuses-measurements
            PublishDeviceBootTimeStatus(mqttClient);
            
            // Example for invoking HTTP Endpoints
            var deviceId = clientId.Replace($"{environmentType}_", string.Empty);
            BiotHttpClient httpClient = new BiotHttpClient(baseEndpoint, deviceId);           
            while (mqttClient.accessToken == string.Empty)
            {
                System.Threading.Thread.Sleep(1000); // test - untill the token arrives.
                System.Console.WriteLine($"No token yet. Token: {mqttClient.accessToken}");
            }

            // Uploading a file to BioT (3 Steps)
            // https://docs.biot-med.com/docs/file-upload 
            var endpoint = httpBaseEndpoint + "/file/v1/files/upload";
            var fileFullName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assetsSubFolder,assetsSampleFile);
            
            // Step 1, Step2: Get Upload URL and Upload a File
            // https://docs.biot-med.com/docs/file-upload#step-1-get-upload-url
            // https://docs.biot-med.com/docs/file-upload#step-2-upload-the-file
            var fileId = await httpClient.SubmitUploadFile(endpoint, mqttClient.accessToken, assetsSampleFile, fileFullName);
            System.Console.WriteLine($"fileId: {fileId}");
            
            // Step 3: Attaching the file to an existing entity
            // https://docs.biot-med.com/docs/file-upload#step-3-attach-the-file-to-an-existing-entity
            PatchAttachFileToDevice(httpClient, httpBaseEndpoint, mqttClient.accessToken, "device_configuration_general", fileId);
        }

        private void PublishDeviceBootTimeStatus(BiotMqttClient mqttClient)
        {
            var epochUtcTime = (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
            var bootUtcTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            var message = "{" + 
                "\"metadata\": {" +
                     "\"timestamp\": \"" + epochUtcTime + "\"," +
                "}," +
                "\"data\": {" +
                    "\"_bootTime\": \"" + bootUtcTime + "\"," +
                "}}";
            
            mqttClient.PublishDeviceStatus(message);
        }

        private static void PatchAttachFileToDevice(BiotHttpClient httpClient, string baseEndpoint, string accessToken, string generalFieldName, string fileId)
        {            
            var endpoint = baseEndpoint + "/device/v2/devices/{id}";

            var body = "{" + 
                "\"" + generalFieldName + "\": {" +
                    "\"id\": \"" + fileId + "\"," +
                "}}";

            Task<string> task = httpClient.BiotAuthorizedHttpClientPatchAsync(endpoint, accessToken, body, fileId);
        }
    }
}

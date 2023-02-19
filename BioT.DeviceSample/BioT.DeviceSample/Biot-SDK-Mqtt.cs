using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using BioT.DeviceSample.configuration;
using Newtonsoft.Json.Linq;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace BioT.DeviceSample
{
    public class BiotMqttClient
    {
        private MqttClient client;
        private string clientId;
        public string accessToken = string.Empty;

    
        public BiotMqttClient(string mqttEndpoint, int mqttPort, bool v, X509Certificate caCert, X509Certificate deviceCert, string clientId)
        {
            client = new MqttClient(mqttEndpoint, mqttPort, true, caCert, deviceCert, MqttSslProtocols.TLSv1_2);
            this.clientId = clientId;
            
            //Event Handler Wiring
            client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
            client.MqttMsgSubscribed += Client_MqttMsgSubscribed;
            client.ConnectionClosed += Client_MqttDisconnected;
        }

        public void Connect()
        {
            client.Connect(clientId);
            Console.WriteLine($"Connected to AWS IoT with client id: {clientId}.");
        }

        private void Client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            Console.WriteLine($"successfully subscribed to the AWS IoT topic {e.MessageId}");
        }
        
        private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            Console.WriteLine($"Message recieved from topic '{e.Topic}' with data: " + JObject.Parse(Encoding.UTF8.GetString(e.Message)));
            var clientId = BioT.DeviceSample.configuration.AppConfig.CLIENT_ID; 
            if (e.Topic.Equals(AppConfig.GetMqttClientIdToDeviceTokenTopic(clientId))){ 
                Console.WriteLine("Fetching JWT Token");
                accessToken = FindJWTTokenInMessage(Encoding.UTF8.GetString(e.Message));
                System.Console.WriteLine($"Token: {accessToken}");
            } 
            else if (e.Topic.Equals(AppConfig.GetMqttConfigUpdateDeltaTopic(clientId))){ 
                Console.WriteLine("Device Configuration Changed");
            }
        }

        private static void Client_MqttDisconnected(object sender, EventArgs e)
        {
            Console.WriteLine($"Client Disconnected");
        }

        public void PublishToGetToken()
        {
            var fromDeviceTokenTopic = BioT.DeviceSample.configuration.AppConfig.GetMqttClientIdFromDeviceTokenTopic(client.ClientId); 
            var message = "{}";
   
            client.Publish(fromDeviceTokenTopic, Encoding.UTF8.GetBytes(message));
            
            Console.WriteLine($"Published: topic: {fromDeviceTokenTopic}, message: {message}");
        }

        public void PublishDeviceStatus(string message)
        {
            var fromDeviceStatusTopic = BioT.DeviceSample.configuration.AppConfig.GetMqttClientIdFromDeviceStatusTopic(client.ClientId); 
            client.Publish(fromDeviceStatusTopic, Encoding.UTF8.GetBytes(message));
            
            Console.WriteLine($"Published: topic: {fromDeviceStatusTopic} message: {message}");
        }

        // Example how to set device status for attaching a file
        // https://docs.biot-med.com/docs/file-upload
        // **** This Method is not being used - found a bug in Core team  *******
        // public static void PublishDeviceStatusAttachFile(MqttClient client, string fileId)
        // {
        //     var fromDeviceStatusTopic = BioT.DeviceSample.configuration.AppConfig.GetMqttClientIdFromDeviceStatusTopic(client.ClientId); 
        //     var epochUtcTime = (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
        //     var message = "{" + 
        //         "\"metadata\": {" +
        //              "\"timestamp\": \"" + epochUtcTime + "\"," +
        //         "}," +
        //         "\"data\": {" +
        //             "\"device_configuration\": {" +
        //                 "\"id\": \"" + fileId + "\"," +
        //         "}," +
        //         "}}";
        //     client.Publish(fromDeviceStatusTopic, Encoding.UTF8.GetBytes(message));
        //     Console.WriteLine($"Published: topic: {fromDeviceStatusTopic} message: {message}");
        // }

        private static string FindJWTTokenInMessage(string message)
        {
            var data = JObject.Parse(message);
            return data.SelectToken(
                "data.accessJwt.token").Value<string>();
        }

        public void SubscribeToGetDeviceToken()
        {
            var toDeviceTokenTopic = BioT.DeviceSample.configuration.AppConfig.GetMqttClientIdToDeviceTokenTopic(client.ClientId); 
            client.Subscribe(new string[] { toDeviceTokenTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
            System.Console.WriteLine($"{client.ClientId} subscribed to topic {toDeviceTokenTopic}");
        }
                     
        public void SubscribeToConfigurationChanges()
        {
            var iotConfigUpdateDeltaTopic = BioT.DeviceSample.configuration.AppConfig.GetMqttConfigUpdateDeltaTopic(client.ClientId); 
            client.Subscribe(new string[] { iotConfigUpdateDeltaTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
            System.Console.WriteLine($"{client.ClientId} subscribed to topic {iotConfigUpdateDeltaTopic}");
        }
  }
}


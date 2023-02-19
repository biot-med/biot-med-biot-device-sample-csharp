using System.Configuration;
using System.IO;
using System;

namespace BioT.DeviceSample.configuration
{
    public static class AppConfig
    {

        // GENERAL CONFIGURATION
        public static string ENVIRONMENT_TYPE = ConfigurationManager.AppSettings["environment-type"];
        public static string ASSETS_SUBFOLDER = ConfigurationManager.AppSettings["assets-subfolder"];
        public static string ASSETS_SAMPLE_FILE = ConfigurationManager.AppSettings["assets-sample-file"];
        private const string thingNameParameter = "{clientId}";


        // MQTT GENERAL CONFIGURATION
        public static string MQTT_ENDPOINT = ConfigurationManager.AppSettings["mqtt-endpoint"];
        public static int MQTT_PORT =  Int32.Parse(ConfigurationManager.AppSettings["mqtt-port"]);
        public static string CLIENT_ID = ConfigurationManager.AppSettings["client-id"];
        public static string CERT_PASS = ConfigurationManager.AppSettings["cert-pass"];
        public static string CERT_SUBFOLDER = ConfigurationManager.AppSettings["cert-subfolder"];
        public static string CA_CERTIFICATE_NAME = ConfigurationManager.AppSettings["ca-certificate-name"];
        public static string CLIENT_CERTIFICATE_NAME = ConfigurationManager.AppSettings["client-certificate-name"];
        public static string CLIENT_CERTIFICATE_PFX_NAME = ConfigurationManager.AppSettings["client-certificate-pfx-name"];
        public static string CLIENT_PRIVATE_KEY_NAME = ConfigurationManager.AppSettings["client-private-key-name"];

        // MQTT ENDPOINT
        private static string MQTT_CLIENTID_FROM_DEVICE_TOKEN_TOPIC = ConfigurationManager.AppSettings["mqtt-clientid-from-device-token-topic"];
        private static string MQTT_CLIENTID_TO_DEVICE_TOKEN_TOPIC = ConfigurationManager.AppSettings["mqtt-clientid-to-device-token-topic"];
        private static string MQTT_CLIENTID_TO_DEVICE_STATUS_TOPIC = ConfigurationManager.AppSettings["mqtt-clientid-from-device-status-topic"];
        private static string MQTT_CONFIG_UPDATE_DELTA_TOPIC = ConfigurationManager.AppSettings["mqtt-configuration-update-delta-topic"];

        // HTTP GENERAL CONFIGURATION
        public static string HTTP_BASE_ENDPOINT = ConfigurationManager.AppSettings["http-base-endpoint"];

        public static string GetCaCertificationPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CERT_SUBFOLDER);
        }
        public static string GetMqttClientIdFromDeviceTokenTopic(string clientId) // clientId = thingName
        {
            return MQTT_CLIENTID_FROM_DEVICE_TOKEN_TOPIC.Replace(thingNameParameter, clientId);
        }
        public static string GetMqttClientIdToDeviceTokenTopic(string clientId) // clientId = thingName
        {
            return MQTT_CLIENTID_TO_DEVICE_TOKEN_TOPIC.Replace(thingNameParameter, clientId);
        }
        public static string GetMqttClientIdFromDeviceStatusTopic(string clientId) // clientId = thingName
        {
            return MQTT_CLIENTID_TO_DEVICE_STATUS_TOPIC.Replace(thingNameParameter, clientId);
        }
        public static string GetMqttConfigUpdateDeltaTopic(string clientId) // clientId = thingName
        {
            return MQTT_CONFIG_UPDATE_DELTA_TOPIC.Replace(thingNameParameter, clientId);
        }
    }
}
    
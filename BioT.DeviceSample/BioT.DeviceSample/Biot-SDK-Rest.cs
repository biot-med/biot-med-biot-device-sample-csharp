using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace BioT.DeviceSample
{
  public class BiotHttpClient
  {
    private string deviceId;
    private string baseEndpoint;

    public BiotHttpClient(string baseEndpoint, string clientId)
    {
      this.deviceId = clientId;
      this.baseEndpoint = baseEndpoint;
    }

    public async Task<string> SubmitUploadFile(string url, string jwtToken, string fileName, string filePath)
    {
        var message = await GetUploadUrl(url, jwtToken, fileName);
        var signedUrl = FindSignedUrlInMessage(message);
        var fileId = FindFileIdInMessage(message);

        System.Console.WriteLine($"signedUrl: {signedUrl}");
        System.Console.WriteLine($"fileId: {fileId}");
        
        var result = await UploadFile(signedUrl, fileId, fileName, filePath);  
        return fileId;
    }

        public async Task <string> GetUploadUrl(string endpoint, string accessToken, string fileName)
        {
            var client = new RestClient(endpoint);
            var request = BuildRequestFromParams(endpoint, Method.Post, accessToken);         
            request.AddParameter("application/json", "{\"name\":\"" + fileName + "\",\"mimeType\":\"application/json\"}", ParameterType.RequestBody);
          
            RestResponse response = await client.ExecuteAsync(request);
            return response.Content;
        }

        public async Task<string> BiotAuthorizedHttpClientPatchAsync(string endpoint, string accessToken, string body, string fileId)
        {
            var parsedEndpoint = endpoint.Replace("{id}",deviceId);
            var client = new RestClient(parsedEndpoint);
            var request = BuildRequestFromParams(parsedEndpoint, Method.Patch, accessToken);         

            // TODO: Refactor this, device_configuration_general should come as Param
            request.AddJsonBody(
            new 
            {
                device_configuration_general = new
                {
                    id = fileId
                }
            }); // AddJsonBody serializes the object automatically

            RestResponse response = await client.ExecuteAsync(request);
            return response.Content;
        }

        public async Task <string> UploadFile(string signUrl, string fileId, string fileName, string filePath)
        {
            var client = new RestClient(signUrl);
            var request = new RestRequest(signUrl, Method.Put);

            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            
            request.AddFile(fileName, filePath);
            System.Console.WriteLine($"File {fileName} was uploaded");

            RestResponse response = await client.ExecuteAsync(request);
            return response.Content;
        }

        private string FindSignedUrlInMessage(string message)
        { 
            var data = JObject.Parse(message);
            return data.SelectToken(
                "signedUrl").Value<string>();
        }

        private string FindFileIdInMessage(string message)
        {
            var data = JObject.Parse(message);
            return data.SelectToken(
                "id").Value<string>();
        }

        private RestRequest BuildRequestFromParams(string endpoint, Method method, string accessToken = null)
        {
            var request = new RestRequest(endpoint, method);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            if (accessToken != null)
            {
                request.AddHeader("Authorization", string.Format("Bearer {0}", accessToken));
            }
            return request; 
        }
    }
}
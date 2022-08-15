using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AppTokenCSharpExample
{
    internal class AppTokenCSharpExample
    {
        private static readonly string EVADAM_SECRET_KEY = "EVADAM_SECRET_KEY"; // Example: Hej2ch71kG2kTd1iIUDZFNsO5C1lh5Gq - Please don't forget to change when switching to production
        private static readonly string EVADAM_APP_TOKEN = "EVADAM_APP_TOKEN";  // Example: uY0CgwELmgUAEyl4hNWxLngb.0WSeQeiYny4WEqmAALEAiK2qTC96fBad - Please don't forget to change when switching to production
        private static readonly string EVADAM_TEST_BASE_URL = "https://api.evadam.io";

        private static void Main(string[] args)
        {


            // Create an applicant
            string externalUserId = $"USER_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            string applicantId = CreateApplicant(externalUserId).Result.id;

            // Get Applicant Status
            var getApplicantResult = GetApplicantStatus(applicantId).Result;
            Console.WriteLine("Applicant status (json string): " + ContentToString(getApplicantResult.Content));

            // Important: please keep this line as async tasks that end unexpectedly will close console window before showing the error.
            Console.ReadLine();
        }

        public static async Task<Applicant> CreateApplicant(string externalUserId, string levelName)
        {
            Console.WriteLine("Creating an applicant...");

            var body = new
            {
                externalUserId = externalUserId
            };

            // Create the request body
            var requestBody = new HttpRequestMessage(HttpMethod.Post, EVADAM_TEST_BASE_URL)
            {
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };

            // Get the response
            var response = await SendPost($"/api/session/createSession", requestBody);
            var applicant = JsonConvert.DeserializeObject<Applicant>(ContentToString(response.Content));

            Console.WriteLine(response.IsSuccessStatusCode
                ? $"The applicant was successfully created: {applicant.id}"
                : $"ERROR: {ContentToString(response.Content)}");

            return applicant;
        }



        public static async Task<HttpResponseMessage> GetApplicantStatus(string applicantId)
        {
            Console.WriteLine("Getting the applicant status...");

            var response = await SendGet($"/api/applicants/{applicantId}/");
            return response;
        }

        private static async Task<HttpResponseMessage> SendPost(string url, HttpRequestMessage requestBody)
        {

            var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var signature = CreateSignature(ts, HttpMethod.Post, url, RequestBodyToBytes(requestBody));

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var client = new HttpClient
            {
                BaseAddress = new Uri(EVADAM_TEST_BASE_URL)
            };
            client.DefaultRequestHeaders.Add("X-App-Token", EVADAM_APP_TOKEN);
            client.DefaultRequestHeaders.Add("X-App-Access-Sig", signature);
            client.DefaultRequestHeaders.Add("X-App-Access-Ts", ts.ToString());

            var response = await client.PostAsync(url, requestBody.Content);

            if (!response.IsSuccessStatusCode)
            {
                // Then perhaps you should throw the exception. (depends on the logic of your code)
            }

            // debug
            //var debugInfo = response.Content.ReadAsStringAsync().Result;
            return response;
        }

        private static async Task<HttpResponseMessage> SendGet(string url)
        {
            long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var client = new HttpClient
            {
                BaseAddress = new Uri(EVADAM_TEST_BASE_URL)
            };
            client.DefaultRequestHeaders.Add("X-App-Token", EVADAM_APP_TOKEN);
            client.DefaultRequestHeaders.Add("X-App-Access-Sig", CreateSignature(ts, HttpMethod.Get, url, null));
            client.DefaultRequestHeaders.Add("X-App-Access-Ts", ts.ToString());

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                // Then perhaps you should throw the exception. (depends on the logic of your code)
            }

            return response;
        }

        private static string CreateSignature(long ts, HttpMethod httpMethod, string path, byte[] body)
        {
            Console.WriteLine("Creating a signature for the request...");

            var hmac256 = new HMACSHA256(Encoding.ASCII.GetBytes(EVADAM_SECRET_KEY));

            byte[] byteArray = Encoding.ASCII.GetBytes(ts + httpMethod.Method + path);

            if (body != null)
            {
                // concat arrays: add body to byteArray
                var s = new MemoryStream();
                s.Write(byteArray, 0, byteArray.Length);
                s.Write(body, 0, body.Length);
                byteArray = s.ToArray();
            }

            var result = hmac256.ComputeHash(
                new MemoryStream(byteArray)).Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);

            return result;
        }

        private static string ContentToString(HttpContent httpContent)
        {
            return httpContent == null ? "" : httpContent.ReadAsStringAsync().Result;
        }

        private static byte[] RequestBodyToBytes(HttpRequestMessage requestBody)
        {
            return requestBody.Content == null ? 
                new byte[] { } : requestBody.Content.ReadAsByteArrayAsync().Result;
        }
    }
}

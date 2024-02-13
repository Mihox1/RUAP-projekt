// .NET Framework 4.7.1 or greater must be used

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CallRequestResponseService
{
    class Program
    {
        static void Main(string[] args)
        {
            InvokeRequestResponseService().Wait();
        }

        static async Task InvokeRequestResponseService()
        {
            var handler = new HttpClientHandler()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                        (httpRequestMessage, cert, cetChain, policyErrors) => { return true; }
            };
            using (var client = new HttpClient(handler))
            {
                // Request data goes here
                // The example below assumes JSON formatting which may be updated
                // depending on the format your endpoint expects.
                // More information can be found here:
                // https://docs.microsoft.com/azure/machine-learning/how-to-deploy-advanced-entry-script
                var requestBody = @"{
                  ""Inputs"": {
                    ""input1"": [
                      {
                        ""Airline"": ""CO"",
                        ""Flight"": 269,
                        ""AirportFrom"": ""SFO"",
                        ""AirportTo"": ""IAH"",
                        ""DayOfWeek"": 3,
                        ""Time"": 15,
                        ""Length"": 205,
                        ""Delay"": 1
                      },
                      {
                        ""Airline"": ""US"",
                        ""Flight"": 1558,
                        ""AirportFrom"": ""PHX"",
                        ""AirportTo"": ""CLT"",
                        ""DayOfWeek"": 3,
                        ""Time"": 15,
                        ""Length"": 222,
                        ""Delay"": 1
                      },
                      {
                        ""Airline"": ""AA"",
                        ""Flight"": 2400,
                        ""AirportFrom"": ""LAX"",
                        ""AirportTo"": ""DFW"",
                        ""DayOfWeek"": 3,
                        ""Time"": 20,
                        ""Length"": 165,
                        ""Delay"": 1
                      }
                    ]
                  },
                  ""GlobalParameters"": {}
                }";
                
                // Replace this with the primary/secondary key or AMLToken for the endpoint
                const string apiKey = "FOGZRVBJPYYU3G1afSihdsjc6TzNUAhg";
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new Exception("A key should be provided to invoke the endpoint");
                }
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", apiKey);
                client.BaseAddress = new Uri("http://679c1631-7f11-4b9e-8236-e0f0f73a3da9.italynorth.azurecontainer.io/score");

                var content = new StringContent(requestBody);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                // WARNING: The 'await' statement below can result in a deadlock
                // if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false)
                // so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)
                HttpResponseMessage response = await client.PostAsync("", content);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Result: {0}", result);
                }
                else
                {
                    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp,
                    // which are useful for debugging the failure
                    Console.WriteLine(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                }
            }
        }
    }
}
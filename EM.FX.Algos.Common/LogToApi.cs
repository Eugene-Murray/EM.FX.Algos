using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace EM.FX.Algos.Common
{
    public class LogToApi
    {
        //https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/calling-a-web-api-from-a-net-client

        static HttpClient client = new HttpClient();

        //static async Task<Uri> CreateProductAsync(Log log)
        //{
        //    var httpContent = new StringContent(log, Encoding.UTF8, "application/json");

        //    HttpResponseMessage response = await client.PostAsync(
        //        "api/log", log);
        //    response.EnsureSuccessStatusCode();

        //    // return URI of the created resource.
        //    return response.Headers.Location;
        //}

    }
}

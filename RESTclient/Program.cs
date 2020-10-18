using System;
using RestSharp;
using Newtonsoft.Json;
namespace StockClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new RestClient("https://stockserver20201009223011.azurewebsites.net/");
            var request = new RestRequest("stockexchanges", DataFormat.Json);
            var responseJson = client.Get(request);
            string[] stockExchanges = JsonConvert.DeserializeObject<string[]>(responseJson.Content);
            foreach (string stockExchange in stockExchanges)
                Console.WriteLine(stockExchange);
            Console.ReadKey();
        }
    }
}

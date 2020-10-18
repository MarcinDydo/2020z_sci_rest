using System;
using RestSharp;
using Newtonsoft.Json;
using System.Net.Http;
using RestSharp.Authenticators;

namespace StockClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new RestClient("https://stockserver20201009223011.azurewebsites.net/");
            client.Authenticator = new HttpBasicAuthenticator("01149354@pw.edu.pl", "sci2020");

            var request = new RestRequest("client", Method.GET, DataFormat.Json);

            var response = client.Execute(request);
            Console.WriteLine(response.Content);
            /*
            string[] stockExchanges = JsonConvert.DeserializeObject<string[]>(response.Content);
            foreach (string stockExchange in stockExchanges)
                Console.WriteLine(stockExchange);
            */
            Console.ReadKey();
        }
    }
}

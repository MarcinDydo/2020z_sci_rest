using System;
using RestSharp;
using Newtonsoft.Json;
using RestSharp.Authenticators;
using System.Collections.Generic;

namespace StockClient
{
    class Program
    {
        public class Company 
        {
            public int time { get; set; }
            public string buySell { get; set; }
            public double price { get; set; }
            public int amount { get; set; }
        }

        static string[] requestStock(RestClient c)
        {
            var request = new RestRequest("stockexchanges", Method.GET, DataFormat.Json);

            var response = c.Execute(request);
            string[] stockExchanges = JsonConvert.DeserializeObject<string[]>(response.Content);
            return stockExchanges;
        }

        static string[] requestShares(string stock, RestClient c)
        {
            var request = new RestRequest("shareslist/"+stock, Method.GET, DataFormat.Json);
            var response = c.Execute(request);
            return JsonConvert.DeserializeObject<string[]>(response.Content);
        }

        /**
         * result[0] = buy <- za ile chcą kupić
         * result[1] = sell <- za ile sprzedają
         */
        static Company[] requestPrice(string stock, string com, RestClient c)
        {
            var request = new RestRequest("shareprice/" + stock + "?share=" + com, Method.GET, DataFormat.Json);
            var response = c.Execute(request);
            List<Company> list = JsonConvert.DeserializeObject<List<Company>>(response.Content);
            Company[] companies = list.ToArray();
            Console.WriteLine(response.Content);
            return companies;
        }

        static void Offer(string stockExchange, string share, string buySell, double price, int amount, RestClient c)
        {
            var request = new RestRequest("offer", Method.POST, DataFormat.Json);
            request.AddJsonBody(new { stockExchange, share , amount, price, buySell});
            var response = c.Execute(request);
            Console.WriteLine(response.Content);
        }

        static void Main(string[] args)
        {
            var client = new RestClient("https://stockserver20201009223011.azurewebsites.net/");
            client.Authenticator = new HttpBasicAuthenticator("01149354@pw.edu.pl", "sci2020");
            UpdateInfo(client);
            //basic transactions
            int amount = 1;
            string buy, sell; //where we buy where we sell
            //Offer("Warszawa", "CCC", "buy", requestPrice("Warszawa", "CCC", client)[1].price, amount, client);
            //Offer("Warszawa", "CCC", "sell", requestPrice("Warszawa", "CCC", client)[0].price, amount, client);
            string[] vector = System.IO.File.ReadAllLines("/Users/marcin/Projects/RESTclient/RESTclient/stockExchanges.json");
            for(int s =1; s<vector.Length;s++)
            {
                string[] A = System.IO.File.ReadAllLines("/Users/marcin/Projects/RESTclient/RESTclient/Warszawa.json");
                string[] B = System.IO.File.ReadAllLines("/Users/marcin/Projects/RESTclient/RESTclient/" + vector[s] + ".json");
                for (int i = 0; i < B.Length; i++)
                {
                    if (Array.BinarySearch(A, B[i]) >= 0) {
                        Company[] w = requestPrice("Warszawa", B[i], client);
                        Company[] o = requestPrice(vector[s], B[i], client);
                        if (w[1].price < o[1].price) buy = "Warszawa";
                        else buy = vector[s];
                        if (w[0].price > o[0].price) sell = "Warszawa";
                        else sell = vector[s];
                        if (!buy.Equals(sell) && Math.Max(w[0].price,o[0].price)>Math.Min(w[1].price,o[1].price))
                        {
                            Offer(buy, B[i], "buy", Math.Min(w[1].price,o[1].price), amount, client);
                            Offer(sell, B[i], "sell", Math.Max(w[0].price, o[0].price), amount, client);
                        }
                        
                    }
                }
            }


        }

        private static void UpdateInfo(RestClient client)
        {
            //request current stock exchange list -> .json
            System.IO.File.WriteAllLines("/Users/marcin/Projects/RESTclient/RESTclient/stockExchanges.json", requestStock(client));
            //request for each SE companies listing -> .json
            foreach (string s in System.IO.File.ReadAllLines("/Users/marcin/Projects/RESTclient/RESTclient/stockExchanges.json"))
            {
                System.IO.File.WriteAllLines("/Users/marcin/Projects/RESTclient/RESTclient/" + s + ".json", requestShares(s, client));
            }
        }
    }
}

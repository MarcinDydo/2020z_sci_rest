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

        static bool Offer(string stockExchange, string share, string buySell, double price, int amount, RestClient c)
        {
            var request = new RestRequest("offer", Method.POST, DataFormat.Json);
            request.AddJsonBody(new { stockExchange, share , amount, price, buySell});
            var response = c.Execute(request);
            Console.WriteLine(response.Content);
            return (response.Content != "[]" && response.IsSuccessful);
        }

        static void Main(string[] args)
        {
            var client = new RestClient("https://stockserver20201009223011.azurewebsites.net/");
            client.Authenticator = new HttpBasicAuthenticator("01149354@pw.edu.pl", "sci2020");
            UpdateInfo(client);
            int amount = 1;
            string buy, sell; //where we buy where we sell
            while (true)
            {
                string[] vector = System.IO.File.ReadAllLines("/Users/marcin/Projects/RESTclient/RESTclient/stockExchanges.json");
                for (int s = 1; s < vector.Length; s++)
                {
                    string[] A = System.IO.File.ReadAllLines("/Users/marcin/Projects/RESTclient/RESTclient/Warszawa.json");
                    string[] B = System.IO.File.ReadAllLines("/Users/marcin/Projects/RESTclient/RESTclient/" + vector[s] + ".json");
                    for (int i = 0; i < B.Length; i++)
                    {
                        if (Array.BinarySearch(A, B[i]) >= 0)
                        {
                            Company[] w = requestPrice("Warszawa", B[i], client);
                            Company[] o = requestPrice(vector[s], B[i], client);
                            if (w[1].price < o[1].price)
                            {
                                buy = "Warszawa";
                                amount = (int)(0.2 * w[1].amount);
                            }
                            else
                            {
                                buy = vector[s];
                                amount = (int)(0.2 * o[1].amount);
                            }
                            if (w[0].price > o[0].price) sell = "Warszawa";
                            else sell = vector[s];
                            double commision = 0.002 * amount * Math.Max(w[0].price, o[0].price) + 0.002 * amount * Math.Min(w[1].price, o[1].price);
                            double income = amount * (Math.Max(w[0].price, o[0].price) - Math.Min(w[1].price, o[1].price));
                            Console.WriteLine("amount = " + amount + " income =" + income + " commision =" + commision + " profit =" + (income - commision));
                            double buyprice = Math.Min(w[1].price, o[1].price);
                            double sellprice = Math.Max(w[0].price, o[0].price);
                            bool success=false;
                            while (!buy.Equals(sell) && income - commision > 0 && !success)
                            {
                                if (success = Offer(buy, B[i], "buy", buyprice, amount, client)) { Console.WriteLine("bought!"); break; }
                                buyprice = Math.Min(requestPrice("Warszawa", B[i], client)[1].price, requestPrice(vector[s], B[i], client)[1].price);
                            }
                            success = false;
                            while (!buy.Equals(sell) && income - commision > 0 && !success)
                            {
                                if (success = Offer(sell, B[i], "sell", sellprice, amount, client)) { Console.WriteLine("sold!"); break; }
                                sellprice = Math.Max(requestPrice("Warszawa", B[i], client)[0].price, requestPrice(vector[s], B[i], client)[0].price)
                            }

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

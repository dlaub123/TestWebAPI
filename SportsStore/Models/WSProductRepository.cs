using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using RestSharp;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;

// Improvements over book example:
// a) use E-F queries to filter product at the database
// b) use RestSharp to invoke WS from MVC c# client
// c) use Restsharp async (along with c# 5 async)
// d) commit local git repo to server git repo: https://github.com/dlaub123/TestWebAPI

namespace SportsStore.Models
{
    public class WSProductRepository : IRepository
    {
        public IEnumerable<Product> Products 
        { 
            get
            {
                // RESTful WS call 
                var client = new RestClient("http://localhost:6100/");
                var request = new RestRequest("api/products", Method.GET);
                IRestResponse<List<Product>> response = client.Execute<List<Product>>(request);
                IEnumerable<Product> products = response.Data; // a .net object
                return products;
            }
        }
        public Product GetUniqueProduct(int id)
        {
            // RESTful WS call 
            var client = new RestClient("http://localhost:6100/");
            var request = new RestRequest("api/products/{id}", Method.GET);
            request.AddUrlSegment("id", id.ToString()); // replaces matching token in request.Resource
            IRestResponse response = client.Execute(request);
            var content = response.Content;  // a json string
            IRestResponse<Product> response2 = client.Execute<Product>(request);
            Product product = response2.Data; // a .net object
            var asyncHandle = client.ExecuteAsync<Product>(request, response3 =>
            {
                if (response3.ResponseStatus == ResponseStatus.Completed) // redundant but clearer
                {
                    Product product3 = response3.Data;  // this will (correctly) not be executed until the asynch op completes, but this doesn't help with simple return values...
                    int i = 0;
                }
            });
            return product; // does NOT wait for aynch op to complete...a good candidate for c#5 async/await/Task  using System.Threading.Tasks;
            //http://pawel.sawicz.eu/async-and-restsharp/
            //http://stackoverflow.com/questions/12232653/example-of-restsharp-async-client-executeasynct-works
            //http://ianobermiller.com/blog/2012/07/23/restsharp-extensions-returning-tasks/#more-80
        }

        public async Task<string> GetUniqueProductAsync(int id)
        {
            // RESTful WS call 
            var client = new RestClient("http://localhost:6100/");
            var request = new RestRequest("api/products/{id}", Method.GET);
            request.AddUrlSegment("id", id.ToString()); // replaces matching token in request.Resource
            var content = await client.GetContentAsync(request);
            return content; // would obviously prefer returning Product object vs. Json string (nicely mitiaged by simple json to object helper)
        }


        public Task<int> SaveProductAsync(Product product)
        {
            return null;
        }
        public Task<Product> DeleteProductAsync(int productID)
        {
            return null;
        }
        public IEnumerable<Order> Orders 
        {
            get
            {
                return null;
            }
        }
        public Task<int> SaveOrderAsync(Order order)
        {
            return null;
        }
        public Task<Order> DeleteOrderAsync(int orderID)
        {
            return null;
        }
    }

    public static class RestClientExtensions
    {
        private static Task<T> SelectAsync<T>(this RestClient client, IRestRequest request, Func<IRestResponse, T> selector)
        {
            var tcs = new TaskCompletionSource<T>();
            var loginResponse = client.ExecuteAsync(request, r =>
            {
                if (r.ErrorException == null)
                {
                    tcs.SetResult(selector(r));
                }
                else
                {
                    tcs.SetException(r.ErrorException);
                }
            });
            return tcs.Task;
        }

        public static Task<string> GetContentAsync(this RestClient client, IRestRequest request)
        {
            return client.SelectAsync(request, r => r.Content);
        }

        public static Task<IRestResponse> GetResponseAsync(this RestClient client, IRestRequest request)
        {
            return client.SelectAsync(request, r => r);
        }
    }

    public class JSonHelper
    {
        public string ConvertObjectToJSon<T>(T obj)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, obj);
            string jsonString = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            return jsonString;
        }

        public T ConvertJSonToObject<T>(string jsonString)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            T obj = (T)serializer.ReadObject(ms);
            return obj;
        }
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SportsStore.Models;
using System.Threading.Tasks;

namespace SportsStore.Controllers
{
    public class ProductsController : ApiController
    {
        public ProductsController()
        {
            Repository = new ProductRepository();
        }

        // GET /api/products
        public IEnumerable<Product> GetProducts()
        {
            return Repository.Products;
        }
        // GET /api/products/1
        public IHttpActionResult /*Product*/ GetProduct(int id)
        {
            Product result = Repository.GetUniqueProduct(id);  // filter in the database
            //return Repository.Products.Where(p => p.Id == id).FirstOrDefault(); // filter after the database, in this method (could be evaluating millions of rows...)
            //if (result == null)
            //{
            //    throw new HttpResponseException(HttpStatusCode.BadRequest);
            //}
            //else
            //{
            //    return result;
            //}
            return result == null ? (IHttpActionResult)BadRequest("No Product Found") : Ok(result);
        }
        public async Task PostProduct(Product product)
        {
            await Repository.SaveProductAsync(product);
        }

        [Authorize(Roles = "Administrators")]
        public async Task DeleteProduct(int id)
        {
            await Repository.DeleteProductAsync(id);
        }
        private IRepository Repository { get; set; }
    }
}

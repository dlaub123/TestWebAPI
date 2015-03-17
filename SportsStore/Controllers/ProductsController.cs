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
        public Product GetProduct(int id)
        {
            return Repository.GetUniqueProduct(id);  // filter in the database
            //return Repository.Products.Where(p => p.Id == id).FirstOrDefault(); // filter after the database, in this method (could be evaluating millions of rows...)
        }
        public async Task PostProduct(Product product)
        {
            await Repository.SaveProductAsync(product);
        }

        public async Task DeleteProduct(int id)
        {
            await Repository.DeleteProductAsync(id);
        }
        private IRepository Repository { get; set; }
    }
}

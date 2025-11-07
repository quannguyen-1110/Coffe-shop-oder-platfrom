using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using CoffeeShopAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoffeeShopAPI.Repository
{
    public class ProductRepository
    {
        private readonly DynamoDBContext _context;

        public ProductRepository(IAmazonDynamoDB dynamoDb)
        {
            _context = new DynamoDBContext(dynamoDb);
        }

        public async Task AddProductAsync(Product product)
        {
            await _context.SaveAsync(product);
        }

        public async Task<Product?> GetProductByIdAsync(string productId)
        {
            return await _context.LoadAsync<Product>(productId);
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = await _context.ScanAsync<Product>(new List<ScanCondition>()).GetRemainingAsync();
            return products;
        }

        public async Task UpdateProductAsync(Product product)
        {
            await _context.SaveAsync(product);
        }

        public async Task DeleteProductAsync(string productId)
        {
            await _context.DeleteAsync<Product>(productId);
        }
    }
}

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using CoffeeShopAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoffeeShopAPI.Repository
{
    public class VoucherRepository
    {
        private readonly DynamoDBContext _context;

        public VoucherRepository(IAmazonDynamoDB dynamoDb)
        {
            _context = new DynamoDBContext(dynamoDb);
        }

        public async Task AddVoucherAsync(Voucher voucher)
        {
            await _context.SaveAsync(voucher);
        }

        public async Task<Voucher?> GetVoucherByIdAsync(string id)
        {
            return await _context.LoadAsync<Voucher>(id);
        }

        public async Task<List<Voucher>> GetAllVouchersAsync()
        {
            return await _context.ScanAsync<Voucher>(new List<ScanCondition>()).GetRemainingAsync();
        }

        public async Task DeleteVoucherAsync(string id)
        {
            await _context.DeleteAsync<Voucher>(id);
        }
    }
}

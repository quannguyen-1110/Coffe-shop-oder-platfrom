using Microsoft.AspNetCore.Mvc;
using Amazon.S3;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AWSTestController : ControllerBase
    {
        private readonly IAmazonS3 _s3;
        private readonly IAmazonDynamoDB _dynamo;

        public AWSTestController()
        {
            _s3 = new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast1);
            _dynamo = new AmazonDynamoDBClient(Amazon.RegionEndpoint.APSoutheast1);
        }

        [HttpGet("s3")]
        public async Task<IActionResult> ListS3Buckets()
        {
            var response = await _s3.ListBucketsAsync();
            return Ok(response.Buckets.Select(b => b.BucketName));
        }

        [HttpGet("dynamodb")]
        public async Task<IActionResult> ListDynamoTables()
        {
            var response = await _dynamo.ListTablesAsync();
            return Ok(response.TableNames);
        }
    }
}

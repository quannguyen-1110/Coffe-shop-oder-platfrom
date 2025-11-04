using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using System.Reflection;

namespace CoffeeShopAPI.Data
{
    public class DynamoDbService
    {
        private readonly IAmazonDynamoDB _client;
        public DynamoDBContext Context { get; }

        public DynamoDbService()
        {
            _client = new AmazonDynamoDBClient(Amazon.RegionEndpoint.APSoutheast1);
            Context = new DynamoDBContext(_client);

            EnsureAllTablesCreated().Wait();
        }

        private async Task EnsureAllTablesCreated()
        {
            Console.WriteLine("🔍 Checking DynamoDB tables...");

            var existingTables = await _client.ListTablesAsync();

            var modelTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.Namespace == "CoffeeShopAPI.Models" && t.GetCustomAttribute<DynamoDBTableAttribute>() != null)
                .ToList();

            foreach (var type in modelTypes)
            {
                var tableAttr = type.GetCustomAttribute<DynamoDBTableAttribute>();
                var tableName = tableAttr.TableName;

                if (existingTables.TableNames.Contains(tableName))
                {
                    Console.WriteLine($"ℹ️ Table '{tableName}' already exists.");
                    continue;
                }

                var hashKeyProp = type.GetProperties()
                    .FirstOrDefault(p => p.GetCustomAttribute<DynamoDBHashKeyAttribute>() != null);

                if (hashKeyProp == null)
                {
                    Console.WriteLine($"⚠️ Model '{type.Name}' does not have a [DynamoDBHashKey] property — skipping table creation.");
                    continue;
                }

                Console.WriteLine($"⚙️ Creating DynamoDB table '{tableName}'...");

                var createRequest = new CreateTableRequest
                {
                    TableName = tableName,
                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition
                        {
                            AttributeName = hashKeyProp.Name,
                            AttributeType = "S"
                        }
                    },
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement
                        {
                            AttributeName = hashKeyProp.Name,
                            KeyType = "HASH"
                        }
                    },
                    BillingMode = BillingMode.PAY_PER_REQUEST
                };

                var response = await _client.CreateTableAsync(createRequest);

                Console.WriteLine($"✅ Created table '{tableName}', waiting for it to become ACTIVE...");

                // 🕒 Đợi bảng sẵn sàng trước khi tiếp tục
                await WaitForTableActiveAsync(tableName);

                Console.WriteLine($"✅ Table '{tableName}' is ACTIVE and ready to use!");
            }

            Console.WriteLine("✅ DynamoDB initialization complete.");
        }

        private async Task WaitForTableActiveAsync(string tableName)
        {
            string status = null;

            do
            {
                await Task.Delay(5000); // đợi 5 giây
                var response = await _client.DescribeTableAsync(tableName);
                status = response.Table.TableStatus;

                Console.WriteLine($"⏳ Waiting for table '{tableName}' status = {status}...");
            }
            while (status != "ACTIVE");
        }
    }
}

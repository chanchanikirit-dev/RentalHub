using Dapper;
using Npgsql;
using RentalHub.DTO;
using RentalHub.Model;
using System.Data;

namespace RentalHub.Services
{
    public class ItemQueryService : IItemQueryService
    {
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;

        public ItemQueryService(IConfiguration configuration)
        {
            _configuration = configuration;
            _baseUrl = configuration["BaseUrl"];
        }

        private IDbConnection CreateConnection()
            => new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        public async Task<List<Item>> GetActiveItemsAsync()
        {
            const string sql = @"
            SELECT 
                ""itemid"",
                ""itemname"",
                COALESCE(""photourl"", '') AS ""PhotoUrl"",
                LPAD(""itemcode"", 3, '0') AS ""ItemCode""
            FROM ""items""
            WHERE ""isactive"" = TRUE
            ORDER BY ""createddate"" DESC;
        ";


            //using var connection = CreateConnection();
            //connection.Open();

            //var items = (await connection.QueryAsync<Item>(sql)).ToList();

            var items = await ExecuteWithRetry(async () =>
            {
                using var connection = CreateConnection();
                return (await connection.QueryAsync<Item>(sql)).ToList();
            });

            // Post-process image URL (fast)
            items.ForEach(x =>
            {
                if (string.IsNullOrWhiteSpace(x.PhotoUrl))
                    x.PhotoUrl = _baseUrl + "item-images/no-image.svg";
            });

            return items;
        }

        public async Task<bool> UpdateItemsAsync(int id, ItemUpdateDto dto)
        {
            string sql = $@"
            update items set itemname='{dto.ItemName}',photourl='{dto.PhotoUrl}'
            WHERE itemid={id}
            ";

            using var connection = CreateConnection();
            connection.Open();

            var items = (await connection.QueryAsync<int>(sql)).FirstOrDefault();
            return items > 0 ?true : false;
        }
        private async Task<T> ExecuteWithRetry<T>(Func<Task<T>> action)
        {
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    return await action();
                }
                catch when (attempt < 3)
                {
                    await Task.Delay(1000);
                }
            }
            throw new Exception("DB operation failed after retries");
        }

    }
}

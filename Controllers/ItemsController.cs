using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RentalHub.Data;
using RentalHub.DTO;
using RentalHub.Model;
using RentalHub.Services;

namespace RentalHub.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/items")]
    public class ItemsController : ControllerBase
    {
        private readonly RentalHubDbContext _db;
        private readonly SupabaseStorageService _storage;
        private readonly IConfiguration _configuration;
        public string _baseUrl = "";
        private readonly IMemoryCache _cache;
        private readonly IItemQueryService _itemQueryService;
        public ItemsController(RentalHubDbContext db, IConfiguration configuration, SupabaseStorageService storage, IMemoryCache cache, IItemQueryService itemQueryService)
        {
            _db = db;
            _configuration = configuration;
            _baseUrl = _configuration["Supabase:Url"];
            _storage = storage;
            _cache = cache;
            _itemQueryService = itemQueryService;
        }
        // ✅ View all items
        [HttpGet]
        //[Authorize]
        public async Task<IActionResult> GetAll()
        {
           // const string cacheKey = "items_cache";

            //if (_cache.TryGetValue(cacheKey, out List<Item> items))
            //    return Ok(items);

            int retryCount = 0;
            int maxRetries = 3;
            TimeSpan delay = TimeSpan.FromSeconds(1);

            while (retryCount < maxRetries)
            {
                try
                {
                    var items = await _db.Items
                        .AsNoTracking()
                        .Where(x => x.IsActive)
                        .OrderByDescending(x => x.CreatedDate)
                        .Select(x => new Item
                        {
                            ItemId = x.ItemId,
                            ItemName = x.ItemName,
                            PhotoUrl = string.IsNullOrWhiteSpace(x.PhotoUrl)
                                ? _baseUrl + "item-images/no-image.svg"
                                : x.PhotoUrl,
                            ItemCode = x.ItemCode.PadLeft(3, '0')
                        })
                        .ToListAsync();

                    // cache only if success
                    //var cacheOptions = new MemoryCacheEntryOptions()
                    //    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    //    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                    //_cache.Set(cacheKey, items, cacheOptions);

                    return Ok(items);
                }
                catch (Exception ex)
                {
                    retryCount++;

                    if (retryCount >= maxRetries)
                        return StatusCode(500, "Database timeout. Please try again later.");

                    await Task.Delay(delay); // ⏳ wait 5 sec before retry
                }
            }

            return StatusCode(500, "Unexpected error");
        }


        // ✅ Add new item
        [HttpPost]
        public async Task<IActionResult> Create(ItemCreateDto dto)
        {
            try
            {
                int retryCount = 0;
                int maxRetries = 3;
                TimeSpan delay = TimeSpan.FromSeconds(2);

                while (retryCount < maxRetries)
                {
                    try
                    {
                        var item = new Item
                        {
                            ItemCode = dto.ItemCode,
                            ItemName = dto.ItemName,
                            PhotoUrl = dto.PhotoUrl,
                            CreatedDate = DateTime.UtcNow,
                            IsActive = true
                        };

                        _db.Items.Add(item);
                        await _db.SaveChangesAsync(); // 🔥 SINGLE DB CALL
                        //_cache.Remove("items_cache");
                        //_cache.Remove("itemcode_cache");
                        return Ok(item);
                    }
                    catch (Exception ex)
                    {
                        retryCount++;

                        if (retryCount >= maxRetries)
                            return StatusCode(500, "Database timeout. Please try again later.");

                        await Task.Delay(delay); // ⏳ wait 5 sec before retry
                    }
                }
                return StatusCode(500, "Unexpected error");
            }
            catch (DbUpdateException)
            {
                return BadRequest("Item code already exists");
            }
        }

        // ✅ Update item
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ItemUpdateDto dto)
        {
            int retryCount = 0;
            int maxRetries = 3;
            TimeSpan delay = TimeSpan.FromSeconds(2);

            while (retryCount < maxRetries)
            {
                try
                {
                    bool update = await _itemQueryService.UpdateItemsAsync(id, dto);

                    //_cache.Remove("items_cache");
                    //_cache.Remove("itemcode_cache");
                    return Ok(dto);
                }
                catch (Exception ex)
                {
                    retryCount++;

                    if (retryCount >= maxRetries)
                        return StatusCode(500, "Database timeout. Please try again later.");

                    await Task.Delay(delay); // ⏳ wait 5 sec before retry
                }
            }

            return StatusCode(500, "Unexpected error");
        }

        [HttpPost("remove-images")]
        public async Task<IActionResult> RemoveImages([FromBody] string imageUrl)
        {
            await _storage.DeleteImageByUrl(imageUrl);
            return Ok("true");
        }
        [HttpPost("order-exists/{id}")]
        public async Task<IActionResult> OrderExists(int id)
        {
            int retryCount = 0;
            int maxRetries = 3;
            TimeSpan delay = TimeSpan.FromSeconds(1);

            while (retryCount < maxRetries)
            {
                try
                {
                    bool hasOrders = await _db.Orders
               .AsNoTracking()
               .AnyAsync(o => o.ItemId == id);

                    if (hasOrders)
                        return BadRequest("This item can't be deleted because orders exist.");
                    return Ok(hasOrders);
                }
                catch (Exception ex)
                {
                    retryCount++;

                    if (retryCount >= maxRetries)
                        return StatusCode(500, "Database timeout. Please try again later.");

                    await Task.Delay(delay); // ⏳ wait 5 sec before retry
                }
            }

            return StatusCode(500, "Unexpected error");
        }

        // ✅ Delete (soft delete recommended)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {

            int retryCount = 0;
            int maxRetries = 3;
            TimeSpan delay = TimeSpan.FromSeconds(1);

            while (retryCount < maxRetries)
            {
                try
                {
                    await _db.Items
            .Where(x => x.ItemId == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.IsActive, false)
            );
                    //_cache.Remove("items_cache");
                    //_cache.Remove("itemcode_cache");
                    return Ok("Item deactivated");
                }
                catch (Exception ex)
                {
                    retryCount++;

                    if (retryCount >= maxRetries)
                        return StatusCode(500, "Database timeout. Please try again later.");

                    await Task.Delay(delay); // ⏳ wait 5 sec before retry
                }
            }

            return StatusCode(500, "Unexpected error");
        }


        [HttpGet("code-exists")]
        public async Task<IActionResult> CodeValidate(int code, int itemId = 0)
        {
            int retryCount = 0;
            int maxRetries = 3;
            TimeSpan delay = TimeSpan.FromSeconds(1);

            while (retryCount < maxRetries)
            {
                try
                {
                    string itemCode = code.ToString("D3");
            bool exists = await _db.Items.AnyAsync(x =>
                   x.IsActive &&
                   x.ItemCode == itemCode &&
                   (itemId == 0 || x.ItemId != itemId)
               );

            return Ok(exists);
                }
                catch (Exception ex)
                {
                    retryCount++;

                    if (retryCount >= maxRetries)
                        return StatusCode(500, "Database timeout. Please try again later.");

                    await Task.Delay(delay); // ⏳ wait 5 sec before retry
                }
            }

            return StatusCode(500, "Unexpected error");

        }

        [HttpGet("next-itemcode")]
        public async Task<IActionResult> GetNextItemCode()
        {
            int retryCount = 0;
            int maxRetries = 3;
            TimeSpan delay = TimeSpan.FromSeconds(1);

            while (retryCount < maxRetries)
            {
                try
                {
                    var item = await _db.Items.OrderByDescending(y => Convert.ToInt32(y.ItemCode)).FirstOrDefaultAsync();
                    var itemsCode = Convert.ToInt32(item.ItemCode) + 1;

                    //var cacheOptions = new MemoryCacheEntryOptions()
                    //    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    //    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                    //_cache.Set(cacheKey, itemsCode.ToString("D3"), cacheOptions);

                    return Ok(itemsCode.ToString("D3"));
                }
                catch (Exception ex)
                {
                    retryCount++;

                    if (retryCount >= maxRetries)
                        return StatusCode(500, "Database timeout. Please try again later.");

                    await Task.Delay(delay); // ⏳ wait 5 sec before retry
                }
            }

            return StatusCode(500, "Unexpected error");

        }
    }
}

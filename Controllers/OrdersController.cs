using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RentalHub.Data;
using RentalHub.Helper;
using RentalHub.Model;

namespace RentalHub.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly RentalHubDbContext _db;
        private readonly IMemoryCache _cache;

        public OrdersController(RentalHubDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        // ================= CREATE =================
        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
        {
            //if (request.FromDate > request.ToDate)
            //    return BadRequest("FromDate cannot be greater than ToDate");

            //bool isBooked = await _db.Orders
            //    .AsNoTracking()
            //    .AnyAsync(o =>
            //        o.ItemId == request.ItemId &&
            //        request.FromDate <= o.ToDate &&
            //        request.ToDate >= o.FromDate
            //    );

            //if (isBooked)
            //    return Conflict("This item is already booked for selected dates");
            int retryCount = 0;
            int maxRetries = 3;
            TimeSpan delay = TimeSpan.FromSeconds(3);

            while (retryCount < maxRetries)
            {
                try
                {
                    var order = new Order
                    {
                        ItemId = request.ItemId,
                        ClientName = request.ClientName,
                        Village = request.Village,
                        FromDate = request.FromDate,
                        ToDate = request.ToDate,
                        Rent = request.Rent,
                        Advance = request.Advance,
                        Remaining = request.Rent - request.Advance,
                        Remark = request.Remark,
                        AdvanceTakenBy = request.AdvanceTakenBy,
                        RemainingTakenBy = request.RemainingTakenBy,
                        RemainingAmount = request.RemainingAmount,
                        MobileNumber = request.MobileNumber,
                        CreatedDate = DateTime.UtcNow
                    };

                    _db.Orders.Add(order);
                    await _db.SaveChangesAsync();

                    //ClearMonthlyCache(order.FromDate, order.ToDate);
                    //_cache.Remove($"UnavailableDates_Item_{order.ItemId}");
                    //_cache.Remove($"month_orders_{order.FromDate.Year}_{order.FromDate.Month}");
                    //_cache.Remove($"month_orders_{order.ToDate.Year}_{order.ToDate.Month}");

                    return Ok(order);
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

        // ================= UPDATE =================
        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrder(int orderId, CreateOrderRequest request)
        {
            int retryCount = 0;
            int maxRetries = 3;
            TimeSpan delay = TimeSpan.FromSeconds(3);

            while (retryCount < maxRetries)
            {
                try
                {
                    var existingOrder = await _db.Orders.FindAsync(orderId);
                    if (existingOrder == null)
                        return NotFound("Order not found");

                    var oldFrom = existingOrder.FromDate;
                    var oldTo = existingOrder.ToDate;
                    var oldItemId = existingOrder.ItemId;

                    //bool isBooked = await _db.Orders
                    //    .AsNoTracking()
                    //    .AnyAsync(o =>
                    //        o.ItemId == request.ItemId &&
                    //        o.OrderId != orderId &&
                    //        request.FromDate <= o.ToDate &&
                    //        request.ToDate >= o.FromDate
                    //    );

                    //if (isBooked)
                    //    return Conflict("This item is already booked for selected dates");

                    existingOrder.ClientName = request.ClientName;
                    existingOrder.Village = request.Village;
                    existingOrder.FromDate = request.FromDate;
                    existingOrder.ToDate = request.ToDate;
                    existingOrder.Rent = request.Rent;
                    existingOrder.Advance = request.Advance;
                    existingOrder.Remaining = request.Rent - request.Advance;
                    existingOrder.Remark = request.Remark;
                    existingOrder.AdvanceTakenBy = request.AdvanceTakenBy;
                    existingOrder.RemainingTakenBy = request.RemainingTakenBy;
                    existingOrder.RemainingAmount = request.RemainingAmount;
                    existingOrder.MobileNumber = request.MobileNumber;

                    await _db.SaveChangesAsync();

                    //_cache.Remove($"month_orders_{oldFrom.Year}_{oldFrom.Month}");
                    //_cache.Remove($"month_orders_{oldTo.Year}_{oldTo.Month}");

                    //_cache.Remove($"month_orders_{existingOrder.FromDate.Year}_{existingOrder.FromDate.Month}");
                    //_cache.Remove($"month_orders_{existingOrder.ToDate.Year}_{existingOrder.ToDate.Month}");
                    //ClearMonthlyCache(oldFrom, oldTo);
                    //ClearMonthlyCache(existingOrder.FromDate, existingOrder.ToDate);

                    //_cache.Remove($"UnavailableDates_Item_{oldItemId}");
                    //_cache.Remove($"UnavailableDates_Item_{existingOrder.ItemId}");

                    return Ok(existingOrder);
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

        // ================= DELETE =================
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            int retryCount = 0;
            int maxRetries = 3;
            TimeSpan delay = TimeSpan.FromSeconds(2);

            while (retryCount < maxRetries)
            {
                try
                {
                    var order = await _db.Orders.FindAsync(orderId);
                    if (order == null)
                        return NotFound();

                    _db.Orders.Remove(order);
                    await _db.SaveChangesAsync();

                    //ClearMonthlyCache(order.FromDate, order.ToDate);
                    //_cache.Remove($"month_orders_{order.FromDate.Year}_{order.FromDate.Month}");
                    //_cache.Remove($"month_orders_{order.ToDate.Year}_{order.ToDate.Month}");
                   // _cache.Remove($"UnavailableDates_Item_{order.ItemId}");

                    return Ok("Order deleted");
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

        // ================= ALL ORDERS =================
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            //var orders = await _cache.GetOrCreateAsync("Orders_List", async entry =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            //    return await _db.Orders.AsNoTracking().ToListAsync();
            //});

            int retryCount = 0;
            int maxRetries = 3;
            TimeSpan delay = TimeSpan.FromSeconds(2);

            while (retryCount < maxRetries)
            {
                try
                {
                    var orders= await _db.Orders.AsNoTracking().ToListAsync();
            return Ok(orders);

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

        // ================= MONTH ORDERS =================
        [HttpGet("month")]
        public async Task<IActionResult> GetMonthOrders(int year, int month,int itemId=0)
        {
            //string cacheKey = $"month_orders_{year}_{month}";

            //if (_cache.TryGetValue(cacheKey, out List<Order> cached))
            //{
            //    return Ok(cached.Where(y=> itemId == 0||y.ItemId==itemId).ToList());

            //}

            int retryCount = 0;
            int maxRetries = 3;
            TimeSpan delay = TimeSpan.FromSeconds(2);

            while (retryCount < maxRetries)
            {
                try
                {
                    var orders = await _db.Orders
                .AsNoTracking()
                .Include(o => o.Item)
                .Where(o => o.FromDate.Year == year && o.FromDate.Month == month)
                .OrderBy(o => o.FromDate)
                .ToListAsync();

                    //_cache.Set(cacheKey, orders, new MemoryCacheEntryOptions
                    //{
                    //    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                    //    SlidingExpiration = TimeSpan.FromMinutes(3)
                    //});

                    return Ok(orders.Where(y => itemId == 0 || y.ItemId == itemId).ToList());
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

        // ================= AVAILABILITY =================
        [HttpGet("check-availability")]
        public async Task<IActionResult> CheckAvailability(
            int itemId,
            DateTime fromDate,
            DateTime toDate,
            int orderId = 0)
        {
            //string cacheKey = $"Availability_{itemId}_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}_{orderId}";

            //if (!_cache.TryGetValue(cacheKey, out bool available))
            //{
            bool available = false;
            int retryCount = 0;
            int maxRetries = 3;
            TimeSpan delay = TimeSpan.FromSeconds(2);

            while (retryCount < maxRetries)
            {
                try
                {
                    bool isBooked = await _db.Orders
                .AsNoTracking()
                .AnyAsync(o =>
                    o.ItemId == itemId &&
                    (orderId == 0 || o.OrderId != orderId) &&
                    o.FromDate <= toDate &&
                    o.ToDate >= fromDate
                );

                    available = !isBooked;

                    //_cache.Set(cacheKey, available, TimeSpan.FromMinutes(2));
                }
                catch (Exception ex)
                {
                    retryCount++;

                    if (retryCount >= maxRetries)
                        return StatusCode(500, "Database timeout. Please try again later.");

                    await Task.Delay(delay); // ⏳ wait 5 sec before retry
                }
            }
            //}

            return Ok(new { available });
        }

        // ================= UNAVAILABLE DATES =================
        [HttpGet("unavailable-dates")]
        public async Task<IActionResult> GetUnavailableDates(int itemId)
        {
            int retryCount = 0;
            int maxRetries = 3;
            TimeSpan delay = TimeSpan.FromSeconds(2);

            while (retryCount < maxRetries)
            {
                try
                {
                    if (itemId <= 0)
                        return BadRequest("Invalid itemId");

                    //string cacheKey = $"UnavailableDates_Item_{itemId}";

                    //if (!_cache.TryGetValue(cacheKey, out List<(DateTime fromDate, DateTime toDate)> bookings))
                    //{
                    var    bookings = await _db.Orders
                            .AsNoTracking()
                            .Where(o => o.ItemId == itemId)
                            .Select(o => new BookingDateDto
                            {
                                FromDate = o.FromDate,
                                ToDate = o.ToDate
                            })
                            .ToListAsync();

                       // _cache.Set(cacheKey, bookings, TimeSpan.FromMinutes(5));
                    //}

                    return Ok(bookings);
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

        // ================= CACHE HELPER =================
        private void ClearMonthlyCache(DateTime fromDate, DateTime toDate)
        {
            var current = new DateTime(fromDate.Year, fromDate.Month, 1);
            var end = new DateTime(toDate.Year, toDate.Month, 1);

            while (current <= end)
            {
                _cache.Remove($"month_orders_{current.Year}_{current.Month}");
                current = current.AddMonths(1);
            }
        }
    }

}

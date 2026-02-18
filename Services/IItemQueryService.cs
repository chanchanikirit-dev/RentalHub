using RentalHub.DTO;
using RentalHub.Model;

namespace RentalHub.Services
{
    public interface IItemQueryService
    {
        Task<List<Item>> GetActiveItemsAsync();
        Task<bool> UpdateItemsAsync(int id, ItemUpdateDto dto);
    }
}

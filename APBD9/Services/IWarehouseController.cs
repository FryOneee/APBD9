// Services/IWarehouseService.cs
using System.Threading.Tasks;
using APBD9.Models;

namespace APBD9.Services
{
    public interface IWarehouseService
    {
        Task<int> RegisterAsync(WarehouseRequest request);
    }
}
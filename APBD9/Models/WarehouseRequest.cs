// Models/WarehouseRequest.cs
using System;

namespace APBD9.Models
{
    public class WarehouseRequest
    {
        public int IdProduct { get; set; }
        public int IdWarehouse { get; set; }
        public int Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
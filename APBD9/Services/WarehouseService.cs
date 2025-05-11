// Services/WarehouseService.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using APBD9.Models;

namespace APBD9.Services
{
    public class WarehouseService : IWarehouseService
    {
        private const string _cs =
            "Data Source=localhost;" +
            "User ID=SA;" +
            "Password=yourStrong(9)Password;" +
            "Initial Catalog=APBD8_2;" +
            "Integrated Security=False;" +
            "Connect Timeout=30;" +
            "Encrypt=False;" +
            "TrustServerCertificate=True;";

        public async Task<int> RegisterAsync(WarehouseRequest req)
        {
            if (req.Amount <= 0)
                throw new ArgumentException("Amount must be > 0");

            using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            try
            {
                // 1) Cena jednostkowa
                decimal unitPrice;
                using (var cmd = new SqlCommand(
                    "SELECT Price FROM Product WHERE IdProduct=@p",
                    conn, tx))
                {
                    cmd.Parameters.AddWithValue("@p", req.IdProduct);
                    var priceObj = await cmd.ExecuteScalarAsync();
                    if (priceObj == null)
                        throw new KeyNotFoundException("Product not found");
                    unitPrice = (decimal)priceObj;
                }

                // 2) Id zamówienia spełniającego warunki
                int idOrder;
                using (var cmd = new SqlCommand(
                    @"SELECT IdOrder FROM [Order]
                      WHERE IdProduct=@p AND Amount=@a AND CreatedAt < @ca",
                    conn, tx))
                {
                    cmd.Parameters.AddWithValue("@p", req.IdProduct);
                    cmd.Parameters.AddWithValue("@a", req.Amount);
                    cmd.Parameters.AddWithValue("@ca", req.CreatedAt);
                    var idOrderObj = await cmd.ExecuteScalarAsync();
                    if (idOrderObj == null)
                        throw new KeyNotFoundException("Matching order not found or not in time");
                    idOrder = (int)idOrderObj;
                }

                // 3) Sprawdź, czy nie zostało już zrealizowane
                using (var cmd = new SqlCommand(
                    "SELECT COUNT(1) FROM Product_Warehouse WHERE IdOrder=@o",
                    conn, tx))
                {
                    cmd.Parameters.AddWithValue("@o", idOrder);
                    bool exist = (int)await cmd.ExecuteScalarAsync() > 0;
                    if (exist)
                        throw new InvalidOperationException("Order already fulfilled");
                }

                // 4) Ustaw FullfilledAt
                using (var cmd = new SqlCommand(
                    "UPDATE [Order] SET FulfilledAt = GETDATE() WHERE IdOrder=@o",
                    conn, tx))
                {
                    cmd.Parameters.AddWithValue("@o", idOrder);
                    await cmd.ExecuteNonQueryAsync();
                }

                // 5) Wstaw nowy wpis w Product_Warehouse
                int newId;
                using (var cmd = new SqlCommand(
                    @"INSERT INTO Product_Warehouse
                      (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                      VALUES (@w,@p,@o,@a,@price,@now);
                      SELECT CAST(SCOPE_IDENTITY() AS int);",
                    conn, tx))
                {
                    cmd.Parameters.AddWithValue("@w", req.IdWarehouse);
                    cmd.Parameters.AddWithValue("@p", req.IdProduct);
                    cmd.Parameters.AddWithValue("@o", idOrder);
                    cmd.Parameters.AddWithValue("@a", req.Amount);
                    cmd.Parameters.AddWithValue("@price", unitPrice * req.Amount);
                    cmd.Parameters.AddWithValue("@now", DateTime.UtcNow);

                    newId = (int)await cmd.ExecuteScalarAsync();
                }

                tx.Commit();
                return newId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
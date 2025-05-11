// Controllers/WarehouseController.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using APBD9.Models;
using APBD9.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;


namespace APBD9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;
        public WarehouseController(IWarehouseService warehouseService)
            => _warehouseService = warehouseService;

        [HttpPut]
        public async Task<IActionResult> Register([FromBody] WarehouseRequest request)
        {
            try
            {
                int newId = await _warehouseService.RegisterAsync(request);
                return Ok(new { ProductWarehouseId = newId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (SqlException ex)
            {
                // błąd połączenia / zapytania SQL
                return StatusCode(500, new { error = "Database error: " + ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Unexpected error: " + ex.Message });
            }
        }
    }
}
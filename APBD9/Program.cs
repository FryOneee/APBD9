// Program.cs
using APBD9.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddOpenApi();

var app = builder.Build();

InitializeDatabase();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler("/error");
app.Map("/error", (HttpContext httpContext) =>
{
    var exceptionFeature = httpContext.Features.Get<IExceptionHandlerFeature>();
    var ex = exceptionFeature?.Error;
    return Results.Problem(
        detail: ex?.Message,
        title: ex?.GetType().Name
    );
});

app.UseAuthorization();
app.MapControllers();
app.Run();

static void InitializeDatabase()
{
    const string _csMaster =
        "Data Source=localhost;" +
        "User ID=SA;" +
        "Password=yourStrong(9)Password;" +
        "Initial Catalog=master;" +
        "Integrated Security=False;" +
        "Connect Timeout=30;" +
        "Encrypt=False;" +
        "TrustServerCertificate=True;";

    const string createDbScript = @"
IF DB_ID('APBD8_2') IS NULL
BEGIN
    CREATE DATABASE APBD8_2;
END;";

    const string schemaScript = @"
-- Created by Vertabelo (http://vertabelo.com)
-- Last modification date: 2021-04-05 12:56:53.13

-- tables
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Order]') AND type = N'U')
BEGIN
    CREATE TABLE [Order] (
        IdOrder int NOT NULL IDENTITY,
        IdProduct int NOT NULL,
        Amount int NOT NULL,
        CreatedAt datetime NOT NULL,
        FulfilledAt datetime NULL,
        CONSTRAINT Order_pk PRIMARY KEY (IdOrder)
    );
END;

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'Product') AND type = N'U')
BEGIN
    CREATE TABLE Product (
        IdProduct int NOT NULL IDENTITY,
        Name nvarchar(200) NOT NULL,
        Description nvarchar(200) NOT NULL,
        Price numeric(25,2) NOT NULL,
        CONSTRAINT Product_pk PRIMARY KEY (IdProduct)
    );
END;

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'Product_Warehouse') AND type = N'U')
BEGIN
    CREATE TABLE Product_Warehouse (
        IdProductWarehouse int NOT NULL IDENTITY,
        IdWarehouse int NOT NULL,
        IdProduct int NOT NULL,
        IdOrder int NOT NULL,
        Amount int NOT NULL,
        Price numeric(25,2) NOT NULL,
        CreatedAt datetime NOT NULL,
        CONSTRAINT Product_Warehouse_pk PRIMARY KEY (IdProductWarehouse)
    );
END;

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'Warehouse') AND type = N'U')
BEGIN
    CREATE TABLE Warehouse (
        IdWarehouse int NOT NULL IDENTITY,
        Name nvarchar(200) NOT NULL,
        Address nvarchar(200) NOT NULL,
        CONSTRAINT Warehouse_pk PRIMARY KEY (IdWarehouse)
    );
END;

-- foreign keys
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'Product_Warehouse_Order')
BEGIN
    ALTER TABLE Product_Warehouse ADD CONSTRAINT Product_Warehouse_Order
        FOREIGN KEY (IdOrder)
        REFERENCES [Order] (IdOrder);
END;

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'Receipt_Product')
BEGIN
    ALTER TABLE [Order] ADD CONSTRAINT Receipt_Product
        FOREIGN KEY (IdProduct)
        REFERENCES Product (IdProduct);
END;

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = '_Product')
BEGIN
    ALTER TABLE Product_Warehouse ADD CONSTRAINT _Product
        FOREIGN KEY (IdProduct)
        REFERENCES Product (IdProduct);
END;

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = '_Warehouse')
BEGIN
    ALTER TABLE Product_Warehouse ADD CONSTRAINT _Warehouse
        FOREIGN KEY (IdWarehouse)
        REFERENCES Warehouse (IdWarehouse);
END;
";

    using var conn = new SqlConnection(_csMaster);
    conn.Open();
    using (var cmd = new SqlCommand(createDbScript, conn))
        cmd.ExecuteNonQuery();

    conn.ChangeDatabase("APBD8_2");

    using (var cmd = new SqlCommand(schemaScript, conn))
        cmd.ExecuteNonQuery();
}
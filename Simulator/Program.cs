using Microsoft.EntityFrameworkCore;
using NLog.Web;
using Simulator.Models;
using CommonItems;
using ProductionPlanManagement;
using WorkOrderManagement;
using ShippingOperationCoordinator;
using InventoryPalletCoordinator;
using ShippingPalletCoordinator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Host.UseNLog();

var connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DefaultDbContext>((options) => options.UseSqlServer(connection, sqlServerOptions => sqlServerOptions.CommandTimeout(60)), ServiceLifetime.Scoped);

builder.Services.AddCommonItems<DefaultDbContext>(builder.Configuration, (options) => options.UseSqlServer(connection, sqlServerOptions => sqlServerOptions.CommandTimeout(60)));
builder.Services.AddProductionPlan<DefaultDbContext>(builder.Configuration, (options) => options.UseSqlServer(connection, sqlServerOptions => sqlServerOptions.CommandTimeout(60)));
builder.Services.AddWorkOrderManagement<DefaultDbContext>(builder.Configuration, (options) => options.UseSqlServer(connection, sqlServerOptions => sqlServerOptions.CommandTimeout(60)));
builder.Services.AddShippingOperationCoordinator<DefaultDbContext>(builder.Configuration, (options) => options.UseSqlServer(connection, sqlServerOptions => sqlServerOptions.CommandTimeout(60)));
builder.Services.AddInventoryPalletCoordinator<DefaultDbContext>(builder.Configuration, (options) => options.UseSqlServer(connection, sqlServerOptions => sqlServerOptions.CommandTimeout(60)));
builder.Services.AddShippingPalletCoordinator<DefaultDbContext>(builder.Configuration, (options) => options.UseSqlServer(connection, sqlServerOptions => sqlServerOptions.CommandTimeout(60)));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using CommonItems.Models;
using ProductionPlanManagement.Models;

namespace Simulator.Models;

public static class ModelBuilderExtensions
{
    public static void ApplyHinbanConversion(this ModelBuilder modelBuilder) {
        var conversion = new ValueConverter<Hinban, string>(v => v.Value, v => new Hinban(v));

        // ProductionPlanManagement
        modelBuilder.Entity<ProductionPlan>().Property(typeof(Hinban), "Hinban").HasConversion(conversion);
    }
}

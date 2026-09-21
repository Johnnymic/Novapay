using BranchCardManagementSystem.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nova.Application.Extension;
using Nova.DataAccess.Extension;
using Nova.DataAccess.Persistence;
using Nova.DataAccess.Persistence.Nova.DataAccess.Seed;
using Nova.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDataAccessServices(builder.Configuration);
builder.Services.AddServices(builder.Configuration);
builder.Services.AddAppSwagger(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<NovaWalletDbContext>();

    dbContext.Database.Migrate();
}


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dbContext =
        services.GetRequiredService<NovaWalletDbContext>();

    var passwordHasher =
        services.GetRequiredService<IPasswordHasher<Customer>>();

    await dbContext.Database.MigrateAsync();

    await AdminSeeder.SeedAsync(
        dbContext,
        passwordHasher);
}




app.UseHttpsRedirection();

app.ConfigureCustomExceptionMiddleware();
app.UseAuthorization();

app.MapControllers();

app.Run();

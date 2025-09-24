using JhonMark.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;

namespace TestProject1;

public class UnitTest1 : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private WebApplicationFactory<Program> customWebAppFactory;
    private readonly PostgreSqlContainer _dbContainer;

    public UnitTest1(WebApplicationFactory<Program> webAppFactory)
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres")
            .WithPassword("Strong_password_123!")
            .Build();

        customWebAppFactory = webAppFactory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<StoreContext>();
                services.RemoveAll<DbContextOptions>();
                services.RemoveAll(typeof(DbContextOptions<StoreContext>));
                services.AddDbContext<StoreContext>(options => options.UseNpgsql(_dbContainer.GetConnectionString()));//Expect this line to give you an error for now...
            });
        });
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        //You can run database migrations if you have them
        using var scope = customWebAppFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreContext>();
        await db.Database.MigrateAsync();

        //Or you can run your custom DDL to set up your database
        var yourInitScriptContents = """"
        -- Put whatever initialization script you want in here.
        CREATE TABLE IF NOT EXISTS "MyData" (
            "Id" SERIAL PRIMARY KEY,
            "Name" TEXT NOT NULL
        );
        INSERT INTO "MyData" ("Name") VALUES ('Hello, World!');
        """";
        await _dbContainer.ExecScriptAsync(yourInitScriptContents);
    }

    [Fact]
    public async Task Test1()
    {
        var client = customWebAppFactory.CreateClient();
        var data = await client.GetFromJsonAsync<IEnumerable<Product>>("/store/products");
        data.Count().ShouldBe(0);

        await client.PostAsJsonAsync("/store/add-product", new Product
        {
            Name = "Test Product",
            Description = "This is a test product",
            Price = 9.99M
        });

        var data2 = await client.GetFromJsonAsync<IEnumerable<Product>>("/store/products");
        data2.Count().ShouldBe(1);
    }
}

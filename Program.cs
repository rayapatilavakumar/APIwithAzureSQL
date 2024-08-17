using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AzureSQLDBConnectionAPI;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System.Data.SqlClient;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AzureDBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("AzureConnectionString")));
builder.Services.AddAzureClients(azureBuilder =>
{
    var credentials = new DefaultAzureCredential();

    var queueUri = builder.Configuration["Storage:QueueEndPoint"]!;
    var keyVaultUri = builder.Configuration["KeyValut:KeyVaultEndPoint"]!;

    azureBuilder.AddSecretClient(new Uri(keyVaultUri));

    azureBuilder.AddQueueServiceClient(new Uri(queueUri)).ConfigureOptions(options =>
    {
        options.MessageEncoding = Azure.Storage.Queues.QueueMessageEncoding.Base64;
    });
    azureBuilder.UseCredential(credentials);

});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

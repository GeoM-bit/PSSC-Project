using Microsoft.Extensions.Azure;
using Project.Common.Services;
using Project.Domain.Repositories;
using Project.Domain.Workflows;
using Project.Events.ServiceBus;
using Project.Events;
using Swashbuckle.AspNetCore.Filters;
using Project.Data.Repositories;
using Project;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.ExampleFilters();
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();


builder.Services.AddSingleton<IEventService, EventService>();
builder.Services.AddDbContext<ProjectContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.EnableSensitiveDataLogging();
});

builder.Services.AddTransient<IOrderRepository, OrderRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<IEventSender, ServiceBusTopicEventSender>();
builder.Services.AddTransient<ModifyOrderWorkflow>();

builder.Services.AddAzureClients(client =>
{
    client.AddServiceBusClient(builder.Configuration.GetConnectionString("ServiceBus"));
});


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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOptions();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapGet("/", () => "User Service is running");

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI(c=>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service API V1");
});
app.UseHealthChecks("/health");
app.UseAuthorization();
app.MapControllers();

app.Run();

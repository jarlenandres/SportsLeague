using Microsoft.EntityFrameworkCore;
using SportsLeague.DataAccess.Context;
using SportsLeague.DataAccess.Repositories;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

var builder = WebApplication.CreateBuilder(args);

// --Entity Framework Core --
builder.Services.AddDbContext<LeagueDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaulConnection")));

// -- Repositories  --
builder.Services.AddScoped(typeof(IGenericRepository<>),
    typeof(GenericRepository<>));
builder.Services.AddScoped<ITeamRepository, TeamRepositiry>();

// -- Services  --
builder.Services.AddScoped<ITeamService, TeamService>();

// -- AutoMapper --
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// -- Controllers --
builder.Services.AddControllers();

// -- Swagger --
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// -- Middleware Pipeline --
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HotelDb>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelDb>();
    db.Database.EnsureCreated();
}

app.MapGet("/hotels", async (IHotelRepository rep) => 
Results.Ok(await rep.GetHotelsAsync()));


app.MapGet("/hotels/{id}", async (int id, IHotelRepository rep) => 
    await rep.GetHotelAsync(id) is Hotel hotel
    ? Results.Ok(hotel)
    : Results.NotFound());


app.MapPost("/hotels", async ([FromBody] Hotel hotel, IHotelRepository rep) => 
    {
        await rep.InsertHotelAsync(hotel);
        await rep.SaveAsync();
        return Results.Created($"/hotels/{hotel.Id}", hotel);
    });


app.MapPut("/hotels", async ([FromBody] Hotel hotel, IHotelRepository rep) => {
    await rep.UpdateHotelAsync(hotel);
    await rep.SaveAsync();
    return Results.NoContent();
});


app.MapDelete("/hotels/{id}", async ( IHotelRepository rep, int id) => {
    await rep.DeleteHotelAsync(id);
    await rep.SaveAsync();
    return Results.NoContent();
});

app.UseHttpsRedirection();
app.Run();


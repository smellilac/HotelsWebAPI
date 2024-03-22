var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<HotelDb>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelDb>();
    db.Database.EnsureCreated();
}

app.MapGet("/hotels", async (IHotelRepository rep) => 
    Results.Extensions.Xml(await rep.GetHotelsAsync()))
    .Produces<List<Hotel>>(StatusCodes.Status200OK)
    .WithName("GetAllHotels")
    .WithTags("Getters");

app.MapGet("hotels/search/name/{query}", async (string name, IHotelRepository rep ) =>
    await rep.GetHotelsAsync(name) is IEnumerable<Hotel> hotels
    ? Results.Ok(hotels)
    : Results.NotFound(Array.Empty<Hotel>()))
    .Produces<List<Hotel>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("GetHotelsWithName")
    .WithTags("Getters");

app.MapGet("hotels/search/location/{coordinate}", 
    async (Coordinate coordinate,  IHotelRepository rep) =>
    await rep.GetHotelsAsync(coordinate) is IEnumerable<Hotel> hotels
     ? Results.Ok(hotels)
    : Results.NotFound(Array.Empty<Hotel>()))
    .Produces<List<Hotel>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("GetHotelsWithLocation")
    .WithTags("Getters");

app.MapGet("/hotels/{id}", async (int id, IHotelRepository rep) => 
    await rep.GetHotelAsync(id) is Hotel hotel
    ? Results.Ok(hotel)
    : Results.NotFound())
    .Produces(StatusCodes.Status200OK)
    .WithName("GetHotel")
    .WithTags("Getters");


app.MapPost("/hotels", async ([FromBody] Hotel hotel, IHotelRepository rep) => 
    {
        await rep.InsertHotelAsync(hotel);
        await rep.SaveAsync();
        return Results.Created($"/hotels/{hotel.Id}", hotel);
    })
    .Accepts<Hotel>("application/json")
    .Produces(StatusCodes.Status201Created)
    .WithName("CreateHotel")
    .WithTags("Creators");


app.MapPut("/hotels", async ([FromBody] Hotel hotel, IHotelRepository rep) => 
    {
        await rep.UpdateHotelAsync(hotel);
        await rep.SaveAsync();
        return Results.NoContent();
    })
    .Accepts<Hotel>("application/json")
    .Produces(StatusCodes.Status204NoContent)
    .WithName("UpdateHotel")
    .WithTags("Updaters");


app.MapDelete("/hotels/{id}", async ( IHotelRepository rep, int id) => {
    await rep.DeleteHotelAsync(id);
    await rep.SaveAsync();
    return Results.NoContent();
})
.Produces(StatusCodes.Status204NoContent)
.WithName("DeleteHotel")
.WithTags("Deleters");

app.UseHttpsRedirection();
app.Run();
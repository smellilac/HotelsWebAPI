var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HotelDb>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelDb>();
    db.Database.EnsureCreated();
}

app.MapGet("/hotels", async (HotelDb db) => await db.Hotels.ToListAsync());


app.MapGet("/hotels/{id}", async (int id, HotelDb db) => 
    await db.Hotels.FirstOrDefaultAsync(h => h.Id == id) is Hotel hotel
    ? Results.Ok(hotel)
    : Results.NotFound());


app.MapPost("/hotels", async ([FromBody] Hotel hotel, HotelDb db) => 
    {
        db.Hotels.Add(hotel);
        await db.SaveChangesAsync();
        return Results.Created($"/hotels/{hotel.Id}", hotel);
    });


app.MapPut("/hotels", async ([FromBody] Hotel hotel, HotelDb db) => {
    var entity = await db.Hotels.FindAsync(new object[] {hotel.Id});
    if ( entity == null  ) return Results.NotFound();
    entity.Latitude = hotel.Latitude;
    entity.Longitude = hotel.Longitude;
    entity.Name = hotel.Name;
    await db.SaveChangesAsync();
    return Results.NoContent();
});


app.MapDelete("/hotels/{id}", async (HotelDb db, int id) => {
    var entity = await db.Hotels.FindAsync(new object[] {id});
    if (entity == null ) return Results.NotFound();
    db.Hotels.Remove(entity);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.UseHttpsRedirection();
app.Run();

public class HotelDb : DbContext
{
    public HotelDb(DbContextOptions<HotelDb> options) : base(options) {}
    public DbSet<Hotel> Hotels => Set<Hotel>();
}
public class Hotel
{
    public int Id { get; set;}

    public string Name { get; set; } = string.Empty;

    public double Latitude { get; set;}
    
    public double Longitude { get; set;}

}
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// builder.Services.AddSwaggerGen(c =>
// {
//     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//     {
//         Description = "JWT Authorization header using the Bearer scheme",
//         Name = "Authorization",
//         In = ParameterLocation.Header,
//         //Type = SecuritySchemeType.ApiKey,
//         Scheme = "Bearer",
//         //BearerFormat = "JWT"
//     });

//     c.AddSecurityRequirement(new OpenApiSecurityRequirement()
//     {
//         {
//             new OpenApiSecurityScheme
//             {
//                 Reference = new OpenApiReference
//                 {
//                     Type = ReferenceType.SecurityScheme,
//                     Id = "Bearer"
//                 },
//                 Scheme = "oauth2",
//                 Name = "Bearer"
//             },
//             new List<string>()
//         }
//     });
// });
builder.Services.AddDbContext<HotelDb>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddSingleton<ITokenService>(new TokenService()); // we are not using lazy
//initialization like <ITokenService, TokenService>, we are creating object of TokenService
// and that object will be used while program lifetime
// !!!!!!!!!  MAYBE NEED TO CHANGE FOR OPTIMISATION !!!!!!!!! 
builder.Services.AddSingleton<IUserRepository>(new UserRepository());
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelDb>();
    db.Database.EnsureCreated();
}

app.MapGet("/login", [AllowAnonymous] async (HttpContext context,
    ITokenService tokenService, IUserRepository userRepository) => {
        UserModel userModel = new()
        {
            UserName = context.Request.Query["username"],
            Password = context.Request.Query["password"]
        };
        var userDto = userRepository.GetUser(userModel);
        if (userDto == null) return Results.Unauthorized();
        var token = tokenService.BuildToken(builder.Configuration["Jwt:Key"],
            builder.Configuration["Jwt:Issuer"], userDto);
        return Results.Ok(token);
    });


app.MapGet("/hotels", [Authorize] async (IHotelRepository rep) => 
    Results.Extensions.Xml(await rep.GetHotelsAsync()))
    .Produces<List<Hotel>>(StatusCodes.Status200OK)
    .WithName("GetAllHotels")
    .WithTags("Getters");

app.MapGet("hotels/search/name/{query}", [Authorize] async (string name, IHotelRepository rep ) =>
    await rep.GetHotelsAsync(name) is IEnumerable<Hotel> hotels
    ? Results.Ok(hotels)
    : Results.NotFound(Array.Empty<Hotel>()))
    .Produces<List<Hotel>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("GetHotelsWithName")
    .WithTags("Getters");

app.MapGet("hotels/search/location/{coordinate}", 
    [Authorize] async (Coordinate coordinate,  IHotelRepository rep) =>
    await rep.GetHotelsAsync(coordinate) is IEnumerable<Hotel> hotels
     ? Results.Ok(hotels)
    : Results.NotFound(Array.Empty<Hotel>()))
    .Produces<List<Hotel>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("GetHotelsWithLocation")
    .WithTags("Getters");

app.MapGet("/hotels/{id}", [Authorize] async (int id, IHotelRepository rep) => 
    await rep.GetHotelAsync(id) is Hotel hotel
    ? Results.Ok(hotel)
    : Results.NotFound())
    .Produces(StatusCodes.Status200OK)
    .WithName("GetHotel")
    .WithTags("Getters");


app.MapPost("/hotels", [Authorize] async ([FromBody] Hotel hotel, IHotelRepository rep) => 
    {
        await rep.InsertHotelAsync(hotel);
        await rep.SaveAsync();
        return Results.Created($"/hotels/{hotel.Id}", hotel);
    })
    .Accepts<Hotel>("application/json")
    .Produces(StatusCodes.Status201Created)
    .WithName("CreateHotel")
    .WithTags("Creators");


app.MapPut("/hotels", [Authorize] async ([FromBody] Hotel hotel, IHotelRepository rep) => 
    {
        await rep.UpdateHotelAsync(hotel);
        await rep.SaveAsync();
        return Results.NoContent();
    })
    .Accepts<Hotel>("application/json")
    .Produces(StatusCodes.Status204NoContent)
    .WithName("UpdateHotel")
    .WithTags("Updaters");


app.MapDelete("/hotels/{id}", [Authorize] async ( IHotelRepository rep, int id) => {
    await rep.DeleteHotelAsync(id);
    await rep.SaveAsync();
    return Results.NoContent();
    })
    .Produces(StatusCodes.Status204NoContent)
    .WithName("DeleteHotel")
    .WithTags("Deleters");

app.UseHttpsRedirection();
app.Run();
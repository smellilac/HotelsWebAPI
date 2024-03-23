var builder = WebApplication.CreateBuilder(args);

RegisterServices(builder.Services);

var app = builder.Build();

Configure(app);

var apis = app.Services.GetServices<IApi>();
foreach (var api in apis)
{
    if (api == null) throw new Exception("Api doesnt found");
    api.Register(app);
}

app.Run();

void RegisterServices(IServiceCollection services)
{
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    services.AddDbContext<HotelDb>(options =>
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
    });
    services.AddScoped<IHotelRepository, HotelRepository>();
    services.AddSingleton<ITokenService>(new TokenService()); // we are not using lazy
    //initialization like <ITokenService, TokenService>, we are creating object of TokenService
    // and that object will be used while program lifetime
    // !!!!!!!!!  MAYBE NEED TO CHANGE FOR OPTIMISATION !!!!!!!!! 
    services.AddSingleton<IUserRepository>(new UserRepository());
    services.AddAuthorization();
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
    services.AddTransient<IApi, HotelApi>();
    services.AddTransient<IApi, AuthApi>();

}

void Configure(WebApplication app)
{
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
    app.UseHttpsRedirection(); 
}
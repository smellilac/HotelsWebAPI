                // from program.cs ----- Authorization for swagger
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

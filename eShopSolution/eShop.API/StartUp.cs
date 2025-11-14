using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;

namespace eShop.API
{
    public static class StartUp
    {
        internal static IServiceCollection AddApiVersioningConfig(this IServiceCollection services)
        {
            services
                .AddApiVersioning(options =>
                {
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                    options.AssumeDefaultVersionWhenUnspecified = true;

                    //  This line triggers OnStarting (disable it)
                    options.ReportApiVersions = false;
                })
                .AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                });

            return services;
        }

        //internal static IServiceCollection AddSwagger(this IServiceCollection services)
        //{
        //    //return services.AddSwaggerGen(options =>
        //    //{

        //    //    options.MapType<byte[]>(() => new Microsoft.OpenApi.Models.OpenApiSchema
        //    //    {
        //    //        Type = "string",
        //    //        Format = "base64"
        //    //    });

        //    //    options.SwaggerDoc("v1", new OpenApiInfo { Title = "MyTemplate", Version = "v1" });

        //    //    var securitySchema = new OpenApiSecurityScheme
        //    //    {
        //    //        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        //    //        Name = "Authorization",
        //    //        In = ParameterLocation.Header,
        //    //        Type = SecuritySchemeType.Http,
        //    //        Scheme = "bearer",
        //    //        Reference = new OpenApiReference
        //    //        {
        //    //            Type = ReferenceType.SecurityScheme,
        //    //            Id = "Bearer"
        //    //        }
        //    //    };

        //    //    options.AddSecurityDefinition("Bearer", securitySchema);

        //    //    var securityRequirement = new OpenApiSecurityRequirement
        //    //    {
        //    //        { securitySchema, new[] { "Bearer" } }
        //    //    };

        //    //    options.AddSecurityRequirement(securityRequirement);


        //    //});
        //}

       

        public static IServiceCollection AddCorsAllowAll(this IServiceCollection services)
        {
            // Add CORS policy
            return services.AddCors(options =>
                                                 {
                                                     options.AddPolicy("AllowAll", policy =>
                                                     {
                                                         policy
                                                             .AllowAnyOrigin()
                                                             .AllowAnyMethod()
                                                             .AllowAnyHeader();
                                                     });
                                                 });
        }




        public static IApplicationBuilder UseCorsAllowAll(this IApplicationBuilder app)
        {
            return app.UseCors("AllowAll");
        }
    }
}

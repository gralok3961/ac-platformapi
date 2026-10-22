using Base.Helpers.Dtos;
using Microsoft.AspNetCore.Builder;

namespace Base.Extensions
{
    public static class ApplicationBuilderExtension
    {
        public static IApplicationBuilder UseSwaggerDocumention(this IApplicationBuilder app, SwaggerConfig config)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{config.Version}/swagger.json", $"{config.Description} Version : {config.Version}");
            });

            return app;
        }
    }
}

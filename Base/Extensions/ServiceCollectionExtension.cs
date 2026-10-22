using Base.Errors;
using Base.Helpers.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net;

namespace Base.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection ValidateModelState(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var errors = actionContext.ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value.Errors)
                    .Select(x => x.ErrorMessage).ToArray();

                    return new BadRequestObjectResult(new ApiResponse(HttpStatusCode.BadRequest, errors.FirstOrDefault()));
                };
            });

            return services;
        }

        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services, SwaggerConfig config)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc($"{config.Version}", config);
                c.DocInclusionPredicate((_, _) => false); // no show swagger
            });

            return services;
        }

    }
}

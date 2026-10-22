var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration
        .AddJsonFile("appsettings.Development.local.json", optional: true, reloadOnChange: false)
        .AddEnvironmentVariables()
        .AddCommandLine(args);
}

builder.Logging.AddAzureWebAppDiagnostics();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.Configure<AzureFileLoggerOptions>(options =>
{
    options.FileName = "hashtrion-platform-";
    options.FileSizeLimit = 10 * 1024 * 1024;
    options.RetainedFileCountLimit = 5;
});
var _config = builder.Configuration;
//** Initial auth config
builder.AddInitialConfig(_config);
// Add services to the container.
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.Converters.Clear(); // remove converters like StringEnumConverter
});
builder.Services.AddApplicationServices(_config);
builder.Services.AddIdentityServices(_config);
builder.Services.AddPolicySchemeServices();
builder.Services.AddSwaggerDocumentation(_config.GetSection(ConfigPath.SWAGGER_CONFIG).Get<SwaggerConfig>());
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();


//For convert Enum to type
var app = builder.Build();
// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseSwaggerDocumention(_config.GetSection(ConfigPath.SWAGGER_CONFIG).Get<SwaggerConfig>());
app.MapControllers();

app.Run();

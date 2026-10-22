using Microsoft.Extensions.Configuration;

namespace Base.Services
{
    public class ConfigService(IConfiguration config)
    {
        protected readonly IConfiguration _config = config;
    }
}

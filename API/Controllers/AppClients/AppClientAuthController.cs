namespace API.Controllers.AppClients
{
    public class AppClientAuthController(IAppSecurityService appSecurityService, IAgentAccountService agentAccountService,
    IHostEnvironment environment) : BaseApiController
    {
        private readonly IAppSecurityService _appSecurityService = appSecurityService;
        private readonly IAgentAccountService _agentAccountService = agentAccountService;

        // Hackathon prototype: Simplified Agent Account platform linking.
        // In a real-world integration, connecting an Agent Account to an external
        // AI platform requires platform integration and explicit wallet-owner authorization.
        // This prototype simplifies that onboarding process to focus on demonstrating
        // the actual Agentic Commerce payment flow.
        [HttpPost("mock/agent-platform-connection")]
        [AllowAnonymous]
        public async Task<IActionResult> ConnectAgentPlatformDemo(
            [FromBody] ConnectAgentPlatformDemoRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!environment.IsDevelopment()) return NotFound();

            if (string.IsNullOrWhiteSpace(request?.AgentId)) return BadRequest("Agent ID is required.");

            var result = await _agentAccountService.ConnectAgentPlatformDemoAsync(request.AgentId.Trim(), cancellationToken);

            return Ok(result);
        }
        // Hackathon prototype: Simplified App Client onboarding.
        // The main Hashtrion Wallet already implements App Client registration
        // and authentication. This prototype uses a fixed client identity
        // to focus on demonstrating the Agentic Commerce payment flow.
        [HttpGet("mock/app-client")]
        [AllowAnonymous]
        public IActionResult TestAppSoftClient()
        {
            if (!environment.IsDevelopment()) return NotFound();

            //Demo Purpose Temporary user auth. this client will permanently removed
            var appClientId = Guid.Parse("feee842d-5457-4763-ab82-b8cf7ca95bf6");

            var result = _appSecurityService.CreateAppClientTokenResponse(appClientId);

            return Ok(result);
        }
    }
}

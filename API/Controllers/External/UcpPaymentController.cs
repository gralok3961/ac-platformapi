using API.Filters;

namespace API.Controllers.External
{
    [TypeFilter(typeof(UcpPaymentExceptionFilter))]
    public class UcpPaymentController(IUcpPaymentService ucpPaymentService, ILogger<UcpPaymentController> logger) : BaseApiController
    {
        private readonly IUcpPaymentService _ucpPaymentService = ucpPaymentService;
        private readonly ILogger<UcpPaymentController> _logger = logger;

        [Authorize(Policy = "UCP_BUSINESS")]
        [HttpPost]
        [Route("process-payment")]
        [RequestSizeLimit(131072)]
        [TypeFilter(typeof(UcpPaymentModelStateFilter), Order = -2001)]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request, CancellationToken cancellationToken)
        {
            var result = await _ucpPaymentService.ProcessPaymentAsync(request, cancellationToken);

            return Ok(result);
        }

        [Authorize(Policy = "UCP_AGENT")]
        [HttpPost]
        [Route("issue-credential")]
        public async Task<IActionResult> IssueCredential([FromBody] IssueCredentialRequest request, CancellationToken cancellationToken)
        {
            var platformProfileUrl = User.FindFirst(HashClaimKeys.UCP_PLATFORM_PROFILE)?.Value;

            if (string.IsNullOrWhiteSpace(platformProfileUrl)) return Unauthorized();

            var result = await _ucpPaymentService.IssueCredentialAsync(request, platformProfileUrl, cancellationToken);

            return Ok(result);
        }
    }
}

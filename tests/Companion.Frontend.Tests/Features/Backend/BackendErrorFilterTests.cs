using System.Net;
using Companion.Frontend.Features.Backend;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Polly.Timeout;

namespace Companion.Frontend.Tests.Features.Backend;

public sealed class BackendErrorFilterTests
{
    private const string _secretDetail = "internal-backend-detail";

    private readonly FakeLogger<BackendErrorFilter> _logger = new();

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "401")]
    [InlineData(HttpStatusCode.Forbidden, "403")]
    [InlineData(HttpStatusCode.InternalServerError, "500")]
    public async Task InvokeAsync_ErrorStatus_LogsStatus(HttpStatusCode status, string expected)
    {
        await InvokeAsync(new HttpRequestException(_secretDetail, null, status));

        Assert.Equal(LogLevel.Warning, _logger.LatestRecord.Level);
        Assert.Equal($"Backend call failed with HttpRequestException, status {expected}", _logger.LatestRecord.Message);
    }

    [Fact]
    public async Task InvokeAsync_NoResponse_LogsNoStatus()
    {
        await InvokeAsync(new HttpRequestException(_secretDetail));

        Assert.Equal("Backend call failed with HttpRequestException, status none", _logger.LatestRecord.Message);
    }

    [Fact]
    public async Task InvokeAsync_Timeout_LogsNoStatus()
    {
        await InvokeAsync(new TimeoutRejectedException(_secretDetail));

        Assert.Equal("Backend call failed with TimeoutRejectedException, status none", _logger.LatestRecord.Message);
    }

    [Fact]
    public async Task InvokeAsync_Failure_OmitsExceptionDetails()
    {
        await InvokeAsync(new HttpRequestException(_secretDetail));

        Assert.DoesNotContain(_secretDetail, _logger.LatestRecord.Message, StringComparison.Ordinal);
        Assert.Null(_logger.LatestRecord.Exception);
    }

    private async Task InvokeAsync(Exception exception)
    {
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext());

        await new BackendErrorFilter(_logger).InvokeAsync(context, _ => throw exception);
    }
}

using Microsoft.AspNetCore.Mvc;

namespace Api.Errors;

public sealed class ApiProblemDetails : ProblemDetails
{
    public string Code { get; init; } = string.Empty;

    public string TraceId { get; init; } = string.Empty;

    public IDictionary<string, string[]>? Errors { get; init; }
}

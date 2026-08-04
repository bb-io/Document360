namespace Apps.Document360.Api.Services;

/// <summary>
/// The outcome of a publish call. Document360 answers a request to publish a version that is already live with a
/// failure rather than a no-op, which is reported here instead of being raised as an error.
/// </summary>
/// <param name="WasAlreadyLive">The requested version was already live, so nothing changed.</param>
/// <param name="Url">The public address of the published translation, absent when nothing changed.</param>
public record PublishArticleResult(bool WasAlreadyLive, string? Url);

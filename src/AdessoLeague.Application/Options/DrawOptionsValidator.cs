using Microsoft.Extensions.Options;

namespace AdessoLeague.Application.Options;

public sealed class DrawOptionsValidator : IValidateOptions<DrawOptions>
{
    public ValidateOptionsResult Validate(string? name, DrawOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (options.MaxPageSize < 1)
        {
            failures.Add($"{nameof(DrawOptions.MaxPageSize)} must be at least 1.");
        }

        if (options.DefaultPageSize < 1)
        {
            failures.Add($"{nameof(DrawOptions.DefaultPageSize)} must be at least 1.");
        }

        if (options.DefaultPageSize > options.MaxPageSize)
        {
            failures.Add(
                $"{nameof(DrawOptions.DefaultPageSize)} cannot exceed {nameof(DrawOptions.MaxPageSize)}.");
        }

        if (options.RequestsPerMinute < 1)
        {
            failures.Add($"{nameof(DrawOptions.RequestsPerMinute)} must be at least 1.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}

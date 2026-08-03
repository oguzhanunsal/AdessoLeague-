namespace AdessoLeague.Application.Options;

public sealed class DrawOptions
{
    public const string SectionName = "Draw";

    public int DefaultPageSize { get; set; } = 20;

    public int MaxPageSize { get; set; } = 100;

    public int RequestsPerMinute { get; set; } = 30;
}

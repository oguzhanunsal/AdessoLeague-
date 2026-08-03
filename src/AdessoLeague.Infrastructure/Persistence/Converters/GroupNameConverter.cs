using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AdessoLeague.Infrastructure.Persistence.Converters;

internal sealed class GroupNameConverter : ValueConverter<GroupName, string>
{
    public GroupNameConverter()
        : base(name => name.Value, value => GroupName.From(value))
    {
    }
}

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AdessoLeague.Infrastructure.Persistence.Converters;

internal sealed class GroupCountConverter : ValueConverter<GroupCount, int>
{
    public GroupCountConverter()
        : base(count => count.Value, value => GroupCount.Create(value).Value)
    {
    }
}

using AdessoLeague.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdessoLeague.Infrastructure.Persistence.Configurations;

internal sealed class DrawGroupConfiguration : IEntityTypeConfiguration<DrawGroup>
{
    public void Configure(EntityTypeBuilder<DrawGroup> builder)
    {
        builder.ToTable("draw_groups");

        builder.HasKey(group => group.Id);
        builder.Property(group => group.Id).ValueGeneratedNever();

        builder.Property(group => group.DrawId).IsRequired();

        builder.Property(group => group.Name)
            .HasConversion(new GroupNameConverter())
            .IsRequired()
            .HasMaxLength(1);

        builder.Property(group => group.Ordinal).IsRequired();

        builder.HasIndex(group => new { group.DrawId, group.Name }).IsUnique();

        builder.HasMany(group => group.Teams)
            .WithOne()
            .HasForeignKey(placement => placement.DrawGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(group => group.Teams).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

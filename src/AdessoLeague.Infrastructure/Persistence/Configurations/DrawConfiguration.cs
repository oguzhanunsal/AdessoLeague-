using AdessoLeague.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdessoLeague.Infrastructure.Persistence.Configurations;

internal sealed class DrawConfiguration : IEntityTypeConfiguration<Draw>
{
    public void Configure(EntityTypeBuilder<Draw> builder)
    {
        builder.ToTable("draws");

        builder.HasKey(draw => draw.Id);
        builder.Property(draw => draw.Id).ValueGeneratedNever();

        builder.OwnsOne(draw => draw.DrawnBy, drawnBy =>
        {
            drawnBy.Property(person => person.FirstName)
                .HasColumnName("drawn_by_first_name")
                .IsRequired()
                .HasMaxLength(DrawnBy.MaxNameLength);

            drawnBy.Property(person => person.LastName)
                .HasColumnName("drawn_by_last_name")
                .IsRequired()
                .HasMaxLength(DrawnBy.MaxNameLength);

            drawnBy.Ignore(person => person.FullName);
        });

        builder.Navigation(draw => draw.DrawnBy).IsRequired();

        builder.Property(draw => draw.GroupCount)
            .HasConversion(new GroupCountConverter())
            .HasColumnName("group_count")
            .IsRequired();

        builder.Property(draw => draw.Seed).IsRequired();

        builder.Property(draw => draw.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Ignore(draw => draw.PlacedTeamCount);
        builder.Ignore(draw => draw.IsComplete);

        builder.HasMany(draw => draw.Groups)
            .WithOne()
            .HasForeignKey(group => group.DrawId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(draw => draw.Groups).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

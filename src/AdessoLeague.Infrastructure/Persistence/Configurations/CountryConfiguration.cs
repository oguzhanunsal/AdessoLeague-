using AdessoLeague.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdessoLeague.Infrastructure.Persistence.Configurations;

internal sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("countries");

        builder.HasKey(country => country.Id);
        builder.Property(country => country.Id).ValueGeneratedNever();

        builder.Property(country => country.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(country => country.Name).IsUnique();

        builder.HasMany(country => country.Teams)
            .WithOne()
            .HasForeignKey(team => team.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        // The getter hands back a read-only wrapper, so the collection is only writable via the field.
        builder.Navigation(country => country.Teams).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasData(LeagueSeedData.Countries.Select(country => new
        {
            country.Id,
            country.Name,
        }));
    }
}

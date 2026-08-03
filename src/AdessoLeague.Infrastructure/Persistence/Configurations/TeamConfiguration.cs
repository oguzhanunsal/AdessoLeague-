using AdessoLeague.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdessoLeague.Infrastructure.Persistence.Configurations;

internal sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("teams");

        builder.HasKey(team => team.Id);
        builder.Property(team => team.Id).ValueGeneratedNever();

        builder.Property(team => team.CountryId).IsRequired();

        builder.Property(team => team.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(team => team.Name).IsUnique();

        builder.HasData(LeagueSeedData.Teams.Select(team => new
        {
            team.Id,
            team.CountryId,
            team.Name,
        }));
    }
}

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdessoLeague.Infrastructure.Persistence.Configurations;

internal sealed class DrawGroupTeamConfiguration : IEntityTypeConfiguration<DrawGroupTeam>
{
    public void Configure(EntityTypeBuilder<DrawGroupTeam> builder)
    {
        builder.ToTable("draw_group_teams");

        builder.HasKey(placement => placement.Id);
        builder.Property(placement => placement.Id).ValueGeneratedNever();

        builder.Property(placement => placement.DrawGroupId).IsRequired();
        builder.Property(placement => placement.DrawId).IsRequired();
        builder.Property(placement => placement.TeamId).IsRequired();
        builder.Property(placement => placement.Position).IsRequired();

        builder.HasOne(placement => placement.Team)
            .WithMany()
            .HasForeignKey(placement => placement.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        // Redundant with the group -> draw path, but it is what makes unique(draw_id, team_id) possible.
        builder.HasOne<Draw>()
            .WithMany()
            .HasForeignKey(placement => placement.DrawId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(placement => new { placement.DrawGroupId, placement.TeamId }).IsUnique();
        builder.HasIndex(placement => new { placement.DrawId, placement.TeamId }).IsUnique();
    }
}

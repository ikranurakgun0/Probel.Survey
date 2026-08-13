using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Probel.Survey.Domain.Entities;

namespace Probel.Survey.Infrastructure.Persistence.Configurations;

public class BolumConfiguration : IEntityTypeConfiguration<Bolum>
{
    public void Configure(EntityTypeBuilder<Bolum> b)
    {
        b.ToTable("BOLUM");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("ID");
        b.Property(x => x.Ad).HasColumnName("AD").HasMaxLength(200);
        b.Property(x => x.Sira).HasColumnName("SIRA");

        b.HasMany(x => x.Sorular)
            .WithOne()
            .HasForeignKey("BOLUM_ID");

        b.Metadata.FindNavigation(nameof(Bolum.Sorular))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Probel.Survey.Domain.Entities;

namespace Probel.Survey.Infrastructure.Persistence.Configurations;

public class SoruConfiguration : IEntityTypeConfiguration<Soru>
{
    public void Configure(EntityTypeBuilder<Soru> b)
    {
        b.ToTable("SORU");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("ID");
        b.Property(x => x.Metin).HasColumnName("METIN").HasMaxLength(1000);
        b.Property(x => x.Tip).HasColumnName("TIP").HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.ZorunluMu).HasColumnName("ZORUNLU_MU")
            .HasConversion(v => v ? 1 : 0, v => v == 1);
        b.Property(x => x.Sira).HasColumnName("SIRA");

        b.HasMany(x => x.Secenekler)
            .WithOne()
            .HasForeignKey("SORU_ID");

        b.Metadata.FindNavigation(nameof(Soru.Secenekler))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
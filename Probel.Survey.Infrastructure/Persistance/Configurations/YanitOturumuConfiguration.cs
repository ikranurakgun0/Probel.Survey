using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Probel.Survey.Domain.Entities;

namespace Probel.Survey.Infrastructure.Persistence.Configurations;

public class YanitOturumuConfiguration : IEntityTypeConfiguration<YanitOturumu>
{
    public void Configure(EntityTypeBuilder<YanitOturumu> b)
    {
        b.ToTable("YANIT_OTURUMU");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("ID");
        b.Property(x => x.DavetId).HasColumnName("DAVET_ID");
        b.Property(x => x.BaslangicZamani).HasColumnName("BASLANGIC_ZAMANI");
        b.Property(x => x.BitisZamani).HasColumnName("BITIS_ZAMANI");
        b.Property(x => x.TamamlandiMi).HasColumnName("TAMAMLANDI_MI")
            .HasConversion(v => v ? 1 : 0, v => v == 1);

        b.HasMany(x => x.Yanitlar)
            .WithOne()
            .HasForeignKey("YANIT_OTURUMU_ID");

        b.Metadata.FindNavigation(nameof(YanitOturumu.Yanitlar))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        b.HasOne(x => x.Demografi)
            .WithOne()
            .HasForeignKey<Demografi>(d => d.YanitOturumuId); //YanitOturumu ile Demografi arasındaki 1-1 ilişkiyi EF Core'a bildiriyor.
    }
}
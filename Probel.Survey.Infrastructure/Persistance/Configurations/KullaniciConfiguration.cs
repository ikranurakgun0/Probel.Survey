using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Probel.Survey.Domain.Entities;

namespace Probel.Survey.Infrastructure.Persistence.Configurations;

public class KullaniciConfiguration : IEntityTypeConfiguration<Kullanici>
{
    public void Configure(EntityTypeBuilder<Kullanici> b)
    {
        b.ToTable("KULLANICI");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("ID");
        b.Property(x => x.KullaniciAdi).HasColumnName("KULLANICI_ADI").HasMaxLength(100);
        b.Property(x => x.SifreHash).HasColumnName("SIFRE_HASH").HasMaxLength(500);
        b.Property(x => x.AdSoyad).HasColumnName("AD_SOYAD").HasMaxLength(200);
        b.Property(x => x.AktifMi).HasColumnName("AKTIF_MI")
            .HasConversion(v => v ? 1 : 0, v => v == 1);

        b.HasIndex(x => x.KullaniciAdi).IsUnique();
    }
}
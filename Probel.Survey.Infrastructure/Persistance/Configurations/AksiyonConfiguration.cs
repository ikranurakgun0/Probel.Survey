using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Probel.Survey.Domain.Entities;

namespace Probel.Survey.Infrastructure.Persistence.Configurations;

public class AksiyonConfiguration : IEntityTypeConfiguration<Aksiyon>
{
    public void Configure(EntityTypeBuilder<Aksiyon> b)
    {
        b.ToTable("AKSIYON");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("ID");
        b.Property(x => x.SoruId).HasColumnName("SORU_ID");
        b.Property(x => x.SorumluId).HasColumnName("SORUMLU_ID");
        b.Property(x => x.Aciklama).HasColumnName("ACIKLAMA").HasMaxLength(2000);
        b.Property(x => x.HedefTarih).HasColumnName("HEDEF_TARIH");
        b.Property(x => x.Durum).HasColumnName("DURUM").HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.OlusturmaTarihi).HasColumnName("OLUSTURMA_TARIHI");
    }
}
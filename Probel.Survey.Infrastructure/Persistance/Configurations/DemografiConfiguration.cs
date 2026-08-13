using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Probel.Survey.Domain.Entities;

namespace Probel.Survey.Infrastructure.Persistence.Configurations;

public class DemografiConfiguration : IEntityTypeConfiguration<Demografi>
{
    public void Configure(EntityTypeBuilder<Demografi> b)
    {
        b.ToTable("DEMOGRAFI");
        b.HasKey(x => x.YanitOturumuId);

        b.Property(x => x.YanitOturumuId).HasColumnName("YANIT_OTURUMU_ID");
        b.Property(x => x.KatilimciTuru).HasColumnName("KATILIMCI_TURU").HasMaxLength(20);
        b.Property(x => x.Cinsiyet).HasColumnName("CINSIYET").HasMaxLength(10);
        b.Property(x => x.YasAraligi).HasColumnName("YAS_ARALIGI").HasMaxLength(20);
        b.Property(x => x.EgitimDurumu).HasColumnName("EGITIM_DURUMU").HasMaxLength(30);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Probel.Survey.Domain.Entities;

namespace Probel.Survey.Infrastructure.Persistence.Configurations;

public class YanitConfiguration : IEntityTypeConfiguration<Yanit>
{
    public void Configure(EntityTypeBuilder<Yanit> b)
    {
        b.ToTable("YANIT");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("ID");
        b.Property(x => x.SoruId).HasColumnName("SORU_ID");
        b.Property(x => x.SecenekId).HasColumnName("SECENEK_ID");
        b.Property(x => x.MetinDeger).HasColumnName("METIN_DEGER").HasMaxLength(2000);
    }
}
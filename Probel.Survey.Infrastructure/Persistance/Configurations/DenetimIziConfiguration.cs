using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Probel.Survey.Domain.Entities;

namespace Probel.Survey.Infrastructure.Persistence.Configurations;

public class DenetimIziConfiguration : IEntityTypeConfiguration<DenetimIzi>
{
    public void Configure(EntityTypeBuilder<DenetimIzi> b)
    {
        b.ToTable("DENETIM_IZI");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("ID");
        b.Property(x => x.KullaniciId).HasColumnName("KULLANICI_ID");
        b.Property(x => x.Islem).HasColumnName("ISLEM").HasMaxLength(100);
        b.Property(x => x.HedefTablo).HasColumnName("HEDEF_TABLO").HasMaxLength(50);
        b.Property(x => x.HedefId).HasColumnName("HEDEF_ID");
        b.Property(x => x.Zaman).HasColumnName("ZAMAN");
    }
}
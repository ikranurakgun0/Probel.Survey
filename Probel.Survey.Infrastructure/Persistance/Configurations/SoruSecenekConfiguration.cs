using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Probel.Survey.Domain.Entities;

namespace Probel.Survey.Infrastructure.Persistence.Configurations;

public class SoruSecenekConfiguration : IEntityTypeConfiguration<SoruSecenek>
{
    public void Configure(EntityTypeBuilder<SoruSecenek> b)
    {
        b.ToTable("SORU_SECENEK");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("ID");
        b.Property(x => x.Metin).HasColumnName("METIN").HasMaxLength(200);
        b.Property(x => x.Agirlik).HasColumnName("AGIRLIK");
        b.Property(x => x.Sira).HasColumnName("SIRA");
    }
}
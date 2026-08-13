using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Probel.Survey.Domain.Entities;

namespace Probel.Survey.Infrastructure.Persistence.Configurations;

public class AnketSurumConfiguration : IEntityTypeConfiguration<AnketSurum>
{
    public void Configure(EntityTypeBuilder<AnketSurum> b)
    {
        b.ToTable("ANKET_SURUM");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("ID");
        b.Property(x => x.AnketId).HasColumnName("ANKET_ID");
        b.Property(x => x.SurumNo).HasColumnName("SURUM_NO");
        b.Property(x => x.Durum).HasColumnName("DURUM").HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.YayinTarihi).HasColumnName("YAYIN_TARIHI");

        b.HasMany(x => x.Bolumler)
            .WithOne()
            .HasForeignKey("ANKET_SURUM_ID");

        b.Metadata.FindNavigation(nameof(AnketSurum.Bolumler))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
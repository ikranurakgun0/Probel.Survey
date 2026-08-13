using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Probel.Survey.Domain.Entities;

namespace Probel.Survey.Infrastructure.Persistence.Configurations;

public class DavetConfiguration : IEntityTypeConfiguration<Davet>
{
    public void Configure(EntityTypeBuilder<Davet> b)
    {
        b.ToTable("DAVET");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("ID");
        b.Property(x => x.AnketSurumId).HasColumnName("ANKET_SURUM_ID");
        b.Property(x => x.Token).HasColumnName("TOKEN").HasMaxLength(64);
        b.Property(x => x.OlusturmaTarihi).HasColumnName("OLUSTURMA_TARIHI");
        b.Property(x => x.SonGecerlilik).HasColumnName("SON_GECERLILIK");
        b.Property(x => x.Durum).HasColumnName("DURUM").HasConversion<string>().HasMaxLength(20);

        b.HasIndex(x => x.Token).IsUnique();//Bu satır, "Token sütunu asla tekrar edemez"
                                            //kısıtını EF Core'a da bildiriyor
                                            //şemandaki UNIQUE kısıtının kod tarafındaki karşılığı.
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Probel.Survey.Domain.Entities;

namespace Probel.Survey.Infrastructure.Persistence.Configurations;

public class AnketConfiguration : IEntityTypeConfiguration<Anket>
{
    public void Configure(EntityTypeBuilder<Anket> b)
    {
        b.ToTable("ANKET");
        b.HasKey(x => x.Id);

        b.Property(x => x.Id).HasColumnName("ID");
        b.Property(x => x.Ad).HasColumnName("AD").HasMaxLength(200);
        b.Property(x => x.HizmetTuru).HasColumnName("HIZMET_TURU").HasMaxLength(30);
    }
}
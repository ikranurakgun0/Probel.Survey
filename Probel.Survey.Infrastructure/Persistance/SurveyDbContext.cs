using Microsoft.EntityFrameworkCore;
using Probel.Survey.Domain.Entities;

namespace Probel.Survey.Infrastructure.Persistence;

public class SurveyDbContext : DbContext
{
    public SurveyDbContext(DbContextOptions<SurveyDbContext> options) : base(options) { }
    public DbSet<Anket> Anketler => Set<Anket>();
    public DbSet<AnketSurum> AnketSurumleri => Set<AnketSurum>(); //Oracle'da ki ANKET_SURUM tablosuna AnketSurumleri ile erişebiliriz anlamına gelir.
    public DbSet<Bolum> Bolumler => Set<Bolum>();
    public DbSet<Soru> Sorular => Set<Soru>();
    public DbSet<SoruSecenek> SoruSecenekleri => Set<SoruSecenek>();
    public DbSet<Davet> Davetler => Set<Davet>();
    public DbSet<YanitOturumu> YanitOturumlari => Set<YanitOturumu>();   
    public DbSet<Demografi> Demografiler => Set<Demografi>();            
    public DbSet<Yanit> Yanitlar => Set<Yanit>();
    public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();
    public DbSet<Aksiyon> Aksiyonlar => Set<Aksiyon>();
    public DbSet<DenetimIzi> DenetimIzleri => Set<DenetimIzi>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(SurveyDbContext).Assembly); //Aynı projedeki tüm Configurationları otomatik bulup uygular.
    }
}
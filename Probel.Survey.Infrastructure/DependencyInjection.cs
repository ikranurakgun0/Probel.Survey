using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Probel.Survey.Domain.Repositories;
using Probel.Survey.Domain.Services;
using Probel.Survey.Infrastructure.Persistence;
using Probel.Survey.Infrastructure.Persistence.Repositories;
using Probel.Survey.Infrastructure.Services;

namespace Probel.Survey.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SurveyDbContext>(opt =>
            opt.UseOracle(configuration.GetConnectionString("OracleDb")));//secrets.json (User Secrets)
        //Bu dosya, projenin hiçbir dosyasında görünmüyor ama builder.Configuration.GetConnectionString("OracleDb")
        //çağrıldığında.NET otomatik olarak oraya bakıyor.Bağlantı dizeni(kullanıcı adı, şifre, Oracle adresi) burada,
        //proje klasörünün tamamen dışında duruyor — Git'e asla gitmez.

        services.AddScoped<IAnketRepository, AnketRepository>();//Bu dosya, "birisi IAnketRepository isterse, ona AnketRepository'yi ver" diyen kayıt defteri.
        services.AddScoped<IKullaniciRepository, KullaniciRepository>();
        services.AddScoped<ITokenUretici, TokenUretici>();
        services.AddScoped<IQrKodUretici, QrKodUretici>();
        services.AddScoped<IDenetimKaydedici, DenetimKaydedici>();
        services.AddScoped<IBildirimGonderici, GmailBildirimGonderici>();
        return services;
    }

}
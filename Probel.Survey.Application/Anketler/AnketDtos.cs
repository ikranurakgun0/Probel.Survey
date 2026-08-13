namespace Probel.Survey.Application.Anketler;

public record AnketListeDto(long Id, int SurumNo, string Durum, string AnketAdi, string? HizmetTuru);
public record SoruDetayDto(long Id, string Metin, string Tip, bool ZorunluMu);
public record BolumDetayDto(long Id, string Ad, int Sira, List<SoruDetayDto> Sorular);
public record AnketDetayDto(long Id, int SurumNo, string Durum, List<BolumDetayDto> Bolumler); //Neden bu üç DTO — hiyerarşik yapıyı ekrana taşımak için. AnketDetayDto,
                                                                                               //içinde bir BolumDetayDto listesi taşıyor;
                                                                                               //her BolumDetayDto da içinde bir SoruDetayDto listesi taşıyor.
                                                                                               //Domain'deki iç içe yapıyı (AnketSurum → Bolumler → Sorular),
                                                                                               //View'a aynı şekilde ama "sade veri" olarak yansıtıyoruz.
public record DavetDto(long Id, string Token, string Durum, DateTime SonGecerlilik);
public record SecenekDto(long Id, string Metin);
public record SoruDoldurmaDto(long Id, string Metin, string Tip, bool ZorunluMu, List<SecenekDto> Secenekler);
public record BolumDoldurmaDto(long Id, string Ad, List<SoruDoldurmaDto> Sorular);
public record AnketDoldurmaDto(long AnketSurumId, string Token, List<BolumDoldurmaDto> Bolumler);
public record CevapGirisi(long SoruId, long? SecenekId, string? MetinDeger);
public record SoruRaporDto(long SoruId, string SoruMetni, int CevapSayisi, double KarsilanmaOrani, bool DusukPerformans, int AcikAksiyonSayisi);
public record AnketRaporDto(long AnketSurumId, int ToplamKatilim, double GenelSkor, List<SoruRaporDto> SoruSonuclari, List<string> AcikUcluYorumlar);
public record AksiyonListeDto(long Id, string SoruMetni, string Aciklama, DateTime? HedefTarih, string Durum, DateTime OlusturmaTarihi);
public record DenetimIziDto(long Id, long? KullaniciId, string Islem, string? HedefTablo, long? HedefId, DateTime Zaman);
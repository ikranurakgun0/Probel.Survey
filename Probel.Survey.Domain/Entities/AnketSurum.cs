using System;
using System.Collections.Generic;
using System.Text;

namespace Probel.Survey.Domain.Entities;

public enum AnketDurumu
{
    Taslak = 0,
    Yayinda = 1,
    Arsiv = 2
}

public class AnketSurum
{
    public long Id { get; private set; } //Bunu sadece ben düzenleyebilirim. Anlamına gelir. 
    public long AnketId { get; private set; }
    public int SurumNo { get; private set; }
    public AnketDurumu Durum { get; private set; }
    public DateTime? YayinTarihi { get; private set; }

    private readonly List<Bolum> _bolumler = new(); // gerçek liste — private
    public IReadOnlyCollection<Bolum> Bolumler => _bolumler.AsReadOnly(); // dışarıya salt-okunur görünüm

    private AnketSurum() { }   // EF Core icin bos kurucu olması şarttır. Ama biz bunu kullanmayacağız. Bu yüzden private yaptık.

    public AnketSurum(long anketId, int surumNo)
    {
        AnketId = anketId;
        SurumNo = surumNo;
        Durum = AnketDurumu.Taslak;
    }

    public void BolumEkle(Bolum bolum)
    {
        if (Durum != AnketDurumu.Taslak)
            throw new InvalidOperationException("Yalnızca taslak ankete bölüm eklenebilir.");

        _bolumler.Add(bolum);
    }

    public void Yayinla()
    {
        if (_bolumler.Count == 0)
            throw new InvalidOperationException("Sorusu olmayan anket yayınlanamaz.");

        if (!_bolumler.Any(b => b.Sorular.Any()))
            throw new InvalidOperationException("En az bir soru olmalı.");

        if (Durum == AnketDurumu.Yayinda)
            throw new InvalidOperationException("Anket zaten yayında.");

        Durum = AnketDurumu.Yayinda;
        YayinTarihi = DateTime.UtcNow;
    }
    public void Arsivle()
    {
        if (Durum != AnketDurumu.Yayinda)
            throw new InvalidOperationException("Yalnızca yayındaki anketler arşivlenebilir.");

        Durum = AnketDurumu.Arsiv;
    }
}
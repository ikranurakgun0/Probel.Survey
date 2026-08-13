using System;
using System.Collections.Generic;
using System.Text;

namespace Probel.Survey.Domain.Entities;

public class Bolum
{
    public long Id { get; private set; }
    public string Ad { get; private set; } = null!;
    public int Sira { get; private set; }

    private readonly List<Soru> _sorular = new(); 
    public IReadOnlyCollection<Soru> Sorular => _sorular.AsReadOnly();

    private Bolum() { }

    public Bolum(string ad, int sira)
    {
        if (string.IsNullOrWhiteSpace(ad))
            throw new ArgumentException("Bölüm adı boş olamaz.");

        Ad = ad;
        Sira = sira;
    }

    public void SoruEkle(Soru soru) => _sorular.Add(soru);
}
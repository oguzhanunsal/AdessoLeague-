# ADR 0002 — Kura algoritması: randomized backtracking

Durum: kabul edildi

## Bağlam

Kura, sözleşmede tanımlı bir sırayla ilerlemek zorunda: `flatIndex = 0..31`, `grup = flatIndex % n`.
Yani önce her grubun 1. takımı, sonra her grubun 2. takımı. Tek kısıt: bir grupta aynı ülkeden iki
takım olamaz.

En doğrudan uygulama "her adımda uygun adaylar arasından rastgele seç, asla geri dönme" olurdu.
Bu yaklaşım çıkmaza girebilir: son turlarda bir grup için havuzda uygun takım kalmayabilir.

Çıkmaz oranı bu depoda ölçüldü. 20.000 deneme (seed 0..19999), aynı sıra ve aynı kısıt:

| Yaklaşım | n=4 | n=8 |
|---|---|---|
| Naif greedy | 0 / 20.000 (%0,000) | **5.850 / 20.000 (%29,250)** |
| Backtracking | 0 / 20.000 | 0 / 20.000 |

`n=8` için naif yaklaşım denemelerin yaklaşık üçte birinde geçerli bir kura üretemiyor. `n=4`'te
çıkmaz gözlenmedi; bu şaşırtıcı değil, çünkü her grup 8 takım alır ve 8 ülke vardır — her grup
ülkelerin bir permütasyonu olmak zorundadır ve kısıt çok daha gevşektir.

## Karar

`BacktrackingRoundRobinStrategy`: yerleştirme sırası korunur, ama bir dal çıkmaza girerse son
yerleştirme geri alınır ve sıradaki aday denenir.

```
TryFill(flatIndex):
    flatIndex == 32 ise başarı
    adaylar = havuzdaki, tüm IDrawRule'ları geçen takımlar
    adayları IRandomProvider ile karıştır
    her aday için:
        yerleştir
        TryFill(flatIndex + 1) başarılıysa başarı
        yerleştirmeyi geri al
    başarısız
```

Rastgelelik `IRandomProvider` üzerinden gelir ve seed `draws.seed` kolonuna yazılır.

Değerlendirilen alternatif: "başarısız olursa baştan başla" (restart-on-failure). Doğru sonuç verir
ve uygulaması daha basittir, ama en kötü durum süresi sınırsızdır — her deneme %29 olasılıkla
başarısız olduğunda beklenen deneme sayısı sonlu olsa da üst sınır yoktur. Backtracking'de arama
uzayı sonlu ve tüketilebilir olduğu için geçerli bir dağılım varsa mutlaka bulunur.

## Sonuçlar

**Kazanç.** Başarısızlık pratikte imkânsız: 40.000 kurada hiç başarısızlık gözlenmedi, kura başına
maliyet ~0,1 ms. Sıra bozulmaz — `flatIndex` daima ileri gider. Rastgelelik korunur: adaylar her
düğümde yeniden karıştırılır, backtracking yalnızca geçersiz dalları eler.

**Bedel.** Aggregate'in append-only kalması bozuldu: `Draw` ve `DrawGroup` üzerine geri alma
metotları eklendi. Bunlar `internal` tutuldu, böylece `Domain` dışından erişilemiyor ve aggregate
diğer tüm çağıranlara append-only görünüyor.

**Sınır.** Yeniden üretilebilen şey takım dağılımıdır; `DrawGroup` ve `DrawGroupTeam` satır
kimlikleri `Guid.NewGuid()` ile üretildiği için aynı seed farklı kimlikler verir. Kimliklerin de
deterministik olması gerekseydi bunlar da draw id + sıra numarasından türetilirdi.

**Koruma.** Bu davranış `DrawEngineTests` içindeki fuzz testiyle korunuyor: `n=4` ve `n=8` için
10.000'er seed, her sonuçta 7 invariant. Ayrıca `DrawEngine`, stratejinin çıktısını veritabanına
gitmeden önce aynı invariantlarla yeniden denetler.

---
name: draw-domain-rules
description: Kura (draw) algoritması, grup dağıtımı, ülke kısıtı, backtracking veya round-robin sırası ile ilgili herhangi bir kod yazılacağında kullan. Algoritmanın referans tanımını, çıkmaz (dead-end) problemini ve doğrulanması gereken invariantları içerir.
---

# Kura Algoritması — Referans Tanım

## Girdi
- `groupCount` (n) ∈ {4, 8}
- `drawnBy` (ad, soyad)
- Sabit havuz: 8 ülke × 4 takım = 32 takım

## Çıktı
`n` grup; grup adları sırayla A, B, C, ... ; her grupta `32 / n` takım.

## Zorunlu akış (sözleşmede açıkça isteniyor)
```
for slot in 0 .. (32/n - 1):
    for group in 0 .. (n-1):
        havuzdan, o gruba uygun bir takımı RASTGELE seç ve yerleştir
```
Yani sütun sütun değil, **satır satır** doldurulur. Önce tüm grupların 1. takımı,
sonra tüm grupların 2. takımı.

## Kısıt
`OneTeamPerCountryRule`: bir grupta aynı ülkeden ikinci takım olamaz.

## ⚠ Çıkmaz (dead-end) problemi — bu projenin can alıcı noktası
Naif yaklaşım ("uygun adaylar arasından rastgele seç, geri dönme") **n=8 için
denemelerin ~%28.6'sında çıkmaza girer**: son turlarda bir grup için uygun
takım kalmaz. (n=4'te ölçülen çıkmaz oranı %0'dır, ama garanti değildir.)

Bu yüzden çözüm **randomized backtracking**:

```csharp
bool TryFill(int flatIndex)          // flatIndex = slot * n + group
{
    if (flatIndex == 32) return true;
    var group = groups[flatIndex % n];
    foreach (var team in Shuffle(pool.Where(t => rules.All(r => r.IsSatisfied(group, t)))))
    {
        pool.Remove(team); group.Add(team);
        if (TryFill(flatIndex + 1)) return true;
        group.RemoveLast(); pool.Add(team);       // geri al
    }
    return false;                                  // bu dal çıkmaz
}
```

- `Shuffle` → `IRandomProvider` üzerinden; seed kaydedilir → kura **tekrar üretilebilir**.
- Rastgelelik kaybolmaz: adaylar her düğümde karıştırılır, backtracking yalnızca
  geçersiz dalları eler.
- Alternatif "restart-on-failure" yaklaşımı da çalışır ama en kötü durum sınırsızdır;
  backtracking tercih edilir. Bu kararı README'ye ADR olarak yaz.

## Doğrulanacak invariantlar (her testte)
1. Dağıtılan takım sayısı = 32
2. Takım tekrarı yok (`teams.Distinct().Count() == 32`)
3. Her grupta ülke tekrarı yok
4. `groups.Count == n` ve her grubun boyutu `32 / n`
5. Grup adları A'dan başlayarak sırayla, boşluksuz
6. Yerleştirme sırası round-robin (position alanı ile doğrulanır)
7. n=4 ise her grupta 8 ülkenin **hepsi** tam birer kez bulunur

## Doğrulama
`groupCount` 4 veya 8 değilse → `ValidationError`, HTTP 400 + ProblemDetails.
Ad/soyad boş veya > 100 karakter → 400. Trim uygula, iç boşlukları normalize et.

## Kalıcılık
Kura sonucu tek transaction'da yazılır: `draws` + `draw_groups` + `draw_group_teams`.
`seed` kolonu saklanır → aynı kura yeniden üretilebilir (denetlenebilirlik).

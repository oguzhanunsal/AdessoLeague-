# ADR 0003 — PostgreSQL + EF Core 8

Durum: kabul edildi

## Bağlam

Kura sonucu ve kurayı çeken kişi kalıcı olarak saklanmalı. Veri ilişkisel: kura → gruplar →
yerleşimler → takımlar → ülkeler. Kayıt bütünlüğü kritik — yarım yazılmış bir kura geçersizdir.

Aynı zamanda birkaç kural veritabanı seviyesinde garanti altına alınabilir: bir kurada aynı grup adı
iki kez olamaz, bir takım aynı kurada iki gruba yerleşemez.

## Karar

PostgreSQL 16, EF Core 8 + Npgsql 8, şema `snake_case` (EFCore.NamingConventions).

- Kura tek `SaveChangesAsync` çağrısıyla yazılır: `draws` + `draw_groups` + `draw_group_teams`
  aynı transaction'da.
- Kurallar unique index olarak da kodlanır: `unique(draw_id, name)`,
  `unique(draw_group_id, team_id)`, `unique(draw_id, team_id)`.
- Ülke ve takım verisi `HasData` ile ilk migration'da seed edilir; kimlikler 1 tabanlı sıra
  numaralarından türetilir, böylece her makinede aynıdır ve sonraki migration'lar bu satırları
  değiştirmez.
- Value object'ler kolonlara indirgenir: `DrawnBy` sahipli tip olarak iki kolona, `GroupCount` ve
  `GroupName` value converter ile `int` ve `varchar(1)`'e.
- Zaman `timestamptz` olarak, daima UTC.
- Okuma sorguları `AsNoTracking` + projeksiyon kullanır; `GET /draws/{id}` tek SQL sorgusu üretir.

Sürümler `net8.0` hedefiyle uyumlu olacak şekilde 8.x ailesinde sabitlenmiştir; 9.x paketleri
`net9.0` gerektirir.

## Sonuçlar

**Kazanç.** Bütünlük iki katmanda korunuyor: aggregate kuralları uygulama tarafında, unique
index'ler veritabanı tarafında. Uygulama hatası olsa bile veritabanı geçersiz bir kurayı kabul
etmez. Migration'lar şema geçmişini kod olarak taşıyor.

**Bedel.** `unique(draw_id, team_id)` indeksinin kurulabilmesi için `draw_group_teams` tablosu
`draw_id`'yi denormalize olarak taşımak zorunda kaldı — bu kolon `draw_groups` üzerinden zaten
türetilebilir. Ayrıca `draw_group_teams`'ten `draws`'a iki ayrı cascade yolu oluştu; PostgreSQL bunu
kabul eder, SQL Server etmezdi.

Value converter'lı özellikler SQL içinde açılamıyor: `group.Name.Value` gibi bir ifade sunucu
tarafında çevrilemez. Bu yüzden okuma projeksiyonu önce anonim bir şekle projekte ediyor, string'e
dönüşüm tek round-trip'ten sonra yapılıyor. Sorgu sayısı yine 1.

EF materialization için entity'lere private parametresiz constructor'lar eklendi. Bunlar hiçbir
yerden çağrılmıyor gibi görünür; silinirse derleme geçer ama çalışma zamanında materialization
kırılır.

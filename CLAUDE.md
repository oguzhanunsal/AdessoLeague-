# Adesso World League — Draw Service

## Proje Amacı
32 takımı (8 ülke × 4 takım) n gruba (n ∈ {4, 8}) kurayla dağıtan .NET Web API.
Kura sonucu ve kurayı çeken kişi PostgreSQL'e kaydedilir.

## Domain Kuralları (ASLA İHLAL EDİLMEZ)
1. Bir grupta aynı ülkeden **birden fazla** takım olamaz.
2. Bir takım **yalnızca bir** gruba ait olabilir; 32 takımın hepsi dağıtılır.
3. Çekiliş sırası **round-robin**: Grup A slot 1 → Grup B slot 1 → ... → Grup n slot 1
   → Grup A slot 2 → ... Tüm gruplar dolana kadar.
4. n yalnızca **4** veya **8** olabilir. n=4 → grup başına 8 takım (her ülkeden tam 1).
   n=8 → grup başına 4 takım (4 farklı ülke).
5. Grup adları sırayla: A, B, C, D, E, F, G, H.
6. Kurayı çeken kişinin **ad ve soyadı** zorunlu parametredir ve kayıtla birlikte saklanır.

### Kritik teknik not
Naif "rastgele uygun takım seç" yaklaşımı **n=8 için ~%28.6 oranında çıkmaza girer**
(ölçüldü, 20.000 deneme). Bu yüzden algoritma **randomized backtracking** kullanır:
çekiliş sırası korunur, ama bir slot çıkmaza sokarsa geri alınır. Sonuç her zaman geçerlidir.
Bu davranış `DrawEngineTests` içinde fuzz testiyle korunur — bozmayın.

## Mimari
Clean Architecture, 4 katman. Bağımlılık yönü daima içeri doğru:

```
src/
  AdessoLeague.Domain/         → saf C#, hiçbir NuGet bağımlılığı yok
  AdessoLeague.Application/    → MediatR handler'lar, DTO, validation, port interface'leri
  AdessoLeague.Infrastructure/ → EF Core, Npgsql, repository, RandomProvider
  AdessoLeague.Api/            → RESTful Controller'lar, DI, middleware, filters
tests/
  AdessoLeague.UnitTests/
  AdessoLeague.IntegrationTests/
```

- **Domain** hiçbir şeye referans vermez. `Infrastructure` ve `Api`, `Application`'a bakar.
- Domain katmanında `using Microsoft.EntityFrameworkCore` görürsen bu bir hatadır.

## Kullanılan Pattern'ler (bilinçli tercih)
| Pattern | Nerede | Neden |
|---|---|---|
| CQRS | `Application/Features/**` (MediatR) | Command/Query ayrımı, okuma-yazma farklı modeller |
| Mediator | MediatR 12.5.0 | Controller ile handler arasında gevşek bağ |
| Pipeline / Decorator | `ValidationBehavior`, `LoggingBehavior` | Kesişen ilgiler handler'a sızmaz |
| Strategy | `IDrawStrategy` → `BacktrackingRoundRobinStrategy` | Algoritma değiştirilebilir, test edilebilir |
| Specification / Rule | `IDrawRule` → `OneTeamPerCountryRule` | Kural eklemek algoritmayı değiştirmez (OCP) |
| Result | `Result<T>` | Akış kontrolü için exception atmayız |
| Repository + UoW | `IDrawRepository`, `IUnitOfWork` | Persistence detayı Application'a sızmaz |
| Factory | `DrawFactory` | Aggregate kurulum kuralları tek yerde |
| Options | `DrawOptions` | Sihirli sabit yok |

## Kodlama Kuralları
- **.NET 8 (LTS) / C# 12**. `Nullable` ve `TreatWarningsAsErrors` açık.
- `record` for DTO/value object, `sealed class` for entity, `readonly struct` yok gereksizse.
- Domain entity setter'ları `private set`; mutasyon yalnızca metotlarla.
- Primary constructor + `file-scoped namespace` + `global usings` kullan.
- Async metotlar `CancellationToken` alır. `async void` yasak.
- `DateTime.UtcNow` doğrudan çağrılmaz → `ITimeProvider`/`TimeProvider` enjekte edilir.
- `Random` doğrudan `new`'lenmez → `IRandomProvider` enjekte edilir (seed ile deterministik test).
- Public tip ve metotlarda XML doc yalnızca davranış aşikâr değilse.
- Magic string yok; grup adları `GroupName.Sequence` üzerinden.

## API Sözleşmesi
RESTful, attribute-routed controller'lar (`[ApiController] : ControllerBase`).
Minimal API kullanılmaz. Her controller action `ActionResult<T>` döner,
`[ProducesResponseType]` ile tüm olası durum kodları belgelenir.

```
POST /api/v1/draws
  body: { "groupCount": 8, "drawnBy": { "firstName": "Oğuzhan", "lastName": "Ünsal" } }
  201 → Location: /api/v1/draws/{id}, body: DrawResponse
  400 → RFC 7807 ProblemDetails (validation)

GET  /api/v1/draws/{id}   → 200 DrawResponse | 404 ProblemDetails
GET  /api/v1/draws        → 200 sayfalı liste (kura geçmişi)
GET  /health/live , /health/ready
```

`DrawResponse` PDF'teki örnekle **birebir** uyumlu olmalı:
```json
{ "groups": [ { "groupName": "A", "teams": [ { "name": "Adesso İstanbul" } ] } ] }
```
Ek alanlar (`id`, `drawnBy`, `createdAtUtc`) `groups`'un yanına eklenebilir ama
`groups` şeması değişmez.

## Veritabanı
PostgreSQL 16, EF Core 8 + Npgsql 8. Şema `snake_case` (EFCore.NamingConventions 8.x).
Tüm EF Core / Npgsql / Design paketleri **8.x** sürüm ailesinde sabitlenir; 9.x'e çıkma.

- `countries(id, name)`
- `teams(id, country_id, name)`
- `draws(id, drawn_by_first_name, drawn_by_last_name, group_count, seed, created_at_utc)`
- `draw_groups(id, draw_id, name, ordinal)` — unique(draw_id, name)
- `draw_group_teams(id, draw_group_id, team_id, position)` — unique(draw_group_id, team_id)

Ülke/takım verisi migration içinde `HasData` ile seed edilir; runtime'da değişmez.
Migration dosyaları elle düzenlenmez, `dotnet ef migrations add` ile üretilir.

## Test Politikası
- Domain algoritması için **fuzz test**: n=4 ve n=8 için 10.000 rastgele seed,
  her sonuçta 6 invariant doğrulanır (32 takım, tekrar yok, ülke çakışması yok,
  grup boyutu, grup sayısı, round-robin sırası).
- Application handler'ları için unit test (validator dahil).
- API için Testcontainers.PostgreSql + `WebApplicationFactory` ile integration test.
- Test isimleri: `Method_Scenario_ExpectedResult`.
- **Yeni davranış eklerken önce test yaz.**

## Commit Disiplini
- Conventional Commits: `feat:`, `fix:`, `test:`, `refactor:`, `chore:`, `docs:`, `ci:`
- Her commit **derlenir ve testleri geçer**. Kırık commit yok.
- Commit mesajı Türkçe değil İngilizce; gövdede "neden" anlatılır, "ne" değil.
- Her adım sonunda commit at; büyük tek commit atma — süreç değerlendiriliyor.
- Commit'e `Co-Authored-By` veya AI imzası **ekleme**.

## Yapma
- **MediatR sürümünü 12.5.0'dan yükseltme.** 13.x ticari lisansa geçti; `Directory.Packages.props`
  içinde sürüm sabitlenmiştir, `dotnet add package` ile güncelleme yapma.
- MediatR'ın üstüne kendi dispatcher/servis katmanını sarma; controller doğrudan `ISender` alır.
- AutoMapper ekleme — mapping'i elle `ToResponse()` extension'ıyla yaz.
- Minimal API (`app.MapPost(...)`) kullanma; her endpoint bir controller action'ı.
- Repository içine iş kuralı koyma.
- Controller içine iş mantığı koyma; action yalnızca `ISender.Send()` çağırır ve sonucu maplar.
- .NET 9 / EF Core 9 API'si veya C# 13 dil özelliği kullanma (hedef net8.0).
- Kapsam dışına çıkma: auth, çok dillilik, admin CRUD, event sourcing **istenmedi**.
  Bunları README'nin "Bilinçli Kapsam Dışı" bölümünde gerekçesiyle belirt.

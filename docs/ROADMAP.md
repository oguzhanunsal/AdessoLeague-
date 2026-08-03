# 6 Saatlik Yol Haritası — Adım Kapsamları ve Kabul Kriterleri

Toplam: 360 dk / 13 adım / 13+ commit.
Her adımın sonunda: build yeşil + test yeşil + commit.

| # | Adım | Süre | Biterken commit |
|---|------|------|-----------------|
| 1 | İskelet, solution, .claude, docker-compose | 20 | `chore(repo): scaffold solution and dev environment` |
| 2 | Domain modeli + sabit takım/ülke verisi | 25 | `feat(domain): add league aggregate and reference data` |
| 3 | Kura motoru (strategy + rules + backtracking) | 35 | `feat(draw): implement backtracking round-robin draw engine` |
| 4 | Algoritma testleri (invariant + fuzz) | 25 | `test(draw): assert draw invariants over 10k seeds` |
| 5 | Persistence: EF Core, config, migration, seed | 35 | `feat(persistence): add ef core schema and seed migration` |
| 6 | Application: MediatR command, pipeline behavior, validation | 30 | `feat(application): add create-draw command with validation pipeline` |
| 7 | API: DrawsController POST + ProblemDetails + Swagger | 30 | `feat(api): expose draw creation endpoint` |
| 8 | Query: GET /draws/{id} ve GET /draws | 20 | `feat(api): add draw retrieval and history endpoints` |
| 9 | Cross-cutting: Serilog, exception handler, health, correlation | 25 | `feat(api): add structured logging, health checks and problem details` |
| 10 | Integration testler (Testcontainers) | 40 | `test(api): add end-to-end tests over a real postgres container` |
| 11 | Dockerfile, compose, GitHub Actions CI | 25 | `ci: containerize the api and run build+test on push` |
| 12 | README, ADR'ler, .http koleksiyonu | 30 | `docs: document architecture, decisions and how to run` |
| 13 | Son gözden geçirme, temizlik, tag | 20 | `chore(release): v1.0.0` |

---

## Adım 1 — İskelet (20 dk)
**Kapsam:** git init, .gitignore, .editorconfig, Directory.Build.props (**net8.0**, LangVersion 12,
nullable, TreatWarningsAsErrors), Directory.Packages.props (central package management —
MediatR **12.5.0** ve tüm EF paketleri **8.x** burada sabitlenir), 4 src + 2 tests projesi,
proje referansları, docker-compose (postgres 16), `.claude/` klasörünün yerleştirilmesi.
**Kabul:** `dotnet build` yeşil; `docker compose up -d db` ayakta; `dotnet run` boş API açılıyor.
**Kapsam dışı:** hiçbir iş mantığı.

## Adım 2 — Domain modeli (25 dk)
**Kapsam:** `Country`, `Team`, `Draw`, `DrawGroup`, `DrawGroupTeam` entity'leri;
`GroupCount` ve `DrawnBy` value object'leri; `GroupName.Sequence`; `LeagueCatalog`
(8 ülke × 4 takım sabit veri); `Result<T>` ve `Error` tipleri.
**Kabul:** Domain projesinde 0 NuGet paketi; `GroupCount.Create(5)` hata döner;
katalog tam 32 takım içerir (test edildi).

## Adım 3 — Kura motoru (35 dk)
**Kapsam:** `IDrawRule` + `OneTeamPerCountryRule`; `IDrawStrategy` +
`BacktrackingRoundRobinStrategy`; `IRandomProvider` + seed; `DrawEngine`.
**Kabul:** n=4 ve n=8 için geçerli sonuç; aynı seed → aynı sonuç; hiç `new Random()` yok.
**Dikkat:** Round-robin sırası korunmalı; backtracking sırayı bozmamalı.

## Adım 4 — Algoritma testleri (25 dk)
**Kapsam:** 7 invariant testi + 10.000 seed fuzz testi (n=4 ve n=8);
determinizm testi; geçersiz n testi.
**Kabul:** `dotnet test` yeşil, süre < 10 sn.

## Adım 5 — Persistence (35 dk)
**Kapsam:** EF Core **8.x** paketleri, `LeagueDbContext`, `IEntityTypeConfiguration` sınıfları, snake_case,
unique index'ler, `HasData` ile ülke/takım seed'i, ilk migration, `IDrawRepository` +
`IUnitOfWork` implementasyonu.
**Kabul:** `dotnet ef database update` çalışıyor; `countries` 8, `teams` 32 satır.

## Adım 6 — Application katmanı (30 dk)
**Kapsam:** MediatR 12.5.0 kurulumu; `CreateDrawCommand : IRequest<Result<DrawResponse>>` + handler;
`ValidationBehavior` ve `LoggingBehavior` pipeline behavior'ları; `FluentValidation` validator;
`DrawResponse`/`GroupResponse`/`TeamResponse` DTO'ları; `ToResponse()` mapper;
`GetDrawByIdQuery`, `GetDrawsQuery`.
**Kabul:** Handler unit testleri yeşil; Application'da EF Core referansı yok;
geçersiz komut handler'a hiç ulaşmadan `ValidationBehavior`'da kesiliyor.

## Adım 7 — API: kura oluşturma (30 dk)
**Kapsam:** `DrawsController : ControllerBase` (`[ApiController]`, attribute routing),
`POST /api/v1/draws` action'ı, 201 + `CreatedAtAction`, RFC 7807 hata gövdesi,
`[ProducesResponseType]` ile tam OpenAPI dokümantasyonu, Swagger, API versiyonlama.
**Kabul:** Swagger'dan n=8 çağrısı PDF'teki şemayla birebir JSON döndürüyor;
n=5 → 400 ProblemDetails.

## Adım 8 — API: sorgular (20 dk)
**Kapsam:** `GET /api/v1/draws/{id}` (404 dahil), `GET /api/v1/draws?page&size`
(kura geçmişi, kimin çektiği ile birlikte).
**Kabul:** Oluşturulan kura id ile geri okunuyor ve içerik aynı.

## Adım 9 — Cross-cutting (25 dk)
**Kapsam:** Serilog (JSON console), `IExceptionHandler` → ProblemDetails,
correlation id middleware, `/health/live` + `/health/ready` (Npgsql check),
`DrawOptions` (Options pattern).
**Kabul:** Beklenmedik hata 500 + ProblemDetails; loglarda correlation id var.

## Adım 10 — Integration testler (40 dk)
**Kapsam:** `Testcontainers.PostgreSql` + `WebApplicationFactory` fixture;
uçtan uca senaryolar: n=4, n=8, geçersiz n, boş isim, kayıt gerçekten DB'de mi,
GET ile geri okuma, eşzamanlı 10 istek.
**Kabul:** Tüm integration testler yeşil, izole (her test kendi şemasıyla).

## Adım 11 — Docker & CI (25 dk)
**Kapsam:** multi-stage `Dockerfile` (sdk:8.0 → aspnet:8.0-alpine, non-root), `docker-compose.yml`
(api + db + healthcheck + migration on startup), `.github/workflows/ci.yml`
(restore/build/test/format check).
**Kabul:** `docker compose up` sonrası tek komutla Swagger açılıyor.

## Adım 12 — Dokümantasyon (30 dk)
**Kapsam:** README (nasıl çalıştırılır, mimari şema, örnek istek/yanıt,
"neden backtracking" ölçüm sonucu, bilinçli kapsam dışı bırakılanlar),
`docs/adr/0001-*.md` (3 ADR), `requests.http`.
**Kabul:** Repoyu ilk kez gören biri 2 komutla ayağa kaldırabiliyor.

## Adım 13 — Son tur (20 dk)
**Kapsam:** `code-reviewer` full pass, `dotnet format`, TODO/yorum temizliği,
gereksiz dosya silme, `git log` okunabilirlik kontrolü, `v1.0.0` tag, push.
**Kabul:** Temiz çalışma dizini, anlamlı 13+ commit, CI yeşil.

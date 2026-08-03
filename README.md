# Adesso World League — Draw Service

32 takımı (8 ülke × 4 takım) `n` gruba (`n ∈ {4, 8}`) kurayla dağıtan .NET 8 Web API.
Kura sonucu ve kurayı çeken kişi PostgreSQL'e kaydedilir.

> Bu dosya proje ilerledikçe genişleyecek. Şu an yalnızca "nasıl çalıştırılır" bölümünü içerir.

## Gereksinimler

| Araç | Sürüm |
|---|---|
| .NET SDK | 8.0.4xx (`global.json` ile sabitlenmiştir) |
| Docker | Compose v2 destekleyen herhangi bir sürüm |

## Nasıl çalıştırılır

### 1. Veritabanını başlat

```bash
docker compose up -d db
```

PostgreSQL 16 `localhost:5433` üzerinde ayağa kalkar
(veritabanı `adesso_league`, kullanıcı/şifre `appuser`/`appuser`).
Konteynerin içindeki port 5432'dir; dışarıya 5433 açılır, çünkü makinede kurulu bir
PostgreSQL servisi 5432'yi sahiplenip konteyneri sessizce gölgeleyebiliyor.
Sağlık durumu:

```bash
docker compose ps
```

Şema ve sabit veri (8 ülke, 32 takım) API ilk kez Development ortamında başlatıldığında
migration ile otomatik oluşur. Elle uygulamak istersen:

```bash
dotnet ef database update -p src/AdessoLeague.Infrastructure -s src/AdessoLeague.Api
```

Bunun için `dotnet tool install --global dotnet-ef --version "8.*"` gerekir.

### 2. API'yi çalıştır

```bash
dotnet run --project src/AdessoLeague.Api
```

Swagger arayüzü: <http://localhost:5154/swagger>

### 3. Derle ve testleri koştur

```bash
dotnet build AdessoLeague.sln -c Release
```

```bash
dotnet test AdessoLeague.sln -c Release
```

### 4. Ortamı kapat

```bash
docker compose down
```

Veritabanı verisini de silmek için `docker compose down -v`.

## Çözüm yapısı

```
src/
  AdessoLeague.Domain/         saf C#, hiçbir NuGet bağımlılığı yok
  AdessoLeague.Application/    CQRS handler'ları, DTO'lar, port arayüzleri
  AdessoLeague.Infrastructure/ EF Core, Npgsql, repository implementasyonları
  AdessoLeague.Api/            RESTful controller'lar, DI, middleware
tests/
  AdessoLeague.UnitTests/
  AdessoLeague.IntegrationTests/
```

Bağımlılık yönü daima içeri doğrudur; `Domain` hiçbir projeye referans vermez.

## Sürüm yönetimi

Tüm NuGet sürümleri kök dizindeki [`Directory.Packages.props`](Directory.Packages.props)
içinde merkezî olarak sabitlenmiştir (central package management). Ortak derleme ayarları
(`net8.0`, C# 12, nullable, warnings-as-errors) [`Directory.Build.props`](Directory.Build.props)
içindedir.

#!/usr/bin/env bash
# PreToolUse: Clean Architecture katman ihlallerini yazılmadan önce engelle.
set -uo pipefail

INPUT=$(cat)
FILE=$(printf '%s' "$INPUT" | jq -r '.tool_input.file_path // empty')
BODY=$(printf '%s' "$INPUT" | jq -r '.tool_input.content // .tool_input.new_string // empty')

# Windows ters bölü ayracını normalize et; aşağıdaki katman pattern'leri tek ayraç varsayar.
FILE=${FILE//'\'//}

deny() {
  jq -n --arg r "$1" '{hookSpecificOutput:{hookEventName:"PreToolUse",permissionDecision:"deny",permissionDecisionReason:$r}}'
  exit 0
}

# --- Sürüm sabitleri: net8.0 / MediatR 12.5.0 / EF Core 8.x ---
case "$FILE" in
  *.csproj|*Directory.Build.props|*Directory.Packages.props|*Dockerfile*|*.yml|*.yaml)
    printf '%s' "$BODY" | grep -qE '<TargetFramework[s]?>[^<]*net(9|10)\.0' \
      && deny "Hedef framework net8.0 (LTS) olarak sabit. net9.0/net10.0 kullanma."
    printf '%s' "$BODY" | grep -qiE '"MediatR"[^>]*Version="1[3-9]' \
      && deny "MediatR 13.x ticari lisansa geçti. Sürüm 12.5.0'da kalmalı."
    printf '%s' "$BODY" | grep -qE '(EntityFrameworkCore|Npgsql[^"]*|EFCore\.NamingConventions)"[^>]*Version="9' \
      && deny "EF Core / Npgsql paketleri 8.x ailesinde kalmalı (net8.0 hedefi)."
    printf '%s' "$BODY" | grep -qE 'dotnet/(sdk|aspnet):9\.' \
      && deny "Docker imajları 8.0 olmalı (sdk:8.0 / aspnet:8.0-alpine)."
    ;;
esac

# --- API stili: Minimal API değil, controller ---
# Katmanlar proje adından bağımsız olarak son ekten tanınır: *.Api / *.Domain / *.Application
case "$FILE" in
  *.Api/*)
    printf '%s' "$BODY" | grep -qE '\b(app|group)\.Map(Post|Get|Put|Delete|Patch)\s*\(' \
      && deny "Bu projede Minimal API kullanılmıyor. Endpoint'i [ApiController] türeten bir controller action'ı olarak yaz."
    ;;
esac

case "$FILE" in
  *.Domain/*)
    printf '%s' "$BODY" | grep -qE 'using (Microsoft\.EntityFrameworkCore|Npgsql|Microsoft\.AspNetCore)' \
      && deny "Domain katmanı EF Core / Npgsql / ASP.NET referansı alamaz. Bu tipi Infrastructure'a taşı veya Domain'de bir port interface tanımla."
    printf '%s' "$BODY" | grep -qE '\bnew Random\(|DateTime\.(UtcNow|Now)' \
      && deny "Domain'de doğrudan Random/DateTime kullanma. IRandomProvider / TimeProvider enjekte et (deterministik test için)."
    ;;
  *.Application/*)
    printf '%s' "$BODY" | grep -qE 'using (Npgsql|Microsoft\.EntityFrameworkCore\b)' \
      && deny "Application katmanı somut persistence teknolojisine bağlanamaz. Repository interface'i üzerinden çalış."
    ;;
  *Migrations*)
    deny "Migration dosyaları elle düzenlenmez. 'dotnet ef migrations add <Name>' ile yeniden üret."
    ;;
esac
exit 0

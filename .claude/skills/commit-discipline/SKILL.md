---
name: commit-discipline
description: Bir adım tamamlanıp commit atılacağı zaman kullan. Commit mesajı formatı, commit öncesi kontrol listesi ve adım adım teslim disiplinini tanımlar.
---

# Commit Disiplini

Bu projede **süreç de önemli**. Tek büyük commit yerine,
her mantıksal adım ayrı commit olmalı; git log okunduğunda çalışma akışı
anlaşılmalı.

## Commit öncesi kontrol listesi
1. `dotnet build` — 0 warning, 0 error (warnings-as-errors açık)
2. `dotnet test` — yeşil
3. `dotnet format --verify-no-changes` — temiz
4. `git status` — kaçak dosya yok (`bin/`, `obj/`, `.env`, `*.user`)
5. `code-reviewer` agent'ını çalıştır, BLOCKER/MAJOR bulgu kalmasın

## Mesaj formatı (Conventional Commits, İngilizce)
```
<type>(<scope>): <özet, emir kipi, ≤72 karakter>

<neden — 1-3 satır. "ne yaptım" değil, "neden böyle yaptım">
```
Tipler: `feat` `fix` `test` `refactor` `perf` `docs` `chore` `ci` `build`
Scope örnekleri: `domain` `draw` `api` `persistence` `tests` `docker`

İyi örnek:
```
feat(draw): add backtracking to guarantee a valid group assignment

Naive greedy selection dead-ends in ~28% of runs when n=8, because a group
can be left with no eligible team in the final rounds. Backtracking keeps the
required round-robin order while making failure impossible.
```

Kötü örnek: `update files`, `fix bug`, `wip`

## Yasak
- `Co-Authored-By: Claude` veya benzeri AI imzası **ekleme**.
- `git push --force`
- Kırık (derlenmeyen / testi kırmızı) commit
- İki farklı işi tek commit'te birleştirme

## Adım sonu ritüeli
Her adım bitince: kontrol listesi → commit → tek cümlelik ilerleme raporu
("Adım 5/13 tamam: EF Core persistence + migration, 3 test eklendi").

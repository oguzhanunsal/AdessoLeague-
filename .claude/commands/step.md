---
description: Yol haritasındaki bir adımı uçtan uca yap ve commit'le
argument-hint: <adım numarası veya adım açıklaması>
allowed-tools: Read, Write, Edit, Glob, Grep, Bash, Task
---

Aşağıdaki adımı uçtan uca tamamla: **$ARGUMENTS**

Sıra:
1. `docs/ROADMAP.md` dosyasından bu adımın kapsamını ve kabul kriterlerini oku.
2. Kapsam dışına çıkma. "Bir de şunu ekleyeyim" yapma.
3. Gerekiyorsa önce testi yaz.
4. Uygula.
5. `dotnet build && dotnet test` — yeşil olana kadar bitirme.
6. `code-reviewer` subagent'ını çalıştır; BLOCKER/MAJOR bulguları düzelt.
7. `commit-discipline` skill'indeki formatla commit at.
8. Bana tek satırlık ilerleme raporu ver ve **dur** — sonraki adıma geçme.

Mevcut durum:
- !`git log --oneline -8`
- !`git status --short`

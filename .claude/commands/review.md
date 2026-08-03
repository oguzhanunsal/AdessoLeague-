---
description: Commit öncesi sıkı gözden geçirme
allowed-tools: Read, Glob, Grep, Bash, Task
---

Şu anki değişiklikleri commit öncesi gözden geçir.

1. `code-reviewer` subagent'ını çalıştır.
2. Ayrıca şunları kendin doğrula:
   - `POST /api/v1/draws` yanıtı `CLAUDE.md`'de sabitlenen `{ "groups": [ { "groupName", "teams": [ { "name" } ] } ] }` şemasıyla birebir uyumlu mu?
   - n=4 ve n=8 için grup ve takım sayıları doğru mu?
   - Aynı ülkeden iki takım aynı grupta olabilir mi — algoritmada bunu mümkün kılan bir yol var mı?
   - Kura sonucu ve çeken kişi gerçekten DB'ye yazılıyor mu (tek transaction)?
   - Geçersiz n için 400 + ProblemDetails dönüyor mu?
   - Tüm projeler net8.0 mı? MediatR sürümü 12.5.0 mı? EF paketleri 8.x mi?
   - Minimal API (`app.MapPost`/`MapGet`) sızmış mı — sızmışsa BLOCKER.
   - Controller action'ları `ActionResult<T>` dönüyor ve `[ProducesResponseType]` tam mı?
   - Validation, handler içinde değil `ValidationBehavior` pipeline'ında mı kesiliyor?
3. Bulguları önem sırasına göre listele. Kod değiştirme, sadece raporla.

- !`git diff --stat`
- !`git diff`

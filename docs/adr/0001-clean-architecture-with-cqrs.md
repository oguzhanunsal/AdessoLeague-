# ADR 0001 — Clean Architecture ve CQRS

Durum: kabul edildi

## Bağlam

Servisin işi küçük ama çekirdeğinde ölçülebilir bir algoritma var: 32 takımı kısıt altında gruplara
dağıtmak. Bu algoritmanın hızlı ve tekrarlanabilir biçimde test edilebilmesi gerekiyor — 20.000
kuralık bir fuzz koşumu, veritabanı veya HTTP katmanına dokunmadan saniyeler içinde bitmeli.

Aynı zamanda okuma ve yazma yolları farklı şekillerde ihtiyaç duyuyor: yazma tarafı `Draw`
aggregate'ini kurup kurallarını uygulamak zorunda, okuma tarafı ise yalnızca düz bir JSON çıktısı
üretiyor ve aggregate'i yükleyip tekrar düzleştirmesi gereksiz.

## Karar

Dört katman: `Domain`, `Application`, `Infrastructure`, `Api`. Bağımlılık oku daima içeri doğru.

- `Domain` hiçbir projeye referans vermez ve **hiçbir NuGet paketi içermez**. Kura motoru, entity'ler
  ve value object'ler burada.
- `Application` yalnızca port arayüzleri tanımlar; MediatR ile CQRS uygular. Kesişen ilgiler
  (`ValidationBehavior`, `LoggingBehavior`) `IPipelineBehavior` üzerinden.
- `Infrastructure` portları EF Core ile gerçekler.
- `Api` yalnızca HTTP'ye çevirir; controller action'ları `ISender.Send()` çağırıp `Result`'ı HTTP
  sonucuna maplar.

Okuma/yazma ayrımı port seviyesinde de yapıldı: `IDrawRepository` yalnızca `AddAsync` içerir
(yazma), `IDrawQueries` doğrudan yanıt modeli döndürür (okuma, `AsNoTracking` + projeksiyon).

## Sonuçlar

**Kazanç.** Kura motorunun 20.000 kuralık fuzz koşumu ~5 saniyede biter, hiçbir altyapı gerekmez.
`Application` katmanında EF Core referansı bulunmadığı için persistence teknolojisi değiştirilebilir.
Yeni bir kısıt eklemek (`IDrawRule`) algoritmayı değiştirmez.

**Bedel.** Bu ölçekteki bir servis için katman sayısı fazla görünebilir; tek bir endpoint eklemek
dört dosyaya dokunmayı gerektiriyor. Port arayüzleri (`IUnitOfWork` gibi) küçük ve neredeyse
şeffaf — soyutlama karşılığında dolaylılık ödeniyor.

**Kabul edilen tavizler.** `DrawQueries` `Application.Contracts` içindeki yanıt modellerini
döndürüyor; katı bir okumada bu, `Infrastructure`'ın sunum modeline bağlanması demek. Alternatifi
ikinci bir DTO seti ve fazladan bir mapping katmanıydı; bu ölçekte değmez.

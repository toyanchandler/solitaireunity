# Handler Unity Architecture Migration Pack

Bu paket eski hypercasual production architecture yapısını daha senior, daha validate edilebilir ve LLM-friendly hale geçirmek için hazırlanmıştır.

Paket iki farklı kullanıma ayrılır:

1. `00_Codex_Refactor` klasörü Codex veya başka bir kod ajanına verilecek migration brief, refactor planı ve validation promptlarını içerir.
2. `01_Permanent_Project_Rules`, `02_How_To`, `03_Hierarchy`, `04_Validation`, `05_Code_Templates` ve `06_Decision_Records` klasörleri projede kalıcı dokümantasyon olarak tutulur.

Ana hedef tek bir oyunu over-engineer etmek değildir. Hedef hypercasual oyun üretimini standartlaştırmaktır:

- Junior developer veya LLM yeni oyun yaparken save, analytics, level contract, camera, character spawn, runtime state ve UI flow kararlarını yeniden keşfetmez.
- Persistent data pipeline otomatik ve predictable olur.
- Analytics game logic içine dağılmaz.
- Runtime ScriptableObject state/context yapısı late subscriber problemini çözer.
- ScriptableObject assetler logic merkezi değil, state/config/context taşıyıcıları olarak kalır.
- Internal static helperlar component sayısını artırmadan logic ayrımı sağlar.
- İleride Saneject tarzı baked wiring eklenebilir, ama Spyke onsite task için default yol değildir.

## Spyke için önerilen kullanım

Task sırasında şu katmanı kullan:

- Explicit serialized references
- LevelReferenceHolder contract
- Solitaire module event registration (`03_Hierarchy/SOLITAIRE_MODULE_RUNTIME_WIRING.md`)
- EventManager veya küçük GameEventBus
- Saveable ScriptableObject data
- Resettable Runtime ScriptableObject contexts
- Internal static Rules, Mappers, Appliers
- Manager-level orchestration

Task sırasında şunu ana yol yapma:

- Full DI container
- Saneject entegrasyonu
- Runtime reflection injection
- MainCanvas altında tüm component registry taraması
- Prebuild baked wiring sistemi

Saneject benzeri baked binding fikrini teknik sohbet ve senior bonus olarak anlat. Taskı çözmek için bağımlı kalma.

## Klasörler

```text
00_Codex_Refactor/          Codex migration planı ve prompts
01_Permanent_Project_Rules/ Projede kalacak rules
02_How_To/                  Junior ve LLM için adım adım rehberler
03_Hierarchy/               Yeni scene ve folder planı
04_Validation/              Checklist, reject criteria, Codex validation spec
05_Code_Templates/          Doğrudan uyarlanacak template kodlar
06_Decision_Records/        Mimari karar kayıtları
99_Source_Baseline/         Mevcut yüklenen rule dokümanlarının kopyası
```

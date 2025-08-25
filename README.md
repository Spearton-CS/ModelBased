# ModelBased
---
## Dependences

* C# .NET 8 (cross-platform designed)
* AOT-compatible, trimmable

---
## When i need this project?

* Model-based project design (e.g: TrackModel, ArtistModel, UserModel)
* Hundreds or thousands of models with unique IDs, which can be used from more threads or more UI-pages.
* You need the max efficient of memory-usage for models (no allocations and deallocation at each rent/return and etc, better than just generic collections)

---
## Design

- [x] Thread-safe
- [x] Async-compatible
- [x] Scalable (through contracts)
- [x] Division of responsibility between collections
- [x] default-lib-like namespaces (e.g: Collections.Generic)
- [x] All have comments and direct names (by actions that does)
- [x] Have MSTests

---
## Where can download?

* [Nuget](https://www.nuget.org/packages/ModelBased/1.0.0#readme-body-tab "ModelBased")
* [Github](https://github.com/Spearton-CS/ModelBased "ModelBased")
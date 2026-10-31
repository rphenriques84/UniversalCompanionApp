# Design principles

Apply these with judgment. They serve the code; they don't justify extra code.
If two conflict, prefer the simpler option.

- **Separation of Concerns:** one project, one area. Keep rules in Domain, HTTP shapes in Contracts, data access in Backend.Data, and UI in the frontend.
- **SOLID:**
  - **Single Responsibility:** a class has one reason to change. Split a feature folder when it starts doing two jobs.
  - **Open/Closed:** add a new game through data and a theme, not by changing shared code.
  - **Liskov Substitution:** an implementation must honor its interface's contract. No `NotSupportedException` for members it can't do.
  - **Interface Segregation:** keep interfaces small and shaped for the caller.
  - **Dependency Inversion:** dependencies point toward Domain. Add an interface only for a real boundary or a second implementation, not for every class.
- **DRY:** one source of truth per rule or fact. Avoid copying logic, but don't merge code that only looks alike. Wait for the third real copy before extracting.
- **KISS:** choose the simplest design that works. No repository layer over EF Core, mediator or CQRS without a concrete need.
- **YAGNI:** build for the current plan task. No speculative options, abstractions or settings for later phases.
- **Functional core, imperative shell:** Domain stays pure, with no I/O, clock or logging. Services at the edges load data, call Domain and save results.
- **Least privilege:** each service, database user and token gets only the access it needs. Secrets live only where they are used.
- **Secure by default:** deny first. Opening access is a deliberate, visible step.
- **Fail fast:** validate configuration at startup and input at the boundary. Don't carry bad state deeper.
- **Composition over inheritance:** build from small parts. Avoid base-class chains in services and Blazor components.

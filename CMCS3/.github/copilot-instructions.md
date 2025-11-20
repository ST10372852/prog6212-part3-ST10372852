Repository: CMCS Lecturer Claim System (prototype)

Purpose
- Help AI coding agents be immediately productive in this repository by documenting the
  architecture, core patterns, developer workflows, and important implementation details.

Quick start (developer)
- Run locally: `dotnet run` from the project folder (or open `CMCS.sln` in Visual Studio).
- Run tests: `dotnet test` in the `CMCS.Tests/` folder.
- Ensure `EncryptedFiles/` exists and is writable by the app at runtime.

Big-picture architecture
- ASP.NET MVC (minimal Program.cs) using Controllers + Views. See `Program.cs` for routing.
- Key controllers:
  - `Controllers/ClaimsController.cs` — claim lifecycle (Submit, Track, Details), in-memory store.
  - `Controllers/DocumentsController.cs` — encrypted upload, download and preview logic.
  - `Controllers/ApprovalsController.cs` — prototype approval endpoints.
- Models live in `Models/` (`ClaimViewModel`, `ClaimLineViewModel`, `DocumentViewModel`).

Important project-specific patterns
- In-memory store: `_claims` dictionary is defined private static inside `ClaimsController` and
  re-used by other controllers via reflection (see `ApprovalsController` and `DocumentsController`).
  Do not remove or rename `_claims` without updating the reflection code paths.
- Aliases and naming: `ClaimViewModel` exposes both `ClaimId` and `ClaimID` (some views expect
  `ClaimID`). Keep both if editing model properties to avoid breaking views.
- Calculations: Line totals and claim totals are computed via properties:
  - `ClaimLineViewModel.LineTotal` = `Hours * Rate`
  - `ClaimViewModel.TotalAmount` sums `Lines`.
  Use these server-side properties rather than duplicating logic in controllers.
- File handling & encryption:
  - Encrypted files stored in `EncryptedFiles/` as `{claimId}_{guid}{ext}.enc`.
  - Encryption is implemented in `DocumentsController` (AES-CBC with PBKDF2-derived key).
  - Prototype passphrase is hard-coded as `passphrase` in `DocumentsController` — move to config
    (`appsettings.json` or secret store) before production.
- Upload rules enforced in `DocumentsController.UploadFile`:
  - Allowed extensions: `.pdf`, `.docx`, `.xlsx`
  - Max file size: 5 MB (also `RequestSizeLimit(10_000_000)` on the action)

Common endpoints & views to reference in changes
- Claims submit / flow: `GET /Claims/Submit`, `POST /Claims/Submit` (`Views/Claims/Submit.cshtml`)
- Track claims: `GET /Claims/Track` (`Views/Claims/Track.cshtml`) — Actions column contains Upload/View.
- Details + document preview: `GET /Claims/Details/{id}` (`Views/Claims/Details.cshtml`)
- Upload documents: `GET /Documents/Upload/{id}`, `POST /Documents/UploadFile`.
- Download: `GET /Documents/Download?claimId={id}&file={storedName}`

Developer guidance for requested feature work (Lecturer/Coordinator/HR views)
- Lecturer view: Reuse `ClaimsController.Submit` + `ClaimLineViewModel`.
  - Client: add JS in `Views/Claims/Submit.cshtml` to auto-calc each line `LineTotal` (Hours*Rate)
  - Server: validate `ModelState`, ensure `Hours` >= 0 and `Rate` >= 0 before saving. `LineTotal` is
    already a computed property — rely on it when rendering totals.
- Coordinator / Manager automation: Current endpoints exist (`CoordinatorApprove/Reject`,
  `ManagerApprove/Reject`) in `ClaimsController`. To add automated checks, implement a service
  (or helper class) and call it from those endpoints instead of directly setting `Status`.
  - Example checks: validate `TotalAmount` bounds, per-line `Rate` thresholds, or document presence.
- HR view (new): Add a new `Controllers/HRController.cs` (or Razor Pages) that reads claims from
  the same in-memory `_claims` (or from EF if you migrate to persistence). For reporting/invoice
  generation, preferred approach:
  1. Add EF Core and persist claims (recommended for production).
  2. Generate reports via LINQ or a reporting library; for prototypes, create CSV/PDF from data.

Maintenance notes / gotchas
- Reflection coupling: `ApprovalsController` and `DocumentsController` access `ClaimsController._claims`
  using reflection. If you replace the in-memory store with EF, update those controllers to use the
  new data access layer and remove reflection usage.
- Encryption key: Move `passphrase` into `appsettings.json` and use `IConfiguration` to read it.
- Persistence: Claims are in-memory — a restart clears data. If the feature work requires persistence,
  scaffold EF Core `Claim`/`ClaimLine`/`Document` entities and update controllers accordingly.

Commands
- Run app:
  ```powershell
  cd <project-folder>
  dotnet run
  ```
- Run tests:
  ```powershell
  cd CMCS.Tests
  dotnet test
  ```

If anything here is unclear or you'd like the instructions to emphasize a different area
 (e.g., EF migration steps, reporting library integration, or authentication setup), tell me
 which part to expand and I will iterate.

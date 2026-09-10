# Technology Research & Architectural Decisions

## Decision 1: Storage Layer Abstraction
- **Problem**: Need local offline operation for training/demo while guaranteeing seamless transition to Azure Blob Storage in production.
- **Decision**: Define `IFileStorageService` in the application service layer. Provide `LocalFileStorageService` operating on `AppData/uploads`.
- **Rationale**: By using streams (`Stream fileStream`), controller and service logic remain completely decoupled from underlying storage mechanisms. Relative paths stored in database columns (`FilePath`) serve equally well as local relative paths or Azure blob identifiers.

## Decision 2: Blazor Server InputFile Lifecycle & Stream Handling
- **Problem**: In Blazor Server, `IBrowserFile.OpenReadStream()` streams can throw `ObjectDisposedException` or fail when asynchronous operations take longer than the circuit connection timeout.
- **Decision**: Read and copy the browser file stream into a local `MemoryStream` or pipe directly to a physical `FileStream` before any database calls, then reset `SelectedFile` to null.
- **Rationale**: Ensures the upload payload is fully buffered locally, eliminating transient Blazor signal pipeline disconnection errors during disk write.

## Decision 3: Database Engine Selection for Lab Environment
- **Problem**: SQL Server LocalDB (`sqllocaldb`) is not available on all developer machines (e.g. non-Windows or machines without SQL tools installed).
- **Decision**: Utilize SQLite (`Microsoft.EntityFrameworkCore.Sqlite`) with connection string `Data Source=ContosoDashboard.db`.
- **Rationale**: SQLite is completely embedded, file-based, zero-configuration, cross-platform, and fully supported by Entity Framework Core with zero behavioral deviation for the dashboard requirements.

## Decision 4: Protected Storage Directory & Download Endpoint
- **Problem**: Files in `wwwroot` are public to anyone with URL knowledge, violating Contoso corporate privacy.
- **Decision**: Files are stored in `AppData/uploads/` outside `wwwroot`. A dedicated MVC controller (`DocumentDownloadController`) serves files after authenticating the session and checking document ownership or project membership.
- **Rationale**: Provides strict defense-in-depth against Insecure Direct Object References (IDOR).

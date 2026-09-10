# ContosoDashboard Project Constitution

## Core Principles

### I. Offline-First & Cloud-Ready Abstractions
The application must run fully offline without mandatory cloud dependencies for local development, demo, and training environments. All storage and external services must be defined behind clear interfaces (e.g., `IFileStorageService`). Implementations like `LocalFileStorageService` must handle local file storage using the filesystem, allowing drop-in replacement with `AzureBlobStorageService` for production cloud deployment through configuration and dependency injection without changes to domain logic or database schemas.

### II. Security & Defense in Depth
Security cannot rely on client-side constraints alone:
- Uploaded files must be stored strictly outside `wwwroot` in a protected folder (e.g., `AppData/uploads/{userId}/{projectId or "personal"}/{guid}.{extension}`).
- Files must be given unique GUID filenames upon storage to prevent path traversal attacks and duplicate filename conflicts.
- File access and downloads must be mediated through authorized controller/service endpoints that verify user ownership, team membership, or project roles before serving streams.
- File validation must enforce strict whitelisting of permitted file types (PDF, Office documents, plain text, JPEG, PNG) and a maximum size limit of 25 MB.
- Authentication claims (including `NameIdentifier`, `Name`, `Email`, `Role`, and `Department`) must be populated to enable granular role- and department-based authorization.

### III. Clean Layered Architecture
Maintain strict separation of concerns across:
1. **Data Layer**: EF Core entities with integer primary keys (consistent with existing `User` and `Project` entities), descriptive text categories, and navigation properties.
2. **Storage Layer**: Interface abstraction (`IFileStorageService`) decoupling physical storage from business logic.
3. **Business Logic Layer**: Domain services (`IDocumentService`) orchestrating validation, disk persistence, DB record creation, and notifications.
4. **Presentation Layer**: Blazor Server UI components utilizing defensive stream handling (`MemoryStream` copy pattern to prevent Blazor circuit disposal issues) and responsive feedback.

### IV. Safe State & Resource Management in Blazor
File upload and asynchronous Blazor interactions must handle component lifecycles cleanly:
- `IBrowserFile` streams must be immediately copied to an in-memory buffer or directly to the storage service before the browser file stream is disposed.
- File reference pointers must be reset to avoid object reuse errors.
- UI components must leverage `@key` bindings on `InputFile` elements to reset file input state predictably after submission.

### V. Testability & Measurable Acceptance Criteria
All functional requirements must map to testable acceptance scenarios formulated in standard `Given-When-Then` format. Performance benchmarks are non-negotiable:
- Document uploads up to 25 MB must finish within 30 seconds on standard network/disk.
- Document list and search queries must complete within 2 seconds for datasets up to 500 documents.
- Preview generation must render within 3 seconds.

### VI. Data Integrity & Transactional Safety
When saving file records:
- Physical storage must precede database record persistence: generate unique path -> save file -> commit DB transaction.
- If physical disk write fails, no orphaned DB record is created.
- File deletions must clean up both database metadata and physical storage files reliably.
- Project and user associations must respect foreign key constraints and cascade rules defined in `ApplicationDbContext`.

### VII. Auditability & Notifications
All key document operations (upload, download, metadata update, deletion, and sharing) must log structured audit information. Sharing a document or uploading to an assigned project must dispatch in-app notifications to affected team members via `INotificationService`.

---

## Technical Standards & Stack
- **Framework**: .NET 8.0 / .NET 9.0 (ASP.NET Core & Blazor Server)
- **Database**: Entity Framework Core with SQLite for offline/cross-platform environments, or SQL Server LocalDB
- **Storage Strategy**: Local filesystem (`AppData/uploads`) with interface-based abstraction for Azure Blob Storage
- **Authentication**: Cookie-based mock authentication with rich claims (`NameIdentifier`, `Name`, `Email`, `Role`, `Department`)
- **Frontend**: Blazor Server, HTML5, Vanilla CSS, Bootstrap icons

---

## Governance
This constitution supersedes ad-hoc coding patterns. Any architectural modifications, additions of third-party cloud SDKs, or alterations to storage pathways must adhere to the core principles above and obtain stakeholder review.

**Version**: 1.0.0 | **Ratified**: 2026-09-10 | **Last Amended**: 2026-09-10

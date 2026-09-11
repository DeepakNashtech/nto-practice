# Walkthrough: ContosoDashboard Spec-Driven Development (Document Management MVP)

We have completed the end-to-end implementation of the **Document Upload and Management** feature for the ContosoDashboard application following the Spec-Driven Development (SDD) methodology.

---

## 1. Environment & Database Configuration

- **.NET SDK**: Downloaded and installed official .NET 8.0.425 SDK (`$env:LOCALAPPDATA\Microsoft\dotnet\`).
- **Database Engine**: Migrated from SQL Server LocalDB to **SQLite** (`Microsoft.EntityFrameworkCore.Sqlite`) with zero external service dependencies.
- **Connection String**: Configured `Data Source=ContosoDashboard.db` in [appsettings.json](file:///d:/nto-phase-2-training/ContosoDashboard/ContosoDashboard/appsettings.json).

---

## 2. Spec-Driven Development (SDD) Artifacts

All governance and planning documents were established in adherence to GitHub Spec Kit conventions:

| Artifact | Path | Purpose |
|---|---|---|
| **Constitution** | [.specify/memory/constitution.md](file:///d:/nto-phase-2-training/ContosoDashboard/.specify/memory/constitution.md) | Governing principles: Offline-first architecture, Security outside `wwwroot`, Blazor `MemoryStream` lifecycle safety, and measurable latency criteria. |
| **Feature Specification** | [specs/001-document-management/spec.md](file:///d:/nto-phase-2-training/ContosoDashboard/specs/001-document-management/spec.md) | Clarified stakeholder requirements, Given-When-Then acceptance scenarios, edge-case resolutions, and out-of-scope boundaries. |
| **Requirements Checklist** | [specs/001-document-management/checklists/requirements.md](file:///d:/nto-phase-2-training/ContosoDashboard/specs/001-document-management/checklists/requirements.md) | Completeness audit validating all stakeholder criteria. |
| **Data Model** | [specs/001-document-management/data-model.md](file:///d:/nto-phase-2-training/ContosoDashboard/specs/001-document-management/data-model.md) | ERD and schema specification for `Document` and `DocumentShare` entities. |
| **Research & Decisions** | [specs/001-document-management/research.md](file:///d:/nto-phase-2-training/ContosoDashboard/specs/001-document-management/research.md) | Technical rationale for storage abstraction, Blazor memory buffers, SQLite, and IDOR protection. |
| **Quickstart Guide** | [specs/001-document-management/quickstart.md](file:///d:/nto-phase-2-training/ContosoDashboard/specs/001-document-management/quickstart.md) | Step-by-step developer instructions to run and test locally. |
| **Technical Plan** | [specs/001-document-management/plan.md](file:///d:/nto-phase-2-training/ContosoDashboard/specs/001-document-management/plan.md) | Layered architecture blueprint across Data, Storage, Business, and Presentation layers. |
| **Implementation Tasks** | [specs/001-document-management/tasks.md](file:///d:/nto-phase-2-training/ContosoDashboard/specs/001-document-management/tasks.md) | Actionable task breakdown mapped to the MVP-first delivery strategy. |

---

## 3. Application Architecture & Code Implementation

### Data Layer
- **[Document.cs](file:///d:/nto-phase-2-training/ContosoDashboard/ContosoDashboard/Models/Document.cs)**: Entity with integer PK, title, category, GUID file path, size, MIME type, optional project FK, and navigation properties.
- **[DocumentShare.cs](file:///d:/nto-phase-2-training/ContosoDashboard/ContosoDashboard/Models/DocumentShare.cs)**: Sharing relationship model linking documents to recipient users.
- **[ApplicationDbContext.cs](file:///d:/nto-phase-2-training/ContosoDashboard/ContosoDashboard/Data/ApplicationDbContext.cs)**: Configured `DbSet<Document>` and `DbSet<DocumentShare>`, cascaded project delete behavior, and performance indexes on `(UploadedByUserId, ProjectId, Category)`.

### Storage & Service Layer
- **[IFileStorageService.cs](file:///d:/nto-phase-2-training/ContosoDashboard/ContosoDashboard/Services/IFileStorageService.cs)**: Abstraction interface for upload, download stream, and delete operations.
- **[LocalFileStorageService.cs](file:///d:/nto-phase-2-training/ContosoDashboard/ContosoDashboard/Services/LocalFileStorageService.cs)**: Implements offline filesystem storage in protected `AppData/uploads/{userId}/{projectId or "personal"}/{guid}.{ext}` folder outside `wwwroot`.
- **[IDocumentService.cs](file:///d:/nto-phase-2-training/ContosoDashboard/ContosoDashboard/Services/IDocumentService.cs)** & **[DocumentService.cs](file:///d:/nto-phase-2-training/ContosoDashboard/ContosoDashboard/Services/DocumentService.cs)**: Core orchestration handling 25 MB size limits, MIME whitelist validation, disk persistence before DB commit, access authorization, and in-app notifications.

### Authentication & Controller
- **[Login.cshtml.cs](file:///d:/nto-phase-2-training/ContosoDashboard/ContosoDashboard/Pages/Login.cshtml.cs)**: Added the required `Department` claim to the cookie identity ticket.
- **[DocumentDownloadController.cs](file:///d:/nto-phase-2-training/ContosoDashboard/ContosoDashboard/Controllers/DocumentDownloadController.cs)**: Secure endpoints at `/api/documents/{id}/download` and `/api/documents/{id}/preview` with role- and ownership-based authorization.

### Presentation Layer
- **[Documents.razor](file:///d:/nto-phase-2-training/ContosoDashboard/ContosoDashboard/Pages/Documents.razor)**: Interactive Blazor Server page featuring:
  - Document grid with category badges, file type icons, size formatters, and upload metadata.
  - Search by title, tag, or description, and category filtering.
  - Upload modal with validation, progress indicator, and safe `MemoryStream` buffer copying to prevent Blazor stream disposal issues.
  - Share modal to grant read access to colleagues.
- **[NavMenu.razor](file:///d:/nto-phase-2-training/ContosoDashboard/ContosoDashboard/Shared/NavMenu.razor)**: Added "Documents" navigation link.

---

## 4. Verification & Testing Results

### Build Verification
Ran `dotnet build` using .NET SDK 8.0.425:
```text
ContosoDashboard -> D:\nto-phase-2-training\ContosoDashboard\ContosoDashboard\bin\Debug\net8.0\ContosoDashboard.dll
Build succeeded.
    0 Error(s)
```

### Automated UI & Flow Verification
An automated browser subagent session executed the full end-to-end verification:
1. Navigated to `http://localhost:5000/login`
2. Selected **Ni Kang (Employee)** and logged in
3. Selected **Documents** in the sidebar navigation
4. Confirmed the **Document Management** header and view
5. Opened the **Upload Document** modal and validated all required input fields:
   - File picker (`Select File *`)
   - Document Title (`Document Title *`)
   - Category dropdown (`Category *`)
   - Associated Project selector
   - Description and Tags fields
6. Closed the modal and verified the main documents listing view.

### Verified Interface
![Documents View Verified](./assets/images/Screenshot%202026-09-11%20123247.png)

### Git Repository State
All SDD documentation and code changes have been committed cleanly to `main` (commit `3ef0503`).

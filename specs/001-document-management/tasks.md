# Implementation Tasks: Document Upload and Management

## Implementation Strategy
- **Strategy**: MVP First
- **Phases**: Setup & Environment → Foundation (Entities & Services) → User Story 1 (MVP: Upload & View) → User Story 2 & 3 (Search, Sharing)
- **Target Deliverable**: Users can upload, view, and download documents within ContosoDashboard.

---

## Phase 1: Environment & Setup
- [x] **T001**: Configure local SQLite database provider in `ContosoDashboard.csproj` and `appsettings.json`.
- [x] **T002**: Update `Program.cs` DbContext configuration for SQLite and register new services.

## Phase 2: Foundation & Data Layer
- [x] **T010**: Create `Document` model in `Models/Document.cs` with validation attributes and foreign keys.
- [x] **T011**: Create `DocumentShare` model in `Models/DocumentShare.cs`.
- [x] **T012**: Update `ApplicationDbContext.cs` with `DbSet<Document>`, `DbSet<DocumentShare>`, indexes, and seed records.
- [x] **T013**: Create `IFileStorageService` interface in `Services/IFileStorageService.cs`.
- [x] **T014**: Implement `LocalFileStorageService` in `Services/LocalFileStorageService.cs` saving files under `AppData/uploads`.
- [x] **T015**: Create `IDocumentService` interface in `Services/IDocumentService.cs`.
- [x] **T016**: Implement `DocumentService` in `Services/DocumentService.cs` with upload validation, authorization, and persistence.
- [x] **T017**: Update `Login.cshtml.cs` to add the `Department` claim to the user claims list.

## Phase 3: User Story 1 (MVP - Upload, Browse, Download)
- [x] **T020**: Create `Controllers/DocumentDownloadController.cs` for serving authorized file downloads.
- [x] **T021**: Add "Documents" item to navigation menu in `Shared/NavMenu.razor`.
- [x] **T022**: Create `Pages/Documents.razor` with document listing table, status badges, and upload modal.
- [x] **T023**: Implement file upload logic using `MemoryStream` and `@key` pattern in `Documents.razor`.
- [x] **T024**: Implement download triggers and file deletion confirmation.

## Phase 4: User Story 2 & 3 (Enhancements - Search, Filter, Sharing)
- [x] **T030**: Add category filtering and search query input to `Documents.razor`.
- [x] **T031**: Add document sharing modal and trigger in-app notification to recipients.

## Phase 5: Verification & Polish
- [x] **T040**: Compile and build application (`dotnet build`).
- [x] **T041**: Verify acceptance scenarios: upload document, verify file in storage, download file, verify validation errors.

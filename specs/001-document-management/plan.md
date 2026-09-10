# Technical Implementation Plan: Document Upload and Management

## Architectural Overview
The Document Upload and Management feature integrates into ContosoDashboard using a layered architecture:

```
[ Presentation Layer: Documents.razor (Blazor Server) ]
                      |
[ Business Layer: DocumentService (Validation & Orchestration) ]
          |                                       |
[ Storage Layer: IFileStorageService ]   [ Data Layer: ApplicationDbContext ]
(LocalFileStorageService -> AppData)     (SQLite / SQL Server LocalDB)
```

## Implementation Modules

### 1. Data Layer
- Models: `Document.cs`, `DocumentShare.cs`
- DbContext updates: Register `DbSet<Document>` and `DbSet<DocumentShare>`, specify keys, indexes, and delete behaviors.
- Ensure database seeding includes sample documents to verify UI on initial load.

### 2. Storage Layer
- Create `IFileStorageService` interface with `UploadAsync`, `DeleteAsync`, `DownloadAsync`, `GetUrlAsync`.
- Create `LocalFileStorageService` implementation:
  - Generates storage path: `AppData/uploads/{userId}/{projectId or "personal"}/{guid}.{extension}`.
  - Automatically creates parent directories.
  - Streams file to disk and returns relative storage path.

### 3. Business Service Layer
- Create `IDocumentService` and `DocumentService`:
  - Enforces 25 MB file size limit and MIME whitelist.
  - Generates physical file via `IFileStorageService`.
  - Persists `Document` metadata in `ApplicationDbContext`.
  - Sends notifications using `INotificationService` when shared or uploaded to project.

### 4. Authentication Update
- Update `Login.cshtml.cs` to include the `Department` claim in the claims identity.

### 5. Presentation Layer
- Blazor Page `Pages/Documents.razor`:
  - Sub-views: "My Documents" and "Upload Document" modal.
  - Form validation for Title and Category.
  - File picker with progress indicator and error messaging.
  - Action buttons for Download and Delete.
- Navigation: Add `Documents` NavLink to `Shared/NavMenu.razor`.
- Controller: Create `Controllers/DocumentDownloadController.cs` for serving authorized downloads outside `wwwroot`.

# Feature Specification: Document Upload and Management

**Feature**: Document Upload and Management  
**Branch**: `001-document-management`  
**Status**: Approved & Clarified  
**Target Application**: ContosoDashboard (Blazor Server / ASP.NET Core)

---

## Executive Summary
Contoso Corporation employees currently encounter fragmentation when storing work documents across email attachments, shared folders, and local drives. The Document Upload and Management feature provides a centralized, secure, role-based workspace within ContosoDashboard to upload, categorize, associate with projects, and share work documents offline with seamless cloud-migration readiness.

---

## User Scenarios & Acceptance Criteria

### User Story 1 (US1): Upload and Manage Documents (MVP Focus)
As an authenticated employee, I want to upload documents with required metadata (title, category, optional project) and view them in my documents list so that I have centralized access to my files.

#### Acceptance Scenario 1.1: Successful Document Upload
- **Given** I am logged in as an employee (e.g., Ni Kang)
- **When** I navigate to the Documents page, choose a valid file (e.g., PDF under 25 MB), provide a Title ("Project Alpha Plan") and Category ("Project Documents"), and click "Upload"
- **Then** the file is saved to protected disk storage (`AppData/uploads/{userId}/...`), a database record is created with an integer ID, and the document immediately appears in my "My Documents" list.

#### Acceptance Scenario 1.2: File Size Exceeded
- **Given** I select a file exceeding 25 MB (e.g., 28 MB archive or video)
- **When** I attempt to upload the file
- **Then** the client/service rejects the file with the message "File size exceeds the 25 MB limit", and no file or DB record is created.

#### Acceptance Scenario 1.3: Unsupported File Type Rejection
- **Given** I select an executable or unsupported script file (e.g., `.exe` or `.bat`)
- **When** validation executes
- **Then** the system rejects the file with an error stating supported formats: PDF, Word, Excel, PowerPoint, Text, JPEG, PNG.

#### Acceptance Scenario 1.4: Download Document
- **Given** I have an uploaded document in "My Documents"
- **When** I click the "Download" button
- **Then** the file stream is retrieved via the authorized download endpoint and downloaded with its original filename.

---

### User Story 2 (US2): Search, Filter, and Organization
As an employee or project lead, I want to search and filter documents by category, project, or title so that I can quickly retrieve relevant materials.

#### Acceptance Scenario 2.1: Category and Search Filtering
- **Given** I have multiple documents in various categories
- **When** I filter by category "Project Documents" or enter search text in the search input
- **Then** the list updates within 2 seconds displaying only matching records I have permission to view.

---

### User Story 3 (US3): Document Sharing and Notifications
As a document owner, I want to share a document with a team member so that they can collaborate and receive an in-app notification.

#### Acceptance Scenario 3.1: Sharing with a Colleague
- **Given** I own a document
- **When** I select "Share", pick a recipient user, and confirm
- **Then** a `DocumentShare` record is created, the recipient receives an in-app notification, and the document appears in their "Shared with Me" view.

---

## Clarifications & Edge Case Resolutions

1. **User Removal from Project**: When an employee is removed from a project, documents previously uploaded to that project remain associated with the project for project continuity.
2. **Project Deletion**: When a project is deleted, documents linked to that project are cleanly deleted from physical storage and database records are removed.
3. **Owner Deletion of Shared Document**: If an owner deletes a document, associated share records are removed, physical storage is deleted, and recipients lose access.
4. **Special Characters in Filenames**: User-supplied filenames containing special characters (e.g., `Q4 Report (2025) & Finance.pdf`) are sanitized for physical disk storage using a generated GUID, while the original display name is preserved in the database `FileName` and `Title` columns.
5. **Disk Storage Full During Upload**: If disk write fails due to insufficient storage, any partial file is immediately deleted, the database transaction is aborted, and an error is presented to the user with zero orphaned records.
6. **Authentication Claims**: The authentication ticket must provide `NameIdentifier`, `Name`, `Email`, `Role`, and `Department` claims to ensure department-level authorization checks succeed.

---

## Requirements Matrix

### Functional Requirements
- **FR-01 (Upload)**: Multi-file selection capability, maximum 25 MB per file.
- **FR-02 (Allowed Types)**: Whitelist: PDF (`application/pdf`), DOCX (`application/vnd.openxmlformats-officedocument.wordprocessingml.document`), XLSX, PPTX, TXT (`text/plain`), PNG (`image/png`), JPG (`image/jpeg`).
- **FR-03 (Metadata)**: Mandatory Title, Category (from predefined list: "Project Documents", "Team Resources", "Personal Files", "Reports", "Presentations", "Other"). Optional Project and Description.
- **FR-04 (Storage Architecture)**: Offline filesystem storage under `AppData/uploads/{userId}/{projectId or "personal"}/{guid}.{ext}` abstracted via `IFileStorageService`.
- **FR-05 (Key Conventions)**: `DocumentId` is an integer auto-increment identity; `Category` is stored as a string.
- **FR-06 (Download & Stream)**: Controller endpoint mediating file retrieval outside `wwwroot` with authorization checks.
- **FR-07 (Dashboard & Nav Integration)**: Navigation item "Documents" added to sidebar navigation.

### Non-Functional Requirements
- **NFR-01 (Performance)**: Upload completion <= 30s for 25 MB files; list/filter rendering <= 2s for 500 documents; preview <= 3s.
- **NFR-02 (Security)**: Storage outside `wwwroot`; GUID-based physical filenames; strict authorization checks before file delivery; prevention of IDOR.
- **NFR-03 (Offline First)**: Full functionality without active internet or external cloud services.

---

## Out of Scope
- Real-time collaborative document editing.
- Version history and differential rollbacks.
- External cloud synchronizers (OneDrive, SharePoint).
- Quota management enforcement.

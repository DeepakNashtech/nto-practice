# Data Model: Document Upload and Management

## Entity Relationship Diagram

```
+------------------------+          +------------------------+
|         User           | 1      * |        Document        |
+------------------------+----------+------------------------+
| UserId (PK, int)       |          | DocumentId (PK, int)   |
| Email (nvarchar)       |          | Title (nvarchar 200)   |
| DisplayName (nvarchar) |          | Description (nvarchar) |
| Department (nvarchar)  |          | Category (nvarchar 50) |
| Role (enum)            |          | FileName (nvarchar 255)|
+------------------------+          | FilePath (nvarchar 500)|
            | 1                     | FileSize (bigint)      |
            |                       | FileType (nvarchar 255)|
            |                       | ProjectId (FK, int?)   |
            |                       | UploadedByUserId (FK)  |
            |                       | UploadDate (datetime)  |
            |                       +------------------------+
            |                                   | 1
            |                                   |
            |                                   | *
            |                       +------------------------+
            +---------------------->|     DocumentShare      |
              (SharedWithUserId)    +------------------------+
                                    | DocumentShareId (PK)   |
                                    | DocumentId (FK, int)   |
                                    | SharedWithUserId (FK)  |
                                    | SharedByUserId (FK)    |
                                    | SharedDate (datetime)  |
                                    | Permission (nvarchar)  |
                                    +------------------------+
```

## Schema Details

### 1. Document
| Field | Type | Nullable | Description |
|---|---|---|---|
| `DocumentId` | `int` (Identity) | No | Primary Key |
| `Title` | `string` (max 200) | No | User-friendly document title |
| `Description` | `string?` (max 1000) | Yes | Optional detailed summary |
| `Category` | `string` (max 50) | No | Category string (e.g. "Project Documents") |
| `FileName` | `string` (max 255) | No | Original client filename |
| `FilePath` | `string` (max 500) | No | Relative path to disk storage |
| `FileSize` | `long` | No | File size in bytes |
| `FileType` | `string` (max 255) | No | MIME content type |
| `ProjectId` | `int?` | Yes | Foreign key to `Projects` table |
| `UploadedByUserId` | `int` | No | Foreign key to `Users` table |
| `UploadDate` | `DateTime` | No | UTC timestamp of upload |

### 2. DocumentShare
| Field | Type | Nullable | Description |
|---|---|---|---|
| `DocumentShareId` | `int` (Identity) | No | Primary Key |
| `DocumentId` | `int` | No | Foreign key to `Documents` |
| `SharedWithUserId` | `int` | No | Recipient User ID |
| `SharedByUserId` | `int` | No | Sharing User ID |
| `SharedDate` | `DateTime` | No | UTC timestamp |
| `Permission` | `string` (max 50) | No | Default: "Read" |

## Indexes
- `IX_Documents_UploadedByUserId`
- `IX_Documents_ProjectId`
- `IX_Documents_Category`
- `IX_DocumentShares_DocumentId_SharedWithUserId` (Unique constraint)

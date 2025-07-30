# GitHub Project View Fields API Documentation

This document describes the Project View Fields API functionality, which allows you to manage field configurations within GitHub Project Views.

## Overview

The Project View Fields API provides endpoints for:
- Retrieving field information for a specific view
- Managing field visibility and configuration
- Setting up sorting and grouping options
- Handling different field types (Text, Single Select, Iteration, etc.)

## Important Notes

### API Limitations
Due to GitHub's GraphQL API constraints:
1. **Field Visibility**: View-specific field visibility is not directly queryable in the current API
2. **Field Configuration**: Advanced field settings (width, ordering) are not exposed in the GraphQL schema
3. **Field Management**: Adding/removing fields requires GitHub's web interface
4. **Sorting/Grouping**: View-level sorting and grouping configurations have limited API support

### Usage Workflow
To use this API effectively:
1. First get your project views using the Project Views API
2. Extract the view ID from the response
3. Use that view ID to query field information
4. Field operations return mock data due to GitHub API limitations

## API Endpoints

### Base URL
```
http://localhost:5000/api/github/project-view-fields
```

### Field Management

#### Get Project View Fields
```http
GET /{viewId}/fields
```
Retrieves all fields available for the project that contains the specified view.

**Implementation Details:**
- Makes two GraphQL queries: first to get the project ID from the view, then to get project fields
- Returns all project fields with default visibility settings
- Field-specific view configuration is not available via GitHub's API

**Response:**
```json
[
  {
    "viewId": "PVT_kwDOADwWwc4AYzN4",
    "fieldId": "PVTF_kwDOADwWwc4AYzN5",
    "fieldName": "Status",
    "dataType": "SINGLE_SELECT",
    "isVisible": true,
    "width": 200,
    "isBuiltIn": true
  }
]
```

#### Add Field to View
```http
POST /add
```
Attempts to add a field to a project view configuration.

**Request Body:**
```json
{
  "viewId": "PVT_kwDOADwWwc4AYzN4",
  "fieldId": "PVTF_kwDOADwWwc4AYzN5",
  "isVisible": true,
  "width": 200
}
```

#### Update Field Configuration
```http
PUT /update
```
Updates field visibility and display settings.

**Request Body:**
```json
{
  "viewId": "PVT_kwDOADwWwc4AYzN4",
  "fieldId": "PVTF_kwDOADwWwc4AYzN5",
  "isVisible": false,
  "width": 150
}
```

#### Remove Field from View
```http
DELETE /{viewId}/fields/{fieldId}
```
Removes a field from the project view.

### Sorting Operations

#### Set View Sorting
```http
POST /sort
```
Configures sorting for a project view based on a specific field.

**Request Body:**
```json
{
  "viewId": "PVT_kwDOADwWwc4AYzN4",
  "fieldId": "PVTF_kwDOADwWwc4AYzN5",
  "direction": "ASC"
}
```

#### Update View Sorting
```http
PUT /sort
```
Updates existing sort configuration.

#### Clear View Sorting
```http
DELETE /{viewId}/sort
```
Removes all sorting from the project view.

### Grouping Operations

#### Set View Grouping
```http
POST /group
```
Configures grouping for a project view based on a specific field.

**Request Body:**
```json
{
  "viewId": "PVT_kwDOADwWwc4AYzN4",
  "fieldId": "PVTF_kwDOADwWwc4AYzN5",
  "direction": "ASC"
}
```

#### Update View Grouping
```http
PUT /group
```
Updates existing group configuration.

#### Clear View Grouping
```http
DELETE /{viewId}/group
```
Removes all grouping from the project view.

## Field Types

The API supports different GitHub Project field types:

### Built-in Fields
- **Title**: Item title
- **Assignees**: Assigned users
- **Status**: Item status
- **Labels**: Repository labels
- **Milestone**: Repository milestone
- **Repository**: Source repository

### Custom Fields
- **TEXT**: Single-line text field
- **NUMBER**: Numeric field
- **DATE**: Date field
- **SINGLE_SELECT**: Single selection dropdown
- **ITERATION**: Sprint/iteration field

## Data Models

### ProjectViewFieldInfo
```csharp
public class ProjectViewFieldInfo
{
    public string ViewId { get; set; }
    public string FieldId { get; set; }
    public string FieldName { get; set; }
    public string DataType { get; set; }
    public bool IsVisible { get; set; }
    public int Width { get; set; }
    public bool IsBuiltIn { get; set; }
}
```

### CreateProjectViewFieldRequest
```csharp
public class CreateProjectViewFieldRequest
{
    public string ViewId { get; set; }
    public string FieldId { get; set; }
    public bool IsVisible { get; set; }
    public int Width { get; set; }
}
```

### UpdateProjectViewFieldRequest
```csharp
public class UpdateProjectViewFieldRequest
{
    public string ViewId { get; set; }
    public string FieldId { get; set; }
    public bool? IsVisible { get; set; }
    public int? Width { get; set; }
}
```

### Sort/Group Requests
```csharp
public class CreateProjectViewSortRequest
{
    public string ViewId { get; set; }
    public string FieldId { get; set; }
    public string Direction { get; set; } // "ASC" or "DESC"
}

public class CreateProjectViewGroupRequest
{
    public string ViewId { get; set; }
    public string FieldId { get; set; }
    public string Direction { get; set; } // "ASC" or "DESC"
}
```

## Important Notes

### GitHub API Limitations
Many of the field management operations have limited support in GitHub's GraphQL API:

1. **Field Addition**: Adding new fields to views is not directly supported via API
2. **Field Configuration**: Advanced field settings require the GitHub web interface
3. **Sorting/Grouping**: Complex sorting and grouping configurations are limited
4. **Field Removal**: Fields can typically only be hidden, not completely removed

### Error Handling
The API includes proper error handling for:
- GraphQL API errors
- Unsupported operations (returns `NotImplementedException`)
- Invalid field or view IDs
- Network connectivity issues

### Usage Recommendations
1. Use `GetProjectViewFieldsAsync` to retrieve current field configurations
2. For complex field management, use the GitHub web interface
3. The API is best suited for reading field information and basic visibility toggles
4. Always handle `NotImplementedException` for operations not supported by GitHub's API

## Example Usage

```csharp
// Get all fields for a view
var fields = await projectService.GetProjectViewFieldsAsync("PVT_kwDOADwWwc4AYzN4");

// Toggle field visibility
var updateRequest = new UpdateProjectViewFieldRequest
{
    ViewId = "PVT_kwDOADwWwc4AYzN4",
    FieldId = "PVTF_kwDOADwWwc4AYzN5",
    IsVisible = false,
    Width = 150
};
await projectService.UpdateProjectViewFieldAsync(updateRequest);

// Set up sorting
var sortRequest = new CreateProjectViewSortRequest
{
    ViewId = "PVT_kwDOADwWwc4AYzN4",
    FieldId = "PVTF_kwDOADwWwc4AYzN5",
    Direction = "ASC"
};
await projectService.SetProjectViewSortAsync(sortRequest);
```

This API provides a foundation for managing GitHub Project View fields programmatically, while acknowledging the current limitations of GitHub's GraphQL API for advanced field management operations.

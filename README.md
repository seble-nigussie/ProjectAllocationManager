# Project Allocation Manager MCP

A Model Context Protocol (MCP) server for managing project allocations, engineers, and tracking resource availability.

## Overview

This MCP server provides:

**Tools** for managing allocations:
- Allocate engineers to projects with specific percentage allocations
- Update existing allocations
- View engineer allocations and project assignments
- Identify engineers on the bench (0% allocated)
- Prevent over-allocation of resources

**Resources** for accessing data:
- Browse all projects, engineers, and allocations
- Get detailed information about specific engineers or projects
- Access data in both formatted and raw JSON formats

## Project Structure

```
ProjectAllocationManager/
├── data/
│   ├── projects.json      # Project definitions
│   ├── engineers.json     # Engineer profiles
│   └── allocations.json   # Allocation records
├── Models/
│   ├── Project.cs
│   ├── Engineer.cs
│   └── Allocation.cs
├── Services/
│   └── AllocationService.cs
├── Tools/
│   ├── AllocationTools.cs # Allocation & update tools
│   └── QueryTools.cs      # Query & list tools
├── Resources/
│   └── AllocationResources.cs # MCP resources for data access
├── Program.cs             # MCP server initialization
└── README.md
```

## Data Structure

### Projects (data/projects.json)
```json
{
  "id": "proj-001",
  "name": "Project Alpha",
  "description": "E-commerce platform development",
  "status": "active"
}
```

### Engineers (data/engineers.json)
```json
{
  "id": "eng-001",
  "name": "Alice Johnson",
  "role": "Senior Software Engineer",
  "skills": ["C#", ".NET", "React"]
}
```

### Allocations (data/allocations.json)
```json
{
  "id": "alloc-001",
  "engineerId": "eng-001",
  "projectId": "proj-001",
  "allocationPercentage": 50,
  "startDate": "2025-01-01",
  "endDate": "2025-06-30"
}
```

## Available MCP Tools

### 1. allocate_engineer
Allocate an engineer to a project with a specified percentage.

**Parameters:**
- `engineerId` (string): The ID of the engineer (e.g., 'eng-001')
- `projectId` (string): The ID of the project (e.g., 'proj-001')
- `allocationPercentage` (number): Percentage of time allocated (0-100)
- `startDate` (string): Start date in YYYY-MM-DD format
- `endDate` (string): End date in YYYY-MM-DD format

**Example:**
```json
{
  "engineerId": "eng-003",
  "projectId": "proj-002",
  "allocationPercentage": 75,
  "startDate": "2025-02-01",
  "endDate": "2025-08-31"
}
```

### 2. update_allocation
Update an existing allocation's percentage, start date, or end date.

**Parameters:**
- `allocationId` (string): The ID of the allocation to update
- `newPercentage` (number, optional): New allocation percentage (0-100)
- `newStartDate` (string, optional): New start date in YYYY-MM-DD format
- `newEndDate` (string, optional): New end date in YYYY-MM-DD format

**Example:**
```json
{
  "allocationId": "alloc-001",
  "newPercentage": 75
}
```

### 3. get_engineer_allocations
View all allocations for a specific engineer.

**Parameters:**
- `engineerId` (string): The ID of the engineer

**Example:**
```json
{
  "engineerId": "eng-001"
}
```

### 4. get_bench_engineers
Get a list of all engineers with 0% allocation (on bench/available).

**Parameters:** None

### 5. get_all_allocations
View all current allocations across all engineers and projects.

**Parameters:** None

### 6. list_engineers
List all engineers with their details.

**Parameters:** None

### 7. list_projects
List all projects with their details.

**Parameters:** None

## Available MCP Resources

Resources provide read-only access to project allocation data in a structured format.

### Static Resources

#### 1. allocation://projects/list
Get a formatted list of all projects with their details, status, and descriptions.

#### 2. allocation://engineers/list
Get a formatted list of all engineers with their roles, skills, and details.

#### 3. allocation://allocations/list
Get a formatted list of all current allocations across engineers and projects.

#### 4. allocation://projects/json
Get raw JSON data of all projects.

#### 5. allocation://engineers/json
Get raw JSON data of all engineers.

#### 6. allocation://allocations/json
Get raw JSON data of all allocations.

### Dynamic Resources

#### 7. allocation://engineer/{engineerId}
Get detailed information about a specific engineer including:
- Personal details (name, role, skills)
- Total allocation percentage
- Available capacity
- List of current project assignments
- Allocation details with dates

**Example:** `allocation://engineer/eng-001`

#### 8. allocation://project/{projectId}
Get detailed information about a specific project including:
- Project details (name, status, description)
- Total engineer allocation
- List of assigned engineers
- Individual allocation percentages and durations

**Example:** `allocation://project/proj-001`

## Setup

### Prerequisites
- .NET 9.0 SDK or later

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd ProjectAllocationManager
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the project:
```bash
dotnet build
```

## Running the Server

### Standard Mode
```bash
dotnet run
```

### Development Mode (with hot reload)
```bash
dotnet watch run
```

## Configuration for Claude Desktop

Add this configuration to your Claude Desktop config file:

**macOS:** `~/Library/Application Support/Claude/claude_desktop_config.json`
**Windows:** `%APPDATA%/Claude/claude_desktop_config.json`

```json
{
  "mcpServers": {
    "project-allocation-manager": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/path/to/ProjectAllocationManager",
        "--no-build"
      ]
    }
  }
}
```

Replace `/path/to/ProjectAllocationManager` with the actual path to your project directory.

**Note:** The `--no-build` flag is recommended to avoid build output interfering with STDIO communication.

## Usage Examples

### Example 1: Allocate an Engineer to a Project
```
Use the allocate_engineer tool to assign Carol Williams (eng-003) to Project Beta (proj-002) at 75% from 2025-02-01 to 2025-08-31.
```

### Example 2: Find Available Engineers
```
Use the get_bench_engineers tool to see which engineers are available for new projects.
```

### Example 3: Check an Engineer's Workload
```
Use the get_engineer_allocations tool with engineerId "eng-001" to see Alice Johnson's current allocations.
```

### Example 4: Update an Allocation
```
Use the update_allocation tool to change allocation "alloc-001" to 75%.
```

## Features

- **Over-allocation Prevention**: Automatically prevents allocating an engineer over 100%
- **Validation**: Validates engineer IDs, project IDs, and percentage ranges
- **Flexible Updates**: Update allocations partially (percentage, dates, or both)
- **Bench Tracking**: Easily identify available resources
- **Comprehensive Reporting**: View allocations by engineer or across all projects

## Data Persistence

All data is stored in JSON files in the `data/` directory. Changes made through the MCP tools are immediately persisted to disk.

## Development

### Adding New Engineers
Edit `data/engineers.json` and add a new entry:
```json
{
  "id": "eng-006",
  "name": "Frank Miller",
  "role": "Data Engineer",
  "skills": ["Python", "SQL", "Airflow"]
}
```

### Adding New Projects
Edit `data/projects.json` and add a new entry:
```json
{
  "id": "proj-004",
  "name": "Project Delta",
  "description": "Machine learning pipeline",
  "status": "planning"
}
```

## Error Handling

The server includes comprehensive error handling for:
- Invalid engineer or project IDs
- Over-allocation attempts
- Invalid percentage ranges (must be 0-100)
- Missing or malformed data

## License

MIT

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

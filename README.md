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

**Prompts** for natural language queries:
- "Who worked on this project?"
- "What projects did this engineer work on?"
- "Who's available with React skills?"
- Get allocation overviews and summaries

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
├── Prompts/
│   └── AllocationPrompts.cs # MCP prompts for natural language queries
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

**Parameters:** (All parameters are REQUIRED)
- `engineerId` (string, required): The ID of the engineer (e.g., 'eng-001')
- `projectId` (string, required): The ID of the project (e.g., 'proj-001')
- `allocationPercentage` (number, required): Percentage of time allocated (0-100)
- `startDate` (string, required): Start date in YYYY-MM-DD format
- `endDate` (string, required): End date in YYYY-MM-DD format

**Note:** All parameters must be provided. The AI assistant will request these values from the user if not provided.

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

### 3. move_engineer_to_bench
Move an engineer to the bench by removing all their project allocations.

**Parameters:**
- `engineerId` (string): The ID of the engineer to move to bench (e.g., 'eng-001')

**Example:**
```json
{
  "engineerId": "eng-002"
}
```

### 4. get_engineer_allocations
View all allocations for a specific engineer.

**Parameters:**
- `engineerId` (string): The ID of the engineer

**Example:**
```json
{
  "engineerId": "eng-001"
}
```

### 5. get_bench_engineers
Get a list of all engineers with 0% allocation (on bench/available).

**Parameters:** None

### 6. get_all_allocations
View all current allocations across all engineers and projects.

**Parameters:** None

### 7. list_engineers
List all engineers with their details.

**Parameters:** None

### 8. list_projects
List all projects with their details.

**Parameters:** None

### 9. get_project_allocation_history
Get the complete allocation history for a specific project in chronological order.

**Parameters:**
- `projectId` (string): The ID of the project (e.g., 'proj-001')

**What it shows:**
- All engineers who ever worked on the project
- Chronological list showing: Engineer from Date1 to Date2
- Each entry marked as [CURRENT] or [PAST] based on end date
- Allocation percentages and time periods

**Example:**
```json
{
  "projectId": "proj-001"
}
```

**Sample Output:**
```
Project: Project Alpha (active)

Allocation History (chronological order by start date):

  - Carol Williams (Frontend Developer): 75%
    Period: 2024-06-01 to 2024-12-31 [PAST]
  - Alice Johnson (Senior Software Engineer): 50%
    Period: 2025-01-01 to 2025-06-30 [CURRENT]
  - Bob Smith (Full Stack Developer): 100%
    Period: 2025-01-01 to 2025-12-31 [CURRENT]
```

### 10. get_engineer_allocation_history
Get the complete allocation history for a specific engineer in chronological order.

**Parameters:**
- `engineerId` (string): The ID of the engineer (e.g., 'eng-001')

**What it shows:**
- All projects the engineer has ever worked on
- Current total allocation and available capacity
- Chronological list showing: Project from Date1 to Date2
- Each entry marked as [CURRENT] or [PAST] based on end date

**Example:**
```json
{
  "engineerId": "eng-001"
}
```

**Sample Output:**
```
Engineer: Alice Johnson (Senior Software Engineer)
Skills: C#, .NET, React

Current Total Allocation: 100%
Available Capacity: 0%

Allocation History (chronological order by start date):

  - Project Gamma: 100%
    Period: 2024-01-01 to 2024-06-30 [PAST]
  - Project Alpha: 50%
    Period: 2025-01-01 to 2025-06-30 [CURRENT]
  - Project Beta: 50%
    Period: 2025-01-01 to 2025-03-31 [CURRENT]
```

## Date-Based Allocation Tracking

The system automatically determines allocation status based on dates:

- **CURRENT**: `endDate >= today` - Allocation is currently active
- **PAST**: `endDate < today` - Allocation has ended

**How it works:**
- All allocations are preserved forever - never deleted
- Active vs. past is determined automatically by comparing end date to today
- When you move an engineer to bench, their active allocations have `endDate` set to yesterday
- This preserves complete history while automatically reflecting current state

**Benefits:**
- Automatic status determination - no manual tracking needed
- Complete audit trail of all allocations
- Answer questions like "Who has worked on this project?" including past team members
- Chronological view shows the timeline of who worked when
- Over-allocation prevention only counts CURRENT allocations (endDate >= today)

**Example Scenarios:**
1. **Allocation ends naturally**: When endDate passes, it automatically becomes PAST
2. **Move to bench**: System sets endDate to yesterday, immediately marking it as PAST
3. **View history**: See complete chronological list: eng1 2024-01-01 to 2024-06-30, eng2 2024-03-01 to 2024-12-31, etc.

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

## Available MCP Prompts

Prompts are **instructional templates** that guide the LLM on how to answer user questions by using the available tools and resources. When a user asks a question, the LLM can select an appropriate prompt to get step-by-step instructions on which tools to call and how to format the response.

### 1. who_worked_on_project
**Purpose:** Provides instructions for answering "Who worked on this project?" questions

**Parameters:**
- `project` (string): The project name or ID the user is asking about

**What it does:** Gives the LLM step-by-step instructions on:
- Which tools to use (`list_projects`, resources, etc.)
- How to handle ambiguous project names
- How to format the response with engineer details

**Example user questions:**
- "Who worked on Project Alpha?"
- "Show me everyone on proj-001"
- "Which engineers are allocated to Project Beta?"

### 2. what_projects_did_engineer_work_on
**Purpose:** Provides instructions for answering "What projects did this engineer work on?" questions

**Parameters:**
- `engineer` (string): The engineer name or ID the user is asking about

**What it does:** Guides the LLM to:
- Find the engineer using `list_engineers`
- Retrieve allocation details using resources or tools
- Calculate and display total allocation and capacity

**Example user questions:**
- "What projects has Alice Johnson worked on?"
- "Show me all projects for eng-002"
- "Which projects is Bob assigned to?"

### 3. get_allocation_overview
**Purpose:** Instructs how to provide a comprehensive allocation overview

**Parameters:** None

**What it does:** Tells the LLM to:
- Combine data from multiple tools (`list_projects`, `list_engineers`, `get_all_allocations`)
- Organize into sections (Summary, Projects, Availability, Issues)
- Highlight potential problems or opportunities

**Example user questions:**
- "Give me an allocation overview"
- "Show me the current resource allocation"
- "What's the status of all projects and engineers?"

### 4. find_available_engineers
**Purpose:** Instructions for finding engineers with available capacity

**Parameters:**
- `skill` (string, optional): Skill to filter by (e.g., 'React', 'Python')

**What it does:** Guides the LLM to:
- Calculate available capacity for each engineer
- Filter by skills if requested
- Sort by availability and format results

**Example user questions:**
- "Which engineers are available?"
- "Find engineers with React skills who have capacity"
- "Who's on the bench with Python experience?"

### 5. plan_project_allocation
**Purpose:** Instructions for helping plan resource allocation for a new project

**Parameters:**
- `project` (string): The project name or description
- `requiredSkills` (string, optional): Skills needed for the project

**What it does:** Guides the LLM to:
- Identify available engineers matching required skills
- Suggest team composition based on capacity
- Provide next steps for creating allocations

**Example user questions:**
- "Help me plan staffing for Project Delta"
- "I need a team for a React project, who's available?"
- "Plan allocation for a Python/ML project"

**How Prompts Work:**
- Prompts are **instructions**, not implementations
- The LLM reads the prompt and follows the steps
- The LLM then uses the specified tools/resources to gather data
- The LLM formats the final response based on the prompt's guidance
- This allows flexible, intelligent responses to natural language questions

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

---
applyTo: '**'
---
# BusBuddy Project Guidelines

## Version History

- **Version 1.0.0 (June 2025)**: Initial standards for .NET 8, Syncfusion, and SQL Server integration.
- **Version 1.1.0 (June 2025)**: Added feature development process, automation guidance, production readiness criteria, and codebase cleanup.
- **Version 1.2.0 (June 27, 2025)**: Consolidated and refactored duplicate instruction files, enhanced Syncfusion implementation guidelines.

## Coding Standards, Domain Knowledge, and Preferences

### File Management and Cleanup Best Practices

#### 🧹 **Temporary File Cleanup**
Always clean up temporary files created during development:

- **Remove `.new`, `.bak`, `.backup`, `.old` files** after successful operations
- **Delete `*_temp`, `*_tmp`, `*_test` files** when no longer needed
- **Clean up `Migration_Backups/` directories** after migration completion
- **Remove duplicate files** (e.g., `file.cs` and `file_new.cs`)

#### 🚫 **Files to Always Remove**
- Build artifacts: `bin/`, `obj/`, `TestResults/`
- IDE files: `.vs/`, `*.user`, `*.suo`
- Temporary downloads: `*.crdownload`, `*.tmp`
- Backup files: `*.backup_*`, `*_backup*`
- Empty directories that serve no purpose

#### 📝 **Git Repository Hygiene**
- Use `.gitignore` to prevent tracking build artifacts
- Remove large binary files from git history if accidentally committed
- Stage only source files, never build artifacts
- Clean up redundant documentation after project phases complete
- **Remove trailing whitespace** at the end of lines and files
- **Ensure files end with a single newline** character
- **Use consistent line endings** (CRLF on Windows, LF on Unix)

#### ✨ **Code Formatting Standards**
- **No trailing whitespace** - remove spaces/tabs at line endings
- **Consistent indentation** - use spaces or tabs consistently (prefer spaces)
- **File endings** - ensure files end with exactly one newline
- **Line length** - keep lines under 120 characters when practical
- **Empty lines** - use sparingly and consistently for logical separation
- **No nullable reference types** - avoid using nullable properties, parameters, or return types in new code

#### 🔄 **Development Workflow**
When creating temporary files:
1. Use descriptive names with clear temporary indicators
2. Set reminders to clean up after task completion
3. Add temporary patterns to `.gitignore` if needed
4. Use `git clean -fd` to remove untracked files periodically

**File Corruption Assessment Protocol:**
1. **First Check**: Identify specific error messages and their line numbers
2. **Scope Analysis**: Determine if errors are localized (missing method, typo) or systemic
3. **Impact Assessment**: Count affected files and error types
4. **User Consultation**: For 3+ files or complex structural issues, ask user before rebuilding
5. **Documentation**: Always report what was found before proposing solution approach

#### 📊 **Repository Size Management**
- Monitor repository size with `git count-objects -vH`
- Use `git gc --aggressive --prune=now` for cleanup
- Remove files from history with `git filter-repo` if needed
- Keep pushes under 10MB when possible

#### 🎯 **Project-Specific Rules**
- Consolidate similar scripts (keep general ones, remove specific variants)
- Remove duplicate config files (e.g., `codecov.yml` vs `.codecov.yml`)
- Archive old documentation rather than keeping in active project
- Prefer PowerShell scripts over batch files for consistency

#### 🧹 **Codebase Cleanup (Unneeded Code Review)**

**Purpose**: Identify and remove or refactor obsolete/redundant code files due to project evolution, retaining useful code (e.g., 10% still relevant).

**Process:**

1. **Identify Candidates:**
   - Use PowerShell to find files not referenced in solution:
   ```powershell
   # List .cs files not in .sln or .csproj
   $slnFiles = Get-Content BusBuddy.sln | Select-String "\.csproj"
   $projFiles = @()
   foreach ($proj in $slnFiles) {
       $projPath = $proj.Line -replace ".*?, `"([^`"]+)`".*", '$1'
       $projFiles += Get-Content $projPath | Select-String "\.cs`""
   }
   Get-ChildItem -Recurse -Include *.cs | Where-Object { $_.FullName -notin $projFiles.Line } | Select-Object FullName
   ```
   - Check for old/unused files: e.g., `Legacy*.cs`, `Old*.cs`, `*V1.cs`
   - Review files with low recent git activity:
   ```powershell
   # Find files not modified in last 6 months
   Get-ChildItem -Recurse -Include *.cs | Where-Object { $_.LastWriteTime -lt (Get-Date).AddMonths(-6) } | Select-Object FullName
   ```

2. **Inspect Files:**
   - Open each candidate in VS Code
   - Check for references in solution (Ctrl+Shift+F for global search)
   - Assess utility: Does it contain unique logic (e.g., 10% useful code)? Is it duplicated elsewhere?
   - Flag files with no references or redundant logic as "delete candidates"

3. **Decide Action:**
   - **Delete**: If file is obsolete (no references, no unique logic), remove:
   ```powershell
   Remove-Item -Path "path\to\file.cs" -Force
   git add "path\to\file.cs"
   git commit -m "Removed obsolete file: file.cs"
   ```
   - **Refactor**: If file has useful code (e.g., 10%), move relevant logic to appropriate file (e.g., `BusBuddy.Business\Service.cs`), then delete original
   - **Archive**: If unsure, move to `Archive/` folder and exclude from solution:
   ```powershell
   Move-Item -Path "path\to\file.cs" -Destination "Archive\file.cs"
   git add "Archive\file.cs"
   git commit -m "Archived potentially obsolete file: file.cs"
   ```

4. **Validate:**
   - Build solution: `dotnet build BusBuddy.sln --configuration Release`
   - Run tests: `dotnet test BusBuddy.Tests`
   - If errors occur, restore file from git (`git checkout -- path\to\file.cs`) and reassess

5. **Document:**
   - Log deletions/refactorings in `CHANGELOG.md`:
   ```
   - Removed obsolete file: LegacyService.cs
   - Refactored useful logic from OldUtils.cs to UtilsService.cs
   ```

**Safeguards:**
- Commit changes before deletion: `git commit -m "Pre-cleanup state"`
- Use git to recover files if needed: `git checkout -- path\to\file.cs`
- For 3+ files or complex logic, consult Copilot with this MD file refreshed before proceeding

**Frequency**: Review monthly or before major feature additions.

#### 📊 **Repository Size Management**

- Monitor size: `git count-objects -vH`
- Clean up: `git gc --aggressive --prune=now`
- Keep pushes <10MB

#### ✨ **Code Formatting**

- **No trailing whitespace**
- **Consistent indentation** (prefer spaces)
- **Single newline** at file end
- **Lines <120 characters** when practical
- **Sparse, consistent empty lines**
- **No nullable reference types** in new code

### Feature Development

**Process:**
1. Define requirements in temporary spec (e.g., `FeatureSpec.md`)
2. Add feature rules to this file before implementation:
   ```markdown
   ### [Feature Name]
   - **Purpose**: [Description]
   - **UI**: [Syncfusion controls, documented patterns]
   - **Data**: [Entities in `BusBuddy.Models`]
   - **Logic**: [Services in `BusBuddy.Business`]
   - **Testing**: [Tests in `BusBuddy.Tests`]
   - **Constraints**: [Prohibitions, e.g., no undocumented APIs]
   ```
3. Implement with Copilot, refreshing this file via "Send to New Chat" per iteration
4. Refine rules post-implementation if Copilot deviates
5. Archive spec after feature is stable

### Copilot Workflow

- **Refresh this file** via "Send to New Chat" in VS Code Copilot Agent for each session to maintain context
- **Split long tasks** into smaller sessions, refreshing this file between tasks

### Syncfusion Guidelines

**See detailed requirements in the "Syncfusion Implementation Requirements" section at the end of this document**

**Mandatory**: Use only official documentation (https://help.syncfusion.com/cr/windowsforms/Syncfusion.html)

**Quick Reference:**
1. Search docs for control/feature
2. Copy patterns from sample browser or "Getting Started" guides
3. Verify properties/methods in API reference
4. Include doc link in code comments

**Forbidden:**
- Custom extensions, wrappers, or undocumented APIs
- Assumed patterns from other frameworks

**Example:**
```csharp
// https://help.syncfusion.com/cr/windowsforms/Syncfusion.html
_dockingManager.DockControl(panel, this, DockingStyle.Left, 280); // Documented enum
```

### Automation

- **Linters**: Use StyleCop for formatting (no trailing whitespace, consistent indentation)
- **Git Hooks**: Pre-commit checks for temporary files:
  ```powershell
  Get-ChildItem -Recurse -Include *.bak,*.tmp | Remove-Item -Force
  ```
- **CI/CD**: GitHub Actions for tests, coverage, and Syncfusion API validation

### Production Readiness

**Criteria:**
- 100% test coverage in `BusBuddy.Tests`
- No temporary files or undocumented Syncfusion APIs
- Zero build errors: `dotnet build --configuration Release`
- Passing CI/CD checks

## BusBuddy Domain Knowledge

#### 🚌 **Core Business Context**
- **School transportation management system** for bus fleet operations
- **Key entities**: Vehicles, Drivers, Routes, Maintenance, Fuel, Activities
- **Primary users**: Transportation coordinators, mechanics, administrators
- **Critical features**: Safety compliance, route optimization, maintenance tracking

#### 🎨 **UI/UX Standards**
- **Syncfusion controls preferred** over standard Windows Forms
- **Material Design theming** with consistent color schemes
- **Responsive layouts** that handle DPI scaling properly
- **Enhanced dashboards** with diagnostic logging and fallback strategies

#### 📚 **Syncfusion Essential Windows Forms Guidelines**

Based on [Syncfusion Windows Forms Overview](https://help.syncfusion.com/windowsforms/overview):

**Core Principles:**
- **100+ Essential Controls** - Comprehensive collection for enterprise applications
- **Performance-first** - Unparalleled performance and rendering
- **Touch-friendly UI** - Modern, responsive interface design
- **Built-in themes** - Professional, consistent visual styling
- **Visual Studio integration** - Seamless development experience

**Key Control Categories:**
- **Data Visualization**: Chart, Diagram, Maps, Gauges, TreeMap, Sparkline
- **Data Management**: DataGrid, Grid Control, Pivot Grid, GridGroupingControl
- **Navigation**: Docking Manager, TabControl, TreeView, Navigation Drawer, Ribbon
- **Layout Management**: Border Layout, Flow Layout, Grid Layout, Tile Layout
- **Input Controls**: MaskedTextBox, AutoComplete, ColorPicker, DateTimePickerAdv
- **File Handling**: PDF Viewer, Spreadsheet, Syntax Editor, HTML Viewer

**BusBuddy-Specific Usage:**
- **Docking Manager**: Primary dashboard layout with professional docking
- **DataGrid**: Vehicle, driver, and route data management
- **Chart Controls**: Fleet analytics and performance visualization
- **Scheduler**: Route planning and maintenance scheduling
- **TabControl**: Multi-module interface organization
- **TreeView**: Hierarchical data navigation (routes, organizations)

**Development Best Practices:**
1. **Getting Started**: Always refer to component-specific "Getting Started" guides
2. **Code Examples**: Use sample browser with hundreds of code examples
3. **API Reference**: Detailed object hierarchy and settings documentation
4. **Search First**: Use search functionality for specific features
5. **Licensing**: Proper license key registration to avoid runtime dialogs

**Resource Utilization:**
- **Knowledge Base**: Common questions and solutions
- **Community Forums**: Peer support and discussions
- **Support Tickets**: Direct technical assistance
- **Feedback Portal**: Feature requests and suggestions

**Version Compatibility:**
- **Target .NET 8.0+** (Support for .NET 6.0/7.0 discontinued in 2025 Volume 1)
- **Regular updates** following Syncfusion release cycles
- **Backward compatibility** considerations for existing components

#### 💾 **Data Architecture**
- **Entity Framework Core** for data access
- **Repository pattern** with dependency injection
- **SQL Server backend** with proper connection management
- **Test database initialization** for development/testing

#### 🧪 **Testing Approach**
- **UI tests** in `BusBuddy.Tests/UI/` directory
- **Integration tests** for data layer operations
- **Coverage reports** generated via PowerShell scripts
- **Avoid legacy test files** - consolidate into organized structure

#### 🔧 **Development Tools**
- **PowerShell (pwsh)** for ALL commands - use abbreviated pwsh when possible
- **Single build approach** - run one build, get data, move on - no repetitive builds
- **VS Code tasks** for build/test operations
- **Syncfusion licensing** handled via helper classes
- **Git hooks** for code quality enforcement

#### 💬 **Communication Preferences**
- **Concise responses** - conserve time with brief, focused answers
- **One-form-at-a-time** approach for fixing build errors
- **Efficient debugging** - minimal tool calls, maximum progress
- **Direct action** - fix issues immediately rather than extensive analysis

#### 🔧 **Problem Resolution Approach**
- **Incremental fixes first** - Always attempt targeted edits before complete rebuilds
- **Assess corruption level** - Check actual errors and their scope before deciding approach
- **User consultation** - Let the user decide on complete overhauls vs. targeted fixes
- **Error analysis** - Identify root causes (missing methods, property name mismatches, duplicates)
- **Minimal viable fix** - Use the smallest change that resolves the issue
- **Escalation path**:
  1. First: Targeted edits for specific errors
  2. Second: Consult user if issues appear complex
  3. Last resort: Complete rebuild only with user approval

#### 🏗️ **Syncfusion Implementation Requirements**

**CRITICAL RULE: Only Use Official Syncfusion Documentation**
- **Reference ONLY**: https://help.syncfusion.com/cr/windowsforms/Syncfusion.html
- **No custom fixes**: Use only documented Syncfusion APIs, methods, and examples
- **No invented code**: Every Syncfusion implementation must be found in official docs
- **Verify before implementing**: Search documentation first, implement only documented patterns
- **API Reference**: Use Syncfusion's complete API reference for all controls and methods

**Documentation-First Development Process:**
1. **Search Syncfusion docs** for the specific control/feature needed
2. **Find official examples** in the documentation or sample browser
3. **Copy exact patterns** from Syncfusion's documented examples
4. **Test with documented parameters** and properties only
5. **No modifications** to documented patterns without verifying in docs

**Common Syncfusion Controls - Documentation Required:**
- **RibbonControlAdv**: Use Header.AddMainItem() for tabs (documented pattern)
- **DockingManager**: Use DockingStyle enum for docking operations
- **TileLayout**: Use LayoutGroup and HubTile as per documentation
- **SfDataGrid**: Follow documented binding and column configuration patterns
- **ChartControl**: Use only documented series types and properties
- **SfButton**: Apply only documented style properties and themes

**Forbidden Practices:**
- ❌ **NO custom Syncfusion extensions** or helper methods
- ❌ **NO invented property combinations** not shown in docs
- ❌ **NO assumed API patterns** based on other frameworks
- ❌ **NO "enhanced" wrappers** around Syncfusion controls
- ❌ **NO undocumented parameters** or method calls

**Required Verification Steps:**
1. **Before any Syncfusion code**: Search help.syncfusion.com for exact usage
2. **Cross-reference examples**: Find matching code in Syncfusion's sample browser
3. **API validation**: Verify all properties/methods exist in official API reference
4. **Documentation links**: Include reference to specific Syncfusion doc page used

**Documentation Resources (USE THESE ONLY):**
- **Main API Reference**: https://help.syncfusion.com/cr/windowsforms/Syncfusion.html
- **Getting Started Guides**: Component-specific documentation
- **Sample Browser**: Code examples and demonstrations
- **Knowledge Base**: Official solutions to common issues

**Example of Correct Documentation-Based Implementation:**
```csharp
// Based on official Syncfusion RibbonControlAdv documentation
var tabItem = new TabHost
{
    Text = "Dashboard"
};
_ribbonControl.Header.AddMainItem(tabItem); // Documented method

// Based on official DockingManager documentation
_dockingManager.DockControl(panel, this, DockingStyle.Left, 280); // Documented enum
```

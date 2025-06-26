---
applyTo: '**'
---
# BusBuddy Project Guidelines

## Coding Standards, Domain Knowledge, and Preferences

### File Management and Cleanup Best Practices

#### 🧹 **Temporary File Cleanup**
Always clean up temporary files created during development:

- **Remove `.new`, `.bak`, `.backup`, `.old` files** after successful operations
- **Delete `*_temp`, `*_tmp`, `*_test` files** when no longer needed
- **Clean up `Migration_Backups/` directories** after migration completion
- **Remove duplicate files** (e.g., `file.cs` and `file_new.cs`)
- **KEEP all run-and-log scripts** (run-and-log.ps1, run-and-check-logs.ps1, run-and-capture-logs.ps1) as they are critical utilities

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
- **KEEP all run-and-log scripts** (run-and-log.ps1, run-and-check-logs.ps1, run-and-capture-logs.ps1) as they serve different important purposes
- Remove duplicate config files (e.g., `codecov.yml` vs `.codecov.yml`)
- Archive old documentation rather than keeping in active project
- Prefer PowerShell scripts over batch files for consistency

### BusBuddy Domain Knowledge

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

#### 📐 **Layout Management Best Practices**

**Documentation References in Code:**
- **Include documentation URLs in comments** for all Syncfusion components
  ```csharp
  // Based on: https://help.syncfusion.com/cr/windowsforms/Syncfusion.Windows.Forms.Tools.CardLayout.html
  var cardLayout = new CardLayout
  {
      ContainerControl = container
  };
  ```
- **Link to specific API documentation** rather than general pages
- **Document method purpose and source** for all Syncfusion-related methods
- **Update documentation references** when upgrading Syncfusion versions

**Responsive Layout Implementation:**
- **Use container-based sizing** instead of fixed pixel dimensions
- **Implement orientation switching** based on container size thresholds
- **Calculate proportional dimensions** using percentages rather than fixed values
- **Handle resize events** to dynamically adjust layouts
- **Validate dimensions** before applying to prevent exceptions
- **Use SplitContainer orientation changes** for responsive layouts
- **Implement fault-tolerant resizing** with proper error handling

**DPI Scaling Requirements:**
- **Test all layouts at multiple DPI settings** (100%, 125%, 150%, 200%)
- **Use Dock and Anchor properties** to ensure proper scaling
- **Avoid fixed pixel sizing** for controls that need to scale
- **Test with Windows display scaling** settings changed
- **Handle system font scaling** appropriately
- **Use AutoScaleMode.Dpi** for all forms
- **Validate layout after DPI changes** with visual inspection

**Layout Manager Selection Guide:**
| Scenario | Recommended Layout | Benefits |
|----------|-------------------|----------|
| Dashboard with statistics | TableLayoutPanel | Consistent proportional sizing |
| Card-based UI | Syncfusion CardLayout | Clean switching between views |
| Dynamic content | Syncfusion FlowLayout | Automatic arrangement with gaps |
| Master-detail views | SplitContainer | Adjustable ratio between sections |
| Complex data entry | Syncfusion Form Layout | Consistent field arrangement |
| Responsive dashboards | Combined approach | Orientation changes on resize |

#### 🔄 **Syncfusion Control Implementation Patterns**

**Layout Manager Implementation:**
- **CardLayout**: Use for switching between different views in the same container
  ```csharp
  // Based on: https://help.syncfusion.com/cr/windowsforms/Syncfusion.Windows.Forms.Tools.CardLayout.html
  var cardLayout = new CardLayout
  {
      ContainerControl = container
  };
  cardLayout.SetCardName(childPanel, "UniqueCardName");
  cardLayout.SelectedCard = "UniqueCardName"; // To switch to this card
  ```

- **FlowLayout**: Use for dynamically arranged controls with proper spacing
  ```csharp
  // Based on: https://help.syncfusion.com/cr/windowsforms/Syncfusion.Windows.Forms.Tools.FlowLayout.html
  var flowLayout = new FlowLayout
  {
      ContainerControl = container,
      HGap = 10, // Horizontal gap between controls
      VGap = 10  // Vertical gap between controls
  };
  ```

- **DockingManager**: Use for professional window management
  ```csharp
  // Based on: https://help.syncfusion.com/cr/windowsforms/Syncfusion.Windows.Forms.Tools.DockingManager.html
  var dockingManager = new DockingManager
  {
      EnableDocumentMode = true,
      DockControlPadding = new Padding(5)
  };
  dockingManager.DockControl(panel, this, DockingStyle.Left, 280);
  ```

- **TileLayout**: Use for modern dashboard tile arrangements
  ```csharp
  // Based on: https://help.syncfusion.com/cr/windowsforms/Syncfusion.Windows.Forms.Tools.TileLayout.html
  var tileLayout = new TileLayout
  {
      Dock = DockStyle.Fill,
      ThemeName = "Office2019Colorful"
  };
  var group = new LayoutGroup();
  tileLayout.LayoutGroups.Add(group);
  ```

**Theming Implementation Pattern:**
- **Use consistent theme names** across all Syncfusion controls
  ```csharp
  // Based on: https://help.syncfusion.com/cr/windowsforms/Syncfusion.Windows.Forms.Tools.SfButton.html#Syncfusion_Windows_Forms_Tools_SfButton_ThemeName
  button.ThemeName = "Office2019Colorful";
  grid.ThemeName = "Office2019Colorful";
  ```
- **Apply theme programmatically** to ensure consistency
  ```csharp
  // Based on: https://help.syncfusion.com/cr/windowsforms/Syncfusion.Windows.Forms.SkinManager.html
  SkinManager.SetVisualStyle(this, VisualStyles.Office2019Colorful);
  ```
- **Use Office2019Colorful theme** as the project standard unless specified otherwise

**High-DPI Implementation:**
- **Test all forms at 4K resolution** with 200% scaling
- **Use AutoScaleDimensions and AutoScaleMode** appropriately
  ```csharp
  this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
  this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
  ```
- **Apply DpiAware attribute** to the application
  ```csharp
  [assembly: System.Windows.Forms.DpiAware(true)]
  ```

#### 🔧 **Advanced Syncfusion Implementation Techniques**

**Fallback Strategies for Layout Problems:**
- **Include error handling in layout code** with clear logging
- **Provide visual fallback** when expected layout fails
- **Use try/catch with meaningful error messages** in all layout methods
- **Log detailed diagnostic information** about control dimensions
- **Implement auto-recovery** for invalid layout states

**Control Reference Management:**
- **Store layout manager references** in control's Tag property
- **Check manager type before operations** to prevent invalid casts
- **Clear references properly** when disposing controls
- **Use consistent container nesting patterns** for complex layouts

**Layout Debugging Techniques:**
- **Add debug borders** to visualize container boundaries
  ```csharp
  #if DEBUG
  container.BorderStyle = BorderStyle.FixedSingle;
  #endif
  ```
- **Log control dimensions** during layout operations
- **Use Dashboard's shared logging** for diagnosing layout issues
- **Record and verify splitter positions** in split container layouts

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
- **Syncfusion licensing** is already properly configured in Program.cs and should not be modified
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
- **DO NOT modify existing Syncfusion licensing**: The licensing is already properly configured in Program.cs and should not be changed
- **NO additional license helpers or managers**: Do not create or use any custom licensing helpers, managers, or wrappers as they are unnecessary

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

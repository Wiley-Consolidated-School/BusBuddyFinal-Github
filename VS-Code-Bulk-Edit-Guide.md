# VS Code Bulk Edit Guide for Null Reference Issues

## Quick Find/Replace Patterns (Ctrl+Shift+H)

### Pattern 1: Controls.Add() - CRITICAL (143 issues)
**Find:** `([a-zA-Z_][a-zA-Z0-9_]*)\.Controls\.Add\(([^)]+)\);`
**Replace:** ```
if ($1?.Controls != null)
{
    $1.Controls.Add($2);
}
```
**Use Regex:** ✅ Enabled

### Pattern 2: Repository Method Calls - HIGH
**Find:** `([a-zA-Z_][a-zA-Z0-9_]*Repository)\.([a-zA-Z][a-zA-Z0-9_]*)\(([^)]*)\)`
**Replace:** ```
if ($1 != null)
{
    $1.$2($3)
}
```
**Use Regex:** ✅ Enabled

### Pattern 3: Service Method Calls - HIGH  
**Find:** `(_[a-zA-Z][a-zA-Z0-9_]*Service)\.([a-zA-Z][a-zA-Z0-9_]*)\(([^)]*)\)`
**Replace:** ```
if ($1 != null)
{
    $1.$2($3)
}
```
**Use Regex:** ✅ Enabled

### Pattern 4: Collection Count/Length - MEDIUM
**Find:** `([a-zA-Z_][a-zA-Z0-9_]*)\.(Count|Length)`
**Replace:** `($1?.$2 ?? 0)`
**Use Regex:** ✅ Enabled

### Pattern 5: Collection Operations - HIGH
**Find:** `([a-zA-Z_][a-zA-Z0-9_]*)\.(Clear|Add|Remove)\(([^)]*)\);`
**Replace:** ```
if ($1 != null)
{
    $1.$2($3);
}
```
**Use Regex:** ✅ Enabled

## Multi-File Search and Replace
1. **Ctrl+Shift+F** - Open Search panel
2. Click **Replace** (triangle icon)
3. Set **Files to Include:** `**/*.cs`
4. Set **Files to Exclude:** `**/bin/**, **/obj/**, **/TestResults/**`
5. Use patterns above
6. Review each match before replacing

## Batch Processing Order
1. **Start with CRITICAL issues** (Controls.Add patterns)
2. **Then HIGH priority** (Repository/Service calls)
3. **Finally MEDIUM** (Collection operations)
4. **Test after each batch**

## File-Specific Targets
### High Priority Files:
- `VehicleEditForm.cs` - 20+ Controls.Add issues
- `VehicleFormSyncfusion.cs` - 30+ Controls.Add issues  
- `Dashboard.cs` - Multiple control management issues
- `*Repository.cs` - Database access patterns
- `*Service.cs` - Service call patterns

### Medium Priority Files:
- `ValidationService.cs` - Method call patterns
- `VehicleManagementForm.cs` - Collection operations
- `*ManagementForm.cs` - Mixed patterns

## Safety Tips
1. **Always backup before bulk edits**
2. **Process one pattern at a time**
3. **Review each replacement**
4. **Test compile after each batch**
5. **Use Git to track changes**

## Command Line Shortcuts
```powershell
# Quick test after bulk edits
dotnet build

# Run null reference analysis
dotnet test --filter "NullReferenceAnalysisTest"

# Check git status
git status
```

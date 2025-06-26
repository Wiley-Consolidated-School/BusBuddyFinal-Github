# Known Issues and Anti-Patterns

This document lists known issues, anti-patterns, and problematic code approaches that should be avoided in the BusBuddy project.

## ⚠️ Syncfusion License Registration Anti-Pattern

**Issue**: There is a recurring tendency to create license manager/helper classes for Syncfusion license registration.

**Correct Approach**: Always register Syncfusion licenses directly in Program.cs as described in [SYNCFUSION_LICENSE_GUIDELINES.md](../SYNCFUSION_LICENSE_GUIDELINES.md).

**Why This Keeps Happening**: 
1. Developers may be following common enterprise patterns of centralizing license management
2. Test failures related to Syncfusion controls might be incorrectly addressed by creating helper classes
3. Misinterpretation of documentation or common practices from other frameworks

**How to Detect**: Watch for classes with names like:
- SyncfusionLicenseManager
- LicenseHelper
- SyncfusionLicenseHelper
- LicenseRegistration
- LicenseService

**Prevention**: 
- See detailed anti-patterns in [SYNCFUSION_LICENSE_GUIDELINES.md](../SYNCFUSION_LICENSE_GUIDELINES.md)
- The license registration MUST remain in Program.cs
- Test failures should be addressed through proper test setup, not by moving license registration

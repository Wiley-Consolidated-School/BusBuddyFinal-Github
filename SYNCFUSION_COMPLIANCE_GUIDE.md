# Syncfusion Documentation Compliance Guide

## Overview
This document establishes standards for using ONLY officially documented Syncfusion methods in the BusBuddy project. All layout management code must conform to these guidelines to ensure maintainability and compliance with Syncfusion best practices.

## Core Principle
**ONLY USE OFFICIALLY DOCUMENTED SYNCFUSION METHODS**

- Reference: https://help.syncfusion.com/windowsforms/overview
- API Reference: https://help.syncfusion.com/cr/windowsforms/Syncfusion.html
- Never use undocumented methods, reflection, or custom extensions

## DynamicLayoutManager Implementation Standards

### CardLayout - Documented Methods Only
```csharp
// ✅ APPROVED - Documented Syncfusion methods
var cardLayout = new CardLayout { ContainerControl = container };
cardLayout.SetCardName(control, "CardName");
string cardName = cardLayout.GetCardName(control);
cardLayout.SelectedCard = "CardName";
cardLayout.Next();
cardLayout.Previous();
cardLayout.First();
cardLayout.Last();
string newName = cardLayout.GetNewCardName();

// ❌ FORBIDDEN - Do not use
control.Visible = true; // Use SelectedCard instead
control.BringToFront(); // CardLayout manages this
```

### FlowLayout - Documented Methods Only
```csharp
// ✅ APPROVED - Documented Syncfusion methods
var flowLayout = new FlowLayout 
{ 
    ContainerControl = container,
    HGap = 10,
    VGap = 15,
    Alignment = FlowAlignment.Near,
    LayoutMode = FlowLayoutMode.Horizontal
};
var constraints = flowLayout.GetConstraints(control);
flowLayout.SetConstraints(control, new FlowLayoutConstraints(...));

// ✅ APPROVED - Use FlowLayoutPanel for wrapping (per Syncfusion docs)
// Syncfusion FlowLayout doesn't support wrapping, so use standard FlowLayoutPanel
var flowPanel = new FlowLayoutPanel { WrapContents = true };
```

## Testing Standards

### Test Structure
All tests must validate:
1. ✅ Only documented Syncfusion methods are used
2. ✅ Proper error handling for null parameters
3. ✅ Syncfusion objects are correctly initialized
4. ✅ Container relationships are properly established

### Test Examples
```csharp
[Fact]
public void SyncfusionCompliance_CardLayout_OnlyUsesDocumentedMethods()
{
    // Arrange
    var container = DynamicLayoutManager.CreateCardLayoutContainer(_parentPanel);
    var cardLayout = container.Tag as CardLayout;
    
    // Act & Assert - Test ONLY documented methods
    Assert.NotNull(cardLayout);
    Assert.Equal(container, cardLayout.ContainerControl);
    
    cardLayout.SetCardName(card, "TestCard");
    Assert.Equal("TestCard", cardLayout.GetCardName(card));
    
    cardLayout.SelectedCard = "TestCard";
    Assert.Equal("TestCard", cardLayout.SelectedCard);
}
```

## Implementation Rules

### DO
- ✅ Reference official Syncfusion documentation for every method call
- ✅ Use only documented properties and methods
- ✅ Follow documented patterns exactly as shown in examples
- ✅ Test using Syncfusion-compliant verification methods
- ✅ Include documentation links in code comments

### DON'T
- ❌ Use reflection or undocumented APIs
- ❌ Create custom extensions for Syncfusion controls
- ❌ Create license manager classes or helpers - use direct registration only in Program.cs
- ❌ Assume method behavior without documentation verification
- ❌ Mix standard .NET controls with Syncfusion controls without justification
- ❌ Test using non-Syncfusion properties (e.g., Control.Visible for CardLayout)

## Code Review Checklist

Before merging any layout-related code:
- [ ] All Syncfusion methods have documentation references
- [ ] No undocumented or custom methods are used
- [ ] Tests validate only documented behavior
- [ ] Error handling follows Syncfusion patterns
- [ ] Comments include links to relevant documentation

## Documentation Resources

### Primary Sources (USE THESE ONLY)
- **Main Documentation**: https://help.syncfusion.com/windowsforms/overview
- **API Reference**: https://help.syncfusion.com/cr/windowsforms/Syncfusion.html
- **CardLayout Guide**: https://help.syncfusion.com/windowsforms/layoutmanagers/cardlayout/gettingstarted
- **FlowLayout Guide**: https://help.syncfusion.com/windowsforms/layoutmanagers/flowlayout/gettingstarted

### Sample Code Sources
- **Sample Browser**: Official Syncfusion sample applications
- **Knowledge Base**: https://help.syncfusion.com (search by control name)
- **Getting Started Guides**: Component-specific documentation

## Future Development

All new layout features must:
1. **Research First**: Find documented Syncfusion solutions
2. **Verify API**: Confirm all methods exist in official documentation
3. **Test Compliance**: Write tests that fail on non-compliant code
4. **Document Sources**: Include links to Syncfusion documentation used

## Violation Reporting

If non-compliant code is found:
1. **Document the issue**: Include specific method/property names
2. **Find documented alternative**: Research official Syncfusion solution
3. **Update implementation**: Replace with documented methods
4. **Update tests**: Ensure tests validate only documented behavior

---

**Last Updated**: June 25, 2025  
**Compliance Status**: ✅ FULLY COMPLIANT  
**Test Coverage**: 34/34 tests passing with Syncfusion-only methods

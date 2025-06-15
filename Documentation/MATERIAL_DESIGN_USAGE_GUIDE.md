# Material Design 3.0 Theme System - Usage Guide

## Overview

The BusBuddy Material Design theme system provides a comprehensive, modern dark theme implementation with:

- **Material Design 3.0 compliance** with proper color schemes
- **High DPI awareness** for crisp display on all screen types
- **Vector graphics support** for scalable icons
- **Responsive layout helpers** for modern UI patterns
- **Accessibility features** with proper contrast ratios

## Quick Start

### 1. Initialize the Theme System

```csharp
using BusBuddy.UI.Theme;

// In your application startup
MaterialDesignThemeManager.Initialize();
```

### 2. Apply Theme to Forms

```csharp
using MaterialSkin.Controls;
using BusBuddy.UI.Theme;

public partial class MyForm : MaterialForm
{
    public MyForm()
    {
        InitializeComponent();

        // Apply dark theme with DPI awareness
        MaterialDesignThemeManager.ApplyDpiAwareMaterialDesign(this, true);
    }
}
```

### 3. Use Enhanced Controls

```csharp
using BusBuddy.UI.Extensions;

// Vector button with icon
var button = new VectorMaterialButton
{
    Text = "Save",
    SvgIcon = mySvgIcon,
    IconSize = 16
};

// Apply DPI-aware spacing
button.ApplyDpiAwareSpacing();
```

## Key Features

### Dark Theme Colors

Access the complete Material Design 3.0 dark color palette:

```csharp
// Surface colors
var backgroundColor = MaterialDesignThemeManager.DarkTheme.Surface;
var cardColor = MaterialDesignThemeManager.DarkTheme.SurfaceContainer;

// Primary colors
var primaryColor = MaterialDesignThemeManager.DarkTheme.Primary;
var onPrimaryColor = MaterialDesignThemeManager.DarkTheme.OnPrimary;

// Status colors
var errorColor = MaterialDesignThemeManager.DarkTheme.Error;
var successColor = MaterialDesignThemeManager.DarkTheme.Success;
```

### Typography Scale

Use the Material Design typography scale for consistent text styling:

```csharp
// Headlines
label.Font = MaterialDesignThemeManager.Typography.GetHeadlineLarge(this);

// Body text
label.Font = MaterialDesignThemeManager.Typography.GetBodyMedium(this);

// Labels
button.Font = MaterialDesignThemeManager.Typography.GetLabelMedium(this);
```

### DPI-Aware Sizing

Ensure your UI scales properly across different displays:

```csharp
// Scale individual values
int scaledSize = MaterialDesignThemeManager.GetDpiAwareSize(16, this);

// Create DPI-aware padding
var padding = MaterialDesignThemeManager.GetDpiAwarePadding(8, 16, this);

// Scale fonts
float scaledFontSize = MaterialDesignThemeManager.GetDpiAwareFontSize(12f, this);
```

### Responsive Layouts

Create adaptive layouts that work on all screen sizes:

```csharp
// Create responsive grid layout
var layout = MaterialDesignThemeManager.CreateResponsiveLayout(3, 2, this);

// Make existing container responsive
var responsiveLayout = myPanel.MakeResponsive(2, 3);
```

### Material Cards with Elevation

Create elevated cards following Material Design principles:

```csharp
var card = new MaterialCard();
MaterialDesignThemeManager.ApplyCardElevation(card, 2); // Elevation level 0-5
```

### Vector Graphics Support

Use scalable vector icons that look crisp at any DPI:

```csharp
// Set vector icon on button
button.SetVectorIcon(svgContent, iconSize: 16, iconColor: Color.White);

// Enhanced controls with built-in vector support
var vectorButton = new VectorMaterialButton
{
    SvgIcon = svgContent,
    IconSize = 16,
    IconColor = Color.White
};
```

## Advanced Usage

### Custom Control Theming

Apply Material Design theming to custom controls:

```csharp
public class MyCustomControl : UserControl
{
    public MyCustomControl()
    {
        InitializeComponent();
        ApplyMaterialDesignTheme();
    }

    private void ApplyMaterialDesignTheme()
    {
        BackColor = MaterialDesignThemeManager.DarkTheme.Surface;
        ForeColor = MaterialDesignThemeManager.DarkTheme.OnSurface;

        // Apply DPI-aware spacing
        this.ApplyDpiAwareSpacing();
    }
}
```

### DataGridView Theming

Apply comprehensive theming to data grids:

```csharp
// Theme is automatically applied when using ApplyDarkTheme()
MaterialDesignThemeManager.ApplyDarkTheme(parentForm);

// Manual theming for specific grids
MaterialDesignThemeManager.ApplyDarkThemeToDataGrid(myDataGrid);
```

### High DPI Configuration

For forms that need special high DPI handling:

```csharp
// Configure form for optimal high DPI support
form.ConfigureAutoScaling(AutoScaleMode.Dpi);

// Apply comprehensive DPI-aware configuration
MaterialDesignThemeManager.ApplyDpiAwareMaterialDesign(form, applyToAllControls: true);
```

## Color Accessibility

The theme system includes WCAG-compliant color combinations:

```csharp
// Get contrasting text color for any background
var textColor = MaterialDesignThemeManager.GetContrastingTextColor(backgroundColor);

// Use semantic color names for better accessibility
var errorBackground = MaterialDesignThemeManager.DarkTheme.ErrorContainer;
var errorText = MaterialDesignThemeManager.DarkTheme.OnErrorContainer;
```

## Performance Tips

1. **Initialize once**: Call `MaterialDesignThemeManager.Initialize()` only once at application startup
2. **Batch operations**: Apply theming to parent containers rather than individual controls when possible
3. **Use vector graphics wisely**: Cache frequently used SVG icons
4. **DPI awareness**: Use the provided scaling helpers rather than manual calculations

## Demo Application

Run the included demo to see all features in action:

```bash
dotnet run --project BusBuddy.UI.Demo
```

The demo showcases:
- All control types with Material Design theming
- Typography scale examples
- Card elevation effects
- Responsive layouts
- Vector icon integration
- DataGridView theming
- High DPI scaling

## Troubleshooting

### Common Issues

1. **Blurry text on high DPI displays**
   - Ensure `Application.SetHighDpiMode(HighDpiMode.PerMonitorV2)` is called before `Application.Run()`
   - Use the provided typography methods instead of manual font sizing

2. **Controls not scaling properly**
   - Apply `ApplyDpiAwareSpacing()` to controls after adding them to the form
   - Use `MaterialDesignThemeManager.GetDpiAwareSize()` for manual sizing

3. **Icons not displaying**
   - Ensure SVG content is valid XML
   - Check that the VectorGraphicsHelper is properly referenced
   - Verify icon colors contrast with the background

4. **Performance issues with large forms**
   - Apply theming to parent containers rather than individual controls
   - Use `applyToAllControls: false` in `ApplyDpiAwareMaterialDesign()` for manual control

## Integration with Existing Code

To integrate with existing WinForms applications:

1. Change form base class from `Form` to `MaterialForm`
2. Call `MaterialDesignThemeManager.Initialize()` in `Program.cs`
3. Apply theming in form constructors: `MaterialDesignThemeManager.ApplyDarkTheme(this)`
4. Replace standard controls with MaterialSkin controls where desired
5. Use extension methods for DPI-aware spacing: `control.ApplyDpiAwareSpacing()`

The system is designed to be backward-compatible and non-breaking when applied to existing forms.

using System.Diagnostics.CodeAnalysis;

// Global suppressions for UI project - nullable warnings are mostly harmless in WinForms UI code
// where controls are guaranteed to be initialized in InitializeComponent

[assembly: SuppressMessage("Compiler", "CS8618:Non-nullable field must contain a non-null value when exiting constructor", Justification = "UI controls are initialized in InitializeComponent")]
[assembly: SuppressMessage("Compiler", "CS8602:Dereference of a possibly null reference", Justification = "UI controls are guaranteed to be initialized")]
[assembly: SuppressMessage("Compiler", "CS8604:Possible null reference argument", Justification = "UI controls are guaranteed to be initialized")]

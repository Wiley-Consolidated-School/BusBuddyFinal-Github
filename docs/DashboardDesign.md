# BusBuddy Dashboard UI Design Specification

**Created**: June 22, 2025  
**Status**: Task 9 - Dashboard UI Design  
**Purpose**: Formalize the dashboard layout for BusBuddyDashboard.cs implementation

## Design Overview

The BusBuddy dashboard implements a modern, data-driven interface optimized for CDE-40 reporting and transportation management. The design follows Syncfusion's Office2016Black theme with professional visual hierarchy and interactive data components.

## Layout Specification

### Header Section (Top, 80px height)
```
+------------------------------------------------------------------+
|  🚌 BusBuddy Transportation Dashboard    [Theme Selector] [X]    |
+------------------------------------------------------------------+
```

**Components:**
- **Title Label**: "🚌 BusBuddy Transportation Dashboard" (Segoe UI, 18pt, Bold, White)
- **Theme Selector ComboBox**: Office2016Black/White/Colorful (150px width, right-aligned)  
- **Close Button**: Standard form close (right-aligned)
- **Background**: Color.FromArgb(68, 68, 68) - Office2016Black primary

### Navigation Section (Left, 250px width, collapsible)
```
+------------------+
| 🚌 BusBuddy      |
|==================|
| 📊 Dashboard     |
| 📋 Routes        |
| 🚌 Vehicles      |
| 👤 Drivers       |
| ⚙️ Maintenance   |
| 📅 Activities    |
| 📈 CDE-40 Report |
| ⚙️ Settings      |
+------------------+
```

**Component**: `NavigationDrawer` (SlidePosition.Left)
- **Header**: "🚌 BusBuddy" with blue accent background
- **Menu Items**: Dashboard, Routes, Vehicles, Drivers, Maintenance, Activities, CDE-40 Report, Settings

### Main Content Area (Fill remaining space)

#### Metrics Panel (Top, 200px height)
```
+------------------------------------------------------------------------+
|  COST PER STUDENT    |    TOTAL MILES     |    PUPILS TRANSPORTED    |
|      $2.70           |      45.2K         |         1,850            |
|   [RadialGauge]      |  [RadialGauge]     |     [RadialGauge]        |
+------------------------------------------------------------------------+
|  STATE CONTRIBUTION: $5.1B    |    LOCAL CONTRIBUTION: $4.54B         |
|  [SfNumericTextBox]           |    [SfNumericTextBox]                  |
+------------------------------------------------------------------------+
| [Generate CDE-40] [Refresh Data] [Analytics] [Driver Pay] [Pay Rates] |
+------------------------------------------------------------------------+
```

**Components:**
- **3 RadialGauge controls**: Cost per student (~$2.70, max 5), Total miles (45.2K), Pupils (1850, max 3000)
- **2 SfNumericTextBox**: State contribution ($5.1B), Local contribution ($4.54B)  
- **Action buttons**: Generate CDE-40 Report, Refresh Data, Generate Analytics, Driver Pay Report, Pay Rates

#### Charts Panel (Middle, ~50% of remaining height)
```
+-------------------------------------+-------------------------------------+
|         MILEAGE TRENDS              |         PUPIL COUNT TRENDS          |
|                                     |                                     |
|    [Line Chart showing daily       |    [Column Chart showing daily     |
|     mileage over time]              |     pupil counts over time]        |
|                                     |                                     |
+-------------------------------------+-------------------------------------+
```

**Components:**
- **Left ChartControl**: Mileage trends (line chart, x-axis: dates, y-axis: total miles)
- **Right ChartControl**: Pupil count trends (column chart, x-axis: dates, y-axis: total pupils)

#### Data Grid Section (Bottom, 250px height)
```
+------------------------------------------------------------------------+
| ROUTES DATA                                                           |
+------------------------------------------------------------------------+
| RouteID | RouteName | Date     | AM Miles | PM Miles | AM Riders | PM |
|---------|-----------|----------|----------|----------|-----------|-----|
| R001    | Route 1   | 06/22/25 | 45.2     | 47.1     | 35        | 42  |
| R002    | Route 2   | 06/22/25 | 52.8     | 54.3     | 28        | 31  |
+------------------------------------------------------------------------+
| ACTIVITY SCHEDULE DATA                                                |
+------------------------------------------------------------------------+
| Date     | Vehicle | Destination      | Scheduled Riders | Distance   |
|----------|---------|------------------|------------------|------------|
| 06/22/25 | Bus 12  | Football Game    | 45               | 12.5 miles |
| 06/22/25 | Bus 03  | Field Trip       | 38               | 8.2 miles  |
+------------------------------------------------------------------------+
```

**Components:**
- **Routes SfDataGrid**: RouteID, RouteName, Date, AM/PM Miles, AM/PM Riders, RouteType
- **Activity Schedule SfDataGrid**: Date, Vehicle, Destination, Scheduled Riders, Distance
- **Features**: Filtering, sorting, inline editing for data management

### Report Viewer (Optional overlay/modal)
```
+------------------------------------------------------------------------+
|                          CDE-40 REPORT PREVIEW                        |
|                         [BoldReportViewer]                            |
|   [Generated PDF report with CDE-40 compliance data and summaries]    |
|                                                                        |
|                    [Print] [Export] [Close]                           |
+------------------------------------------------------------------------+
```

**Component**: `BoldReportViewer` for PDF report generation and preview

## Color Scheme (Office2016Black Theme)

- **Primary Background**: Color.FromArgb(68, 68, 68)
- **Secondary Background**: Color.FromArgb(45, 45, 48)  
- **Accent Color**: Color.FromArgb(63, 81, 181) - Blue accent
- **Text Color**: Color.White
- **Success Color**: Color.FromArgb(76, 175, 80) - Green
- **Warning Color**: Color.FromArgb(255, 193, 7) - Amber

## Responsive Behavior

- **Minimum Size**: 800x600px
- **Navigation Drawer**: Collapsible on smaller screens
- **Charts Panel**: Stack vertically on narrow layouts
- **Data Grids**: Horizontal scrolling for overflow columns
- **Metrics Panel**: Responsive gauge sizing

## CDE-40 Alignment Validation

✅ **Mileage Data**: Covered by Routes SfDataGrid (AM/PM Miles) and Mileage Trends ChartControl  
✅ **Pupil Counts**: Covered by Routes SfDataGrid (AM/PM Riders) and Pupil Trends ChartControl  
✅ **Costs**: Covered by Cost per Student RadialGauge and financial contribution displays  
✅ **Financial Contributions**: Covered by State/Local SfNumericTextBox displays  
✅ **Vehicle Details**: Accessible via Vehicles navigation menu  
✅ **Calendar Data**: Accessible via Activities navigation menu  
✅ **Report Generation**: CDE-40 Report button triggers BoldReportViewer  

## Implementation Notes

- **Theme Toggle**: Live switching between Office2016Black/White/Colorful
- **Data Binding**: Real-time updates from BusBuddy database
- **Error Handling**: Graceful fallbacks for missing data
- **Performance**: Lazy loading for large datasets
- **Accessibility**: ARIA labels and keyboard navigation support

## Next Steps (Task 10)

This design will be implemented in `BusBuddyDashboard.cs` with:
1. DashboardViewModel for data binding
2. Service integration for live data updates  
3. Report generation integration with xAI Grok 3 API
4. Theme management and persistence
5. Navigation service integration

---
*This design document serves as the blueprint for Task 11: Implement New Dashboard*

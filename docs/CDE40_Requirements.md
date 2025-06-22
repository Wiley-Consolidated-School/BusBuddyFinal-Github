# CDE-40 Report Requirements

**Status**: Draft | **Last Updated**: June 22, 2025

## Overview
The CDE-40 report is a mandatory annual submission to the Colorado Department of Education (CDE) by September 15, per the Colorado Code of Regulations, to support the public school transportation fund. The BusBuddy dashboard will facilitate data collection, processing, and reporting for the CDE-40, showcasing school (~$4.3B property taxes, $241.7M vehicle taxes) and state (~$5.1B, Public School Finance Act) financial contributions, and proving transportation's value through metrics like total miles driven, kids transported, and cost per student (~$2.70/day).

## Data Points
The CDE-40 report requires data from the following tables (per `BusBuddy Tables.pdf`):

### Mileage
- **Source**: Routes (AM/PM Begin/End Miles), Activity Schedule (trip distances).
- **Fields**:
  - Routes: `Date`, `AMBeginMiles`, `AMEndMiles`, `PMBeginMiles`, `PMEndMiles`.
  - Activity Schedule: `Date`, `ScheduledVehicle`, `Destination` (for distance estimation).
- **Calculation**: Total miles = (AMEndMiles - AMBeginMiles) + (PMEndMiles - PMBeginMiles) + Activity trip distances.
- **Output**: Aggregate miles for routes, activities, and administrative purposes.

### Pupil Counts
- **Source**: Routes (AM/PM Riders), Activity Schedule (Scheduled Riders).
- **Fields**:
  - Routes: `AMRiders`, `PMRiders`.
  - Activity Schedule: `ScheduledRiders`.
- **Calculation**: Total students transported per day, month, and year.
- **Output**: Summarized pupil counts for CDE-40.

### Costs
- **Source**: Fuel (Fuel Date, Location), Maintenance (Repair Cost).
- **Fields**:
  - Fuel: `FuelDate`, `Vehicle`, `FuelLocation`.
  - Maintenance: `Date`, `Vehicle`, `RepairCost`.
- **Calculation**: Sum fuel and maintenance costs, excluding non-allowable expenses (e.g., capital expenditures).
- **Output**: Total operating costs for transportation.

### Vehicle Details
- **Source**: Vehicles (Seating Capacity, Fuel Type, Date Last Inspection).
- **Fields**: `Model`, `SeatingCapacity`, `FuelType`, `DateLastInspection`, `VINNumber`.
- **Output**: Fleet summary for CDE-40 compliance.

### Calendar
- **Source**: School Calendar (school days, holidays, events).
- **Fields**: Date ranges for school year, holidays (e.g., Thanksgiving Break), events.
- **Output**: Contextualize transportation data by school operational days.

## Financial Contributions
- **State Contribution**: ~$5.1B from Public School Finance Act (2024-25).
- **Local Contribution**: ~$4.3B property taxes, $241.7M vehicle taxes.
- **Display**: Use `RadialGauge` or `SfNumericTextBox` to show percentages or dollar amounts ([SfNumericTextBox Docs](https://help.syncfusion.com/windowsforms/numerictextbox/getting-started)).
- **Source**: [Transportation Funding](https://www.cde.state.co.us/cdefinance/transportation).

## Transportation Value Metrics
- **Metrics**:
  - Total miles driven (school year).
  - Total kids transported (AM/PM Riders, Scheduled Riders).
  - Cost per student (~$2.70/day).
  - Activity trips (e.g., number of sports trips).
- **Display**: Statistics panel with `RadialGauge` for metrics and `ChartControl` for trends ([ChartControl Docs](https://help.syncfusion.com/windowsforms/chart/getting-started)).
- **xAI Grok 3 API**: Generate insights (e.g., "Cost per student increased due to fuel prices") ([xAI API Docs](https://docs.x.ai)).

## Dashboard Visualization Components
The CDE-40 dashboard will utilize the following Syncfusion components:

### Primary Display Controls
- **SfDataGrid**: For tabular data display of routes, vehicles, and maintenance records
  - Implementation: [SfDataGrid Getting Started](https://help.syncfusion.com/windowsforms/datagrid/getting-started)
  - Features: Grouping, filtering, and sorting capabilities for detailed analysis
  - Data Source: Entity Framework bound collections from Routes, Vehicles, and Maintenance tables

- **ChartControl**: For visualizing trends in mileage, pupil counts, and costs
  - Implementation: [ChartControl Getting Started](https://help.syncfusion.com/windowsforms/chart/getting-started)
  - Chart Types: Line charts for trend analysis, Bar charts for comparative metrics
  - Features: Interactive legends, tooltips, and drill-down capabilities

- **RadialGauge**: For key performance indicators (KPIs) and goal tracking
  - Implementation: [RadialGauge Getting Started](https://help.syncfusion.com/windowsforms/radial-gauge/getting-started)
  - Metrics: Cost per student, daily ridership percentages, route efficiency

### Supporting Components
- **SfNavigationDrawer**: For navigating between different report sections
  - Implementation: [SfNavigationDrawer Getting Started](https://help.syncfusion.com/windowsforms/navigation-drawer/getting-started)
  - Sections: Mileage Summary, Pupil Counts, Financial Analysis, Vehicle Stats

- **BoldReportViewer**: For final CDE-40 report generation and export
  - Features: PDF export capability for submission to CDE
  - Templated design matching official CDE-40 form layout

- **SfTabControl**: For organizing different analysis views
  - Implementation: [TabControlAdv Getting Started](https://help.syncfusion.com/windowsforms/tabcontrol/getting-started)
  - Tabs: Daily Operations, Special Events, Cost Analysis, Summary

### Theming
- **Office2016Black**: Primary theme for dashboard for professional appearance
  - Implementation: `ApplyTheme(VisualStyle.Office2016Black)` on all forms
  - Consistent visual styling across all dashboard components

## xAI Grok 3 API Integration Priority
Based on CDE-40 reporting requirements, prioritize these data points for API processing:

### Primary Focus (Daily Operations)
- **Routes Table**: AM/PM Miles and Riders for consistent daily transportation metrics
- **Calculation**: `(AMEndMiles - AMBeginMiles) + (PMEndMiles - PMBeginMiles)` and `AMRiders + PMRiders`
- **API Prompt**: "Analyze daily route efficiency: total miles, riders transported, cost per mile"

### Secondary Focus (Special Events)
- **Activity Schedule**: Scheduled Riders and trip distances for activity transportation value
- **Calculation**: Sum of all activity trips and scheduled riders
- **API Prompt**: "Summarize activity transportation impact: trips completed, students served, additional value"

### Tertiary Focus (Cost Analysis)
- **Fuel + Maintenance**: Combined operating costs for comprehensive financial picture
- **API Prompt**: "Generate cost insights: fuel trends, maintenance patterns, cost optimization opportunities"

## CDE-40 Submission Timeline
The CDE-40 report follows a strict annual timeline:

| Milestone | Date | Description |
|-----------|------|-------------|
| Data Collection Period | July 1, 2024 - June 30, 2025 | Full fiscal year data collection |
| Internal Review | July 1 - August 15, 2025 | District transportation department review |
| Final Approval | August 16 - September 1, 2025 | District superintendent approval |
| Submission Deadline | September 15, 2025 | Mandatory submission to CDE |

## Data Validation Requirements
To ensure CDE-40 compliance, the dashboard must validate:

1. **Data Completeness**: All required fields populated with valid values
   - Validation: Null checks on critical fields (mileage, riders, costs)
   - UI Indicator: Warning icons for missing data with drill-down capability

2. **Data Consistency**: Logical validation of related values
   - Validation: End miles > Begin miles, PMRiders ≈ AMRiders
   - Alert: Highlight outliers for transportation coordinator review

3. **Calculation Accuracy**: Verify formula-based metrics
   - Validation: Cross-check calculations against raw data
   - Audit: Maintain calculation logs for transparency

4. **Historical Comparison**: Compare with previous years' submissions
   - Validation: Flag significant deviations from historical patterns
   - Analysis: Provide context for variances (e.g., route changes, fuel price impacts)

## Validation
- Cross-check with `BusBuddy Tables.pdf` for field accuracy.
- Verify alignment with CDE-40 guidelines ([CDE-40 Info](https://www.cde.state.co.us/idm/transportation)).
- Ensure metrics are calculable from available data.

## References
- [CDE Transportation](https://www.cde.state.co.us/cdefinance/transportation)
- [CDE-40 Info](https://www.cde.state.co.us/idm/transportation)
- Contact: Yolanda Lucero (lucero_y@cde.state.co.us)
- [Syncfusion Documentation](https://help.syncfusion.com/windowsforms)
- [xAI API Docs](https://docs.x.ai)
- [Syncfusion Windows Forms Controls](https://help.syncfusion.com/windowsforms/overview)

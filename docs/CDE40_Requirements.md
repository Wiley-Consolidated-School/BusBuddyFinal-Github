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

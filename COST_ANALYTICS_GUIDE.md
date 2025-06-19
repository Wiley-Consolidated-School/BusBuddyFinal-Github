# BusBuddy Cost Analytics Documentation

## Overview
The BusBuddy cost analytics feature calculates cost per student metrics to help school administrators understand transportation costs. This feature displays two key metrics:

1. **Cost per student per day** for regular bus routes
2. **Cost per student** for activity trips (Sports and Field Trips)

## How It Works

### Data Sources
The analytics use data from these existing database tables:
- **Routes** - Regular daily bus routes with mileage and rider counts
- **Activities** - Sports trips and field trips with activity types

### Cost Calculations

#### Regular Routes
For each route, the system calculates:
- **Fuel Cost**: Total miles × $3.50/gallon ÷ 6 MPG (bus fuel efficiency)
- **Maintenance Cost**: Total miles × $0.20/mile (estimated maintenance)
- **Driver Cost**: 2 hours × $16.50/hour (AM + PM route time)
- **Total Route Cost**: Fuel + Maintenance + Driver costs
- **Cost per Student per Day**: Total Route Cost ÷ Total Student-Days

#### Activity Trips
For sports and field trips:
- **Fuel Cost**: Estimated miles × fuel calculation
- **Maintenance Cost**: Estimated miles × $0.20/mile
- **Driver Cost**: $50 flat rate (teacher/coach stipend)
- **Sports Trips**: Assumes 50 miles average, 20 students per trip
- **Field Trips**: Assumes 75 miles average, 25 students per trip
- **Cost per Student**: Total Activity Cost ÷ Total Students

### Dashboard Display
The cost metrics appear in a "💰 Cost Per Student" panel on the Syncfusion dashboard showing:
```
📊 Last 30 Days

Route Cost/Student/Day:
$X.XX

Sports Cost/Student:
$X.XX

Field Trip Cost/Student:
$X.XX
```

## Expected Results (With Data)
Typical cost ranges when data is present:
- **Routes**: $2.50 - $5.00 per student per day
- **Sports**: $8.00 - $20.00 per student per trip
- **Field Trips**: $10.00 - $25.00 per student per trip

## Current Status (No Data)
With no route or activity data in the database, the panel shows:
- All costs display as $0.00
- Message: "⚠️ No data - Ready for input"

## Adding Data
To see actual cost calculations, add:

### Route Data (Routes table)
```sql
INSERT INTO Routes (Date, RouteName, AMRiders, PMRiders, AMBeginMiles, AMEndMiles, PMBeginMiles, PMEndMiles)
VALUES ('2025-06-19', 'Main Route', 25, 23, 1000, 1030, 1030, 1060);
```

### Activity Data (Activities table)
```sql
INSERT INTO Activities (Date, ActivityType, Destination)
VALUES ('2025-06-19', 'Sports Trip', 'Away Game'),
       ('2025-06-20', 'Field Trip', 'Science Museum');
```

## Technical Implementation
- **Service**: `RouteAnalyticsService.CalculateCostPerStudentMetricsAsync()`
- **Model**: `CostPerStudentMetrics`
- **UI**: `BusBuddyDashboardSyncfusion.AddCostAnalyticsToStatsPanel()`
- **Validation**: `CostAnalyticsValidator.ValidateAnalytics()`

## Troubleshooting
If costs show as $0.00:
1. Check if Routes table has data with AMRiders/PMRiders > 0
2. Check if Activities table has data with appropriate ActivityType
3. Verify date ranges (looks at last 30 days)
4. Check console output for validation messages

The system is designed to work reliably when data is added while gracefully handling the no-data state during development.

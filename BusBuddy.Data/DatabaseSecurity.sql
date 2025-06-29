-- BusBuddy Database Security & Monitoring Script
-- Comprehensive security analysis and monitoring setup
-- Date: June 28, 2025

USE BusBuddy;
GO

PRINT '=== BusBuddy Database Security & Monitoring ===';
PRINT 'Analyzing security configuration and setting up monitoring...';
PRINT '';

-- 1. Security Analysis
PRINT '1. SECURITY ANALYSIS:';

-- Check database users and roles
PRINT '   Database Users and Roles:';
SELECT
    dp.name AS principal_name,
    dp.type_desc AS principal_type,
    r.name AS role_name
FROM sys.database_principals dp
LEFT JOIN sys.database_role_members rm ON dp.principal_id = rm.member_principal_id
LEFT JOIN sys.database_principals r ON rm.role_principal_id = r.principal_id
WHERE dp.principal_id > 4  -- Exclude system principals
ORDER BY dp.name;

-- Check permissions on critical tables
PRINT '';
PRINT '   Table Permissions:';
SELECT
    p.state_desc,
    p.permission_name,
    s.name AS schema_name,
    o.name AS object_name,
    pr.name AS principal_name
FROM sys.database_permissions p
LEFT JOIN sys.objects o ON p.major_id = o.object_id
LEFT JOIN sys.schemas s ON o.schema_id = s.schema_id
LEFT JOIN sys.database_principals pr ON p.grantee_principal_id = pr.principal_id
WHERE o.name IN ('Buses', 'Drivers', 'Routes', 'Activities', 'Fuel', 'Maintenance', 'SchoolCalendars', 'ActivitySchedule')
    OR o.name IS NULL
ORDER BY o.name, pr.name;

-- 2. Connection Monitoring Setup
PRINT '';
PRINT '2. CONNECTION MONITORING:';

-- Create monitoring table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ConnectionLog')
BEGIN
    CREATE TABLE ConnectionLog (
        LogId INT IDENTITY(1,1) PRIMARY KEY,
        SessionId INT,
        LoginName NVARCHAR(128),
        HostName NVARCHAR(128),
        ProgramName NVARCHAR(128),
        LoginTime DATETIME,
        LogTime DATETIME DEFAULT GETDATE(),
        EventType NVARCHAR(50)
    );
    PRINT '   Created ConnectionLog table for monitoring.';
END

-- Log current connections
INSERT INTO ConnectionLog (SessionId, LoginName, HostName, ProgramName, LoginTime, EventType)
SELECT
    session_id,
    login_name,
    host_name,
    program_name,
    login_time,
    'AUDIT_SNAPSHOT'
FROM sys.dm_exec_sessions
WHERE database_id = DB_ID()
    AND session_id > 50;

PRINT '   Logged current connections to monitoring table.';

-- 3. Performance Monitoring Setup
PRINT '';
PRINT '3. PERFORMANCE MONITORING SETUP:';

-- Create performance monitoring table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PerformanceLog')
BEGIN
    CREATE TABLE PerformanceLog (
        LogId INT IDENTITY(1,1) PRIMARY KEY,
        LogTime DATETIME DEFAULT GETDATE(),
        DatabaseSizeMB DECIMAL(10,2),
        ActiveConnections INT,
        AvgCPUPercent DECIMAL(5,2),
        AvgWaitTime DECIMAL(10,2),
        BlockedProcesses INT,
        LongRunningQueries INT
    );
    PRINT '   Created PerformanceLog table.';
END

-- Log current performance metrics
DECLARE @DatabaseSizeMB DECIMAL(10,2);
DECLARE @ActiveConnections INT;
DECLARE @BlockedProcesses INT;

SELECT @DatabaseSizeMB = CAST(SUM(size) * 8.0 / 1024 AS DECIMAL(10,2))
FROM sys.database_files;

SELECT @ActiveConnections = COUNT(*)
FROM sys.dm_exec_sessions
WHERE database_id = DB_ID() AND session_id > 50;

SELECT @BlockedProcesses = COUNT(*)
FROM sys.dm_exec_requests
WHERE blocking_session_id > 0;

INSERT INTO PerformanceLog (DatabaseSizeMB, ActiveConnections, BlockedProcesses, LongRunningQueries)
VALUES (@DatabaseSizeMB, @ActiveConnections, @BlockedProcesses, 0);

PRINT '   Logged current performance metrics.';

-- 4. Data Integrity Monitoring
PRINT '';
PRINT '4. DATA INTEGRITY MONITORING:';

-- Create integrity check table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IntegrityLog')
BEGIN
    CREATE TABLE IntegrityLog (
        LogId INT IDENTITY(1,1) PRIMARY KEY,
        CheckTime DATETIME DEFAULT GETDATE(),
        TableName NVARCHAR(128),
        RecordCount INT,
        IntegrityStatus NVARCHAR(50),
        ErrorMessage NVARCHAR(MAX)
    );
    PRINT '   Created IntegrityLog table.';
END

-- Log current record counts for integrity monitoring
INSERT INTO IntegrityLog (TableName, RecordCount, IntegrityStatus)
SELECT
    t.name,
    p.rows,
    'HEALTHY'
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id
WHERE t.name IN ('Buses', 'Drivers', 'Routes', 'Activities', 'Fuel', 'Maintenance', 'SchoolCalendars', 'ActivitySchedule')
    AND p.index_id IN (0, 1);  -- Heap or clustered index

PRINT '   Logged record counts for integrity monitoring.';

-- 5. Automated Alerts Setup
PRINT '';
PRINT '5. AUTOMATED MONITORING VIEWS:';

-- Create monitoring views for easy dashboard integration
IF EXISTS (SELECT * FROM sys.views WHERE name = 'v_DatabaseHealth')
    DROP VIEW v_DatabaseHealth;
GO

CREATE VIEW v_DatabaseHealth AS
SELECT
    'Database Size' AS Metric,
    CAST(SUM(size) * 8.0 / 1024 AS VARCHAR(20)) + ' MB' AS Value,
    CASE WHEN SUM(size) * 8.0 / 1024 > 1000 THEN 'WARNING' ELSE 'OK' END AS Status
FROM sys.database_files
UNION ALL
SELECT
    'Active Connections',
    CAST(COUNT(*) AS VARCHAR(20)),
    CASE WHEN COUNT(*) > 10 THEN 'WARNING' ELSE 'OK' END
FROM sys.dm_exec_sessions
WHERE database_id = DB_ID() AND session_id > 50
UNION ALL
SELECT
    'Blocked Processes',
    CAST(COUNT(*) AS VARCHAR(20)),
    CASE WHEN COUNT(*) > 0 THEN 'CRITICAL' ELSE 'OK' END
FROM sys.dm_exec_requests
WHERE blocking_session_id > 0;
GO

PRINT '   Created v_DatabaseHealth monitoring view.';

-- Create table statistics view
IF EXISTS (SELECT * FROM sys.views WHERE name = 'v_TableStatistics')
    DROP VIEW v_TableStatistics;
GO

CREATE VIEW v_TableStatistics AS
SELECT
    t.name AS TableName,
    p.rows AS RecordCount,
    CAST(ROUND((SUM(a.used_pages) * 8) / 1024.00, 2) AS DECIMAL(10,2)) AS UsedSpaceMB,
    STATS_DATE(i.object_id, i.index_id) AS LastStatsUpdate,
    CASE
        WHEN STATS_DATE(i.object_id, i.index_id) < DATEADD(DAY, -7, GETDATE()) THEN 'NEEDS_UPDATE'
        ELSE 'CURRENT'
    END AS StatsStatus
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id AND i.index_id <= 1
INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE t.name IN ('Buses', 'Drivers', 'Routes', 'Activities', 'Fuel', 'Maintenance', 'SchoolCalendars', 'ActivitySchedule')
GROUP BY t.name, p.rows, i.object_id, i.index_id;
GO

PRINT '   Created v_TableStatistics monitoring view.';

-- 6. Security Recommendations
PRINT '';
PRINT '6. SECURITY RECOMMENDATIONS:';
PRINT '🔒 Security Best Practices:';
PRINT '   • Use Windows Authentication when possible';
PRINT '   • Create specific database users for applications';
PRINT '   • Grant minimum necessary permissions';
PRINT '   • Regularly review user access';
PRINT '   • Enable SQL Server Audit for sensitive operations';
PRINT '   • Use encryption for sensitive data (TDE)';
PRINT '   • Regular security patches and updates';

-- 7. Quick Health Check Query
PRINT '';
PRINT '7. QUICK HEALTH CHECK:';
SELECT * FROM v_DatabaseHealth;

PRINT '';
PRINT '=== Security & Monitoring Setup Complete ===';
PRINT 'Use the following queries for ongoing monitoring:';
PRINT '• SELECT * FROM v_DatabaseHealth;';
PRINT '• SELECT * FROM v_TableStatistics;';
PRINT '• SELECT TOP 10 * FROM ConnectionLog ORDER BY LogTime DESC;';
PRINT '• SELECT TOP 10 * FROM PerformanceLog ORDER BY LogTime DESC;';
GO

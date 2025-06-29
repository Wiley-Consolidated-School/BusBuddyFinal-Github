-- BusBuddy SQL Server Express Optimization Script
-- This script optimizes SQL Server Express settings for better performance and reliability
-- Run with: sqlcmd -S "localhost\SQLEXPRESS" -E -i "DatabaseOptimization.sql"

USE master;
GO

PRINT 'BusBuddy SQL Server Express Optimization';
PRINT '=======================================';
PRINT 'Current Time: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '';

-- =====================================
-- 1. Memory Configuration
-- =====================================
PRINT '1. Configuring Memory Settings...';

-- Show current memory configuration
SELECT
    name,
    value_in_use AS CurrentValue,
    description
FROM sys.configurations
WHERE name IN ('max server memory (MB)', 'min server memory (MB)');

-- Enable advanced options
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;

-- Set max server memory to 1GB (adjust based on available RAM)
-- Leave enough for OS (recommend 25% of total RAM for SQL Server Express)
EXEC sp_configure 'max server memory (MB)', 1024;
RECONFIGURE;

PRINT '   ✓ Memory configured to 1024 MB maximum';

-- =====================================
-- 2. Database-Specific Optimizations
-- =====================================
PRINT '2. Optimizing BusBuddy Database Settings...';

USE BusBuddy;
GO

-- Set recovery model to SIMPLE for development (better performance, less logging)
ALTER DATABASE BusBuddy SET RECOVERY SIMPLE;
PRINT '   ✓ Recovery model set to SIMPLE';

-- Enable auto-update statistics for better query performance
ALTER DATABASE BusBuddy SET AUTO_UPDATE_STATISTICS ON;
ALTER DATABASE BusBuddy SET AUTO_UPDATE_STATISTICS_ASYNC ON;
PRINT '   ✓ Auto-update statistics enabled';

-- Enable auto-create statistics
ALTER DATABASE BusBuddy SET AUTO_CREATE_STATISTICS ON;
PRINT '   ✓ Auto-create statistics enabled';

-- =====================================
-- 3. File Growth Configuration
-- =====================================
PRINT '3. Configuring File Growth Settings...';

-- Check current file settings
SELECT
    name AS FileName,
    type_desc AS FileType,
    size * 8 / 1024 AS CurrentSizeMB,
    CASE
        WHEN is_percent_growth = 1 THEN CAST(growth AS VARCHAR) + '%'
        ELSE CAST(growth * 8 / 1024 AS VARCHAR) + ' MB'
    END AS GrowthSetting,
    physical_name
FROM sys.master_files
WHERE database_id = DB_ID('BusBuddy');

-- Set data file growth to 128MB increments (more efficient than percentage)
ALTER DATABASE BusBuddy
MODIFY FILE (NAME = 'BusBuddy', FILEGROWTH = 128MB, MAXSIZE = 2048MB);

-- Set log file growth to 64MB increments
ALTER DATABASE BusBuddy
MODIFY FILE (NAME = 'BusBuddy_log', FILEGROWTH = 64MB, MAXSIZE = 512MB);

PRINT '   ✓ File growth configured (Data: 128MB, Log: 64MB)';

-- =====================================
-- 4. Query Store Configuration
-- =====================================
PRINT '4. Enabling Query Store...';

-- Enable Query Store for query performance monitoring
ALTER DATABASE BusBuddy SET QUERY_STORE = ON (
    OPERATION_MODE = READ_WRITE,
    CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30),
    DATA_FLUSH_INTERVAL_SECONDS = 900,
    INTERVAL_LENGTH_MINUTES = 60,
    MAX_STORAGE_SIZE_MB = 100
);

PRINT '   ✓ Query Store enabled with 30-day retention';

-- =====================================
-- 5. Index Optimization
-- =====================================
PRINT '5. Optimizing Indexes...';

-- Update statistics on all tables
EXEC sp_updatestats;
PRINT '   ✓ Statistics updated for all tables';

-- Check for missing indexes (will show recommendations in Query Store)
SELECT
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    CASE WHEN i.is_primary_key = 1 THEN 'PRIMARY KEY' ELSE 'INDEX' END AS KeyType
FROM sys.tables t
LEFT JOIN sys.indexes i ON t.object_id = i.object_id
WHERE i.index_id > 0  -- Exclude heaps
ORDER BY t.name, i.name;

-- =====================================
-- 6. Connection and Security Settings
-- =====================================
PRINT '6. Verifying Security Configuration...';

-- Check SQL Server version and edition
SELECT
    @@VERSION AS SQLServerVersion,
    SERVERPROPERTY('Edition') AS Edition,
    SERVERPROPERTY('ProductLevel') AS ProductLevel;

-- Show current connection settings
SELECT
    name,
    value_in_use
FROM sys.configurations
WHERE name IN (
    'user connections',
    'remote access',
    'network packet size (B)',
    'cost threshold for parallelism'
);

-- =====================================
-- 7. Performance Counters and Monitoring
-- =====================================
PRINT '7. Setting up Performance Monitoring...';

-- Create a view for easy performance monitoring
IF OBJECT_ID('dbo.vw_DatabasePerformance', 'V') IS NOT NULL
    DROP VIEW dbo.vw_DatabasePerformance;
GO

CREATE VIEW dbo.vw_DatabasePerformance AS
SELECT
    'Database Size' AS Metric,
    CAST(SUM(size * 8.0 / 1024) AS DECIMAL(10,2)) AS ValueMB,
    'Total database size in MB' AS Description
FROM sys.master_files
WHERE database_id = DB_ID()
UNION ALL
SELECT
    'Connection Count' AS Metric,
    COUNT(*) AS ValueMB,
    'Current active connections' AS Description
FROM sys.dm_exec_connections
WHERE session_id > 50  -- Exclude system processes
UNION ALL
SELECT
    'Buffer Cache Hit Ratio' AS Metric,
    CAST((cntr_value * 100.0 / base_cntr_value) AS DECIMAL(5,2)) AS ValueMB,
    'Percentage of pages found in buffer cache' AS Description
FROM sys.dm_os_performance_counters
WHERE counter_name = 'Buffer cache hit ratio'
    AND object_name LIKE '%Buffer Manager%';
GO

PRINT '   ✓ Performance monitoring view created';

-- =====================================
-- 8. Backup Configuration
-- =====================================
PRINT '8. Configuring Backup Settings...';

-- Ensure backup directory exists (create if needed)
DECLARE @BackupPath NVARCHAR(256) = 'C:\Backups\BusBuddy\';
DECLARE @SQL NVARCHAR(MAX);

-- Create backup directory via xp_cmdshell (if enabled) or manually
PRINT '   ℹ  Backup directory should be: ' + @BackupPath;
PRINT '   ℹ  Create manually if it does not exist';

-- Set backup compression (if available in this SQL Server version)
IF EXISTS (SELECT * FROM sys.configurations WHERE name = 'backup compression default')
BEGIN
    EXEC sp_configure 'backup compression default', 1;
    RECONFIGURE;
    PRINT '   ✓ Backup compression enabled';
END
ELSE
BEGIN
    PRINT '   ℹ  Backup compression not available in this edition';
END

-- =====================================
-- Final Status Report
-- =====================================
PRINT '';
PRINT 'Optimization Complete!';
PRINT '====================';

-- Show final configuration summary
SELECT
    'Memory (MB)' AS Setting,
    value_in_use AS CurrentValue
FROM sys.configurations
WHERE name = 'max server memory (MB)'
UNION ALL
SELECT
    'Recovery Model',
    recovery_model_desc
FROM sys.databases
WHERE name = 'BusBuddy'
UNION ALL
SELECT
    'Query Store',
    CASE WHEN is_query_store_on = 1 THEN 'ENABLED' ELSE 'DISABLED' END
FROM sys.databases
WHERE name = 'BusBuddy';

-- Show database file configuration
PRINT '';
PRINT 'Database File Configuration:';
SELECT
    name AS FileName,
    type_desc AS FileType,
    CAST(size * 8.0 / 1024 AS DECIMAL(10,2)) AS CurrentSizeMB,
    CASE
        WHEN is_percent_growth = 1 THEN CAST(growth AS VARCHAR) + '%'
        ELSE CAST(growth * 8 / 1024 AS VARCHAR) + ' MB'
    END AS GrowthSetting,
    CASE
        WHEN max_size = -1 THEN 'UNLIMITED'
        ELSE CAST(max_size * 8 / 1024 AS VARCHAR) + ' MB'
    END AS MaxSizeMB
FROM sys.master_files
WHERE database_id = DB_ID('BusBuddy');

PRINT '';
PRINT 'Next Steps:';
PRINT '- Test application connectivity with new settings';
PRINT '- Monitor performance using Query Store';
PRINT '- Set up regular backup schedule';
PRINT '- Consider creating non-admin database user for application';

USE master;
GO

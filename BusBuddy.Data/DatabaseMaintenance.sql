-- BusBuddy Database Maintenance Script
-- Run this weekly to maintain optimal database performance
-- Date: June 28, 2025

USE BusBuddy;
GO

PRINT '=== Starting BusBuddy Database Maintenance ===';
PRINT 'This script will optimize indexes, update statistics, and clean up logs.';
PRINT '';

-- 1. Update Statistics for all tables
PRINT '1. UPDATING STATISTICS...';
DECLARE @TableName NVARCHAR(128);
DECLARE table_cursor CURSOR FOR
SELECT name FROM sys.tables
WHERE name IN ('Buses', 'Drivers', 'Routes', 'Activities', 'Fuel', 'Maintenance', 'SchoolCalendars', 'ActivitySchedule');

OPEN table_cursor;
FETCH NEXT FROM table_cursor INTO @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT '  Updating statistics for: ' + @TableName;
    EXEC('UPDATE STATISTICS [' + @TableName + '] WITH FULLSCAN');
    FETCH NEXT FROM table_cursor INTO @TableName;
END

CLOSE table_cursor;
DEALLOCATE table_cursor;

-- 2. Rebuild fragmented indexes
PRINT '';
PRINT '2. CHECKING AND REBUILDING FRAGMENTED INDEXES...';

DECLARE @sql NVARCHAR(MAX);
DECLARE index_cursor CURSOR FOR
SELECT
    'ALTER INDEX ' + i.name + ' ON ' + t.name + ' REBUILD' AS RebuildCommand
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
CROSS APPLY sys.dm_db_index_physical_stats(DB_ID(), t.object_id, i.index_id, NULL, 'LIMITED') ips
WHERE t.name IN ('Buses', 'Drivers', 'Routes', 'Activities', 'Fuel', 'Maintenance', 'SchoolCalendars', 'ActivitySchedule')
    AND i.index_id > 0
    AND ips.avg_fragmentation_in_percent > 10;

OPEN index_cursor;
FETCH NEXT FROM index_cursor INTO @sql;

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT '  Executing: ' + @sql;
    EXEC sp_executesql @sql;
    FETCH NEXT FROM index_cursor INTO @sql;
END

CLOSE index_cursor;
DEALLOCATE index_cursor;

-- 3. Shrink log file if it's grown too large
PRINT '';
PRINT '3. CHECKING LOG FILE SIZE...';
DECLARE @LogSizeMB DECIMAL(10,2);
SELECT @LogSizeMB = CAST(size * 8.0 / 1024 AS DECIMAL(10,2))
FROM sys.database_files
WHERE type_desc = 'LOG';

PRINT '  Current log file size: ' + CAST(@LogSizeMB AS VARCHAR(20)) + ' MB';

IF @LogSizeMB > 100  -- If log is larger than 100MB
BEGIN
    PRINT '  Log file is large, performing maintenance...';
    CHECKPOINT;
    DBCC SHRINKFILE(2, 10);  -- Shrink log to 10MB
    PRINT '  Log file maintenance complete.';
END
ELSE
BEGIN
    PRINT '  Log file size is acceptable.';
END

-- 4. Check database integrity
PRINT '';
PRINT '4. CHECKING DATABASE INTEGRITY...';
DBCC CHECKDB WITH NO_INFOMSGS;
PRINT '  Database integrity check complete.';

-- 5. Performance recommendations
PRINT '';
PRINT '5. PERFORMANCE RECOMMENDATIONS:';

-- Check for tables without primary keys
IF EXISTS (
    SELECT 1 FROM sys.tables t
    LEFT JOIN sys.key_constraints kc ON t.object_id = kc.parent_object_id AND kc.type = 'PK'
    WHERE t.name IN ('Buses', 'Drivers', 'Routes', 'Activities', 'Fuel', 'Maintenance', 'SchoolCalendars', 'ActivitySchedule')
        AND kc.object_id IS NULL
)
BEGIN
    PRINT '  ⚠️  Warning: Some tables may be missing primary keys.';
END

-- Check for foreign keys without indexes
SELECT
    '  💡 Consider adding index on: ' + t.name + '.' + c.name AS Recommendation
FROM sys.foreign_key_columns fkc
INNER JOIN sys.tables t ON fkc.parent_object_id = t.object_id
INNER JOIN sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
LEFT JOIN sys.index_columns ic ON fkc.parent_object_id = ic.object_id AND fkc.parent_column_id = ic.column_id
WHERE t.name IN ('Buses', 'Drivers', 'Routes', 'Activities', 'Fuel', 'Maintenance', 'SchoolCalendars', 'ActivitySchedule')
    AND ic.object_id IS NULL;

PRINT '';
PRINT '=== Database Maintenance Complete ===';
PRINT 'Next maintenance recommended in 7 days.';
GO

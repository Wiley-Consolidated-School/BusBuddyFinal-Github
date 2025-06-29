-- BusBuddy Database Health Check Script
-- This script provides comprehensive health information for the BusBuddy database

-- =====================================
-- Database Information
-- =====================================
SELECT
    'Database Info' AS Category,
    DB_NAME() AS DatabaseName,
    CAST(SUM(size * 8.0 / 1024) AS DECIMAL(10,2)) AS SizeMB,
    COUNT(*) AS FileCount
FROM sys.database_files;

-- =====================================
-- Table Summary with Row Counts
-- =====================================
SELECT
    'Table Summary' AS Category,
    t.name AS TableName,
    p.rows AS RecordCount,
    CAST(SUM(a.total_pages) * 8.0 / 1024 AS DECIMAL(10,2)) AS SizeMB
FROM sys.tables t
INNER JOIN sys.indexes i ON t.OBJECT_ID = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE i.index_id <= 1  -- Only count clustered index or heap
GROUP BY t.name, p.rows
ORDER BY p.rows DESC;

-- =====================================
-- Index Information and Fragmentation
-- =====================================
SELECT
    'Index Health' AS Category,
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    CAST(f.avg_fragmentation_in_percent AS DECIMAL(5,2)) AS FragmentationPercent,
    f.page_count AS PageCount
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
CROSS APPLY sys.dm_db_index_physical_stats(DB_ID(), t.object_id, i.index_id, NULL, 'LIMITED') f
WHERE i.index_id > 0  -- Exclude heaps
    AND f.page_count > 100  -- Only show indexes with significant pages
ORDER BY f.avg_fragmentation_in_percent DESC;

-- =====================================
-- Foreign Key Relationships
-- =====================================
SELECT
    'Foreign Keys' AS Category,
    fk.name AS ForeignKeyName,
    tp.name AS ParentTable,
    cp.name AS ParentColumn,
    tr.name AS ReferencedTable,
    cr.name AS ReferencedColumn,
    CASE fk.is_disabled WHEN 1 THEN 'DISABLED' ELSE 'ENABLED' END AS Status
FROM sys.foreign_keys fk
INNER JOIN sys.tables tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables tr ON fk.referenced_object_id = tr.object_id
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns cp ON fkc.parent_object_id = cp.object_id AND fkc.parent_column_id = cp.column_id
INNER JOIN sys.columns cr ON fkc.referenced_object_id = cr.object_id AND fkc.referenced_column_id = cr.column_id
ORDER BY tp.name, fk.name;

-- =====================================
-- Data Distribution Check
-- =====================================
-- Check for tables that might need attention (empty or very large)
SELECT
    'Data Distribution' AS Category,
    TableName,
    RecordCount,
    CASE
        WHEN RecordCount = 0 THEN 'EMPTY - Consider adding sample data'
        WHEN RecordCount > 10000 THEN 'LARGE - Monitor performance'
        WHEN RecordCount < 10 THEN 'SMALL - May need more test data'
        ELSE 'NORMAL'
    END AS Recommendation
FROM (
    SELECT
        t.name AS TableName,
        p.rows AS RecordCount
    FROM sys.tables t
    INNER JOIN sys.indexes i ON t.OBJECT_ID = i.object_id
    INNER JOIN sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
    WHERE i.index_id <= 1  -- Only count clustered index or heap
) AS TableStats
ORDER BY RecordCount DESC;

-- =====================================
-- Database File Information
-- =====================================
SELECT
    'File Info' AS Category,
    name AS FileName,
    type_desc AS FileType,
    CAST(size * 8.0 / 1024 AS DECIMAL(10,2)) AS CurrentSizeMB,
    CASE max_size
        WHEN -1 THEN 'UNLIMITED'
        ELSE CAST(max_size * 8.0 / 1024 AS VARCHAR(20)) + ' MB'
    END AS MaxSize,
    CAST(growth * 8.0 / 1024 AS DECIMAL(10,2)) AS GrowthMB,
    physical_name AS FilePath
FROM sys.database_files;

-- =====================================
-- Connection and Performance Info
-- =====================================
SELECT
    'Connection Info' AS Category,
    @@SERVERNAME AS ServerName,
    @@VERSION AS SQLServerVersion,
    DB_NAME() AS CurrentDatabase,
    SYSTEM_USER AS CurrentUser,
    GETDATE() AS CheckTime;

-- =====================================
-- Health Summary
-- =====================================
DECLARE @EmptyTables INT, @FragmentedIndexes INT, @DisabledFKs INT;

SELECT @EmptyTables = COUNT(*)
FROM sys.tables t
INNER JOIN sys.indexes i ON t.OBJECT_ID = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
WHERE i.index_id <= 1 AND p.rows = 0;

SELECT @FragmentedIndexes = COUNT(*)
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
CROSS APPLY sys.dm_db_index_physical_stats(DB_ID(), t.object_id, i.index_id, NULL, 'LIMITED') f
WHERE i.index_id > 0 AND f.avg_fragmentation_in_percent > 30 AND f.page_count > 100;

SELECT @DisabledFKs = COUNT(*)
FROM sys.foreign_keys
WHERE is_disabled = 1;

SELECT
    'Health Summary' AS Category,
    @EmptyTables AS EmptyTables,
    @FragmentedIndexes AS HighlyFragmentedIndexes,
    @DisabledFKs AS DisabledForeignKeys,
    CASE
        WHEN @EmptyTables = 0 AND @FragmentedIndexes = 0 AND @DisabledFKs = 0 THEN 'EXCELLENT'
        WHEN @EmptyTables <= 2 AND @FragmentedIndexes <= 2 AND @DisabledFKs = 0 THEN 'GOOD'
        WHEN @EmptyTables <= 5 AND @FragmentedIndexes <= 5 AND @DisabledFKs <= 1 THEN 'FAIR'
        ELSE 'NEEDS ATTENTION'
    END AS OverallHealth;

-- =====================================
-- Recommendations
-- =====================================
SELECT
    'Recommendations' AS Category,
    'Database Health Check Complete' AS Message,
    'Review fragmented indexes if >30% fragmentation' AS IndexMaintenance,
    'Populate empty tables with sample data for testing' AS DataRecommendation,
    'Consider backup strategy for production use' AS BackupAdvice;

-- BusBuddy Database Backup & Recovery Script
-- Professional-grade backup strategy for data protection
-- Date: June 28, 2025

USE master;
GO

DECLARE @BackupPath NVARCHAR(500);
DECLARE @DatabaseName NVARCHAR(50) = 'BusBuddy';
DECLARE @Timestamp NVARCHAR(20);

-- Create timestamp for unique backup names
SET @Timestamp = REPLACE(REPLACE(REPLACE(CONVERT(VARCHAR(19), GETDATE(), 120), '-', ''), ':', ''), ' ', '_');

-- Set backup path (adjust as needed for your environment)
SET @BackupPath = 'C:\DatabaseBackups\BusBuddy\';

PRINT '=== BusBuddy Database Backup & Recovery ===';
PRINT 'Backup Path: ' + @BackupPath;
PRINT 'Timestamp: ' + @Timestamp;
PRINT '';

-- 1. Create backup directory if it doesn't exist (PowerShell command)
PRINT '1. ENSURING BACKUP DIRECTORY EXISTS...';
DECLARE @CreateDirCmd NVARCHAR(500);
SET @CreateDirCmd = 'PowerShell -Command "if (!(Test-Path ''' + @BackupPath + ''')) { New-Item -ItemType Directory -Path ''' + @BackupPath + ''' -Force }"';
EXEC xp_cmdshell @CreateDirCmd;

-- 2. Full Database Backup
PRINT '';
PRINT '2. PERFORMING FULL DATABASE BACKUP...';
DECLARE @FullBackupFile NVARCHAR(500);
SET @FullBackupFile = @BackupPath + @DatabaseName + '_Full_' + @Timestamp + '.bak';

BACKUP DATABASE BusBuddy
TO DISK = @FullBackupFile
WITH
    INIT,
    COMPRESSION,
    CHECKSUM,
    STATS = 10,
    NAME = 'BusBuddy Full Backup',
    DESCRIPTION = 'Complete backup of BusBuddy database';

PRINT 'Full backup completed: ' + @FullBackupFile;

-- 3. Verify Backup
PRINT '';
PRINT '3. VERIFYING BACKUP INTEGRITY...';
RESTORE VERIFYONLY FROM DISK = @FullBackupFile;
PRINT 'Backup verification successful.';

-- 4. Log Backup (if not in Simple recovery mode)
DECLARE @RecoveryModel NVARCHAR(20);
SELECT @RecoveryModel = recovery_model_desc FROM sys.databases WHERE name = @DatabaseName;

IF @RecoveryModel <> 'SIMPLE'
BEGIN
    PRINT '';
    PRINT '4. PERFORMING LOG BACKUP...';
    DECLARE @LogBackupFile NVARCHAR(500);
    SET @LogBackupFile = @BackupPath + @DatabaseName + '_Log_' + @Timestamp + '.trn';

    BACKUP LOG BusBuddy
    TO DISK = @LogBackupFile
    WITH
        INIT,
        COMPRESSION,
        CHECKSUM,
        STATS = 10,
        NAME = 'BusBuddy Log Backup';

    PRINT 'Log backup completed: ' + @LogBackupFile;
END
ELSE
BEGIN
    PRINT '4. SKIPPING LOG BACKUP (Simple recovery model)';
END

-- 5. Backup Information
PRINT '';
PRINT '5. RECENT BACKUP HISTORY:';
SELECT TOP 5
    s.database_name,
    s.backup_start_date,
    s.backup_finish_date,
    CASE s.type
        WHEN 'D' THEN 'Full'
        WHEN 'I' THEN 'Differential'
        WHEN 'L' THEN 'Log'
    END AS BackupType,
    CAST(s.backup_size / 1024.0 / 1024.0 AS DECIMAL(10,2)) AS BackupSizeMB,
    CAST(s.compressed_backup_size / 1024.0 / 1024.0 AS DECIMAL(10,2)) AS CompressedSizeMB,
    m.physical_device_name
FROM msdb.dbo.backupset s
INNER JOIN msdb.dbo.backupmediafamily m ON s.media_set_id = m.media_set_id
WHERE s.database_name = @DatabaseName
ORDER BY s.backup_start_date DESC;

-- 6. Cleanup old backups (keep last 30 days)
PRINT '';
PRINT '6. CLEANING UP OLD BACKUPS...';
DECLARE @CleanupDate DATETIME = DATEADD(DAY, -30, GETDATE());
DECLARE @CleanupCmd NVARCHAR(1000);

-- Use PowerShell to clean up old backup files
SET @CleanupCmd = 'PowerShell -Command "Get-ChildItem ''' + @BackupPath + ''' -Filter ''*.bak'' | Where-Object {$_.CreationTime -lt ''' + CONVERT(VARCHAR(20), @CleanupDate, 120) + '''} | Remove-Item -Force"';
EXEC xp_cmdshell @CleanupCmd;

SET @CleanupCmd = 'PowerShell -Command "Get-ChildItem ''' + @BackupPath + ''' -Filter ''*.trn'' | Where-Object {$_.CreationTime -lt ''' + CONVERT(VARCHAR(20), @CleanupDate, 120) + '''} | Remove-Item -Force"';
EXEC xp_cmdshell @CleanupCmd;

PRINT 'Cleanup completed (removed backups older than 30 days).';

-- 7. Database restore script template
PRINT '';
PRINT '7. RESTORE SCRIPT TEMPLATE:';
PRINT '-- To restore this backup, use:';
PRINT '-- RESTORE DATABASE BusBuddy FROM DISK = ''' + @FullBackupFile + ''' WITH REPLACE, STATS = 10;';
PRINT '';

-- 8. Recovery recommendations
PRINT '8. RECOVERY RECOMMENDATIONS:';
PRINT '📋 Backup Strategy Recommendations:';
PRINT '   • Schedule full backups weekly';
PRINT '   • Schedule differential backups daily';
PRINT '   • Schedule log backups every 15 minutes (if Full recovery mode)';
PRINT '   • Test restore procedures monthly';
PRINT '   • Store backups on separate drive/server';
PRINT '   • Consider geo-redundant backup storage';

PRINT '';
PRINT '=== Backup Process Complete ===';
PRINT 'Next full backup recommended: ' + CONVERT(VARCHAR(20), DATEADD(WEEK, 1, GETDATE()), 120);
GO

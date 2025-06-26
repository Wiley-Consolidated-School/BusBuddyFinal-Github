-- Script to add Date column to Routes table if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Routes' AND COLUMN_NAME = 'Date')
BEGIN
    ALTER TABLE Routes ADD Date NVARCHAR(50) NOT NULL DEFAULT '2025-06-24T00:00:00.0000000';

    -- Update existing records with current date if the column was just added
    UPDATE Routes SET Date = '2025-06-24T00:00:00.0000000' WHERE Date IS NULL OR Date = '';

    PRINT 'Date column added to Routes table and populated with default value.';
END
ELSE
BEGIN
    PRINT 'Date column already exists in Routes table. No changes made.';
END

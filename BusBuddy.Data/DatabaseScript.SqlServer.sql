-- BusBuddy SQL Server Schema Script
-- This script creates the core tables for the BusBuddy application.
-- Restore location: BusBuddy.Data/DatabaseScript.SqlServer.sql

-- =========================
-- Table: Buses
-- =========================
IF OBJECT_ID('dbo.Buses', 'U') IS NOT NULL
    DROP TABLE dbo.Buses;
GO

CREATE TABLE dbo.Buses (
    BusId INT IDENTITY(1,1) PRIMARY KEY,
    BusNumber NVARCHAR(50) NOT NULL,
    Year INT NULL,
    Make NVARCHAR(50) NULL,
    Model NVARCHAR(50) NULL,
    Capacity INT NOT NULL,
    VIN NVARCHAR(50) NULL,
    LicenseNumber NVARCHAR(50) NULL,
    LastInspectionDate DATETIME NULL,
    Status NVARCHAR(50) NULL
);
GO

-- =========================
-- Table: Maintenance
-- =========================
IF OBJECT_ID('dbo.Maintenance', 'U') IS NOT NULL
    DROP TABLE dbo.Maintenance;
GO

CREATE TABLE dbo.Maintenance (
    MaintenanceID INT IDENTITY(1,1) PRIMARY KEY,
    Date NVARCHAR(20) NULL,
    BusId INT NULL,
    OdometerReading DECIMAL(18,2) NULL,
    MaintenanceCompleted NVARCHAR(100) NULL,
    Vendor NVARCHAR(100) NULL,
    RepairCost DECIMAL(18,2) NULL,
    Notes NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Maintenance_Bus FOREIGN KEY (BusId) REFERENCES dbo.Buses(BusId)
);
GO

-- =========================
-- Indexes and Constraints
-- =========================
CREATE INDEX IX_Maintenance_BusId ON dbo.Maintenance (BusId);
GO

-- =========================
-- (Add additional tables as needed for Drivers, Routes, Fuel, etc.)
-- =========================

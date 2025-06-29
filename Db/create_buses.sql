CREATE TABLE Buses (
    BusId INT IDENTITY(1,1) PRIMARY KEY,
    BusNumber NVARCHAR(50) NOT NULL,
    Make NVARCHAR(100) NULL,
    Model NVARCHAR(100) NULL,
    Year INT NULL,
    Capacity INT NULL,
    Status NVARCHAR(50) NULL
    -- Add other columns as required by your application
);

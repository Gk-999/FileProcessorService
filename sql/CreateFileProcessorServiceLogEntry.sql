IF EXISTS (SELECT [Name] FROM SysObjects WHERE [name] = 'FileProcessorServiceLogEntry' AND XType = 'U')
	DROP TABLE FileProcessorServiceLogEntry
GO

CREATE TABLE FileProcessorServiceLogEntry(
    FileProcessorServiceLogEntryId INT IDENTITY (1, 1) NOT NULL CONSTRAINT FileProcessorServiceLogEntryId PRIMARY KEY,
    OccurredAt DATETIME NOT NULL,
    Thread VARCHAR(500) NOT NULL,
    [Level] VARCHAR(50) NOT NULL,
    Logger VARCHAR(500) NOT NULL,
    [Message] VARCHAR(4000) NOT NULL,
    [Filename] VARCHAR(250) NULL,
    Reference VARCHAR(50) NULL,
    [Exception] VARCHAR(2000) NULL)
GO
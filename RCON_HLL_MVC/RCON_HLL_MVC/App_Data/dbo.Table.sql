CREATE TABLE [dbo].[ServerLogs]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [CreatedOn] NCHAR(10) NOT NULL, 
    [RawLogs] NVARCHAR(MAX) NOT NULL
)

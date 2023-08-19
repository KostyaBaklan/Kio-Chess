USE [ChessData]
GO

/****** Object:  Table [dbo].[Books]    Script Date: 16/08/2023 22:52:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE TABLE [dbo].[Books](
	[History] [nvarchar](400) NOT NULL,
	[NextMove] [smallint] NOT NULL,
	[White] [int] default 0 NOT NULL ,
	[Draw] [int] default 0 NOT NULL ,
	[Black] [int] default 0 NOT NULL 
	
	CONSTRAINT [Book_PK] UNIQUE  
	(
		[History] ,[NextMove]
	)			
) 
CREATE INDEX IX_Book_History ON [dbo].[Books] ([History]);
GO



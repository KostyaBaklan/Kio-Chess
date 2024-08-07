USE [ChessData]
GO

/****** Object:  Table [dbo].[Books]    Script Date: 16/08/2023 22:52:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Books](
	[History] [varbinary](64) NOT NULL,
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


CREATE TABLE [dbo].[Pieces](
	[Piece] [tinyint] NOT NULL,
	[Key] [nchar](2) NOT NULL,
	[Name] [varchar](12) NOT NULL,
 CONSTRAINT [PK_Pieces] PRIMARY KEY CLUSTERED 
(
	[Piece] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[Squares](
	[ID] [tinyint] NOT NULL,
	[Name] [nchar](2) NOT NULL,
 CONSTRAINT [PK_Squares] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[Moves](
	[ID] [smallint] NOT NULL,
	[Piece] [tinyint] NOT NULL,
	[From] [tinyint] NOT NULL,
	[To] [tinyint] NOT NULL,
	[Type] [varchar](20) NOT NULL,
	[IsAttack] [bit] NOT NULL,
	[IsCastle] [bit] NOT NULL,
	[IsPromotion] [bit] NOT NULL,
	[IsPassed] [bit] NOT NULL,
	[IsEnPassant] [bit] NOT NULL,
	[CanReduce] [bit] NOT NULL,
	[IsIrreversible] [bit] NOT NULL,
	[IsFutile] [bit] NOT NULL,
	[IsPromotionToQueen] [bit] NOT NULL,
	[IsWhite] [bit] NOT NULL,
	[IsBlack] [bit] NOT NULL,
	[IsPromotionExtension] [bit] NOT NULL,
 CONSTRAINT [PK_Moves] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TYPE BooksTableType AS TABLE
(
	[History] [varbinary](64) NOT NULL,
	[NextMove] [smallint] NOT NULL
);

CREATE PROCEDURE dbo.UpsertDraw @UpdateDraw
	dbo.BooksTableType READONLY
AS
BEGIN
    MERGE INTO dbo.Books AS Target
    USING @UpdateDraw AS Source
    ON Target.History = Source.History and Target.NextMove = Source.NextMove

    WHEN MATCHED THEN
        UPDATE SET Target.Draw = Target.Draw + 1

    WHEN NOT MATCHED THEN           
        INSERT (History, NextMove, Draw)
        VALUES (Source.History, Source.NextMove,1);
END

CREATE PROCEDURE dbo.UpsertWhite @UpdateWhite
	dbo.BooksTableType READONLY
AS
BEGIN
    MERGE INTO dbo.Books AS Target
    USING @UpdateWhite AS Source
    ON Target.History = Source.History and Target.NextMove = Source.NextMove

    WHEN MATCHED THEN
        UPDATE SET Target.White = Target.White + 1

    WHEN NOT MATCHED THEN           
        INSERT (History, NextMove, White)
        VALUES (Source.History, Source.NextMove,1);
END

CREATE PROCEDURE dbo.UpsertBlack @UpdateBlack
	dbo.BooksTableType READONLY
AS
BEGIN
    MERGE INTO dbo.Books AS Target
    USING @UpdateBlack AS Source
    ON Target.History = Source.History and Target.NextMove = Source.NextMove

    WHEN MATCHED THEN
        UPDATE SET Target.Black = Target.Black + 1

    WHEN NOT MATCHED THEN           
        INSERT (History, NextMove, Black)
        VALUES (Source.History, Source.NextMove,1);
END

CREATE FUNCTION GameTotalHistory (@MaxSequence INT)
RETURNS TABLE AS
RETURN
	select History, NextMove, [White]+[Draw]+[Black] AS Total
	from [dbo].[Books]
	where [White]+[Draw]+[Black] > 1 AND LEN(History) < @MaxSequence;

CREATE FUNCTION GameTotalHistoryByLength (@MaxSequence INT)
RETURNS TABLE AS
RETURN
	select History, NextMove, [White]+[Draw]+[Black] AS Total
	from [dbo].[Books]
	where LEN(History) < @MaxSequence;


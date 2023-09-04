/****** Script for SelectTopNRows command from SSMS  ******/
SELECT [ID]
      ,[Name]
      ,[Variation]
      ,[Moves]
  FROM [ChessData].[dbo].[OpeningList]

  SELECT [ID] ,[Moves] FROM [dbo].[OpeningList]
  WHERE [Moves] <> ''

  SELECT [ID]
      ,[Name]
      ,[Variation]
      ,[Moves]
	  ,LEN(Moves) AS LENGTH
  FROM [ChessData].[dbo].[OpeningList]
  WHERE [Moves] <> ''

  SELECT [Name],[Variation]
  FROM [ChessData].[dbo].[OpeningList]
  WHERE [Variation] = ''

  SELECT [Name],[Moves]
  FROM [ChessData].[dbo].[OpeningList]
  WHERE [Variation] = ''

  SELECT [Sequence]
      ,[OpeningID]
  FROM [dbo].[Openings]
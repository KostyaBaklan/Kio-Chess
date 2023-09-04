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
  ORDER BY LEN(Moves)

  SELECT [Name],[Variation]
  FROM [ChessData].[dbo].[OpeningList]
  WHERE [Variation] = ''

  SELECT [Name],[Moves]
  FROM [ChessData].[dbo].[OpeningList]
  WHERE [Variation] = ''

  SELECT [ID],[Name],[Variation],[Moves]
  FROM [ChessData].[dbo].[OpeningList]
  WHERE [Moves] like '%d4%'

  SELECT [Sequence],[OpeningID]
  FROM [dbo].[Openings]

  SELECT [Sequence],[OpeningID]
  FROM [dbo].[Openings]
  WHERE [Sequence] like '%7686%'

  SELECT ol.[Name] + ' ' + ol.[Variation]
  FROM [dbo].[Openings] o INNER JOIN [dbo].OpeningList ol on o.OpeningID = ol.ID
  WHERE o.[Sequence] = '7680'

  UPDATE [dbo].OpeningList
  SET [Moves] = 'd4'
  WHERE [ID] = 2715

  DELETE from [dbo].OpeningList
  WHERE [ID] = 2722

  DELETE from [dbo].Openings
  WHERE [OpeningID] = 2722
SELECT [History]
      ,[NextMove]
      ,[White]
      ,[Draw]
      ,[Black]
  FROM [ChessData].[dbo].[Books]

  SELECT TOP (100000) [History]
      ,[NextMove]
      ,[White]
      ,[Draw]
      ,[Black]
  FROM [ChessData].[dbo].[Books]
  WHERE History = ''

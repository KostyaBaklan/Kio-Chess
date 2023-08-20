SELECT [History]
      ,[NextMove]
      ,[White]
      ,[Draw]
      ,[Black]
  FROM [ChessData].[dbo].[Books]

  SELECT m.[ID] , p.[Key] ,s1.[Name], s2.[Name], b.[White] , b.[Draw] , b.[Black]
  FROM [dbo].[Books] as b, [dbo].[Moves] m,[dbo].[Pieces] p,[dbo].[Squares] s1,[dbo].[Squares] s2
  WHERE b.History = '' and b.NextMove = m.[ID] and m.Piece = p.[Piece] and m.[From] = s1.ID and m.[To] = s2.ID

  SELECT b.[History], m.[ID] , p.[Key]+' '+'['+s1.[Name]+','+s2.[Name]+']' as Move, b.[White] , b.[Draw] , b.[Black]
  FROM [dbo].[Books] as b, [dbo].[Moves] m,[dbo].[Pieces] p,[dbo].[Squares] s1,[dbo].[Squares] s2
  WHERE b.History = '' and b.NextMove = m.[ID] and m.Piece = p.[Piece] and m.[From] = s1.ID and m.[To] = s2.ID

  delete from dbo.Pieces

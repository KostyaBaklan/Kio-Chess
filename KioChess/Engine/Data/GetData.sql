SELECT [History]
      ,[NextMove]
      ,[White]
      ,[Draw]
      ,[Black]
  FROM [ChessData].[dbo].[Books]

  SELECT b.[History],m.[ID] , p.[Key] ,s1.[Name], s2.[Name], b.[White] , b.[Draw] , b.[Black]
  FROM [dbo].[Books] as b, [dbo].[Moves] m,[dbo].[Pieces] p,[dbo].[Squares] s1,[dbo].[Squares] s2
  WHERE (b.History = '' or len(b.History) < 6) and b.NextMove = m.[ID] and m.Piece = p.[Piece] and m.[From] = s1.ID and m.[To] = s2.ID

  SELECT b.[History], m.[ID] , p.[Key]+' '+'['+s1.[Name]+','+s2.[Name]+']' as Move, b.[White] , b.[Draw] , b.[Black]
  FROM [dbo].[Books] as b, [dbo].[Moves] m,[dbo].[Pieces] p,[dbo].[Squares] s1,[dbo].[Squares] s2
  WHERE b.History = '' and b.NextMove = m.[ID] and m.Piece = p.[Piece] and m.[From] = s1.ID and m.[To] = s2.ID

  select * from dbo.Moves 
  where [Piece] = 0 and [From] = 13 and [IsAttack] = 0

  select * from dbo.Moves 
  where [Piece] = 0 and [From] = 11 and [IsAttack] = 0

  select * from dbo.Moves 
  where [Piece] = 0 and [From] = 12 and [IsAttack] = 0

  select * from dbo.Moves 
  where [Piece] = 6 and [To] = 44 and [IsAttack] = 0

  select * from dbo.Moves 
  where [Piece] = 2 and [From] = 2 and [To] = 29 and [IsAttack] = 0

  select * from dbo.Moves 
  where [Piece] = 2 and [From] = 5 and [To] = 26 and [IsAttack] = 0


  delete from dbo.Pieces

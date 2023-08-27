SELECT [History]
      ,[NextMove]
      ,[White]
      ,[Draw]
      ,[Black]
  FROM [ChessData].[dbo].[Books]

  SELECT DISTINCT [History]
  FROM [ChessData].[dbo].[Books]

  SELECT [White],[Draw],[Black]
  FROM [ChessData].[dbo].[Books]
   WHERE [History] = ''

  SELECT [White]+[Draw]+[Black]
  FROM [ChessData].[dbo].[Books]
  WHERE [History] = ''

  SELECT SUM([White]+[Draw]+[Black])
  FROM [ChessData].[dbo].[Books]
  WHERE [History] = ''

  SELECT p.[Key]+' '+'['+s1.[Name]+','+s2.[Name]+']' as Move, b.[White] , b.[Draw] , b.[Black],[White]+[Draw]+[Black]
  FROM [dbo].[Books] as b, [dbo].[Moves] m,[dbo].[Pieces] p,[dbo].[Squares] s1,[dbo].[Squares] s2
  WHERE b.History = '' and b.NextMove = m.[ID] and m.Piece = p.[Piece] and m.[From] = s1.ID and m.[To] = s2.ID

  SELECT [History]
      ,[NextMove]
      ,[White]
      ,[Draw]
      ,[Black]
  FROM [ChessData].[dbo].[Books]
  WHERE LEN(History) < 100

  SELECT b.[History],m.[ID] , p.[Key] ,s1.[Name], s2.[Name], b.[White] , b.[Draw] , b.[Black]
  FROM [dbo].[Books] as b, [dbo].[Moves] m,[dbo].[Pieces] p,[dbo].[Squares] s1,[dbo].[Squares] s2
  WHERE (b.History = '' or len(b.History) < 6) and b.NextMove = m.[ID] and m.Piece = p.[Piece] and m.[From] = s1.ID and m.[To] = s2.ID

  SELECT b.[History], m.[ID] , p.[Key]+' '+'['+s1.[Name]+','+s2.[Name]+']' as Move, b.[White] , b.[Draw] , b.[Black]
  FROM [dbo].[Books] as b, [dbo].[Moves] m,[dbo].[Pieces] p,[dbo].[Squares] s1,[dbo].[Squares] s2
  WHERE b.History = '' and b.NextMove = m.[ID] and m.Piece = p.[Piece] and m.[From] = s1.ID and m.[To] = s2.ID

  select ID from dbo.Moves 
  where [Piece] = 0 and [IsAttack] = 0 and [From] > 7 and [From] < 16 and [To] > 15 and [To] < 24

  select * from dbo.Moves 
  where [Piece] = 6 and [IsAttack] = 0 and [To] > 39

  select * from dbo.Moves 
  where [Piece] = 10 and [IsAttack] = 0 and [From] = 59

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

  delete from dbo.Books where len(History) >= 100

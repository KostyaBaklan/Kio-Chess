SELECT [History]
      ,[NextMove]
      ,[White]
      ,[Draw]
      ,[Black]
  FROM [ChessData].[dbo].[Books] WITH (NOLOCK)

  SELECT [History]
      ,[NextMove]
      ,[White]+[Draw]+[Black] as Total
  FROM [ChessData].[dbo].[Books] WITH (NOLOCK)
  WHERE ([White]+[Draw]+[Black]) > 8

  SELECT [History]
      ,[NextMove]
      ,[White]
      ,[Draw]
      ,[Black]
  FROM [ChessData].[dbo].[Books] WITH (NOLOCK)
  WHERE ([White]+[Draw]+[Black]) > 8

  SELECT [History]
      ,[NextMove]
      ,[White]
      ,[Draw]
      ,[Black]
  FROM [ChessData].[dbo].[Books] WITH (NOLOCK)
  WHERE ABS([White]-[Black]) > 3 or ([White]+[Draw]+[Black]) > 10

  SELECT distinct [History]
  FROM [ChessData].[dbo].[Books] WITH (NOLOCK)
  WHERE ABS([White]-[Black]) > 3 or ([White]+[Draw]+[Black]) > 10

  SELECT DISTINCT [History]
  FROM [ChessData].[dbo].[Books] WITH (NOLOCK)

  SELECT [White],[Draw],[Black]
  FROM [ChessData].[dbo].[Books] WITH (NOLOCK)
   WHERE [History] = 0x

  SELECT SUM([White]+[Draw]+[Black])
  FROM [ChessData].[dbo].[Books] WITH (NOLOCK)
  WHERE [History] = 0x

  --delete from dbo.Pieces

  --delete from dbo.Books where NextMove < 0
  --delete from dbo.Books

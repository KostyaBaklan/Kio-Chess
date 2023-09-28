SELECT [History]
      ,[NextMove]
      ,[White]
      ,[Draw]
      ,[Black]
  FROM [dbo].[Books] WITH (NOLOCK)

  SELECT [History]
      ,[NextMove]
      ,[White]+[Draw]+[Black] as Total
  FROM [dbo].[Books] WITH (NOLOCK)
  WHERE ([White]+[Draw]+[Black]) > 8

  SELECT [History]
      ,[NextMove]
      ,[White]
      ,[Draw]
      ,[Black]
  FROM [dbo].[Books] WITH (NOLOCK)
  WHERE ([White]+[Draw]+[Black]) > 8

  SELECT DISTINCT [History]
  FROM [dbo].[Books] WITH (NOLOCK)

  SELECT [White],[Draw],[Black]
  FROM [dbo].[Books] WITH (NOLOCK)
   WHERE [History] = 0x

  SELECT SUM([White]+[Draw]+[Black])
  FROM [dbo].[Books] WITH (NOLOCK)
  WHERE [History] = 0x

  SELECT count(*)
  FROM [dbo].[Books] WITH (NOLOCK)

  Select 1.0*(SELECT count(*)
  FROM [dbo].[Books] WITH (NOLOCK))/(SELECT SUM([White]+[Draw]+[Black])
  FROM [dbo].[Books] WITH (NOLOCK)
  WHERE [History] = 0x)

  --delete from dbo.Books

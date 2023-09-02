SELECT [History]
      ,[NextMove]
      ,[White]
      ,[Draw]
      ,[Black]
  FROM [ChessData].[dbo].[Books] WITH (NOLOCK)

  SELECT [History]
      ,[NextMove]
      ,[White]
      ,[Black]
	  ,([White]+[Draw]+[Black]) AS Total
  FROM [ChessData].[dbo].[Books] WITH (NOLOCK)
  WHERE ([White]+[Draw]+[Black]) > 10 OR ABS([White]-[Black]) > 3

  SELECT HistoryMoveTotal.History, HistoryMoveTotal.NextMove, HistoryMoveTotal.Total
  FROM
	  (SELECT [History], MAX([White]+[Draw]+[Black]) AS MaxTotal
	  FROM [ChessData].[dbo].[Books] WITH (NOLOCK)
	  WHERE ([White]+[Draw]+[Black]) > 10
	  GROUP BY [History]) AS HistoryTotal
  INNER JOIN 
	  (SELECT [History]
      ,[NextMove]
      ,([White]+[Draw]+[Black]) AS Total
	  FROM [ChessData].[dbo].[Books] WITH (NOLOCK)
	  WHERE ([White]+[Draw]+[Black]) > 10) AS HistoryMoveTotal
  ON HistoryTotal.History = HistoryMoveTotal.History AND HistoryTotal.MaxTotal = HistoryMoveTotal.Total

  SELECT [History]
      ,[NextMove]
      ,([White]+[Draw]+[Black]) AS Total
  FROM [ChessData].[dbo].[Books] WITH (NOLOCK)
  WHERE ([White]+[Draw]+[Black]) > 10

  SELECT WhiteDifference.History, WhiteDifference.NextMove, WhiteMaxDifference.MaxDifference, WhiteMaxDifference.MinDifference
  FROM 
		(SELECT b.[History], MAX(b.[White]-b.[Black]) AS MaxDifference, MIN(b.[White]-b.[Black]) AS MinDifference
		  FROM [ChessData].[dbo].[Books] b WITH (NOLOCK)
			INNER JOIN [ChessData].[dbo].[Moves] m ON b.NextMove = m.ID
		  WHERE ABS(b.[White]-b.[Black]) > 3 AND m.[IsWhite] = 1
		  GROUP BY b.History) AS WhiteMaxDifference
  INNER JOIN
		(SELECT b.[History]
			  ,b.[NextMove]
			  ,(b.[White]-b.[Black]) AS Difference
		  FROM [ChessData].[dbo].[Books] b WITH (NOLOCK)
			INNER JOIN [ChessData].[dbo].[Moves] m ON b.NextMove = m.ID
		  WHERE ABS(b.[White]-b.[Black]) > 3 AND m.[IsWhite] = 1) AS WhiteDifference
	ON WhiteMaxDifference.History = WhiteDifference.History
	WHERE WhiteMaxDifference.MaxDifference = WhiteDifference.Difference OR WhiteMaxDifference.MinDifference = WhiteDifference.Difference 


  SELECT b.[History]
      ,b.[NextMove]
      ,(b.[White]-b.[Black]) AS Difference
  FROM [ChessData].[dbo].[Books] b WITH (NOLOCK)
	INNER JOIN [ChessData].[dbo].[Moves] m ON b.NextMove = m.ID
  WHERE ABS(b.[White]-b.[Black]) > 3 AND m.[IsWhite] = 1

  SELECT [History]
      ,[NextMove]
      ,([Black] - [White]) AS Difference
  FROM [ChessData].[dbo].[Books] b WITH (NOLOCK)
	INNER JOIN [ChessData].[dbo].[Moves] m ON b.NextMove = m.ID
  WHERE ABS(b.[White]-b.[Black]) > 3  AND m.[IsBlack] = 1

  SELECT [History]
      ,[NextMove]
      ,[White]
      ,[Draw]
      ,[Black]
	  ,[White]-[Black] AS WhiteDifference,[Black]-[White] AS BlackDifference
  FROM [ChessData].[dbo].[Books] WITH (NOLOCK)
  WHERE ABS([White]-[Black]) > 3 or ABS([Black]-[White]) > 3
  ORDER BY LEN([History])

  SELECT DISTINCT [History]
  FROM [ChessData].[dbo].[Books] WITH (NOLOCK)

  SELECT [White],[Draw],[Black]
  FROM [ChessData].[dbo].[Books] WITH (NOLOCK)
   WHERE [History] = ''

  SELECT [White]+[Draw]+[Black]
  FROM [ChessData].[dbo].[Books] WITH (NOLOCK)
  WHERE [History] = ''

  SELECT SUM([White]+[Draw]+[Black])
  FROM [ChessData].[dbo].[Books] WITH (NOLOCK)
  WHERE [History] = ''


SELECT [NextMove] ,[White], [Draw], [Black] FROM [ChessDB-1].[dbo].[Books] WITH (NOLOCK) WHERE [History] = 0x order by NextMove
--UNION ALL
SELECT [NextMove] ,[White], [Draw], [Black] FROM [ChessDB-2].[dbo].[Books] WITH (NOLOCK) WHERE [History] = 0x order by NextMove

SELECT *
FROM 
(
	SELECT ISNULL(B1.[History],B2.[History]) AS History, ISNULL(B1.[NextMove],B2.[NextMove]) AS NextMove,
		CASE
			WHEN B1.[History] IS NOT NULL AND B2.[History] IS NOT NULL THEN (B1.[White]+B1.[Draw]+B1.[Black]+B2.[White]+B2.[Draw]+B2.[Black])
			WHEN B1.[History] IS NULL THEN (B2.[White]+B2.[Draw]+B2.[Black])
			ELSE (B1.[White]+B1.[Draw]+B1.[Black])
		END AS Total
	FROM [ChessDB-1].[dbo].[Books] B1 FULL OUTER JOIN [ChessDB-2].[dbo].[Books] B2 ON B1.[History] = B2.[History] AND B1.[NextMove] = B2.[NextMove]
) AS BOOKJOIN
WHERE Total > 8
ORDER BY LEN(History)

select History, NextMove, [White]+[Draw]+[Black] AS Total
from [ChessDB-1].[dbo].[Books]

select History, NextMove, [White]+[Draw]+[Black] AS Total
from [ChessDB-2].[dbo].[Books]


SELECT * FROM [ChessDB-1].[dbo].[GameTotalHistory] (49)
SELECT * FROM [ChessDB-2].[dbo].[GameTotalHistory] (49)
SELECT * FROM [ChessDB-2].[dbo].[GameTotalHistory] (41)

SELECT *
FROM 
(
	SELECT ISNULL(B1.[History],B2.[History]) AS History, ISNULL(B1.[NextMove],B2.[NextMove]) AS NextMove,
		CASE
			WHEN B1.[History] IS NOT NULL AND B2.[History] IS NOT NULL THEN (B1.[Total]+B2.[Total])
			WHEN B1.[History] IS NULL THEN (B2.[Total])
			ELSE (B1.[Total])
		END AS Total
	FROM [ChessDB-1].[dbo].[GameTotalHistory] (41) AS B1 FULL OUTER JOIN [ChessDB-2].[dbo].[GameTotalHistory] (41) AS B2 ON B1.[History] = B2.[History] AND B1.[NextMove] = B2.[NextMove]
) AS BOOKJOIN
WHERE Total > 8

SELECT *
FROM 
(
	SELECT ISNULL(B1.[History],B2.[History]) AS History, ISNULL(B1.[NextMove],B2.[NextMove]) AS NextMove,
		CASE
			WHEN B1.[History] IS NOT NULL AND B2.[History] IS NOT NULL THEN (B1.[Total]+B2.[Total])
			WHEN B1.[History] IS NULL THEN (B2.[Total])
			ELSE (B1.[Total])
		END AS Total
	FROM [ChessDB-1].[dbo].[GameTotalHistory] (49) AS B1 FULL OUTER JOIN [ChessDB-2].[dbo].[GameTotalHistory] (49) AS B2 ON B1.[History] = B2.[History] AND B1.[NextMove] = B2.[NextMove]
) AS BOOKJOIN
WHERE Total > 8

select History,NextMove, SUM(Total) AS TotalGames
from
	(
		select * from [ChessDB-1].[dbo].[GameTotalHistory](49)
		union all
		select * from [ChessDB-2].[dbo].[GameTotalHistory](49)
	) AS UNIONALL
Group by History,NextMove
having SUM(Total) > 8

SELECT *
FROM 
(
	SELECT ISNULL(B1.[History],B2.[History]) AS History, ISNULL(B1.[NextMove],B2.[NextMove]) AS NextMove,
		CASE
			WHEN B1.[History] IS NOT NULL AND B2.[History] IS NOT NULL THEN (B1.[Total]+B2.[Total])
			WHEN B1.[History] IS NULL THEN (B2.[Total])
			ELSE (B1.[Total])
		END AS Total
	FROM [ChessDB-1].[dbo].[GameTotalHistoryByLength] (49) AS B1 FULL OUTER JOIN [ChessDB-2].[dbo].[GameTotalHistoryByLength] (49) AS B2 ON B1.[History] = B2.[History] AND B1.[NextMove] = B2.[NextMove]
) AS BOOKJOIN
WHERE Total > 8

select History,NextMove, SUM(Total) AS TotalGames
from
	(
		select * from [ChessDB-1].[dbo].[GameTotalHistoryByLength](49)
		union all
		select * from [ChessDB-2].[dbo].[GameTotalHistoryByLength](49)
	) AS UNIONALL
Group by History,NextMove
having SUM(Total) > 8

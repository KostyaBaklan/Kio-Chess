
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


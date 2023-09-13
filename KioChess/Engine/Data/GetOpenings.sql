SELECT [ID] ,[Name]
  FROM [dbo].[Openings]
  Where [Name] like 'English Opening'

  SELECT [ID],[Name]
  FROM [dbo].[Variations]
  Where [Name] like '%Grünfeld%'

  SELECT [ID]
      ,[Name]
      ,[OpeningID]
      ,[VariationID]
      ,[Moves]
  FROM [dbo].[OpeningVariations]
  WHERE [Name] like '%Van Geet Opening%Reversed%'

  SELECT [ID]
      ,[Name]
      ,[OpeningID]
      ,[VariationID]
      ,[Moves]
  FROM [dbo].[OpeningVariations]
  WHERE [Name] like '%Tarrasch%'

  SELECT [ID]
      ,[Name]
      ,[OpeningID]
      ,[VariationID]
      ,[Moves]
  FROM [dbo].[OpeningVariations]
  WHERE [OpeningID] = 140

  SELECT [ID]
      ,[Name]
      ,[OpeningID]
      ,[VariationID]
      ,[Moves]
  FROM [dbo].[OpeningVariations]
  WHERE [ID] in (2433,2408)

  SELECT [ID]
      ,[Name]
      ,[OpeningID]
      ,[VariationID]
      ,[Moves]
  FROM [dbo].[OpeningVariations]
  WHERE [ID] > 3075

  SELECT [ID]
      ,[Name]
      ,[OpeningID]
      ,[VariationID]
      ,[Moves]
  FROM [dbo].[OpeningVariations]
  WHERE [Moves] like 'e4 c5 Nf3 d6 c3'

    SELECT top 20 [ID]
      ,[Name]
      ,[OpeningID]
      ,[VariationID]
      ,[Moves]
  FROM [dbo].[OpeningVariations]
  WHERE [VariationID] = 1
  Order BY LEN ([Moves])

      SELECT top 20 [Name]
  FROM [dbo].[OpeningVariations]
  WHERE [VariationID] = 1
  Order BY LEN ([Moves])

  SELECT [Moves] FROM [dbo].[OpeningVariations]

	  --UPDATE [dbo].[OpeningVariations]
	  --SET Moves = 'e4 e5 Nf3 Nc6 c3 Nge7'
	  --WHERE ID = 2433

  SELECT [ID] ,[Moves] FROM [dbo].[OpeningVariations] Order BY LEN ([Moves])

  SELECT COUNT([ID]) FROM [dbo].[OpeningVariations] WHERE [Name] = ''

  SELECT COUNT([ID]) FROM [dbo].[OpeningVariations] WHERE [OpeningID] = 1000 AND [VariationID] = 1

  SELECT [ID] FROM [dbo].[Openings] WHERE [Name] = ''

  SELECT [ID] FROM [dbo].[Variations] WHERE [Name] = ''

  SELECT  [OpeningVariationID] FROM [dbo].[OpeningSequences] WHERE [Sequence] = ''

  SELECT [ID]
      ,[Sequence]
      ,[OpeningVariationID]
  FROM [dbo].[OpeningSequences]
  Where [OpeningVariationID] > 2484
  order by [OpeningVariationID]

    SELECT [ID]
      ,[Sequence]
      ,[OpeningVariationID]
  FROM [dbo].[OpeningSequences]
  Where [OpeningVariationID] in (2357)
  order by [OpeningVariationID]

  SELECT [Sequence], count (*)
  FROM [dbo].[OpeningSequences]
  group by [Sequence]
  having count(*) > 1

  SELECT [OpeningVariationID], count (*)
  FROM [dbo].[OpeningSequences]
  group by [OpeningVariationID]
  having count(*) > 1

  SELECT [Sequence] FROM [dbo].[OpeningSequences]

  SELECT os.[Sequence],ov.[Name], ov.[Moves]
  FROM [dbo].[OpeningSequences] os INNER JOIN [dbo].[OpeningVariations] ov ON os.[OpeningVariationID] = ov.[ID]
  ORDER BY ov.Name

  SELECT os.[ID], os.[Sequence],ov.[Name], ov.[Moves]
  FROM [dbo].[OpeningSequences] os INNER JOIN [dbo].[OpeningVariations] ov ON os.[OpeningVariationID] = ov.[ID]
  WHERE os.[Sequence] in (SELECT [Sequence]
						  FROM [dbo].[OpeningSequences]
						  group by [Sequence]
						  having count(*) > 1)
ORDER BY os.[Sequence]

  SELECT os.[Sequence],ov.[Name], ov.[Moves]
  FROM [dbo].[OpeningSequences] os INNER JOIN [dbo].[OpeningVariations] ov ON os.[OpeningVariationID] = ov.[ID]
  Where os.[OpeningVariationID] > 2002
  ORDER BY ov.[Name]
  --WHERE ov.[ID] > 1000

  --delete from dbo.[OpeningSequences] where [OpeningVariationID]  = 2357
  --delete from dbo.OpeningVariations where ID =2357
  --delete from dbo.[OpeningSequences] where [ID]  in (1959,7589,6464)
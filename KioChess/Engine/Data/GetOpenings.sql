SELECT [ID] ,[Name]
  FROM [dbo].[Openings]

  SELECT [ID],[Name]
  FROM [dbo].[Variations]

  SELECT [ID]
      ,[Name]
      ,[OpeningID]
      ,[VariationID]
      ,[Moves]
  FROM [dbo].[OpeningVariations]
  WHERE ID = 213

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

  UPDATE [dbo].[OpeningVariations]
  SET [Moves] = 'f4 Nf6 c4'
  WHERE ID = 213

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

  SELECT ov.[Name]
  FROM [dbo].[OpeningSequences] os INNER JOIN [dbo].[OpeningVariations] ov ON os.[OpeningVariationID] = ov.[ID]
  WHERE os.[Sequence] = '7681'

  SELECT os.[Sequence],ov.[Name], ov.[Moves]
  FROM [dbo].[OpeningSequences] os INNER JOIN [dbo].[OpeningVariations] ov ON os.[OpeningVariationID] = ov.[ID]
  WHERE os.[OpeningVariationID] = 46

  delete from dbo.OpeningVariations where ID = 46
  delete from dbo.[OpeningSequences] where [OpeningVariationID]  = 46
  delete from dbo.[OpeningSequences] where [ID]  = 1002
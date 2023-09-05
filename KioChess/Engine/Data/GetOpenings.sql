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

  UPDATE [dbo].[OpeningVariations]
  SET [Moves] = 'f4 Nf6 c4'
  WHERE ID = 213

  SELECT [ID] ,[Moves] FROM [dbo].[OpeningVariations] Order BY LEN ([Moves])

  SELECT COUNT([ID]) FROM [dbo].[OpeningVariations] WHERE [Name] = ''

  SELECT [ID] FROM [dbo].[Openings] WHERE [Name] = ''

  SELECT [ID] FROM [dbo].[Variations] WHERE [Name] = ''

  SELECT [ID]
      ,[Sequence]
      ,[OpeningVariationID]
  FROM [dbo].[OpeningSequences]

  SELECT os.[Sequence],ov.[Name], ov.[Moves]
  FROM [dbo].[OpeningSequences] os INNER JOIN [dbo].[OpeningVariations] ov ON os.[OpeningVariationID] = ov.[ID]
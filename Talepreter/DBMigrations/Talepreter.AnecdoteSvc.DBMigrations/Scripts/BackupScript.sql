CREATE PROCEDURE dbo.TPBACKUPTOVERSION @taleId UNIQUEIDENTIFIER, @sourceVersionId UNIQUEIDENTIFIER, @targetVersionId UNIQUEIDENTIFIER
AS

-- PluginRecords table
INSERT INTO [dbo].[PluginRecords] ([Id], [TaleId], [TaleVersionId] , [BaseId], [Type], [PluginData], [WriterId], [LastUpdate], [LastUpdatedChapter], [LastUpdatedPageInChapter], [PublishState])
SELECT                             [Id], [TaleId], @targetVersionId, [BaseId], [Type], [PluginData], [WriterId], [LastUpdate], [LastUpdatedChapter], [LastUpdatedPageInChapter], [PublishState] FROM [dbo].[PluginRecords]
WHERE [TaleId] = @taleId AND [TaleVersionId] = @sourceVersionId;

-- Triggers table

INSERT INTO [dbo].[Triggers] ([Id], [TaleId], [TaleVersionId] , [WriterId], [LastUpdate], [State], [TriggerAt], [Target], [GrainType], [GrainId], [Type], [Parameter])
SELECT                        [Id], [TaleId], @targetVersionId, [WriterId], [LastUpdate], [State], [TriggerAt], [Target], [GrainType], [GrainId], [Type], [Parameter] FROM [dbo].[Triggers]
WHERE [TaleId] = @taleId AND [TaleVersionId] = @sourceVersionId;

-- Anecdotes table

INSERT INTO [dbo].[Anecdotes] ([Id], [TaleId], [TaleVersionId] , [ParentId], [PluginData], [WriterId], [LastUpdate], [LastUpdatedChapter], [LastUpdatedPageInChapter], [PublishState], [Entries])
SELECT                         [Id], [TaleId], @targetVersionId, [ParentId], [PluginData], [WriterId], [LastUpdate], [LastUpdatedChapter], [LastUpdatedPageInChapter], [PublishState], [Entries] FROM [dbo].[Anecdotes]
WHERE [TaleId] = @taleId AND [TaleVersionId] = @sourceVersionId;

GO

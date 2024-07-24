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

-- Persons table

INSERT INTO [dbo].[Persons] ([Id], [TaleId], [TaleVersionId] , [Tags], [Physics], [Identity], [LastSeen], [LastSeenLocation_Settlement], [LastSeenLocation_Extension], [PluginData], [WriterId], [LastUpdate], [LastUpdatedChapter], [LastUpdatedPageInChapter], [PublishState], [StartsAt], [ExpiresAt], [ExpiredAt], [ExpireState], [Notes])
SELECT                       [Id], [TaleId], @targetVersionId, [Tags], [Physics], [Identity], [LastSeen], [LastSeenLocation_Settlement], [LastSeenLocation_Extension], [PluginData], [WriterId], [LastUpdate], [LastUpdatedChapter], [LastUpdatedPageInChapter], [PublishState], [StartsAt], [ExpiresAt], [ExpiredAt], [ExpireState], [Notes] FROM [dbo].[Persons]
WHERE [TaleId] = @taleId AND [TaleVersionId] = @sourceVersionId;

GO

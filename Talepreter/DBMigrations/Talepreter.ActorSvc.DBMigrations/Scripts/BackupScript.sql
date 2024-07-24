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

-- Actors table
INSERT INTO [dbo].[Actors] ([Id], [TaleId], [TaleVersionId] , [Physics], [Identity], [LastSeen], [PluginData], [WriterId], [LastUpdate], [LastUpdatedChapter], [LastUpdatedPageInChapter], [PublishState], [StartsAt], [ExpiresAt], [ExpiredAt], [ExpireState], [LastSeenLocation], [Notes])
SELECT                      [Id], [TaleId], @targetVersionId, [Physics], [Identity], [LastSeen], [PluginData], [WriterId], [LastUpdate], [LastUpdatedChapter], [LastUpdatedPageInChapter], [PublishState], [StartsAt], [ExpiresAt], [ExpiredAt], [ExpireState], [LastSeenLocation], [Notes] FROM [dbo].[Actors]
WHERE [TaleId] = @taleId AND [TaleVersionId] = @sourceVersionId;

-- Traits table

INSERT INTO [dbo].[Traits] ([Id], [TaleId], [TaleVersionId] , [OwnerName], [Type], [Description], [PluginData], [WriterId], [LastUpdate], [LastUpdatedChapter], [LastUpdatedPageInChapter], [PublishState], [StartsAt], [ExpiresAt], [ExpiredAt], [ExpireState])
SELECT                      [Id], [TaleId], @targetVersionId, [OwnerName], [Type], [Description], [PluginData], [WriterId], [LastUpdate], [LastUpdatedChapter], [LastUpdatedPageInChapter], [PublishState], [StartsAt], [ExpiresAt], [ExpiredAt], [ExpireState] FROM [dbo].[Traits]
WHERE [TaleId] = @taleId AND [TaleVersionId] = @sourceVersionId;

GO

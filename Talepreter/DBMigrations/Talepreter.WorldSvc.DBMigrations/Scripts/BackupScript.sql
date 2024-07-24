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

-- Settlements table

INSERT INTO [dbo].[Settlements] ([Id], [TaleId], [TaleVersionId] , [Description], [FirstVisited], [LastVisited], [PluginData], [WriterId], [LastUpdate], [LastUpdatedChapter], [LastUpdatedPageInChapter], [PublishState])
SELECT                           [Id], [TaleId], @targetVersionId, [Description], [FirstVisited], [LastVisited], [PluginData], [WriterId], [LastUpdate], [LastUpdatedChapter], [LastUpdatedPageInChapter], [PublishState] FROM [dbo].[Settlements]
WHERE [TaleId] = @taleId AND [TaleVersionId] = @sourceVersionId;

-- Worlds table

INSERT INTO [dbo].[Worlds] ([Id], [TaleId], [TaleVersionId] , [Description], [PluginData], [WriterId], [LastUpdate], [LastUpdatedChapter], [LastUpdatedPageInChapter], [PublishState])
SELECT                      [Id], [TaleId], @targetVersionId, [Description], [PluginData], [WriterId], [LastUpdate], [LastUpdatedChapter], [LastUpdatedPageInChapter], [PublishState] FROM [dbo].[Worlds]
WHERE [TaleId] = @taleId AND [TaleVersionId] = @sourceVersionId;

-- Chapters table

INSERT INTO [dbo].[Chapters] ([Id], [TaleId], [TaleVersionId] , [WorldName], [Title], [Summary], [Reference], [WriterId], [LastUpdate], [LastUpdatedChapter], [LastUpdatedPageInChapter], [PublishState])
SELECT                        [Id], [TaleId], @targetVersionId, [WorldName], [Title], [Summary], [Reference], [WriterId], [LastUpdate], [LastUpdatedChapter], [LastUpdatedPageInChapter], [PublishState] FROM [dbo].[Chapters]
WHERE [TaleId] = @taleId AND [TaleVersionId] = @sourceVersionId;

-- Pages table

INSERT INTO [dbo].[Pages] ([Id], [TaleId], [TaleVersionId] , [ChapterId], [Location_Settlement], [Location_Extension], [StartDate], [StayAtLocation], [Travel_Duration], [Travel_Destination_Settlement], [Travel_Destination_Extension], [Notes], [WriterId], [LastUpdate], [LastUpdatedChapter], [LastUpdatedPageInChapter], [PublishState])
SELECT                     [Id], [TaleId], @targetVersionId, [ChapterId], [Location_Settlement], [Location_Extension], [StartDate], [StayAtLocation], [Travel_Duration], [Travel_Destination_Settlement], [Travel_Destination_Extension], [Notes], [WriterId], [LastUpdate], [LastUpdatedChapter], [LastUpdatedPageInChapter], [PublishState] FROM [dbo].[Pages]
WHERE [TaleId] = @taleId AND [TaleVersionId] = @sourceVersionId;

GO

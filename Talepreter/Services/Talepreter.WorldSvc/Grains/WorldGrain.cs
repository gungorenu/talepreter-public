using Microsoft.EntityFrameworkCore;
using Orleans.Runtime;
using Talepreter.BaseTypes;
using Talepreter.Contracts.Orleans.Execute;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Contracts.Orleans.Grains.Entities;
using Talepreter.Exceptions;
using Talepreter.WorldSvc.DBContext;
using Talepreter.Common;
using Talepreter.Operations.Grains;
using Talepreter.Operations.Grains.States;
using Talepreter.Operations;
using Talepreter.Common.RabbitMQ.Interfaces;

namespace Talepreter.WorldSvc.Grains
{
    public class WorldGrain : EntityGrain<WorldGrainState, WorldSvcDBContext, IWorldGrain>, IWorldGrain
    {
        public WorldGrain([PersistentState("persistentState", "WorldSvcStorage")] IPersistentState<WorldGrainState> persistentState,
            ILogger<WorldGrain> logger,
            IPublisher publisher,
            ITalepreterServiceIdentifier serviceId)
            : base(persistentState, logger, publisher, serviceId)
        {
        }

        protected override IContainerGrain FetchContainer(CommandId commandId) => GrainFactory.FetchWorldContainer(commandId.TaleId, commandId.TaleVersionId);

        protected override async Task<EntityDbBase> ExecuteTriggerCommand(Command command, WorldSvcDBContext dbContext, CancellationToken token)
        {
            // in Talepreter itself there is nothing to expire about worlds, but this may be required for plugins, and we need to return a world instance back for them
            // otherwise there is nothing special here
            var worlds = await dbContext.Worlds.OfTale(command).Where(x => x.Id == command.Target).ToArrayAsync(token);
            if (worlds.Length > 1) throw new CommandExecutionException(command, "More than one world found with unique key");
            if (worlds.Length < 1) throw new CommandExecutionException(command, "World not found for the trigger");
            return worlds[0];
        }

        protected override async Task<EntityDbBase> ExecuteCommand(Command command, WorldSvcDBContext dbContext, CancellationToken token)
        {
            return command.Tag switch
            {
                CommandIds.World => await ExecuteCommand_World(command, dbContext, token),
                CommandIds.Chapter => await ExecuteCommand_Chapter(command, dbContext, token),
                CommandIds.Page => await ExecuteCommand_Page(command, dbContext, token),
                _ => await ExecuteCommand_World(command, dbContext, token),
            };
        }

        private async Task<World> ExecuteCommand_World(Command command, WorldSvcDBContext dbContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            // no id search here, system does not support multiple worlds for now
            var worlds = await dbContext.Worlds.OfTale(command).ToArrayAsync(token);
            if (worlds.Length > 1) throw new CommandExecutionException(command, "More than one world found, system does not support this for now");
            World world;
            if (worlds.Length == 1)
            {
                world = worlds[0];
                world.LastUpdatedChapter = command.ChapterId;
                world.LastUpdatedPageInChapter = command.PageId;
                world.LastUpdate = command.OperationTime;
                world.WriterId = command.WriterId;
            }
            else
            {
                if (command.Tag != CommandIds.World) throw new CommandExecutionBlockedException(command, "World entity must exist before executing a plugin command");

                world = new World
                {
                    TaleId = command.TaleId,
                    TaleVersionId = command.TaleVersionId,
                    Id = command.Target,
                    LastUpdatedChapter = command.ChapterId,
                    LastUpdatedPageInChapter = command.PageId,
                    LastUpdate = command.OperationTime,
                    WriterId = command.WriterId,
                    IsNew = true
                };
                dbContext.Worlds.Add(world);
            }

            token.ThrowIfCancellationRequested();

            // only World command is processed here, rest is on plugins
            if (command.Tag != CommandIds.World) return world;

            if (!string.IsNullOrEmpty(command.Comments)) world.Description = command.Comments;

            return world;
        }

        private async Task<Chapter> ExecuteCommand_Chapter(Command command, WorldSvcDBContext dbContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var chapters = await dbContext.Chapters.OfTale(command).Where(x => x.Id == command.Target).ToArrayAsync(token);
            if (chapters.Length > 1) throw new CommandExecutionException(command, "More than one chapter found with unique key");
            Chapter chapter = null!;
            if (chapters.Length == 1)
            {
                chapter = chapters[0];
                chapter.LastUpdatedChapter = command.ChapterId;
                chapter.LastUpdatedPageInChapter = command.PageId;
                chapter.LastUpdate = command.OperationTime;
                chapter.WriterId = command.WriterId;
            }
            else
            {
                var worlds = await dbContext.Worlds.OfTale(command).ToArrayAsync(token);
                if (worlds.Length > 1) throw new CommandExecutionException(command, "More than one world found, system does not support this for now");
                if (worlds.Length == 0) throw new CommandExecutionBlockedException(command, "World entity must exist before executing command");

                if (command.Tag != CommandIds.Chapter) throw new CommandExecutionBlockedException(command, "Chapter entity must exist before executing a plugin command");

                chapter = new Chapter
                {
                    TaleId = command.TaleId,
                    TaleVersionId = command.TaleVersionId,
                    Id = command.Target,
                    LastUpdatedChapter = command.ChapterId,
                    LastUpdatedPageInChapter = command.PageId,
                    LastUpdate = command.OperationTime,
                    WriterId = command.WriterId,
                    IsNew = true
                };
                dbContext.Chapters.Add(chapter);

                chapter.World = worlds[0];
                chapter.WorldName = worlds[0].Id;
            }

            token.ThrowIfCancellationRequested();

            // only Chapter command is processed here, rest is on plugins
            if (command.Tag != CommandIds.Chapter) return chapter;

            if (command.NamedParameters != null) // apply named parameters
            {
                try
                {
                    var namedParam = command.NamedParameters?.FirstOrDefault(x => x.Name == CommandIds.ChapterCommand.Title);
                    if (namedParam != null) chapter.Title = namedParam.Value;

                    namedParam = command.NamedParameters?.FirstOrDefault(x => x.Name == CommandIds.ChapterCommand.Reference);
                    if (namedParam != null) chapter.Reference = namedParam.Value;
                }
                catch (Exception ex) // dirty trick to pick validation error
                {
                    throw new CommandValidationException(command, ex.Message);
                }
            }

            token.ThrowIfCancellationRequested();

            if (!string.IsNullOrEmpty(command.Comments)) chapter.Summary = command.Comments;

            return chapter;
        }

        private async Task<Page> ExecuteCommand_Page(Command command, WorldSvcDBContext dbContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var pages = await dbContext.Pages.OfTale(command).Where(x => x.Id == command.Target).ToArrayAsync(token);
            if (pages.Length > 1) throw new CommandExecutionException(command, "More than one page found with unique key");
            Page page = null!;
            if (pages.Length == 1)
            {
                page = pages[0];
                page.LastUpdatedChapter = command.ChapterId;
                page.LastUpdatedPageInChapter = command.PageId;
                page.LastUpdate = command.OperationTime;
                page.WriterId = command.WriterId;
            }
            else
            {
                if (string.IsNullOrEmpty(command.Parent)) throw new CommandValidationException(command, "Page command has no parent");
                var chapters = await dbContext.Chapters.OfTale(command).Where(x => x.Id == command.Parent).ToArrayAsync(token);
                if (chapters.Length > 1) throw new CommandExecutionException(command, "More than one chapter found with unique key");
                if (chapters.Length == 0) throw new CommandExecutionBlockedException(command, "Owner chapter for page not found");

                if (command.Tag != CommandIds.Page) throw new CommandExecutionBlockedException(command, "Page entity must exist before executing a plugin command");

                page = new Page
                {
                    TaleId = command.TaleId,
                    TaleVersionId = command.TaleVersionId,
                    Id = command.Target,
                    LastUpdatedChapter = command.ChapterId,
                    LastUpdatedPageInChapter = command.PageId,
                    LastUpdate = command.OperationTime,
                    WriterId = command.WriterId,
                    IsNew = true
                };
                dbContext.Pages.Add(page);

                page.Owner = chapters[0];
                page.ChapterId = chapters[0].Id;
            }

            token.ThrowIfCancellationRequested();

            // only Page command is processed here, rest is on plugins
            if (command.Tag != CommandIds.Page) return page;

            if (command.NamedParameters != null) // apply named parameters
            {
                foreach (var namedParam in command.NamedParameters)
                {
                    token.ThrowIfCancellationRequested();

                    try
                    {
                        switch (namedParam.Name)
                        {
                            case CommandIds.PageCommand.MetActors:
                            case CommandIds.PageCommand.MetPersons:
                                break; // these two are handled elsewhere, by actor and person objects
                            case CommandIds.PageCommand.Date:
                                page.StartDate = namedParam.Value.ToLong();
                                break;
                            case CommandIds.PageCommand.Location:
                                var result = await ParseLocation(namedParam, command, dbContext, token)
                                    ?? throw new CommandValidationException(command, "Page location could not be determined");
                                page.Location = result;
                                break;
                            case CommandIds.PageCommand.Stay:
                                page.StayAtLocation = namedParam.Value.ToLong();
                                break;
                            case CommandIds.PageCommand.Travel:
                                var destination = await ParseLocation(namedParam, command, dbContext, token)
                                    ?? throw new CommandValidationException(command, "Page travel destination could not be determined");
                                page.Travel ??= new Journey();
                                page.Travel.Destination = destination;
                                break;
                            case CommandIds.PageCommand.Voyage:
                                page.Travel ??= new Journey();
                                page.Travel.Duration = namedParam.Value.ToLong();
                                break;
                            default: continue; // other parameters are plugin stuff
                        }
                    }
                    catch (CommandExecutionBlockedException) // this happens sometimes if settlement is not there yet
                    {
                        throw;
                    }
                    catch (Exception ex) // dirty trick to pick validation error
                    {
                        throw new CommandValidationException(command, ex.Message);
                    }
                }
            }

            if (!string.IsNullOrEmpty(command.Comments)) page.Notes = command.Comments;

            return page;
        }

        public async Task<Location?> ParseLocation(NamedParameter param, Command command, WorldSvcDBContext dbContext, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(nameof(param));
            ArgumentNullException.ThrowIfNull(nameof(command));
            ArgumentNullException.ThrowIfNull(nameof(dbContext));
            token.ThrowIfCancellationRequested();

            var args = param.GetArray(',', s => s);
            Location? result = null;

            if (args.Length == 2) result = new Location { Settlement = args[0], Extension = args[1] };
            else if (args.Length == 1) result = new Location { Settlement = args[0], Extension = null };
            else throw new CommandValidationException(command, "Location value is not acceptable");

            var doesSettlementExist = await dbContext.Settlements.OfTale(command).Where(x => x.Id == result.Settlement).CountAsync(token);
            if (doesSettlementExist == 0) // here is an example of concurrency issue. maybe settlement command already exists but not yet executed due to ordering or any other reason. we still throw exc but a different one, this is not a validation exc now
                throw new CommandExecutionBlockedException(command, $"Settlement {args[0]} does not exist");

            token.ThrowIfCancellationRequested();
            return result;
        }
    }

    [GenerateSerializer]
    public class WorldGrainState : EntityGrainStateBase
    {
    }
}

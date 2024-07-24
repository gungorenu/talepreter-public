using Microsoft.EntityFrameworkCore;
using Orleans.Runtime;
using Talepreter.AnecdoteSvc.DBContext;
using Talepreter.BaseTypes;
using Talepreter.Contracts.Orleans.Execute;
using Talepreter.Contracts.Orleans.Grains;
using Talepreter.Contracts.Orleans.Grains.Entities;
using Talepreter.Exceptions;
using Talepreter.Common;
using Talepreter.Operations.Grains;
using Talepreter.Operations;
using Talepreter.Operations.Grains.States;
using Talepreter.Common.RabbitMQ.Interfaces;

namespace Talepreter.AnecdoteSvc.Grains
{
    public class AnecdoteGrain : EntityGrain<AnecdoteGrainState, AnecdoteSvcDBContext, IAnecdoteGrain>, IAnecdoteGrain
    {
        public AnecdoteGrain([PersistentState("persistentState", "AnecdoteSvcStorage")] IPersistentState<AnecdoteGrainState> persistentState,
            ILogger<AnecdoteGrain> logger,
            IPublisher publisher,
            ITalepreterServiceIdentifier serviceId)
            : base(persistentState, logger, publisher, serviceId)
        {
        }

        protected override IContainerGrain FetchContainer(CommandId commandId) => GrainFactory.FetchAnecdoteContainer(commandId.TaleId, commandId.TaleVersionId);

        protected override async Task<EntityDbBase> ExecuteTriggerCommand(Command command, AnecdoteSvcDBContext dbContext, CancellationToken token)
        {
            // in Talepreter itself there is nothing to expire about anecdotes, but this may be required for plugins, and we need to return an anecdote instance back for them
            // otherwise there is nothing special here
            var anecdotes = await dbContext.Anecdotes.OfTale(command).Where(x => x.Id == command.Target).ToArrayAsync(token);
            if (anecdotes.Length > 1) throw new CommandExecutionException(command, "More than one anecdote found with unique key");
            if (anecdotes.Length < 1) throw new CommandExecutionException(command, "Anecdote not found for the trigger");
            return anecdotes[0];
        }

        protected override async Task<EntityDbBase> ExecuteCommand(Command command, AnecdoteSvcDBContext dbContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var anecdotes = await dbContext.Anecdotes.OfTale(command).Where(x => x.Id == command.Target).ToArrayAsync(token);
            if (anecdotes.Length > 1) throw new CommandExecutionException(command, "More than one anecdote found with unique key");
            Anecdote anecdote = null!;
            if (anecdotes.Length == 1)
            {
                anecdote = anecdotes[0];
                anecdote.LastUpdatedChapter = command.ChapterId;
                anecdote.LastUpdatedPageInChapter = command.PageId;
                anecdote.LastUpdate = command.OperationTime;
                anecdote.WriterId = command.WriterId;
                if (!string.IsNullOrEmpty(command.Parent) && anecdote.ParentId != command.Parent) throw new CommandExecutionBlockedException(command, $"Anecdote parent cannot be changed");
            }
            else
            {
                if (command.Tag != CommandIds.Anecdote) throw new CommandExecutionException(command, "Anecdote entity must exist before executing a plugin command");

                anecdote = new Anecdote
                {
                    TaleId = command.TaleId,
                    TaleVersionId = command.TaleVersionId,
                    Id = command.Target,
                    LastUpdatedChapter = command.ChapterId,
                    LastUpdatedPageInChapter = command.PageId,
                    LastUpdate = command.OperationTime,
                    WriterId = command.WriterId,
                    Entries = new MentionEntryMetadata { List = new List<MentionEntry>() },
                    IsNew = true
                };
                dbContext.Anecdotes.Add(anecdote);

                // set parent
                if (command.Parent != null)
                {
                    var parentAnecdotes = await dbContext.Anecdotes.OfTale(command).Where(x => x.Id == command.Parent).ToArrayAsync(token);
                    if (parentAnecdotes.Length > 1) throw new CommandExecutionException(command, "More than one anecdote found as parent with unique key");
                    if (parentAnecdotes.Length == 0) throw new CommandExecutionBlockedException(command, $"Anecdote {command.Parent} does not exist");
                    anecdote.Parent = parentAnecdotes[0];
                    anecdote.ParentId = parentAnecdotes[0].Id;
                }
            }

            token.ThrowIfCancellationRequested();

            // only Anecdote command is processed here, rest is on plugins
            if (command.Tag != CommandIds.Anecdote) return anecdote;

            long date = 0L;
            Location? location = null!;
            bool overwrite = false;
            if (command.NamedParameters != null) // apply named parameters
            {
                foreach (var namedParam in command.NamedParameters)
                {
                    token.ThrowIfCancellationRequested();

                    switch (namedParam.Name)
                    {
                        case CommandIds.AnecdoteCommand.Date:
                            date = namedParam.Value.ToLong();
                            break;
                        case CommandIds.AnecdoteCommand.Location:
                            location = await namedParam.ParseLocation(command, dbContext, token);
                            break;
                        case CommandIds.AnecdoteCommand.Overwrite:
                            overwrite = true;
                            break;
                        default: continue;
                    }
                }
            }

            // overwrite means we overwrite all entries
            if (overwrite) anecdote.Entries.List.Clear();

            token.ThrowIfCancellationRequested();

            // use case for below is like an event some actors learned/witnessed but then sharing it with others in further pages, which could be in same chapter, then the notes would be merged as much as possible. visualization will be cluttered with so many entries "I think"
            if (!string.IsNullOrEmpty(command.Comments))
            {
                // validate actors, if they really exist or not
                if (command.ArrayParameters != null && command.ArrayParameters?.Length > 0)
                {
                    var actorList = command.ArrayParameters?.ToArray() ?? [];
                    var actors = await dbContext.PluginRecords.OfTale(command).Where(x => x.Type == CommandIds.Actor && actorList.Contains(x.BaseId))
                        .Select(x => x.BaseId).ToArrayAsync(token);
                    if (actorList.Any(x => !actors.Contains(x))) throw new CommandExecutionBlockedException(command, $"Some mentioned actors do not exist");
                }

                var entry = anecdote.Entries.List.FirstOrDefault(x => x.Chapter == command.ChapterId); // we merge entries from same chapter
                if (entry != null) // merge entries
                {
                    entry.Content = $"{entry.Content}\r\n{command.Comments}".Trim(); // merge, but can be empty
                    entry.Page = command.PageId; // this is a decision I took, we only merge same chapter stuff but adding two commands in same page made little sense for me, so I merge same chapter records
                }
                else // new entry
                {
                    entry = new MentionEntry
                    {
                        Content = command.Comments,
                        Chapter = command.ChapterId,
                        Actors = [],
                        Page = command.PageId
                    };
                    anecdote.Entries.List.Add(entry);
                }

                entry.Location = location ?? entry.Location;
                if (date != 0L)
                {
                    if (entry.Date != null && entry.Date > date) throw new CommandExecutionException(command, "Anecdote mention date cannot be in past");
                    entry.Date = date;
                }

                // merge actor entries if there is any
                var entryActors = new List<string>();
                if (command.ArrayParameters != null)
                {
                    foreach (var actor in command.ArrayParameters) if (!entry.Actors.Contains(actor)) entryActors.Add(actor);
                    entry.Actors = [.. entryActors];
                }
            }

            return anecdote;
        }
    }

    [GenerateSerializer]
    public class AnecdoteGrainState : EntityGrainStateBase
    {
    }
}

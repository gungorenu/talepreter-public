using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Talepreter.Common;
using Talepreter.DB.Common;

namespace Talepreter.BaseTypes
{
    public static class Extensions
    {
        public static ModelBuilder RegisterEntityDBBase<T>(this ModelBuilder builder)
            where T : EntityDbBase
        {
            builder.Entity<T>().HasKey(p => new { p.TaleId, p.TaleVersionId, p.Id });
            builder.Entity<T>().Property(p => p.Id).ValueGeneratedNever().IsRequired(true);
            builder.Entity<T>().Property(p => p.TaleId).IsRequired(true);
            builder.Entity<T>().Property(p => p.TaleVersionId).IsRequired(true);
            builder.Entity<T>().Property(p => p.WriterId).IsRequired(true);
            builder.Entity<T>().Property(p => p.LastUpdatedChapter).IsRequired(true);
            builder.Entity<T>().Property(p => p.LastUpdatedPageInChapter).IsRequired(true);
            builder.Entity<T>().Property(p => p.PublishState).IsRequired(true);
            builder.Entity<T>().Ignore(p => p.EntityContainer);
            builder.Entity<T>().Ignore(p => p.IsNew);
            builder.Entity<T>().HasIndex(p => p.TaleId);
            builder.Entity<T>().HasIndex(p => new { p.TaleId, p.TaleVersionId });
            builder.Entity<T>().HasIndex(p => new { p.TaleId, p.TaleVersionId, p.Id, p.PublishState });
            return builder;
        }

        public static ModelBuilder RegisterExpiringEntityDBBase<T>(this ModelBuilder builder)
            where T : ExpiringEntityDbBase
        {
            builder.RegisterEntityDBBase<T>();
            builder.Entity<T>().HasIndex(p => new { p.TaleVersionId, p.ExpireState, p.ExpiresAt }).HasFilter("[ExpiresAt] IS NOT NULL");
            return builder;
        }

        public static ModelBuilder RegisterExpandedEntity<T>(this ModelBuilder builder)
            where T : class, IExpandedEntity
        {
            builder.Entity<T>().Property(a => a.PluginData).HasConversion(
                v => JsonSerializer.Serialize(v, Container.SerializationOptions),
                v => JsonSerializer.Deserialize<Container>(v, Container.SerializationOptions) ?? new Container());
            return builder;
        }

        public static ModelBuilder RegisterTriggerData(this ModelBuilder builder)
        {
            builder.Entity<Trigger>().HasKey(p => new { p.TaleId, p.TaleVersionId, p.Id });
            builder.Entity<Trigger>().Property(p => p.Id).ValueGeneratedNever().IsRequired(true);
            builder.Entity<Trigger>().Property(p => p.TaleId).IsRequired(true);
            builder.Entity<Trigger>().Property(p => p.TaleVersionId).IsRequired(true);
            builder.Entity<Trigger>().Property(p => p.WriterId).IsRequired(true);
            builder.Entity<Trigger>().Property(p => p.GrainType).IsRequired(true);
            builder.Entity<Trigger>().Property(p => p.GrainId).IsRequired(true);
            builder.Entity<Trigger>().HasIndex(p => p.TaleId);
            builder.Entity<Trigger>().HasIndex(p => new { p.TaleId, p.TaleVersionId, p.State, p.TriggerAt });
            builder.Entity<Trigger>().HasIndex(p => new { p.TaleId, p.TaleVersionId, p.Id, p.Type });
            return builder;
        }

        public static ModelBuilder RegisterExtensionData(this ModelBuilder builder)
        {
            builder.RegisterEntityDBBase<ExtensionData>();
            builder.Entity<ExtensionData>().Property(p => p.BaseId).IsRequired(true);
            builder.Entity<ExtensionData>().Property(p => p.Type).IsRequired(true);
            builder.RegisterExpandedEntity<ExtensionData>();
            builder.Entity<ExtensionData>().HasIndex(p => new { p.TaleId, p.TaleVersionId, p.Type });
            builder.Entity<ExtensionData>().HasIndex(p => new { p.TaleId, p.TaleVersionId, p.BaseId, p.Type, p.PublishState });
            return builder;
        }

        public static ModelBuilder RegisterCommandExecuteTaskEntity(this ModelBuilder builder)
        {
            builder.Entity<Command>().HasKey(p => new { p.TaleId, p.TaleVersionId, p.ChapterId, p.PageId, p.Index, p.Phase, p.SubIndex });
            builder.Entity<Command>().Property(p => p.GrainId).IsRequired(true);
            builder.Entity<Command>().Property(p => p.TaleId).IsRequired(true);
            builder.Entity<Command>().Property(p => p.TaleVersionId).IsRequired(true);
            builder.Entity<Command>().Property(p => p.WriterId).IsRequired(true);
            builder.Entity<Command>().Property(p => p.OperationTime).IsRequired(true);
            builder.Entity<Command>().Property(p => p.ChapterId).IsRequired(true);
            builder.Entity<Command>().Property(p => p.PageId).IsRequired(true);
            builder.Entity<Command>().Property(p => p.Phase).IsRequired(true);

            builder.Entity<Command>().Property(p => p.GrainId).IsRequired(true);
            builder.Entity<Command>().Property(p => p.GrainType).IsRequired(true);

            builder.Entity<Command>().Property(p => p.Index).IsRequired(true);
            builder.Entity<Command>().Property(p => p.SubIndex).HasDefaultValueSql("NEXT VALUE FOR shared.SubIndexSequence");

            builder.Entity<Command>().Property(p => p.Tag).IsRequired(true);
            builder.Entity<Command>().Property(p => p.Target).IsRequired(true);
            builder.Entity<Command>().OwnsMany(p => p.NamedParameters, navigation => navigation.ToJson());

            builder.Entity<Command>().Property(p => p.Result).IsRequired(true);
            builder.Entity<Command>().Property(p => p.Attempts).IsRequired(true);

            builder.Entity<Command>().HasIndex(p => new { p.TaleId, p.TaleVersionId, p.ChapterId, p.PageId, p.Phase });
            builder.Entity<Command>().Ignore(p => p.CalculatedIndex);

            return builder;
        }

        public static async Task PurgeEntities<T>(this DbSet<T> dbSet, Guid taleId, Guid? taleVersionId, CancellationToken token)
            where T : EntityDbBase
        {
            if (taleVersionId == null) await dbSet.Where(x => x.TaleId == taleId).ExecuteDeleteAsync(token);
            else await dbSet.Where(x => x.TaleId == taleId && x.TaleVersionId == taleVersionId).ExecuteDeleteAsync(token);
        }

        public static async Task PurgeCommands(this DbSet<Command> dbSet, Guid taleId, Guid? taleVersionId, CancellationToken token)
        {
            if (taleVersionId == null) await dbSet.Where(x => x.TaleId == taleId).ExecuteDeleteAsync(token);
            else await dbSet.Where(x => x.TaleId == taleId && x.TaleVersionId == taleVersionId).ExecuteDeleteAsync(token);
        }

        public static async Task PurgeTriggers(this DbSet<Trigger> dbSet, Guid taleId, Guid? taleVersionId, CancellationToken token)
        {
            if (taleVersionId == null) await dbSet.Where(x => x.TaleId == taleId).ExecuteDeleteAsync(token);
            else await dbSet.Where(x => x.TaleId == taleId && x.TaleVersionId == taleVersionId).ExecuteDeleteAsync(token);
        }

        public static async Task ResetPublishState<T>(this DbSet<T> dbSet, Guid taleId, Guid? taleVersionId, CancellationToken token)
            where T : EntityDbBase
        {
            if (taleVersionId == null) await dbSet.Where(x => x.TaleId == taleId && x.PublishState != PublishState.Skipped).
                    ExecuteUpdateAsync(x => x.SetProperty(k => k.PublishState, PublishState.None), token);
            else await dbSet.Where(x => x.TaleId == taleId && x.TaleVersionId == taleVersionId && x.PublishState != PublishState.Skipped).
                    ExecuteUpdateAsync(x => x.SetProperty(k => k.PublishState, PublishState.None), token);
        }

        public static Trigger MapTrigger<TGrain>(this Command command, string id, long triggerAt, string target, string grainId, string type, string? parameter)
            where TGrain : IGrain
        {
            return new Trigger
            {
                Id = id,
                TaleId = command.TaleId,
                TaleVersionId = command.TaleVersionId,
                WriterId = command.WriterId,
                LastUpdate = command.OperationTime,
                State = TriggerState.Set,
                TriggerAt = triggerAt,
                Target = target,
                GrainId = grainId,
                GrainType = typeof(TGrain).Name,
                Type = type,
                Parameter = parameter
            };
        }

        public static IQueryable<T> OfTale<T>(this DbSet<T> dbSet, ITaleIdentifier taleInfo) where T : EntityDbBase
            => dbSet.Where(x => x.TaleId == taleInfo.TaleId && x.TaleVersionId == taleInfo.TaleVersionId);
    }
}

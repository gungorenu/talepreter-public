using Talepreter.Contracts.Orleans.Grains.Containers;
using Talepreter.Contracts.Orleans.Grains.Entities;
using Talepreter.Contracts.Orleans.Grains.Plugins;
using Talepreter.Exceptions;

namespace Talepreter.Contracts.Orleans.Grains
{
    public static class GrainFetcher
    {
        public static ITaleGrain FetchTale(this IGrainFactory factory, Guid taleId)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<ITaleGrain> Tale id is empty guid");
            var grain = factory.GetGrain<ITaleGrain>(FetchTale(taleId));
            return grain;
        }

        public static IPublishGrain FetchPublish(this IGrainFactory factory, Guid taleId, Guid taleVersionId)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IPublishGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IPublishGrain> Tale version id is empty guid");
            var grain = factory.GetGrain<IPublishGrain>(FetchPublish(taleId, taleVersionId));
            return grain;
        }

        public static IChapterGrain FetchChapter(this IGrainFactory factory, Guid taleId, Guid taleVersionId, int chapterId)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IChapterGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IChapterGrain> Tale version id is empty guid");
            if (chapterId < 0) throw new GrainIdException("<IChapterGrain> Chapter id is negative number");
            var grain = factory.GetGrain<IChapterGrain>(FetchChapter(taleId, taleVersionId, chapterId));
            return grain;
        }

        public static IPageGrain FetchPage(this IGrainFactory factory, Guid taleId, Guid taleVersionId, int chapterId, int pageId)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IPageGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IPageGrain> Tale version id is empty guid");
            if (chapterId < 0) throw new GrainIdException("<IPageGrain> Chapter id is negative number");
            if (pageId < 0) throw new GrainIdException("<IPageGrain> Page id is negative number");
            var grain = factory.GetGrain<IPageGrain>(FetchPage(taleId, taleVersionId, chapterId, pageId));
            return grain;
        }

        // --

        public static IActorContainerGrain FetchActorContainer(this IGrainFactory factory, Guid taleId, Guid taleVersionId)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IActorContainerGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IActorContainerGrain> Tale version id is empty guid");
            var grain = factory.GetGrain<IActorContainerGrain>(FetchActorContainer(taleId, taleVersionId));
            return grain;
        }

        public static IAnecdoteContainerGrain FetchAnecdoteContainer(this IGrainFactory factory, Guid taleId, Guid taleVersionId)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IAnecdoteContainerGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IAnecdoteContainerGrain> Tale version id is empty guid");
            var grain = factory.GetGrain<IAnecdoteContainerGrain>(FetchAnecdoteContainer(taleId, taleVersionId));
            return grain;
        }

        public static IPersonContainerGrain FetchPersonContainer(this IGrainFactory factory, Guid taleId, Guid taleVersionId)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IPersonContainerGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IPersonContainerGrain> Tale version id is empty guid");
            var grain = factory.GetGrain<IPersonContainerGrain>(FetchPersonContainer(taleId, taleVersionId));
            return grain;
        }

        public static IWorldContainerGrain FetchWorldContainer(this IGrainFactory factory, Guid taleId, Guid taleVersionId)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IWorldContainerGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IWorldContainerGrain> Tale version id is empty guid");
            var grain = factory.GetGrain<IWorldContainerGrain>(FetchWorldContainer(taleId, taleVersionId));
            return grain;
        }

        // --

        public static IActorPluginGrain FetchActorPlugin(this IGrainFactory factory, Guid taleId, Guid taleVersionId, string tag, string target)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IActorPluginGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IActorPluginGrain> Tale version id is empty guid");
            if (string.IsNullOrEmpty(tag)) throw new GrainIdException("<IActorPluginGrain> Tag is empty string or null");
            if (string.IsNullOrEmpty(target)) throw new GrainIdException("<IActorPluginGrain> Target is empty string or null");
            var grain = factory.GetGrain<IActorPluginGrain>(FetchActorPlugin(taleId, taleVersionId, tag, target));
            return grain;
        }

        public static IAnecdotePluginGrain FetchAnecdotePlugin(this IGrainFactory factory, Guid taleId, Guid taleVersionId, string tag, string target)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IAnecdotePluginGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IAnecdotePluginGrain> Tale version id is empty guid");
            if (string.IsNullOrEmpty(tag)) throw new GrainIdException("<IAnecdotePluginGrain> Tag is empty string or null");
            if (string.IsNullOrEmpty(target)) throw new GrainIdException("<IAnecdotePluginGrain> Target is empty string or null");
            var grain = factory.GetGrain<IAnecdotePluginGrain>(FetchAnecdotePlugin(taleId, taleVersionId, tag, target));
            return grain;
        }

        public static IPersonPluginGrain FetchPersonPlugin(this IGrainFactory factory, Guid taleId, Guid taleVersionId, string tag, string target)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IPersonPluginGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IPersonPluginGrain> Tale version id is empty guid");
            if (string.IsNullOrEmpty(tag)) throw new GrainIdException("<IPersonPluginGrain> Tag is empty string or null");
            if (string.IsNullOrEmpty(target)) throw new GrainIdException("<IPersonPluginGrain> Target is empty string or null");
            var grain = factory.GetGrain<IPersonPluginGrain>(FetchPersonPlugin(taleId, taleVersionId, tag, target));
            return grain;
        }

        public static IWorldPluginGrain FetchWorldPlugin(this IGrainFactory factory, Guid taleId, Guid taleVersionId, string tag, string target)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IWorldPluginGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IWorldPluginGrain> Tale version id is empty guid");
            if (string.IsNullOrEmpty(tag)) throw new GrainIdException("<IWorldPluginGrain> Tag is empty string or null");
            if (string.IsNullOrEmpty(target)) throw new GrainIdException("<IWorldPluginGrain> Target is empty string or null");
            var grain = factory.GetGrain<IWorldPluginGrain>(FetchWorldPlugin(taleId, taleVersionId, tag, target));
            return grain;
        }

        // --

        public static IActorGrain FetchActor(this IGrainFactory factory, Guid taleId, Guid taleVersionId, string target)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IActorGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IActorGrain> Tale version id is empty guid");
            if (string.IsNullOrEmpty(target)) throw new GrainIdException("<IActorGrain> Target is empty string or null");
            var grain = factory.GetGrain<IActorGrain>(FetchActor(taleId, taleVersionId, target));
            return grain;
        }

        public static IActorTraitGrain FetchActorTrait(this IGrainFactory factory, Guid taleId, Guid taleVersionId, string target)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IActorTraitGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IActorTraitGrain> Tale version id is empty guid");
            if (string.IsNullOrEmpty(target)) throw new GrainIdException("<IActorTraitGrain> Target is empty string or null");
            var grain = factory.GetGrain<IActorTraitGrain>(FetchActorTrait(taleId, taleVersionId, target));
            return grain;
        }

        public static IAnecdoteGrain FetchAnecdote(this IGrainFactory factory, Guid taleId, Guid taleVersionId, string target)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IAnecdoteGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IAnecdoteGrain> Tale version id is empty guid");
            if (string.IsNullOrEmpty(target)) throw new GrainIdException("<IAnecdoteGrain> Target is empty string or null");
            var grain = factory.GetGrain<IAnecdoteGrain>(FetchAnecdote(taleId, taleVersionId, target));
            return grain;
        }

        public static IPersonGrain FetchPerson(this IGrainFactory factory, Guid taleId, Guid taleVersionId, string target)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IPersonGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IPersonGrain> Tale version id is empty guid");
            if (string.IsNullOrEmpty(target)) throw new GrainIdException("<IPersonGrain> Target is empty string or null");
            var grain = factory.GetGrain<IPersonGrain>(FetchPerson(taleId, taleVersionId, target));
            return grain;
        }

        public static ISettlementGrain FetchSettlement(this IGrainFactory factory, Guid taleId, Guid taleVersionId, string target)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<ISettlementGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<ISettlementGrain> Tale version id is empty guid");
            if (string.IsNullOrEmpty(target)) throw new GrainIdException("<ISettlementGrain> Target is empty string or null");
            var grain = factory.GetGrain<ISettlementGrain>(FetchSettlement(taleId, taleVersionId, target));
            return grain;
        }

        public static IWorldGrain FetchWorld(this IGrainFactory factory, Guid taleId, Guid taleVersionId)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IWorldGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IWorldGrain> Tale version id is empty guid");
            var grain = factory.GetGrain<IWorldGrain>(FetchWorld(taleId, taleVersionId));
            return grain;
        }

        // -- ID preparers

        public static Guid FetchTale(Guid taleId)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<ITaleGrain> Tale id is empty guid");
            return taleId;
        }

        public static Guid FetchPublish(Guid taleId, Guid taleVersionId)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IPublishGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IPublishGrain> Tale version id is empty guid");
            return taleVersionId;
        }

        public static string FetchChapter(Guid taleId, Guid taleVersionId, int chapterId)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IChapterGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IChapterGrain> Tale version id is empty guid");
            if (chapterId < 0) throw new GrainIdException("<IChapterGrain> Chapter id is negative number");
            return $"{taleVersionId}\\CHAPTER:{chapterId}";
        }

        public static string FetchPage(Guid taleId, Guid taleVersionId, int chapterId, int pageId)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IPageGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IPageGrain> Tale version id is empty guid");
            if (chapterId < 0) throw new GrainIdException("<IPageGrain> Chapter id is negative number");
            if (pageId < 0) throw new GrainIdException("<IPageGrain> Page id is negative number");
            return $"{taleVersionId}\\PAGE:{chapterId}#{pageId}";
        }

        // --

        public static string FetchActorContainer(Guid taleId, Guid taleVersionId)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IActorContainerGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IActorContainerGrain> Tale version id is empty guid");
            return $"{taleVersionId}\\ActorContainer";
        }

        public static string FetchAnecdoteContainer(Guid taleId, Guid taleVersionId)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IAnecdoteContainerGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IAnecdoteContainerGrain> Tale version id is empty guid");
            return $"{taleVersionId}\\AnecdoteContainer";
        }

        public static string FetchPersonContainer(Guid taleId, Guid taleVersionId)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IPersonContainerGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IPersonContainerGrain> Tale version id is empty guid");
            return $"{taleVersionId}\\PersonContainer";
        }

        public static string FetchWorldContainer(Guid taleId, Guid taleVersionId)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IWorldContainerGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IWorldContainerGrain> Tale version id is empty guid");
            return $"{taleVersionId}\\WorldContainer";
        }

        // --

        public static string FetchActorPlugin(Guid taleId, Guid taleVersionId, string tag, string target)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IActorPluginGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IActorPluginGrain> Tale version id is empty guid");
            if (string.IsNullOrEmpty(tag)) throw new GrainIdException("<IActorPluginGrain> Tag is empty string or null");
            if (string.IsNullOrEmpty(target)) throw new GrainIdException("<IActorPluginGrain> Target is empty string or null");
            return $"{taleVersionId}\\!{tag}:{target}";
        }

        public static string FetchAnecdotePlugin(Guid taleId, Guid taleVersionId, string tag, string target)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IAnecdotePluginGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IAnecdotePluginGrain> Tale version id is empty guid");
            if (string.IsNullOrEmpty(tag)) throw new GrainIdException("<IAnecdotePluginGrain> Tag is empty string or null");
            if (string.IsNullOrEmpty(target)) throw new GrainIdException("<IAnecdotePluginGrain> Target is empty string or null");
            return $"{taleVersionId}\\!{tag}:{target}";
        }

        public static string FetchPersonPlugin(Guid taleId, Guid taleVersionId, string tag, string target)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IPersonPluginGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IPersonPluginGrain> Tale version id is empty guid");
            if (string.IsNullOrEmpty(tag)) throw new GrainIdException("<IPersonPluginGrain> Tag is empty string or null");
            if (string.IsNullOrEmpty(target)) throw new GrainIdException("<IPersonPluginGrain> Target is empty string or null");
            return $"{taleVersionId}\\!{tag}:{target}";
        }

        public static string FetchWorldPlugin(Guid taleId, Guid taleVersionId, string tag, string target)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IWorldPluginGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IWorldPluginGrain> Tale version id is empty guid");
            if (string.IsNullOrEmpty(tag)) throw new GrainIdException("<IWorldPluginGrain> Tag is empty string or null");
            if (string.IsNullOrEmpty(target)) throw new GrainIdException("<IWorldPluginGrain> Target is empty string or null");
            return $"{taleVersionId}\\!{tag}:{target}";
        }

        // --

        public static string FetchActor(Guid taleId, Guid taleVersionId, string target)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IActorGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IActorGrain> Tale version id is empty guid");
            if (string.IsNullOrEmpty(target)) throw new GrainIdException("<IActorGrain> Target is empty string or null");
            return $"{taleVersionId}\\ACTOR:{target}";
        }

        public static string FetchActorTrait(Guid taleId, Guid taleVersionId, string target)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IActorTraitGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IActorTraitGrain> Tale version id is empty guid");
            if (string.IsNullOrEmpty(target)) throw new GrainIdException("<IActorTraitGrain> Target is empty string or null");
            return $"{taleVersionId}\\ACTORTRAIT:{target}";
        }

        public static string FetchAnecdote(Guid taleId, Guid taleVersionId, string target)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IAnecdoteGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IAnecdoteGrain> Tale version id is empty guid");
            if (string.IsNullOrEmpty(target)) throw new GrainIdException("<IAnecdoteGrain> Target is empty string or null");
            return $"{taleVersionId}\\ANECDOTE:{target}";
        }

        public static string FetchPerson(Guid taleId, Guid taleVersionId, string target)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IPersonGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IPersonGrain> Tale version id is empty guid");
            if (string.IsNullOrEmpty(target)) throw new GrainIdException("<IPersonGrain> Target is empty string or null");
            return $"{taleVersionId}\\PERSON:{target}";
        }

        public static string FetchSettlement(Guid taleId, Guid taleVersionId, string target)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<ISettlementGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<ISettlementGrain> Tale version id is empty guid");
            if (string.IsNullOrEmpty(target)) throw new GrainIdException("<ISettlementGrain> Target is empty string or null");
            return $"{taleVersionId}\\SETTLEMENT:{target}";
        }

        public static string FetchWorld(Guid taleId, Guid taleVersionId)
        {
            if (Guid.Empty == taleId) throw new GrainIdException("<IWorldGrain> Tale id is empty guid");
            if (Guid.Empty == taleVersionId) throw new GrainIdException("<IWorldGrain> Tale version id is empty guid");
            return $"{taleVersionId}\\WORLD";
        }
    }
}

using Talepreter.Common;
using Talepreter.Contracts;

namespace Talepreter.Operations
{
    public static class EnumExtensions
    {
        public static ResponsibleService Map(this ServiceId self)
            => self switch
            {
                ServiceId.ActorSvc => ResponsibleService.ActorSvc,
                ServiceId.AnecdoteSvc => ResponsibleService.AnecdoteSvc,
                ServiceId.PersonSvc => ResponsibleService.PersonSvc,
                ServiceId.WorldSvc => ResponsibleService.WorldSvc,
                _ => throw new InvalidOperationException($"Value {self} cannot be mapped to type {nameof(ResponsibleService)}")
            };
    }
}

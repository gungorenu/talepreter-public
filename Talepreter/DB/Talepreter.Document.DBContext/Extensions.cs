using Microsoft.Extensions.DependencyInjection;

namespace Talepreter.Document.DBContext
{
    public static class Extensions
    {
        public static IServiceCollection RegisterDocumentDB(this IServiceCollection self)
        {
            self.AddSingleton<IDocumentDBContext, DocumentDBContext>();
            return self;
        }
    }
}

namespace Talepreter.Document.DBContext
{
    public interface IDocumentDBContext
    {
        Task Put<T>(T document, CancellationToken token) where T : EntityBase;

        Task PurgeTale(Guid taleId, CancellationToken token);
        
        Task PurgePublish(Guid taleId, Guid taleVersionId, CancellationToken token);

        Task Setup();
    }
}

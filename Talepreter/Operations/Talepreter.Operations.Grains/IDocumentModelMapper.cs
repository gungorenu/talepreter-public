using Talepreter.BaseTypes;
using Talepreter.Document.DBContext;

namespace Talepreter.Operations.Grains
{
    public interface IDocumentModelMapper
    {
        EntityBase MapEntity(EntityDbBase entity);
    }
}

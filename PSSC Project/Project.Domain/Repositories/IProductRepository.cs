using LanguageExt;
using Project.Domain.Models;

namespace Project.Domain.Repositories
{
    public interface IProductRepository
    {
        TryAsync<List<EvaluatedProduct>> TryGetExistentProducts();
    }
}

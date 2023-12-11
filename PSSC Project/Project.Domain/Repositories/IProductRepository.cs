using LanguageExt;
using LanguageExt.ClassInstances;
using Project.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Repositories
{
    public interface IProductRepository
    {
        TryAsync<List<Product>> TryGetExistentProducts();
    }
}

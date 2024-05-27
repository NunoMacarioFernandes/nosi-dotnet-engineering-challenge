using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOS.Engineering.Challenge.Cache;

public interface ICacheService<T>
{
    Task<T?> GetAsync(Guid id);
    Task SetAsync(Guid id, T item);
    Task RemoveAsync(Guid id);
}

using eCommerce.API.Models;

namespace eCommerce.API.Repositories;

interface IRepository<T> where T : class
{
    public List<T> Get();
    public T Get(int id);
    public void Insert(T obj);
    public void Update(T obj);
    public void Delete(int id);
}

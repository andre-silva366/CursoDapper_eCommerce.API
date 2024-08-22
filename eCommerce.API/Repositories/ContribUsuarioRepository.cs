using Dapper.Contrib.Extensions;
using eCommerce.API.Models;
using System.Data;
using System.Data.SqlClient;

namespace eCommerce.API.Repositories;

public class ContribUsuarioRepository : IRepository<UsuarioContrib>
{
    private IDbConnection _connection;

    public ContribUsuarioRepository()
    {
       _connection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=eCommerceDapper;Integrated Security=True");
    }
    
    public List<UsuarioContrib> Get()
    {
        return _connection.GetAll<UsuarioContrib>().ToList();
    }

    public UsuarioContrib? Get(int id)
    {
       return _connection.Get<UsuarioContrib>(id);
    }

    public void Insert(UsuarioContrib usuario)
    {
        try
        {
            // Para retornar o usuario com o Id
            usuario.Id = Convert.ToInt32(_connection.Insert(usuario));
        }
        catch(Exception e)
        {
            throw new Exception($"Ocorreu um erro durante a operação, erro: {e.Message}");
        }
        
    }

    public void Update(UsuarioContrib usuario)
    {
        
        try
        {
           _connection.Update(usuario);
        }
        catch (Exception e)
        {            
            throw new Exception(e.Message);
        }
        
    }

    public void Delete(int id)
    {    
        // Não é possivel usar o id direto
        _connection.Delete(Get(id));
    }

}

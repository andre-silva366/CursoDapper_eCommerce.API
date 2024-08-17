using eCommerce.API.Models;

namespace eCommerce.API.Repositories;

public class UsuarioRepository : IRepository<Usuario>
{
    private static List<Usuario> _db = new()
    {
        new Usuario(){Id=1,Nome="Filipe Rodrigues",Email="filipe.rodrigues@gmail.com"},
        new Usuario(){Id=2,Nome="Marcelo Rodrigues",Email="marcelo.rodrigues@gmail.com"},
        new Usuario(){Id=3,Nome="Jéssica Rodrigues",Email="jessica.rodrigues@gmail.com"},
    };

    public List<Usuario> Get()
    {
        return _db;
    }

    public Usuario Get(int id)
    {
        return _db.FirstOrDefault(u => u.Id == id);
    }

    public void Insert(Usuario usuario)
    {
        var ultimoUsuario = _db.LastOrDefault();

        if(ultimoUsuario == null)
        {
            usuario.Id = 1;
        }
        else
        {
            usuario.Id = ultimoUsuario.Id + 1;
        }

        _db.Add(usuario);
    }

    public void Update(Usuario usuario)
    {
        _db.Remove( _db.FirstOrDefault(u => u.Id == usuario.Id));
        _db.Add(usuario);
    }

    public void Delete(int id)
    {
        _db.Remove(_db.FirstOrDefault(u => u.Id == id));
    }


}

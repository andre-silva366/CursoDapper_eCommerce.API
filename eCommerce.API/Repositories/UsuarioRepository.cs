using Dapper;
using eCommerce.API.Models;
using System.Data;
using System.Data.SqlClient;

namespace eCommerce.API.Repositories;

public class UsuarioRepository : IRepository<Usuario>
{
    private IDbConnection _connection;

    public UsuarioRepository()
    {
       _connection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=eCommerceDapper;Integrated Security=True");
    }
    
    // ADO.NET > Dapper > EF
    public List<Usuario> Get()
    {
        return _connection.Query<Usuario>("SELECT * FROM Usuarios").ToList();
    }

    public Usuario Get(int id)
    {
        return _connection.QuerySingleOrDefault<Usuario>("SELECT * FROM Usuarios WHERE Id = @Id",new {Id = id});
    }

    public void Insert(Usuario usuario)
    {
        string query = "INSERT INTO Usuarios(Nome, Email, Sexo, RG, CPF, NomeMae, SituacaoCadastro, DataCadastro) VALUES (@Nome, @Email, @Sexo, @RG, @CPF, @NomeMae, @SituacaoCadastro, @DataCadastro);SELECT CAST (SCOPE_IDENTITY() AS INT);";

        // Ao invés do usuario poderia ser um objeto anonimo
        usuario.Id = _connection.Query<int>(query,usuario).Single();
    }

    public void Update(Usuario usuario)
    {
        string query = "UPDATE Usuarios SET Nome = @Nome,Email = @Email,Sexo = @Sexo,RG = @RG, CPF = @CPF,NomeMae = @NomeMae,SituacaoCadastro = @SituacaoCadastro,DataCadastro = @DataCadastro WHERE Id = @Id";

        _connection.Execute(query, usuario);
    }

    public void Delete(int id)
    {
        //string query = $"DELETE Usuarios WHERE Id = {id}";
        //_connection.Execute(query);

        // Para proteger do SqlInjection
        _connection.Execute("DELETE Usuarios WHERE Id = @Id", new {Id = id});
    }

}

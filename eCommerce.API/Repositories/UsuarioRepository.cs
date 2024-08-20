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

    public Usuario? Get(int id)
    {
        return _connection.Query<Usuario,Contato,Usuario>("SELECT * FROM Usuarios as u LEFT JOIN Contatos AS c ON u.Id = c.UsuarioId WHERE U.Id = @Id;", (usuario,contato) => 
                        { 
                            usuario.Contato = contato;
                            return usuario; 
                        },
                        new {Id = id}).SingleOrDefault();
    }

    public void Insert(Usuario usuario)
    {
        _connection.Open();
        
        var transaction = _connection.BeginTransaction();

        try
        {
            // Insere apenas o usuario
            string queryUsuario = "INSERT INTO Usuarios(Nome, Email, Sexo, RG, CPF, NomeMae, SituacaoCadastro, DataCadastro) VALUES (@Nome, @Email, @Sexo, @RG, @CPF, @NomeMae, @SituacaoCadastro, @DataCadastro);SELECT CAST (SCOPE_IDENTITY() AS INT);";

            // Ao invés do usuario poderia ser um objeto anonimo
            usuario.Id = _connection.Query<int>(queryUsuario, usuario,transaction).Single();

            // Verificado se o usuario possui contato
            if (usuario.Contato != null)
            {
                // Adicionando o Id do usuario na tabela contato
                usuario.Contato.UsuarioId = usuario.Id;
                string queryContato = "INSERT INTO Contatos(UsuarioId,Telefone,Celular) VALUES (@UsuarioId, @Telefone, @Celular);" +
                    "SELECT CAST (SCOPE_IDENTITY() AS INT);";
                usuario.Contato.Id = _connection.Query<int>(queryContato, usuario.Contato,transaction).Single();
            }

            transaction.Commit();
        }
        catch (Exception e)
        {
            try
            {
                transaction.Rollback();
            }
            catch(Exception ex)
            {
                throw new Exception($"Ocorreu um erro durante a operação, erro: {ex.Message}");
            }
            throw new Exception($"Ocorreu um erro durante a operação, erro: {e.Message}");                    
        }
        finally
        {
            _connection.Close();
        }

        
    }

    public void Update(Usuario usuario)
    {
        
        _connection.Open();
        var transaction = _connection.BeginTransaction();

        try
        {
            string queryUsuario = "UPDATE Usuarios SET Nome = @Nome,Email = @Email,Sexo = @Sexo,RG = @RG, CPF = @CPF,NomeMae = @NomeMae,SituacaoCadastro = @SituacaoCadastro,DataCadastro = @DataCadastro WHERE Id = @Id";
            _connection.Execute(queryUsuario, usuario,transaction);

            if(usuario.Contato != null)
            {
                string queryContato = "UPDATE Contatos SET UsuarioId = @UsuarioId, Telefone = @Telefone, Celular = @Celular WHERE Id = @Id;";
                _connection.Execute(queryContato, usuario.Contato, transaction);
            }                        

            transaction.Commit();
        }
        catch (Exception e)
        {
            try
            {
                transaction.Rollback();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            throw new Exception(e.Message);
        }
        finally
        {
            _connection.Close();
        }
        
    }

    public void Delete(int id)
    {
        //string query = $"DELETE Usuarios WHERE Id = {id}";
        //_connection.Execute(query);

        // Para proteger do SqlInjection
        _connection.Execute("DELETE Usuarios WHERE Id = @Id", new {Id = id});
    }

}

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
        //return _connection.Query<Usuario>("SELECT * FROM Usuarios").ToList();

        List<Usuario> usuarios = [];

        string query = "SELECT * FROM Usuarios AS U LEFT JOIN Contatos C ON C.UsuarioId = U.Id LEFT JOIN EnderecosEntrega AS EE ON EE.UsuarioId = u.Id;";

        // usuario é o principal
        // Essa função anonima é executava uma vez por linha
        // Evita duplicidade
        _connection.Query<Usuario, Contato, EnderecoEntrega, Usuario>(query,
            (usuario, contato, enderecoEntrega) =>
            {
                // Verifica se já existe o usuário
                if(usuarios.SingleOrDefault(u => u.Id == usuario.Id) == null)
                {
                    usuario.EnderecoEntrega = new List<EnderecoEntrega>();
                    usuario.Contato = contato;
                    usuarios.Add(usuario);
                }
                else
                {
                    usuario = usuarios.SingleOrDefault(u => u.Id == usuario.Id);
                }

                usuario.EnderecoEntrega.Add(enderecoEntrega);

                return usuario;
            });
        return usuarios;
    }

    public Usuario? Get(int id)
    {
        List<Usuario> usuarios = [];

        string query = "SELECT * FROM Usuarios AS U LEFT JOIN Contatos C ON C.UsuarioId = U.Id LEFT JOIN EnderecosEntrega AS EE ON EE.UsuarioId = u.Id WHERE U.Id = @Id;";

        // usuario é o principal
        // Essa função anonima é executava uma vez por linha
        // Evita duplicidade
        _connection.Query<Usuario, Contato, EnderecoEntrega, Usuario>(query,
            (usuario, contato, enderecoEntrega) =>
            {
                // Verifica se já existe o usuário
                if (usuarios.SingleOrDefault(u => u.Id == usuario.Id) == null)
                {
                    usuario.EnderecoEntrega = new List<EnderecoEntrega>();
                    usuario.Contato = contato;
                    usuarios.Add(usuario);
                }
                else
                {
                    usuario = usuarios.SingleOrDefault(u => u.Id == usuario.Id);
                }

                usuario.EnderecoEntrega.Add(enderecoEntrega);

                return usuario;
            },new {Id = id});

        // Para que retorne somente um usuario, foi aproveitada a lógica do método acima
        return usuarios.SingleOrDefault();
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

            // Verificando se o usuario possui endereço de entrega
            if(usuario.EnderecoEntrega != null && usuario.EnderecoEntrega.Count > 0)
            {
                foreach (var enderecoEntrega in usuario.EnderecoEntrega)
                {
                    enderecoEntrega.UsuarioId = usuario.Id;
                    string queryEndereco = "INSERT INTO EnderecosEntrega (UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) VALUES (@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento);SELECT CAST (SCOPE_IDENTITY() AS INT);";
                    
                      enderecoEntrega.Id = _connection.Query<int>(queryEndereco, enderecoEntrega, transaction).Single();
                }
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

            // Deletando os endereços de entrega
            string queryDeleteEnderecos = "DELETE FROM EnderecosEntrega WHERE UsuarioId = @Id";
            _connection.Execute(queryDeleteEnderecos, usuario, transaction);

            if (usuario.EnderecoEntrega != null && usuario.EnderecoEntrega.Count > 0)
            {
                foreach (var enderecoEntrega in usuario.EnderecoEntrega)
                {
                    enderecoEntrega.UsuarioId = usuario.Id;
                    string queryEndereco = "INSERT INTO EnderecosEntrega (UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) VALUES (@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento);SELECT CAST (SCOPE_IDENTITY() AS INT);";

                    enderecoEntrega.Id = _connection.Query<int>(queryEndereco, enderecoEntrega, transaction).Single();
                }
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

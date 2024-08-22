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
        Usuario usuario = new Usuario();

        string query = "SELECT U.*, C.*, EE.*, D.* FROM Usuarios U LEFT JOIN Contatos C ON C.UsuarioId = U.Id LEFT JOIN EnderecosEntrega EE ON EE.UsuarioId = U.Id LEFT JOIN UsuariosDepartamentos UD ON UD.UsuarioId = U.Id LEFT JOIN Departamentos D ON D.Id = UD.DepartamentoId;";

        // usuario é o principal
        // Essa função anonima é executava uma vez por linha
        // Evita duplicidade
        _connection.Query<Usuario, Contato, EnderecoEntrega, Departamento,Usuario>(query,
            (usuario, contato, enderecoEntrega, departamento) =>
            {              

                // Verifica se já existe o usuário

                var existeUsuario = usuarios.SingleOrDefault(u => u.Id == usuario.Id);
                if ( existeUsuario == null)
                {
                    usuario.EnderecoEntrega = new List<EnderecoEntrega>();
                    usuario.Departamentos = new List<Departamento>();
                    usuario.Contato = contato;
                    usuarios.Add(usuario);
                    existeUsuario = usuario;
                }
                
                // Verifica se o endereço já existe
                
                if (enderecoEntrega != null && existeUsuario.EnderecoEntrega.SingleOrDefault(e => e.Id == enderecoEntrega.Id) == null)
                {
                    existeUsuario.EnderecoEntrega.Add(enderecoEntrega);
                }


                // Verificação do departamento
                if (departamento!= null && existeUsuario.Departamentos.SingleOrDefault(d => d.Id == departamento.Id) == null)
                {
                    existeUsuario.Departamentos.Add(departamento);
                }                

                return existeUsuario;
            });
        return usuarios;
    }

    public Usuario Get(int id)
    {
        List<Usuario> usuarios = [];
        Usuario usuario = new Usuario();

        string query = "SELECT U.*, C.*, EE.*, D.* FROM Usuarios U LEFT JOIN Contatos C ON C.UsuarioId = U.Id LEFT JOIN EnderecosEntrega EE ON EE.UsuarioId = U.Id LEFT JOIN UsuariosDepartamentos UD ON UD.UsuarioId = U.Id LEFT JOIN Departamentos D ON D.Id = UD.DepartamentoId WHERE U.Id = @Id;";

        // usuario é o principal
        // Essa função anonima é executava uma vez por linha
        // Evita duplicidade
        _connection.Query<Usuario, Contato, EnderecoEntrega, Departamento, Usuario>(query,
            (usuario, contato, enderecoEntrega, departamento) =>
            {

                // Verifica se já existe o usuário

                var existeUsuario = usuarios.SingleOrDefault(u => u.Id == usuario.Id);
                if (existeUsuario == null)
                {
                    usuario.EnderecoEntrega = new List<EnderecoEntrega>();
                    usuario.Departamentos = new List<Departamento>();
                    usuario.Contato = contato;
                    usuarios.Add(usuario);
                    existeUsuario = usuario;
                }

                // Verifica se o endereço já existe

                if (enderecoEntrega != null && existeUsuario.EnderecoEntrega.SingleOrDefault(e => e.Id == enderecoEntrega.Id) == null)
                {
                    existeUsuario.EnderecoEntrega.Add(enderecoEntrega);
                }


                // Verificação do departamento
                if (departamento != null && existeUsuario.Departamentos.SingleOrDefault(d => d.Id == departamento.Id) == null)
                {
                    existeUsuario.Departamentos.Add(departamento);
                }

                return existeUsuario;
            },new {Id = id});
        return usuarios.FirstOrDefault();
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

            // Verificando se o usuario possui departamento
            if(usuario.Departamentos != null && usuario.Departamentos.Count > 0)
            {
                foreach (var departamento in usuario.Departamentos)
                {
                    string queryUsuarioDepartamento = "INSERT INTO UsuariosDepartamentos (UsuarioId, DepartamentoId) VALUES (@UsuarioId, @DepartamentoId);";
                    _connection.Execute(queryUsuarioDepartamento, new {UsuarioId = usuario.Id,DepartamentoId = departamento.Id}, transaction);
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

            // Deletando os departamentos
            string queryDeleteDepartamentos = "DELETE FROM UsuariosDepartamentos WHERE UsuarioId = @Id";
            _connection.Execute(queryDeleteDepartamentos, usuario, transaction);

            if (usuario.Departamentos != null && usuario.Departamentos.Count > 0)
            {
                foreach (var departamento in usuario.Departamentos)
                {
                    
                    string queryDepartamento = "INSERT INTO UsuariosDepartamentos (UsuarioId, DepartamentoId) VALUES (@UsuarioId, @DepartamentoId);";

                    _connection.Execute(queryDepartamento, new {UsuarioId = usuario.Id, DepartamentoId = departamento.Id}, transaction);
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

using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using eCommerce.API.Models;
using Dapper.FluentMap;
using eCommerce.API.Mappers;

namespace eCommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TipsController : ControllerBase
{
    private IDbConnection _connection;

    public TipsController()
    {
        _connection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=eCommerceDapper;Integrated Security=True");
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        string query = "SELECT * FROM Usuarios WHERE Id = @Id;" +
                       "SELECT * FROM Contatos WHERE UsuarioId = @Id;" +
                       "SELECT * FROM EnderecosEntrega WHERE UsuarioId = @Id;" +
                       "SELECT D.* FROM UsuariosDepartamentos UD INNER JOIN Departamentos D " +
                       "ON DepartamentoId = D.Id WHERE UsuarioId = @Id;";

        // O objeto anônimo contem o Id (parametro) e o id que veio com o método
        using (var multipleResultSets = _connection.QueryMultiple(query, new { Id = id }))
        {
            // Usar o SingleOrDefault pq o retorno será apenas um objeto e não uma lista, melhor do que retornar uma lista e depois converter
            var usuario = multipleResultSets.Read<Usuario>().SingleOrDefault();
            var contato = multipleResultSets.Read<Contato>().SingleOrDefault();
            var enderecoEntrega = multipleResultSets.Read<EnderecoEntrega>().ToList();
            var departamentos = multipleResultSets.Read<Departamento>().ToList();

            if(usuario != null)
            {
                usuario.Contato = contato;
                usuario.EnderecoEntrega = enderecoEntrega;
                usuario.Departamentos = departamentos;

                return Ok(usuario);
            }

            return NotFound("Usuario não encontrado!");
        }
    }

    [HttpGet("stored/usuarios")]
    public IActionResult StoredGet()
    {
        // Outra possibilidade
        //_connection.Query<Usuario>("exec SelecionarUsuarios");

        var usuarios = _connection.Query<Usuario>("SelecionarUsuarios",CommandType.StoredProcedure);

        return Ok(usuarios);
    }

    [HttpGet("stored/usuario/{id}")]
    public IActionResult StoredGet(int id)
    {
        var usuario = _connection.Query<Usuario>("SelecionarUsuario", new { Id = id},
            commandType: CommandType.StoredProcedure);

        return Ok(usuario);
    }

    [HttpGet("mapper1/usuarios")]
    public IActionResult Mapper1()
    {
        // O UsuarioTwo possui alguns nomes de coluna diferentes do banco de dados, o dapper não mapeia
        //var usuarios = _connection.Query<UsuarioTwo>("SELECT * FROM Usuarios").ToList();

        // 1º solução SQL(MER) - usar AS (apelido)
        var usuarios = _connection.Query<UsuarioTwo>("SELECT Id AS Cod, Nome AS NomeCompleto, Email, Sexo, RG, CPF, NomeMae AS NomeCompletoMae, SituacaoCadastro AS Situacao, DataCadastro FROM Usuarios;");

        return Ok(usuarios);
    }

    [HttpGet("mapper2/usuarios")]
    public IActionResult Mapper2()
    {
        // O UsuarioTwo possui alguns nomes de coluna diferentes do banco de dados, o dapper não mapeia
        //var usuarios = _connection.Query<UsuarioTwo>("SELECT * FROM Usuarios").ToList();

        // 2º solução C#(POO) usando a biblioteca Dapper.FluentMap

        FluentMapper.Initialize(config =>
        {
            config.AddMap(new UsuarioTwoMap());
        });

        var usuarios = _connection.Query<UsuarioTwo>("SELECT * FROM Usuarios;");

        return Ok(usuarios);
    }
}

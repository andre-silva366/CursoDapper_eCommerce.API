using Dapper.FluentMap.Mapping;
using eCommerce.API.Models;

namespace eCommerce.API.Mappers;

public class UsuarioTwoMap : EntityMap<UsuarioTwo>
{
    public UsuarioTwoMap()
    {
        Map(p => p.Cod).ToColumn("Id");
        Map(p => p.NomeCompleto).ToColumn("Nome");
        Map(p => p.NomeCompletoMae).ToColumn("NomeMae");
        Map(p => p.Situacao).ToColumn("SituacaoCadastro");
    }
}

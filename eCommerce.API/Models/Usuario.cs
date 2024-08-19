namespace eCommerce.API.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Sexo { get; set; }
    public string RG { get; set; }
    public string CPF { get; set; }
    public string NomeMae { get; set; }
    public string SituacaoCadastro { get; set; }

    // DateTimeoffset indica o fuso horário, muito importante em banco de dados
    public DateTimeOffset DataCadastro { get; set; }

    // Composição -> um usuário só tem um contato, 1:1
    public Contato Contato { get; set; }

    // Um usuario pode ter varios endereços
    public ICollection<EnderecoEntrega> EnderecoEntrega { get; set; }

    // Um usuario pode ter varios departamentos
    public ICollection<Departamento> Departamentos { get; set; }
}

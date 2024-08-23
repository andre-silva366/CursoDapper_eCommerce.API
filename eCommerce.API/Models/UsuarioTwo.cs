namespace eCommerce.API.Models;

public class UsuarioTwo
{
    public int Cod { get; set; }
    public string NomeCompleto { get; set; }
    public string Email { get; set; }
    public string Sexo { get; set; }
    public string RG { get; set; }
    public string CPF { get; set; }
    public string NomeCompletoMae { get; set; }
    public string Situacao { get; set; }

    // DateTimeoffset indica o fuso horário, muito importante em banco de dados
    public DateTimeOffset DataCadastro { get; set; }

    // Composição -> um usuário só tem um contato, 1:1
    public Contato Contato { get; set; }

    // Um usuario pode ter varios endereços
    public ICollection<EnderecoEntrega> EnderecoEntrega { get; set; }

    // Um usuario pode ter varios departamentos
    public ICollection<Departamento> Departamentos { get; set; }
}

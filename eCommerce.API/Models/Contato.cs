namespace eCommerce.API.Models;

public class Contato
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Telefone { get; set; }
    public string Celular { get; set; }

    // Cada contato pertence a um usuario
    public Usuario Usuario { get; set; }
}

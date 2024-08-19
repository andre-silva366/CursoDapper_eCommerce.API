using eCommerce.API.Models;
using eCommerce.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers;

[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private IRepository<Usuario> _repository;

    public UsuariosController()
    {
        _repository = new UsuarioRepository();
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_repository.Get());
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var usuario = _repository.Get(id);

        if(usuario == null)
        {
            return NotFound();
        }

        return Ok(usuario);
    }

    [HttpPost]
    public IActionResult Post([FromBody]Usuario usuario)
    {
        _repository.Insert(usuario);
        return Ok(usuario);
    }

    [HttpPut]
    public IActionResult Put([FromBody] Usuario usuario)
    {
        _repository.Update(usuario);
        return Ok(usuario);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var usuario = _repository.Get(id);

        if(usuario != null)
        {
            _repository.Delete(id);
            return Ok("Usuario deletado com sucesso");
        }

        return NotFound($"Não encontrado usuario com id: {id}.");

    }
}

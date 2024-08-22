using eCommerce.API.Models;
using eCommerce.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers;

[Route("api/Contrib/Usuarios")]
[ApiController]
public class ContribUsuariosController : ControllerBase
{
    private IRepository<UsuarioContrib> _repository;

    public ContribUsuariosController()
    {
        _repository = new ContribUsuarioRepository();
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

        if (usuario == null || usuario.Id < 1)
        {
            return NotFound($"Não encontrado usuario com id = {id}");
        }

        return Ok(usuario);
    }

    [HttpPost]
    public IActionResult Post([FromBody] UsuarioContrib usuario)
    {
        try
        {
            _repository.Insert(usuario);
            return Ok(usuario);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    [HttpPut]
    public IActionResult Put([FromBody] UsuarioContrib usuario)
    {
        _repository.Update(usuario);
        return Ok(usuario);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var usuario = _repository.Get(id);

        if (usuario != null)
        {
            _repository.Delete(id);
            return Ok("Usuario deletado com sucesso");
        }

        return NotFound($"Não encontrado usuario com id: {id}.");

    }
}

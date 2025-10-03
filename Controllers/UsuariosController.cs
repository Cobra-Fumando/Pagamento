using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pic.Config;
using Pic.Parametros;

namespace Pic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly Users users;

        public UsuariosController(Users users)
        {
            this.users = users;
        }

        [HttpPost("Criar")]
        public async Task<IActionResult> Criar([FromBody] UsuarioDto usuario)
        {
            if (!ModelState.IsValid) return BadRequest(new { Mensagem = ModelState });
            var Result = await users.Criar(usuario);

            if (!Result.Sucesso) return BadRequest(new { Mensagem = Result.Mensagem });
            return Ok(new { Mensagem = Result.Mensagem, Conta = Result.Dados});
        }

        [HttpPost("Logar")]
        public async Task<IActionResult> Login([FromBody] Logar logar)
        {
            if (!ModelState.IsValid) return BadRequest(new { Mensagem = ModelState });

            var Result = await users.Logar(logar);
            if (!Result.Sucesso) return BadRequest(new { Mensagem = Result.Mensagem });

            return Ok(new { Mensagem = Result.Mensagem, Token = Result.Dados });
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pic.Config;
using Pic.Parametros;

namespace Pic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferirController : ControllerBase
    {
        private readonly Enviar enviar;
        public TransferirController(Enviar enviar)
        {
            this.enviar = enviar;
        }

        [HttpPost("Enviar")]
        [Authorize(AuthenticationSchemes = "Usuarios")]
        [EnableRateLimiting("Fixed")]
        public async Task<IActionResult> Transfer([FromBody] Transferir transferir)
        {
            var id = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;

            if (id is null) return Unauthorized("Token inválido");

            if(!int.TryParse(id, out int Id)) return BadRequest(new {Mensagem = "Id invalido"});

            var result = await enviar.Transacao(Id, transferir.Valor, transferir.EmailT);

            if(!result.Sucesso) return BadRequest(new { Mensagem = result.Mensagem });
            return Ok(new { Mensagem = result.Mensagem } );
        }

        [HttpGet("GetTransacoes")]
        [Authorize(AuthenticationSchemes = "Usuarios")]
        [EnableRateLimiting("Fixed")]
        public async Task<IActionResult> GetTransacoes()
        {
            var id = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;

            if (id is null) return Unauthorized("Token inválido");

            if(!int.TryParse(id, out int Id)) return BadRequest(new {Mensagem = "Id invalido"});

            var result = await enviar.GetTransacao(Id);

            if(!result.Sucesso) return BadRequest(new { Mensagem = result.Mensagem });

            return Ok(new { Mensagem = result.Mensagem, Dados = result.Dados } );
        }
    }
}

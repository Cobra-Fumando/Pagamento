using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pic.Interface;
using Pic.Parametros;

namespace Pic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutos produtos;
        public ProdutosController(IProdutos produtos) 
        { 
            this.produtos = produtos;
        }

        [HttpPost("Adicionar")]
        [Authorize(AuthenticationSchemes = "Usuarios")]
        [EnableRateLimiting("Fixed")]
        public async Task<IActionResult> AdicionarProduto([FromBody] ProdutosDto produto)
        {

            if(!ModelState.IsValid) return BadRequest(new { message = ModelState });

            var id = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
            if(!int.TryParse(id, out int Userid)) return BadRequest(new { message = "Não foi possivel converte o Id" });

            var result = await produtos.AdicionarProduto(produto, Userid);
            if (!result.Sucesso) return BadRequest(new { message = result.Mensagem });
            return Ok(new { message = result.Mensagem, data = result.Dados });
        }

        [HttpGet("Listar")]
        [Authorize(AuthenticationSchemes = "Usuarios")]
        [EnableRateLimiting("Fixed")]
        public async Task<IActionResult> ListarProdutos([FromQuery] int Tamanho, [FromQuery] int Pagina)
        {
            var result = await produtos.ListarProdutos(Tamanho, Pagina);
            if (!result.Sucesso) return BadRequest(new { message = result.Mensagem });
            return Ok(new { message = result.Mensagem, data = result.Dados });
        }

        [HttpGet("MeusProdutos/{pagina}")]
        [Authorize(AuthenticationSchemes = "Usuarios")]
        [EnableRateLimiting("Fixed")]
        public async Task<IActionResult> MeusProdutos(int pagina)
        {

            var id = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;

            if(!int.TryParse(id, out int Userid)) return BadRequest(new { message = "Não foi possivel converte o Id" });
            var result = await produtos.SeusProdutos(Userid, 10, pagina);

            if (!result.Sucesso) return BadRequest(new { message = result.Mensagem });
            return Ok(new { message = result.Mensagem, data = result.Dados });
        }
    }
}

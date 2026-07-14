using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OsobljeController : ControllerBase
    {
        // GET api/osoblje?tip=
        [HttpGet]
        public ActionResult<List<object>> VratiSve([FromQuery] string? tip)
        {
            return Ok(DTOManager.vratiSveOsoblje(tip));
        }

        // GET api/osoblje/agenti
        [HttpGet("agenti")]
        public ActionResult<List<AgentBasic>> VratiAgente()
        {
            return Ok(DTOManager.vratiSveAgente());
        }

        // GET api/osoblje/procenitelji
        [HttpGet("procenitelji")]
        public ActionResult<List<ProceniteljBasic>> VratiProcenitelje()
        {
            return Ok(DTOManager.vratiSveProcenitelje());
        }

        // POST api/osoblje
        [HttpPost]
        public IActionResult Dodaj([FromBody] OsobljeBasic dto)
        {
            DTOManager.dodajOsoblje(dto);
            return StatusCode(201);
        }

        // POST api/osoblje/agenti
        [HttpPost("agenti")]
        public IActionResult DodajAgenta([FromBody] AgentBasic dto)
        {
            DTOManager.dodajAgenta(dto);
            return StatusCode(201);
        }

        // POST api/osoblje/lekari
        [HttpPost("lekari")]
        public IActionResult DodajLekara([FromBody] LekarBasic dto)
        {
            DTOManager.dodajLekara(dto);
            return StatusCode(201);
        }

        // PUT api/osoblje/5
        [HttpPut("{id:int}")]
        public IActionResult Azuriraj(int id, [FromBody] OsobljeBasic dto)
        {
            dto.OsobljeId = id;
            DTOManager.azurirajOsoblje(dto);
            return NoContent();
        }

        // PUT api/osoblje/agenti/5
        [HttpPut("agenti/{id:int}")]
        public IActionResult AzurirajAgenta(int id, [FromBody] AgentBasic dto)
        {
            dto.OsobljeId = id;
            DTOManager.azurirajAgenta(dto);
            return NoContent();
        }

        // PUT api/osoblje/lekari/5
        [HttpPut("lekari/{id:int}")]
        public IActionResult AzurirajLekara(int id, [FromBody] LekarBasic dto)
        {
            dto.OsobljeId = id;
            DTOManager.azurirajLekara(dto);
            return NoContent();
        }

        // DELETE api/osoblje/5
        [HttpDelete("{id:int}")]
        public IActionResult Obrisi(int id)
        {
            DTOManager.obrisiOsoblje(id);
            return NoContent();
        }
    }
}

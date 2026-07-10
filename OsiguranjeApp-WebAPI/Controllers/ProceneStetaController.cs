using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProceneStetaController : ControllerBase
    {
        // GET api/procenesteta/steta/5
        [HttpGet("steta/{stetaId:int}")]
        public ActionResult<List<ProcenaStetaBasic>> VratiZaStetu(int stetaId) =>
            Ok(DTOManager.vratiProceneZaStetu(stetaId));

        // POST api/procenesteta
        [HttpPost]
        public IActionResult Dodaj([FromBody] ProcenaStetaBasic dto)
        {
            DTOManager.dodajProcenu(dto);
            return StatusCode(201);
        }

        // PUT api/procenesteta/5
        [HttpPut("{id:int}")]
        public IActionResult Azuriraj(int id, [FromBody] ProcenaStetaBasic dto)
        {
            dto.ProcenaId = id;
            DTOManager.azurirajProcenu(dto);
            return NoContent();
        }

        // DELETE api/procenesteta/5
        [HttpDelete("{id:int}")]
        public IActionResult Obrisi(int id)
        {
            DTOManager.obrisiProcenu(id);
            return NoContent();
        }
    }
}

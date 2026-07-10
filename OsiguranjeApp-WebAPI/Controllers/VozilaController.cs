using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VozilaController : ControllerBase
    {
        // GET api/vozila
        [HttpGet]
        public ActionResult<List<VoziloBasic>> VratiSva()
        {
            return Ok(DTOManager.vratiSvaVozila());
        }

        // POST api/vozila
        [HttpPost]
        public IActionResult Dodaj([FromBody] VoziloBasic dto)
        {
            DTOManager.dodajVozilo(dto);
            return StatusCode(201);
        }

        // PUT api/vozila/5
        [HttpPut("{id:int}")]
        public IActionResult Azuriraj(int id, [FromBody] VoziloBasic dto)
        {
            dto.VoziloId = id;
            DTOManager.azurirajVozilo(dto);
            return NoContent();
        }

        // DELETE api/vozila/5
        [HttpDelete("{id:int}")]
        public IActionResult Obrisi(int id)
        {
            DTOManager.obrisiVozilo(id);
            return NoContent();
        }
    }
}

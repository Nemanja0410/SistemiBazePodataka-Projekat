using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OstecenaLicaController : ControllerBase
    {
        // GET api/ostecenalica/steta/5
        [HttpGet("steta/{stetaId:int}")]
        public ActionResult<List<OstecenLiceBasic>> VratiZaStetu(int stetaId) =>
            Ok(DTOManager.vratiOstecenaLicaZaStetu(stetaId));

        // POST api/ostecenalica
        [HttpPost]
        public IActionResult Dodaj([FromBody] OstecenLiceBasic dto)
        {
            DTOManager.dodajOstecenoLice(dto);
            return StatusCode(201);
        }

        // PUT api/ostecenalica/5
        [HttpPut("{id:int}")]
        public IActionResult Azuriraj(int id, [FromBody] OstecenLiceBasic dto)
        {
            dto.OstecenLiceId = id;
            DTOManager.azurirajOstecenoLice(dto);
            return NoContent();
        }

        // DELETE api/ostecenalica/5
        [HttpDelete("{id:int}")]
        public IActionResult Obrisi(int id)
        {
            DTOManager.obrisiOstecenoLice(id);
            return NoContent();
        }
    }
}

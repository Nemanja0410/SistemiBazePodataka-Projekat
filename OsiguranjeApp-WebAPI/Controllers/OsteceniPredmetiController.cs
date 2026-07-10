using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OsteceniPredmetiController : ControllerBase
    {
        // GET api/osteceniPredmeti/steta/5
        [HttpGet("steta/{stetaId:int}")]
        public ActionResult<List<OsteceniPredmetBasic>> VratiZaStetu(int stetaId) =>
            Ok(DTOManager.vratiOsteceniPredmetiZaStetu(stetaId));

        // POST api/osteceniPredmeti
        [HttpPost]
        public IActionResult Dodaj([FromBody] OsteceniPredmetBasic dto)
        {
            DTOManager.dodajOsteceniPredmet(dto);
            return StatusCode(201);
        }

        // PUT api/osteceniPredmeti/5
        [HttpPut("{id:int}")]
        public IActionResult Azuriraj(int id, [FromBody] OsteceniPredmetBasic dto)
        {
            dto.OsteceniPredmetId = id;
            DTOManager.azurirajOsteceniPredmet(dto);
            return NoContent();
        }

        // DELETE api/osteceniPredmeti/5
        [HttpDelete("{id:int}")]
        public IActionResult Obrisi(int id)
        {
            DTOManager.obrisiOsteceniPredmet(id);
            return NoContent();
        }
    }
}

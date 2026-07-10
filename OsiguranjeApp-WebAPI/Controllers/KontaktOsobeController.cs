using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    // Kontakt osobe pravnih lica / javnih institucija.
    [ApiController]
    [Route("api/[controller]")]
    public class KontaktOsobeController : ControllerBase
    {
        // GET api/kontaktosobe/klijent/5
        [HttpGet("klijent/{klijentId:int}")]
        public ActionResult<List<KontaktOsobaBasic>> VratiZaKlijenta(int klijentId) =>
            Ok(DTOManager.vratiKontakteZaKlijenta(klijentId));

        // POST api/kontaktosobe
        [HttpPost]
        public IActionResult Dodaj([FromBody] KontaktOsobaBasic dto)
        {
            DTOManager.dodajKontaktOsobu(dto);
            return StatusCode(201);
        }

        // PUT api/kontaktosobe/5
        [HttpPut("{id:int}")]
        public IActionResult Azuriraj(int id, [FromBody] KontaktOsobaBasic dto)
        {
            dto.KontaktId = id;
            DTOManager.azurirajKontaktOsobu(dto);
            return NoContent();
        }

        // DELETE api/kontaktosobe/5
        [HttpDelete("{id:int}")]
        public IActionResult Obrisi(int id)
        {
            DTOManager.obrisiKontaktOsobu(id);
            return NoContent();
        }
    }
}

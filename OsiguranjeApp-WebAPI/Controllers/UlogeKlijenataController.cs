using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    // Uloga klijenta na polisi (npr. suvozac, korisnik).
    [ApiController]
    [Route("api/[controller]")]
    public class UlogeKlijenataController : ControllerBase
    {
        // GET api/ulogeklijenata/polisa/5
        [HttpGet("polisa/{polisaId:int}")]
        public ActionResult<List<UlogaKlijentaBasic>> VratiZaPolisu(int polisaId) =>
            Ok(DTOManager.vratiUlogeZaPolisu(polisaId));

        // POST api/ulogeklijenata
        [HttpPost]
        public IActionResult Dodaj([FromBody] UlogaKlijentaBasic dto)
        {
            DTOManager.dodajUlogu(dto);
            return StatusCode(201);
        }

        // PUT api/ulogeklijenata/5
        [HttpPut("{id:int}")]
        public IActionResult Azuriraj(int id, [FromBody] UlogaKlijentaBasic dto)
        {
            dto.UlogaId = id;
            DTOManager.azurirajUlogu(dto);
            return NoContent();
        }

        // DELETE api/ulogeklijenata/5
        [HttpDelete("{id:int}")]
        public IActionResult Obrisi(int id)
        {
            DTOManager.obrisiUlogu(id);
            return NoContent();
        }
    }
}

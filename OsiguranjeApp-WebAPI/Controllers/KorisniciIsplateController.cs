using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    // Korisnici isplate kod zivotnog osiguranja.
    [ApiController]
    [Route("api/[controller]")]
    public class KorisniciIsplateController : ControllerBase
    {
        // GET api/korisniciisplate/polisa/5
        [HttpGet("polisa/{polisaId:int}")]
        public ActionResult<List<KorisnikIsplateBasic>> VratiZaPolisu(int polisaId) =>
            Ok(DTOManager.vratiKorisnikeIsplateZaPolisu(polisaId));

        // POST api/korisniciisplate
        [HttpPost]
        public IActionResult Dodaj([FromBody] KorisnikIsplateBasic dto)
        {
            DTOManager.dodajKorisnikaIsplate(dto);
            return StatusCode(201);
        }

        // PUT api/korisniciisplate/5
        [HttpPut("{id:int}")]
        public IActionResult Azuriraj(int id, [FromBody] KorisnikIsplateBasic dto)
        {
            dto.KorisnikId = id;
            DTOManager.azurirajKorisnikaIsplate(dto);
            return NoContent();
        }

        // DELETE api/korisniciisplate/5
        [HttpDelete("{id:int}")]
        public IActionResult Obrisi(int id)
        {
            DTOManager.obrisiKorisnikaIsplate(id);
            return NoContent();
        }
    }
}

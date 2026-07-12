using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    // Klijent (bazni entitet) + FizickoLice / PravnoLice / JavnaInstitucija (podklase).
    [ApiController]
    [Route("api/[controller]")]
    public class KlijentiController : ControllerBase
    {
        // GET api/klijenti?naziv=&tip=
        [HttpGet]
        public ActionResult<List<KlijentPregled>> VratiSve([FromQuery] string? naziv, [FromQuery] string? tip)
        {
            return Ok(DTOManager.pretraziKlijente(naziv ?? "", tip));
        }

        // GET api/klijenti/5
        [HttpGet("{id:int}")]
        public ActionResult<KlijentBasic> VratiJednog(int id)
        {
            return Ok(DTOManager.vratiKlijenta(id));
        }

        // GET api/klijenti/5/detalji  (ukljucuje specificna polja podtipa - JMBG/PIB/nivo itd.)
        [HttpGet("{id:int}/detalji")]
        public ActionResult<object> VratiDetalje(int id)
        {
            return Ok(DTOManager.vratiKlijentaDetaljno(id));
        }

        // POST api/klijenti/fizicko-lice
        [HttpPost("fizicko-lice")]
        public IActionResult DodajFizickoLice([FromBody] FizickoLiceBasic dto)
        {
            DTOManager.dodajFizickoLice(dto);
            return StatusCode(201);
        }

        // PUT api/klijenti/fizicko-lice/5
        [HttpPut("fizicko-lice/{id:int}")]
        public IActionResult AzurirajFizickoLice(int id, [FromBody] FizickoLicePregled dto)
        {
            dto.KlijentId = id;
            DTOManager.azurirajFizickoLice(dto);
            return NoContent();
        }

        // POST api/klijenti/pravno-lice
        [HttpPost("pravno-lice")]
        public IActionResult DodajPravnoLice([FromBody] PravnoLiceBasic dto)
        {
            DTOManager.dodajPravnoLice(dto);
            return StatusCode(201);
        }

        // PUT api/klijenti/pravno-lice/5
        [HttpPut("pravno-lice/{id:int}")]
        public IActionResult AzurirajPravnoLice(int id, [FromBody] PravnoLicePregled dto)
        {
            dto.KlijentId = id;
            DTOManager.azurirajPravnoLice(dto);
            return NoContent();
        }

        // POST api/klijenti/javna-institucija
        [HttpPost("javna-institucija")]
        public IActionResult DodajJavnuInstituciju([FromBody] JavnaInstitucijaBasic dto)
        {
            DTOManager.dodajJavnuInstituciju(dto);
            return StatusCode(201);
        }

        // PUT api/klijenti/javna-institucija/5
        [HttpPut("javna-institucija/{id:int}")]
        public IActionResult AzurirajJavnuInstituciju(int id, [FromBody] JavnaInstitucijaPregled dto)
        {
            dto.KlijentId = id;
            DTOManager.azurirajJavnuInstituciju(dto);
            return NoContent();
        }

        // PUT api/klijenti/5  (samo zajednicka polja - koristi se kad tip podklase nije bitan)
        [HttpPut("{id:int}")]
        public IActionResult Azuriraj(int id, [FromBody] KlijentBasic dto)
        {
            dto.KlijentId = id;
            DTOManager.azurirajKlijenta(dto);
            return NoContent();
        }

        // DELETE api/klijenti/5
        [HttpDelete("{id:int}")]
        public IActionResult Obrisi(int id)
        {
            DTOManager.obrisiKlijenta(id);
            return NoContent();
        }
    }
}

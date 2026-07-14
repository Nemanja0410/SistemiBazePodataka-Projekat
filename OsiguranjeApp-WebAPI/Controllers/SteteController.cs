using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    // Steta (bazni entitet) + AutoSteta/ZdravstvenaSteta/ImovinskSteta (podklase) + FazaObrade.
    [ApiController]
    [Route("api/[controller]")]
    public class SteteController : ControllerBase
    {
        // GET api/stete?vrsta=&status=
        [HttpGet]
        public ActionResult<List<StetaPregled>> VratiSve([FromQuery] string? vrsta, [FromQuery] string? status)
        {
            return Ok(DTOManager.vratiSveStete(vrsta, status));
        }

        // GET api/stete/5
        [HttpGet("{id:int}")]
        public ActionResult<object> VratiJednu(int id)
        {
            return Ok(DTOManager.vratiStetuDetaljno(id));
        }

        // POST api/stete  (bazna steta, bez specificnih polja podtipa)
        [HttpPost]
        public IActionResult Dodaj([FromBody] StetaBasic dto)
        {
            DTOManager.dodajStetu(dto);
            return StatusCode(201);
        }

        // PUT api/stete/5  (samo zajednicka polja)
        [HttpPut("{id:int}")]
        public IActionResult Azuriraj(int id, [FromBody] StetaBasic dto)
        {
            dto.StetaId = id;
            DTOManager.azurirajStetu(dto);
            return NoContent();
        }

        // DELETE api/stete/5
        [HttpDelete("{id:int}")]
        public IActionResult Obrisi(int id)
        {
            DTOManager.obrisiStetu(id);
            return NoContent();
        }

        // ---------- FazaObrade ----------

        [HttpPost("faze")]
        public IActionResult DodajFazu([FromBody] FazaObradeBasic dto)
        {
            DTOManager.dodajFazuObrade(dto);
            return StatusCode(201);
        }

        [HttpPut("faze/{id:int}")]
        public IActionResult AzurirajFazu(int id, [FromBody] FazaObradeBasic dto)
        {
            dto.FazaId = id;
            DTOManager.azurirajFazuObrade(dto);
            return NoContent();
        }

        [HttpDelete("faze/{id:int}")]
        public IActionResult ObrisiFazu(int id)
        {
            DTOManager.obrisiFazuObrade(id);
            return NoContent();
        }

        // ---------- AutoSteta ----------

        [HttpGet("auto")]
        public ActionResult<List<AutoStetaPregled>> VratiAuto() => Ok(DTOManager.vratiSveAutoStete());

        [HttpPost("auto")]
        public IActionResult DodajAuto([FromBody] AutoStetaPregled dto)
        {
            DTOManager.dodajAutoStetu(dto);
            return StatusCode(201);
        }

        [HttpPut("auto/{id:int}")]
        public IActionResult AzurirajAuto(int id, [FromBody] AutoStetaPregled dto)
        {
            dto.StetaId = id;
            DTOManager.azurirajAutoStetu(dto);
            return NoContent();
        }

        // ---------- ZdravstvenaSteta ----------

        [HttpGet("zdravstvena")]
        public ActionResult<List<ZdravstvenaStetaPregled>> VratiZdravstvenu() => Ok(DTOManager.vratiSveZdravstveneStete());

        [HttpPost("zdravstvena")]
        public IActionResult DodajZdravstvenu([FromBody] ZdravstvenaStetaPregled dto)
        {
            DTOManager.dodajZdravstvenuStetu(dto);
            return StatusCode(201);
        }

        [HttpPut("zdravstvena/{id:int}")]
        public IActionResult AzurirajZdravstvenu(int id, [FromBody] ZdravstvenaStetaPregled dto)
        {
            dto.StetaId = id;
            DTOManager.azurirajZdravstvenuStetu(dto);
            return NoContent();
        }

        // ---------- ImovinskSteta ----------

        [HttpGet("imovinska")]
        public ActionResult<List<ImovinskStetaPregled>> VratiImovinsku() => Ok(DTOManager.vratiSveImovinskeStete());

        [HttpPost("imovinska")]
        public IActionResult DodajImovinsku([FromBody] ImovinskStetaPregled dto)
        {
            DTOManager.dodajImovinskuStetu(dto);
            return StatusCode(201);
        }

        [HttpPut("imovinska/{id:int}")]
        public IActionResult AzurirajImovinsku(int id, [FromBody] ImovinskStetaPregled dto)
        {
            dto.StetaId = id;
            DTOManager.azurirajImovinskuStetu(dto);
            return NoContent();
        }
    }
}

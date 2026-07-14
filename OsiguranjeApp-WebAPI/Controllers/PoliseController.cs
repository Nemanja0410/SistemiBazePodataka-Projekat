using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    // Polisa (bazni entitet) + Auto/Zivotno/Zdravstveno/Putno/Imovinsko osiguranje (podklase).
    [ApiController]
    [Route("api/[controller]")]
    public class PoliseController : ControllerBase
    {
        // GET api/polise?tip=&status=
        [HttpGet]
        public ActionResult<List<PolisaPregled>> VratiSve([FromQuery] string? tip, [FromQuery] string? status)
        {
            return Ok(DTOManager.vratiSvePolise(tip, status));
        }

        // GET api/polise/5
        [HttpGet("{id:int}")]
        public ActionResult<object> VratiJednu(int id)
        {
            return Ok(DTOManager.vratiPolisuDetaljno(id));
        }

        // POST api/polise  (bazna polisa, bez specificnih polja podtipa)
        [HttpPost]
        public IActionResult Dodaj([FromBody] PolisaBasic dto)
        {
            DTOManager.dodajPolisu(dto);
            return StatusCode(201);
        }

        // PUT api/polise/5  (samo zajednicka polja)
        [HttpPut("{id:int}")]
        public IActionResult Azuriraj(int id, [FromBody] PolisaBasic dto)
        {
            dto.PolisaId = id;
            DTOManager.azurirajPolisu(dto);
            return NoContent();
        }

        // DELETE api/polise/5
        [HttpDelete("{id:int}")]
        public IActionResult Obrisi(int id)
        {
            DTOManager.obrisiPolisu(id);
            return NoContent();
        }

        // ---------- AutoOsiguranje ----------

        [HttpGet("auto")]
        public ActionResult<List<AutoPolisaPregled>> VratiAuto() => Ok(DTOManager.vratiSvaAutoOsiguranja());

        [HttpPost("auto")]
        public IActionResult DodajAuto([FromBody] AutoPolisaPregled dto)
        {
            DTOManager.dodajAutoOsiguranje(dto);
            return StatusCode(201);
        }

        [HttpPut("auto/{id:int}")]
        public IActionResult AzurirajAuto(int id, [FromBody] AutoPolisaPregled dto)
        {
            dto.PolisaId = id;
            DTOManager.azurirajAutoOsiguranje(dto);
            return NoContent();
        }

        // ---------- ZivotnoOsiguranje ----------

        [HttpGet("zivotno")]
        public ActionResult<List<ZivotnoPregled>> VratiZivotno() => Ok(DTOManager.vratiSvaZivotnaOsiguranja());

        [HttpPost("zivotno")]
        public IActionResult DodajZivotno([FromBody] ZivotnoPregled dto)
        {
            DTOManager.dodajZivotnoOsiguranje(dto);
            return StatusCode(201);
        }

        [HttpPut("zivotno/{id:int}")]
        public IActionResult AzurirajZivotno(int id, [FromBody] ZivotnoPregled dto)
        {
            dto.PolisaId = id;
            DTOManager.azurirajZivotnoOsiguranje(dto);
            return NoContent();
        }

        // ---------- ZdravstvenoOsiguranje ----------

        [HttpGet("zdravstveno")]
        public ActionResult<List<ZdravstvenoPregled>> VratiZdravstveno() => Ok(DTOManager.vratiSvaZdravstvenaOsiguranja());

        [HttpPost("zdravstveno")]
        public IActionResult DodajZdravstveno([FromBody] ZdravstvenoPregled dto)
        {
            DTOManager.dodajZdravstvenoOsiguranje(dto);
            return StatusCode(201);
        }

        [HttpPut("zdravstveno/{id:int}")]
        public IActionResult AzurirajZdravstveno(int id, [FromBody] ZdravstvenoPregled dto)
        {
            dto.PolisaId = id;
            DTOManager.azurirajZdravstvenoOsiguranje(dto);
            return NoContent();
        }

        // ---------- PutnoOsiguranje ----------

        [HttpGet("putno")]
        public ActionResult<List<PutnoPregled>> VratiPutno() => Ok(DTOManager.vratiSvaPutnaOsiguranja());

        [HttpPost("putno")]
        public IActionResult DodajPutno([FromBody] PutnoPregled dto)
        {
            DTOManager.dodajPutnoOsiguranje(dto);
            return StatusCode(201);
        }

        [HttpPut("putno/{id:int}")]
        public IActionResult AzurirajPutno(int id, [FromBody] PutnoPregled dto)
        {
            dto.PolisaId = id;
            DTOManager.azurirajPutnoOsiguranje(dto);
            return NoContent();
        }

        // ---------- ImovinskOsiguranje ----------

        [HttpGet("imovinsko")]
        public ActionResult<List<ImovinskoPregled>> VratiImovinsko() => Ok(DTOManager.vratiSvaImovinskaOsiguranja());

        [HttpPost("imovinsko")]
        public IActionResult DodajImovinsko([FromBody] ImovinskoPregled dto)
        {
            DTOManager.dodajImovinskoOsiguranje(dto);
            return StatusCode(201);
        }

        [HttpPut("imovinsko/{id:int}")]
        public IActionResult AzurirajImovinsko(int id, [FromBody] ImovinskoPregled dto)
        {
            dto.PolisaId = id;
            DTOManager.azurirajImovinskoOsiguranje(dto);
            return NoContent();
        }

        // ---------- PoljoprivrednoOsiguranje ----------

        [HttpGet("poljoprivredno")]
        public ActionResult<List<PoljoprivrednoPregled>> VratiPoljoprivredno() => Ok(DTOManager.vratiSvaPoljoprivrednaOsiguranja());

        [HttpPost("poljoprivredno")]
        public IActionResult DodajPoljoprivredno([FromBody] PoljoprivrednoPregled dto)
        {
            DTOManager.dodajPoljoprivrednoOsiguranje(dto);
            return StatusCode(201);
        }

        [HttpPut("poljoprivredno/{id:int}")]
        public IActionResult AzurirajPoljoprivredno(int id, [FromBody] PoljoprivrednoPregled dto)
        {
            dto.PolisaId = id;
            DTOManager.azurirajPoljoprivrednoOsiguranje(dto);
            return NoContent();
        }

        // ---------- OsiguranjeOdgovornosti ----------

        [HttpGet("odgovornost")]
        public ActionResult<List<OdgovornostPregled>> VratiOdgovornost() => Ok(DTOManager.vratiSvaOsiguranjaOdgovornosti());

        [HttpPost("odgovornost")]
        public IActionResult DodajOdgovornost([FromBody] OdgovornostPregled dto)
        {
            DTOManager.dodajOsiguranjeOdgovornosti(dto);
            return StatusCode(201);
        }

        [HttpPut("odgovornost/{id:int}")]
        public IActionResult AzurirajOdgovornost(int id, [FromBody] OdgovornostPregled dto)
        {
            dto.PolisaId = id;
            DTOManager.azurirajOsiguranjeOdgovornosti(dto);
            return NoContent();
        }

        // ---------- SpecijalizovanoOsiguranje ----------

        [HttpGet("specijalizovano")]
        public ActionResult<List<SpecijalizovanoPregled>> VratiSpecijalizovano() => Ok(DTOManager.vratiSvaSpecijalizovanaOsiguranja());

        [HttpPost("specijalizovano")]
        public IActionResult DodajSpecijalizovano([FromBody] SpecijalizovanoPregled dto)
        {
            DTOManager.dodajSpecijalizovanoOsiguranje(dto);
            return StatusCode(201);
        }

        [HttpPut("specijalizovano/{id:int}")]
        public IActionResult AzurirajSpecijalizovano(int id, [FromBody] SpecijalizovanoPregled dto)
        {
            dto.PolisaId = id;
            DTOManager.azurirajSpecijalizovanoOsiguranje(dto);
            return NoContent();
        }
    }
}

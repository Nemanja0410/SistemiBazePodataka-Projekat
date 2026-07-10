using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DodatnaPokricaController : ControllerBase
    {
        // GET api/dodatnapokrica/polisa/5
        [HttpGet("polisa/{polisaId:int}")]
        public ActionResult<List<DodatnoPokrBasic>> VratiZaPolisu(int polisaId) =>
            Ok(DTOManager.vratiDodatnaPokricaZaPolisu(polisaId));

        // POST api/dodatnapokrica
        [HttpPost]
        public IActionResult Dodaj([FromBody] DodatnoPokrBasic dto)
        {
            DTOManager.dodajDodatnoPokrice(dto);
            return StatusCode(201);
        }

        // PUT api/dodatnapokrica/5
        [HttpPut("{id:int}")]
        public IActionResult Azuriraj(int id, [FromBody] DodatnoPokrBasic dto)
        {
            dto.PokriceId = id;
            DTOManager.azurirajDodatnoPokrice(dto);
            return NoContent();
        }

        // DELETE api/dodatnapokrica/5
        [HttpDelete("{id:int}")]
        public IActionResult Obrisi(int id)
        {
            DTOManager.obrisiDodatnoPokrice(id);
            return NoContent();
        }
    }
}

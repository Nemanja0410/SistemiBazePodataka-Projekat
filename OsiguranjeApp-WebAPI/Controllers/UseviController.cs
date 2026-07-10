using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    // Usev - predmet osiguranja kod poljoprivrednog osiguranja.
    [ApiController]
    [Route("api/[controller]")]
    public class UseviController : ControllerBase
    {
        [HttpGet]
        public ActionResult<List<UsevBasic>> VratiSve() => Ok(DTOManager.vratiSveUseve());

        [HttpPost]
        public IActionResult Dodaj([FromBody] UsevBasic dto)
        {
            DTOManager.dodajUsev(dto);
            return StatusCode(201);
        }

        [HttpPut("{id:int}")]
        public IActionResult Azuriraj(int id, [FromBody] UsevBasic dto)
        {
            dto.UsevId = id;
            DTOManager.azurirajUsev(dto);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public IActionResult Obrisi(int id)
        {
            DTOManager.obrisiUsev(id);
            return NoContent();
        }
    }
}

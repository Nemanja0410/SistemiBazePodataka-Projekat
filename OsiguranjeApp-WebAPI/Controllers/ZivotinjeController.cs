using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    // Zivotinja - predmet osiguranja kod poljoprivrednog osiguranja.
    [ApiController]
    [Route("api/[controller]")]
    public class ZivotinjeController : ControllerBase
    {
        [HttpGet]
        public ActionResult<List<ZivotinjaBasic>> VratiSve() => Ok(DTOManager.vratiSveZivotinje());

        [HttpPost]
        public IActionResult Dodaj([FromBody] ZivotinjaBasic dto)
        {
            DTOManager.dodajZivotinju(dto);
            return StatusCode(201);
        }

        [HttpPut("{id:int}")]
        public IActionResult Azuriraj(int id, [FromBody] ZivotinjaBasic dto)
        {
            dto.ZivotinjaId = id;
            DTOManager.azurirajZivotinju(dto);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public IActionResult Obrisi(int id)
        {
            DTOManager.obrisiZivotinju(id);
            return NoContent();
        }
    }
}

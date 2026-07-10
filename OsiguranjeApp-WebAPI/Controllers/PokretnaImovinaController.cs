using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    // PokretnaImovina - dodatni predmet osiguranja kod imovinskog osiguranja.
    [ApiController]
    [Route("api/[controller]")]
    public class PokretnaImovinaController : ControllerBase
    {
        [HttpGet]
        public ActionResult<List<PokretnaImovinaBasic>> VratiSve() => Ok(DTOManager.vratiSvuPokretnuImovinu());

        [HttpPost]
        public IActionResult Dodaj([FromBody] PokretnaImovinaBasic dto)
        {
            DTOManager.dodajPokretnuImovinu(dto);
            return StatusCode(201);
        }

        [HttpPut("{id:int}")]
        public IActionResult Azuriraj(int id, [FromBody] PokretnaImovinaBasic dto)
        {
            dto.PokretnaImovinaId = id;
            DTOManager.azurirajPokretnuImovinu(dto);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public IActionResult Obrisi(int id)
        {
            DTOManager.obrisiPokretnuImovinu(id);
            return NoContent();
        }
    }
}

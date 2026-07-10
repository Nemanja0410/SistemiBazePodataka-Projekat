using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    // Fotografije uz stetu (auto i imovinske stete).
    [ApiController]
    [Route("api/[controller]")]
    public class FotografijeController : ControllerBase
    {
        [HttpGet("steta/{stetaId:int}")]
        public ActionResult<List<FotografijaBasic>> VratiZaStetu(int stetaId) =>
            Ok(DTOManager.vratiFotografijeZaStetu(stetaId));

        [HttpPost]
        public IActionResult Dodaj([FromBody] FotografijaBasic dto)
        {
            DTOManager.dodajFotografiju(dto);
            return StatusCode(201);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Obrisi(int id)
        {
            DTOManager.obrisiFotografiju(id);
            return NoContent();
        }
    }
}

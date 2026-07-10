using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NekretnineController : ControllerBase
    {
        // GET api/nekretnine
        [HttpGet]
        public ActionResult<List<NekretninaBasic>> VratiSve() => Ok(DTOManager.vratiSveNekretnine());

        // POST api/nekretnine
        [HttpPost]
        public IActionResult Dodaj([FromBody] NekretninaBasic dto)
        {
            DTOManager.dodajNekretninu(dto);
            return StatusCode(201);
        }

        // PUT api/nekretnine/5
        [HttpPut("{id:int}")]
        public IActionResult Azuriraj(int id, [FromBody] NekretninaBasic dto)
        {
            dto.NekretninaId = id;
            DTOManager.azurirajNekretninu(dto);
            return NoContent();
        }

        // DELETE api/nekretnine/5
        [HttpDelete("{id:int}")]
        public IActionResult Obrisi(int id)
        {
            DTOManager.obrisiNekretninu(id);
            return NoContent();
        }
    }
}

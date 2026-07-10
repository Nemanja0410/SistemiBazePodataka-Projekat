using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    // Istorija promena polise - log, samo dodavanje i citanje (bez update/delete).
    [ApiController]
    [Route("api/[controller]")]
    public class IstorijaPoliseController : ControllerBase
    {
        // GET api/istorijapolise/polisa/5
        [HttpGet("polisa/{polisaId:int}")]
        public ActionResult<List<IstorijaPoliseBasic>> VratiZaPolisu(int polisaId) =>
            Ok(DTOManager.vratiIstorijuPolise(polisaId));

        // POST api/istorijapolise
        [HttpPost]
        public IActionResult Dodaj([FromBody] IstorijaPoliseBasic dto)
        {
            DTOManager.dodajIstorijuPolise(dto);
            return StatusCode(201);
        }
    }
}

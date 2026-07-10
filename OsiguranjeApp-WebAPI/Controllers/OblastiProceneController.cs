using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    // Oblasti procene kojima se bavi konkretan Procenitelj.
    [ApiController]
    [Route("api/[controller]")]
    public class OblastiProceneController : ControllerBase
    {
        // GET api/oblastiprocene/procenitelj/5
        [HttpGet("procenitelj/{proceniteljId:int}")]
        public ActionResult<List<OblastProcBasic>> VratiZaProcenitelja(int proceniteljId) =>
            Ok(DTOManager.vratiOblastiZaProcenitelja(proceniteljId));

        // POST api/oblastiprocene
        [HttpPost]
        public IActionResult Dodaj([FromBody] OblastProcBasic dto)
        {
            DTOManager.dodajOblastProc(dto);
            return StatusCode(201);
        }

        // PUT api/oblastiprocene/5
        [HttpPut("{id:int}")]
        public IActionResult Azuriraj(int id, [FromBody] OblastProcBasic dto)
        {
            dto.OblastId = id;
            DTOManager.azurirajOblastProc(dto);
            return NoContent();
        }

        // DELETE api/oblastiprocene/5
        [HttpDelete("{id:int}")]
        public IActionResult Obrisi(int id)
        {
            DTOManager.obrisiOblastProc(id);
            return NoContent();
        }
    }
}

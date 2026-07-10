using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NaloziController : ControllerBase
    {
        // GET api/nalozi
        [HttpGet]
        public ActionResult<List<NalogPregled>> VratiSve()
        {
            return Ok(DTOManager.vratiSveNaloge());
        }

        // GET api/nalozi/istorija-prijava?nalogId=
        [HttpGet("istorija-prijava")]
        public ActionResult<List<IstorijaPrijaveBasic>> VratiIstorijuPrijava([FromQuery] int? nalogId)
        {
            return Ok(DTOManager.vratiIstorijuPrijava(nalogId));
        }

        // GET api/nalozi/na-cekanju
        [HttpGet("na-cekanju")]
        public ActionResult<List<NalogPregled>> VratiNaCekanju()
        {
            return Ok(DTOManager.vratiNeodobreneNaloge());
        }

        // GET api/nalozi/osoblje-za-registraciju
        [HttpGet("osoblje-za-registraciju")]
        public ActionResult<List<OsobljePregled>> VratiOsobljeZaRegistraciju()
        {
            return Ok(DTOManager.vratiOsobljeZaRegistraciju());
        }

        // POST api/nalozi/registracija
        [HttpPost("registracija")]
        public IActionResult Registruj([FromBody] RegistracijaZahtev zahtev)
        {
            var (uspeh, poruka) = DTOManager.registrujNalog(zahtev);
            return uspeh ? Ok(new { poruka }) : BadRequest(new { poruka });
        }

        // POST api/nalozi/prijava
        [HttpPost("prijava")]
        public IActionResult Prijava([FromBody] PrijavaZahtev zahtev)
        {
            var rezultat = DTOManager.prijaviSe(zahtev.KorisnickoIme ?? "", zahtev.Lozinka ?? "");
            if (!rezultat.Uspesno)
                return Unauthorized(rezultat);

            var token = TokenService.GenerisiToken(rezultat.Nalog!);
            return Ok(new { token, nalog = rezultat.Nalog });
        }

        // POST api/nalozi/5/odobri
        [HttpPost("{id:int}/odobri")]
        public IActionResult Odobri(int id, [FromBody] OdobrenjeZahtev zahtev)
        {
            DTOManager.odobriNalog(id, zahtev.DodeljenaUloga ?? "");
            return NoContent();
        }

        // POST api/nalozi/5/odbij
        [HttpPost("{id:int}/odbij")]
        public IActionResult Odbij(int id)
        {
            DTOManager.odbijNalog(id);
            return NoContent();
        }

        // POST api/nalozi/5/zakljucaj
        [HttpPost("{id:int}/zakljucaj")]
        public IActionResult Zakljucaj(int id)
        {
            DTOManager.zakljucajNalog(id);
            return NoContent();
        }

        // POST api/nalozi/5/otkljucaj
        [HttpPost("{id:int}/otkljucaj")]
        public IActionResult Otkljucaj(int id)
        {
            DTOManager.otkljucajNalog(id);
            return NoContent();
        }

        // DELETE api/nalozi/5
        [HttpDelete("{id:int}")]
        public IActionResult Obrisi(int id)
        {
            DTOManager.obrisiNalog(id);
            return NoContent();
        }

        // POST api/nalozi/5/promeni-lozinku
        [HttpPost("{id:int}/promeni-lozinku")]
        public IActionResult PromeniLozinku(int id, [FromBody] PromenaLozinkeZahtev zahtev)
        {
            var (uspeh, poruka) = DTOManager.promeniLozinku(id, zahtev.StaraLozinka ?? "", zahtev.NovaLozinka ?? "");
            return uspeh ? Ok(new { poruka }) : BadRequest(new { poruka });
        }

        // POST api/nalozi/5/resetuj-lozinku
        [HttpPost("{id:int}/resetuj-lozinku")]
        public IActionResult ResetujLozinku(int id, [FromBody] ResetLozinkeZahtev zahtev)
        {
            DTOManager.resetujLozinku(id, zahtev.PrivremenaLozinka ?? "");
            return NoContent();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Controllers
{
    // Fotografije uz stetu (auto i imovinske stete). Fajlovi se cuvaju na disku servera,
    // NAMERNO van projektnog foldera (vidi Program.cs), u bazi se pamti samo putanja (isto polje kao ranije).
    [ApiController]
    [Route("api/[controller]")]
    public class FotografijeController : ControllerBase
    {
        private static readonly string[] DozvoljeneEkstenzije = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxVelicinaBajtova = 10 * 1024 * 1024; // 10 MB

        private readonly IWebHostEnvironment _env;
        public FotografijeController(IWebHostEnvironment env) { _env = env; }

        // Ista lokacija kao u Program.cs.
        private string UploadsRoot => Path.Combine(_env.ContentRootPath, "Uploads");

        [HttpGet("steta/{stetaId:int}")]
        public ActionResult<List<FotografijaBasic>> VratiZaStetu(int stetaId) =>
            Ok(DTOManager.vratiFotografijeZaStetu(stetaId));

        // POST api/fotografije  (rucni unos putanje, npr. mrezni deljeni folder - ostaje kao opcija)
        [HttpPost]
        public IActionResult Dodaj([FromBody] FotografijaBasic dto)
        {
            DTOManager.dodajFotografiju(dto);
            return StatusCode(201);
        }

        // POST api/fotografije/upload  (multipart/form-data: stetaId, opis, fajl)
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] int stetaId, [FromForm] string? opis, IFormFile fajl)
        {
            if (fajl == null || fajl.Length == 0)
                return BadRequest(new { title = "Nije priložen fajl." });
            if (fajl.Length > MaxVelicinaBajtova)
                return BadRequest(new { title = "Fajl je prevelik (maksimum 10 MB)." });

            string ekstenzija = Path.GetExtension(fajl.FileName).ToLowerInvariant();
            if (Array.IndexOf(DozvoljeneEkstenzije, ekstenzija) < 0)
                return BadRequest(new { title = "Dozvoljeni formati: JPG, PNG, WEBP." });

            string folder = Path.Combine(UploadsRoot, "stete", stetaId.ToString());
            Directory.CreateDirectory(folder);

            string imeFajla = $"{Guid.NewGuid()}{ekstenzija}";
            string punaPutanja = Path.Combine(folder, imeFajla);
            using (var stream = System.IO.File.Create(punaPutanja))
                await fajl.CopyToAsync(stream);

            string relativnaPutanja = $"/uploads/stete/{stetaId}/{imeFajla}";
            DTOManager.dodajFotografiju(new FotografijaBasic { StetaId = stetaId, Putanja = relativnaPutanja, Opis = opis });
            return StatusCode(201, new { putanja = relativnaPutanja });
        }

        [HttpDelete("{id:int}")]
        public IActionResult Obrisi(int id)
        {
            string? putanja = DTOManager.obrisiFotografiju(id);
            if (!string.IsNullOrEmpty(putanja) && putanja.StartsWith("/uploads/"))
            {
                string relativno = putanja.Substring("/uploads/".Length).Replace('/', Path.DirectorySeparatorChar);
                string puniPut = Path.Combine(UploadsRoot, relativno);
                if (System.IO.File.Exists(puniPut)) System.IO.File.Delete(puniPut);
            }
            return NoContent();
        }
    }
}

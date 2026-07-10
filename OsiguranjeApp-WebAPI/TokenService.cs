using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OsiguranjApp.DTOs;

namespace OsiguranjApp
{
    // Isti princip kao konekcioni string u DataLayer.cs: tajni ključ je zakucan radi
    // jednostavnosti studentskog projekta. U pravoj aplikaciji bi ovo bilo u konfiguraciji/tajnama.
    public static class TokenService
    {
        private const string TajniKljuc = "OsiguranjApp-Faza3-Tajni-Kljuc-Za-Potpisivanje-JWT-Tokena-2026-min32char";
        private const string Izdavalac = "OsiguranjApp";
        private static readonly TimeSpan Trajanje = TimeSpan.FromHours(8);

        public static SymmetricSecurityKey PotpisniKljuc =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TajniKljuc));

        public static string IzdavalacValue => Izdavalac;

        public static string GenerisiToken(NalogPregled nalog)
        {
            var claims = new List<Claim>
            {
                new Claim("nalogId", nalog.NalogId.ToString()),
                new Claim(ClaimTypes.Name, nalog.KorisnickoIme ?? ""),
                new Claim(ClaimTypes.Role, nalog.Uloga ?? ""),
                new Claim("imeOsoblja", nalog.ImeOsoblja ?? ""),
                new Claim("prezimeOsoblja", nalog.PrezimeOsoblja ?? ""),
                new Claim("tipOsoblja", nalog.TipOsoblja ?? "")
            };

            var creds = new SigningCredentials(PotpisniKljuc, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Izdavalac,
                audience: Izdavalac,
                claims: claims,
                expires: DateTime.UtcNow.Add(Trajanje),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

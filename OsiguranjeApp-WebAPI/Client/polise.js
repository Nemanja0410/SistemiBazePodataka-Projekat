zahtevajPrijavu();
ucitajNavigaciju("polise");

const greska = document.getElementById("greska");
// Isto kao WinForms PoliseForma._samoPregled: samo ADMIN/AGENT smeju da menjaju, ostali su read-only.
const smeUpis = Auth.imaUlogu("ADMIN", "AGENT");
let odabrana = null;
let punaOdabrana = null;
let trenutnaLista = [];
let sveKlijenti = [];
let svaVozila = [];
let sveNekretnine = [];
let svaPokretnaImovina = [];
let sviUsevi = [];
let sveZivotinje = [];
const offDetalji = new bootstrap.Offcanvas(document.getElementById("offDetalji"));
const modalIzmeni = new bootstrap.Modal(document.getElementById("modalIzmeni"));
const modalKorisnici = new bootstrap.Modal(document.getElementById("modalKorisnici"));
const modalPokrica = new bootstrap.Modal(document.getElementById("modalPokrica"));
const modalIstorija = new bootstrap.Modal(document.getElementById("modalIstorija"));

if (!smeUpis) {
    document.getElementById("btnDodaj").style.display = "none";
    // detaljiAkcije ima Bootstrap klasu d-flex (display:flex !important u stylesheet-u),
    // koja pobedi obican inline style bez !important - zato mora setProperty sa "important".
    document.getElementById("detaljiAkcije").style.setProperty("display", "none", "important");
}

function pill(status) {
    const klasa = "status-" + (status || "").toLowerCase();
    return `<span class="status-pill ${klasa}">${status ?? ""}</span>`;
}

function datum(iso) {
    return iso ? new Date(iso).toLocaleDateString("sr-RS") : "";
}

// putEndpointZaTip: svih 8 tipova polise ima svoju podklasu tabelu i poseban endpoint
// (bazni POST/PUT /polise cuva samo zajednicka polja, bez tip-specificnih).
function putEndpointZaTip(tip) {
    return {
        ZIVOTNO: "/polise/zivotno",
        ZDRAVSTVENO: "/polise/zdravstveno",
        AUTO: "/polise/auto",
        PUTNO: "/polise/putno",
        IMOVINSKO: "/polise/imovinsko",
        POLJOPRIVREDNO: "/polise/poljoprivredno",
        ODGOVORNOST: "/polise/odgovornost",
        SPECIJALIZOVANO: "/polise/specijalizovano"
    }[tip];
}

function selektovaneVrednosti(sel) {
    return Array.from(sel.selectedOptions).map(o => parseInt(o.value));
}

function postaviSelektovane(sel, ids) {
    const skup = new Set((ids || []).map(String));
    for (const opt of sel.options) opt.selected = skup.has(opt.value);
}

async function ucitajPolise() {
    const tip = document.getElementById("filterTip").value;
    const status = document.getElementById("filterStatus").value;
    try {
        const qs = new URLSearchParams();
        if (tip) qs.set("tip", tip);
        if (status) qs.set("status", status);
        trenutnaLista = await apiFetch(`/polise?${qs.toString()}`);
        prikaziPolise(trenutnaLista);
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

function prikaziPolise(lista) {
    const tbody = document.getElementById("tbodyPolise");
    tbody.innerHTML = "";
    for (const p of lista) {
        const tr = document.createElement("tr");
        tr.style.cursor = "pointer";
        tr.innerHTML = `
            <td>${p.brojPolise ?? ""}</td>
            <td>${p.tipOsiguranja ?? ""}</td>
            <td>${p.ugovaracNaziv ?? ""}</td>
            <td>${p.osnovnaPremija?.toFixed(2)} ${p.valuta ?? ""}</td>
            <td>${pill(p.status)}</td>
            <td>${datum(p.datumPocetka)}</td>
            <td>${datum(p.datumIsteka)}</td>
            <td class="text-end"></td>
        `;

        if (smeUpis) {
            const btnIzmeni = document.createElement("button");
            btnIzmeni.className = "btn btn-sm btn-osig-plava me-1";
            btnIzmeni.textContent = "Izmeni";
            btnIzmeni.onclick = async (e) => {
                e.stopPropagation();
                const puna = await apiFetch(`/polise/${p.polisaId}`);
                punaOdabrana = puna;
                otvoriIzmeni(puna);
            };

            const btnObrisi = document.createElement("button");
            btnObrisi.className = "btn btn-sm btn-osig-crvena";
            btnObrisi.textContent = "Obriši";
            btnObrisi.onclick = async (e) => {
                e.stopPropagation();
                if (!confirm(`Obrisati polisu "${p.brojPolise}"?`)) return;
                try { await apiFetch(`/polise/${p.polisaId}`, { method: "DELETE" }); ucitajPolise(); }
                catch (err) { prikaziGresku(greska, err); }
            };

            tr.lastElementChild.appendChild(btnIzmeni);
            tr.lastElementChild.appendChild(btnObrisi);
        }
        tr.addEventListener("click", () => otvoriDetalje(p));
        tbody.appendChild(tr);
    }
    document.getElementById("lblBroj").textContent = `Ukupno: ${lista.length}`;
}

omoguciSortiranje(document.querySelector("thead"), () => trenutnaLista, prikaziPolise);

document.getElementById("filterTip").addEventListener("change", ucitajPolise);
document.getElementById("filterStatus").addEventListener("change", ucitajPolise);
document.getElementById("btnOsvezi").addEventListener("click", ucitajPolise);

// ---------- Detalji + akcije (offcanvas) ----------

function nazivIliId(lista, id, idPolje, tekstFn) {
    const x = lista.find(v => v[idPolje] === id);
    return x ? tekstFn(x) : `#${id}`;
}

function tipSpecificniDetaljiHtml(p) {
    switch (p.tipOsiguranja) {
        case "ZIVOTNO":
            return `
                <h6 class="text-muted mt-3">Životno osiguranje</h6>
                <table class="table table-sm mb-2">
                    <tr><th style="width:40%">Suma osiguranja</th><td>${p.sumaOsiguranja?.toFixed(2) ?? ""} ${p.valuta ?? ""}</td></tr>
                    <tr><th>Tip isplate</th><td>${p.tipIsplate ?? ""}</td></tr>
                </table>`;
        case "ZDRAVSTVENO":
            return `
                <h6 class="text-muted mt-3">Zdravstveno osiguranje</h6>
                <table class="table table-sm mb-2">
                    <tr><th style="width:55%">Mreža ustanova</th><td>${p.mrezaUstanova ?? ""}</td></tr>
                    <tr><th>Limit — specijalisti</th><td>${p.limitSpecijalista?.toFixed(2) ?? ""}</td></tr>
                    <tr><th>Limit — stomatolog</th><td>${p.limitStomatologa?.toFixed(2) ?? ""}</td></tr>
                    <tr><th>Limit — bolničke intervencije</th><td>${p.limitBolnickih?.toFixed(2) ?? ""}</td></tr>
                    <tr><th>Limit — bolnički dan</th><td>${p.limitBolnickiDan?.toFixed(2) ?? ""}</td></tr>
                </table>
                <p class="mb-1"><strong>Obuhvaćena pokrića:</strong></p>
                <p>${p.pokrica ?? "/"}</p>`;
        case "AUTO": {
            let html = `
                <h6 class="text-muted mt-3">Auto osiguranje</h6>
                <table class="table table-sm mb-2">
                    <tr><th style="width:40%">Vozilo</th><td>${p.voziloOpis ?? ""}</td></tr>
                    <tr><th>Bonus-malus klasa</th><td>${p.bonusMalusKlasa ?? ""}</td></tr>
                    <tr><th>Teritorijalno važenje</th><td>${p.teritorijanoVazenje ?? ""}</td></tr>
                </table>`;
            if (p.voziciIds && p.voziciIds.length > 0) {
                html += `<p class="mb-1"><strong>Dodatni vozači:</strong></p><ul class="mb-0">`;
                for (const id of p.voziciIds) html += `<li>${nazivIliId(sveKlijenti, id, "klijentId", k => k.naziv)}</li>`;
                html += `</ul>`;
            }
            return html;
        }
        case "PUTNO": {
            let html = `
                <h6 class="text-muted mt-3">Putno osiguranje</h6>
                <table class="table table-sm mb-2">
                    <tr><th style="width:40%">Destinacije</th><td>${p.destinacije ?? ""}</td></tr>
                    <tr><th>Datum polaska</th><td>${datum(p.datumPolaska)}</td></tr>
                    <tr><th>Datum povratka</th><td>${datum(p.datumPovratka)}</td></tr>
                </table>`;
            if (p.osiguranaLicaIds && p.osiguranaLicaIds.length > 0) {
                html += `<p class="mb-1"><strong>Osigurana lica:</strong></p><ul class="mb-0">`;
                for (const id of p.osiguranaLicaIds) html += `<li>${nazivIliId(sveKlijenti, id, "klijentId", k => k.naziv)}</li>`;
                html += `</ul>`;
            }
            return html;
        }
        case "IMOVINSKO": {
            let html = `
                <h6 class="text-muted mt-3">Imovinsko osiguranje</h6>
                <table class="table table-sm mb-2">
                    <tr><th style="width:40%">Vrste rizika</th><td>${p.vrsteRizika ?? ""}</td></tr>
                </table>`;
            if (p.nekretnineIds && p.nekretnineIds.length > 0) {
                html += `<p class="mb-1"><strong>Nekretnine:</strong></p><ul class="mb-0">`;
                for (const id of p.nekretnineIds) html += `<li>${nazivIliId(sveNekretnine, id, "nekretninaId", n => n.adresa)}</li>`;
                html += `</ul>`;
            }
            if (p.pokretneImovineIds && p.pokretneImovineIds.length > 0) {
                html += `<p class="mb-1 mt-2"><strong>Pokretna imovina:</strong></p><ul class="mb-0">`;
                for (const id of p.pokretneImovineIds) html += `<li>${nazivIliId(svaPokretnaImovina, id, "pokretnaImovinaId", pi => pi.naziv)}</li>`;
                html += `</ul>`;
            }
            return html;
        }
        case "POLJOPRIVREDNO": {
            let html = `<h6 class="text-muted mt-3">Poljoprivredno osiguranje</h6>`;
            if (p.useviIds && p.useviIds.length > 0) {
                html += `<p class="mb-1"><strong>Usevi:</strong></p><ul class="mb-0">`;
                for (const id of p.useviIds) html += `<li>${nazivIliId(sviUsevi, id, "usevId", u => u.vrsta)}</li>`;
                html += `</ul>`;
            }
            if (p.zivotinjeIds && p.zivotinjeIds.length > 0) {
                html += `<p class="mb-1 mt-2"><strong>Životinje:</strong></p><ul class="mb-0">`;
                for (const id of p.zivotinjeIds) html += `<li>${nazivIliId(sveZivotinje, id, "zivotinjaId", z => z.vrsta)}</li>`;
                html += `</ul>`;
            }
            if (!html.includes("<li>")) html += `<p class="text-muted">Nema povezanih useva ni životinja.</p>`;
            return html;
        }
        case "ODGOVORNOST":
            return `
                <h6 class="text-muted mt-3">Osiguranje od odgovornosti</h6>
                <table class="table table-sm mb-2">
                    <tr><th style="width:55%">Vrsta odgovornosti</th><td>${p.vrstaOdgovornosti ?? ""}</td></tr>
                    <tr><th>Limit odgovornosti</th><td>${p.limitOdgovornosti?.toFixed(2) ?? ""}</td></tr>
                </table>`;
        case "SPECIJALIZOVANO":
            return `
                <h6 class="text-muted mt-3">Specijalizovano osiguranje</h6>
                <table class="table table-sm mb-2">
                    <tr><th style="width:40%">Naziv specijalizacije</th><td>${p.nazivSpecijalizacije ?? ""}</td></tr>
                </table>
                <p>${p.opisUslova ?? ""}</p>`;
        default:
            return "";
    }
}

async function otvoriDetalje(p) {
    odabrana = p;
    const naslov = document.getElementById("detaljiNaziv");
    const sadrzaj = document.getElementById("detaljiSadrzaj");
    naslov.textContent = p.brojPolise ?? "Detalji polise";
    sadrzaj.innerHTML = '<p class="text-muted">Učitavanje...</p>';
    offDetalji.show();
    try {
        const puna = await apiFetch(`/polise/${p.polisaId}`);
        punaOdabrana = puna;
        let html = `
            <table class="table table-sm mb-2">
                <tr><th style="width:40%">Tip</th><td>${p.tipOsiguranja ?? ""}</td></tr>
                <tr><th>Status</th><td>${pill(p.status)}</td></tr>
                <tr><th>Ugovarač</th><td>${p.ugovaracNaziv ?? ""}</td></tr>
                <tr><th>Agent</th><td>${p.agentIme ?? "/"}</td></tr>
            </table>
            <h6 class="text-muted mt-3">Finansije</h6>
            <table class="table table-sm mb-2">
                <tr><th style="width:40%">Premija</th><td>${p.osnovnaPremija?.toFixed(2)} ${p.valuta ?? ""}</td></tr>
                <tr><th>Način plaćanja</th><td>${p.nacinPlacanja ?? ""}</td></tr>
            </table>
            <h6 class="text-muted mt-3">Trajanje</h6>
            <table class="table table-sm mb-2">
                <tr><th style="width:40%">Važi od</th><td>${datum(p.datumPocetka)}</td></tr>
                <tr><th>Važi do</th><td>${datum(p.datumIsteka)}</td></tr>`;
        if (p.status === "AKTIVNA") {
            const dana = Math.round((new Date(p.datumIsteka) - new Date()) / 86400000);
            html += `<tr><th>Preostalo</th><td>${dana} dana</td></tr>`;
        }
        html += `</table>`;

        html += tipSpecificniDetaljiHtml(puna);

        if (puna.dodatnaPokrića && puna.dodatnaPokrića.length > 0) {
            html += `<h6 class="text-muted mt-3">Dodatna pokrića (${puna.dodatnaPokrića.length})</h6><ul class="mb-0">`;
            for (const dp of puna.dodatnaPokrića)
                html += `<li>${dp.naziv} — +${(dp.dodatnaPremija ?? 0).toFixed(2)} RSD</li>`;
            html += `</ul>`;
        }
        sadrzaj.innerHTML = html;
    } catch (err) {
        sadrzaj.innerHTML = `<div class="greska-box">${err.message}</div>`;
    }

    // Korisnici isplate postoje samo kod zivotnog osiguranja.
    document.getElementById("redKorisnici").classList.toggle("d-none", p.tipOsiguranja !== "ZIVOTNO");
}

document.getElementById("btnDetaljiObrisi").addEventListener("click", async () => {
    if (!odabrana) return;
    if (!confirm(`Obrisati polisu "${odabrana.brojPolise}"?`)) return;
    try {
        await apiFetch(`/polise/${odabrana.polisaId}`, { method: "DELETE" });
        offDetalji.hide();
        ucitajPolise();
    } catch (err) {
        prikaziGresku(greska, err);
    }
});

document.getElementById("btnDetaljiIzmeni").addEventListener("click", () => {
    if (!punaOdabrana) return;
    otvoriIzmeni(punaOdabrana);
});

// ---------- Korisnici isplate (samo kod zivotnog osiguranja) ----------

document.getElementById("btnDetaljiKorisnici").addEventListener("click", () => {
    if (!odabrana) return;
    otvoriKorisnici(odabrana);
});

async function otvoriKorisnici(p) {
    document.getElementById("greskaKorisnici").classList.add("d-none");
    document.getElementById("korisniciNaslov").textContent = `Korisnici isplate — ${p.brojPolise ?? ""}`;
    document.getElementById("formaKorisnik").reset();
    modalKorisnici.show();
    await ucitajKorisnike(p.polisaId);
}

async function ucitajKorisnike(polisaId) {
    const g = document.getElementById("greskaKorisnici");
    try {
        const lista = await apiFetch(`/korisniciisplate/polisa/${polisaId}`);
        prikaziKorisnike(lista);
    } catch (err) {
        prikaziGresku(g, err);
    }
}

function prikaziKorisnike(lista) {
    const tbody = document.getElementById("tbodyKorisnici");
    tbody.innerHTML = "";
    for (const k of lista) {
        const tr = document.createElement("tr");
        tr.innerHTML = `
            <td>${k.klijentNaziv ?? ""}</td>
            <td>${k.procenatUdela?.toFixed(2)}%</td>
            <td class="text-end"></td>
        `;
        const btnObrisi = document.createElement("button");
        btnObrisi.className = "btn btn-sm btn-osig-crvena";
        btnObrisi.textContent = "Obriši";
        btnObrisi.onclick = async () => {
            if (!confirm(`Obrisati korisnika isplate "${k.klijentNaziv}"?`)) return;
            try { await apiFetch(`/korisniciisplate/${k.korisnikId}`, { method: "DELETE" }); await ucitajKorisnike(odabrana.polisaId); }
            catch (err) { prikaziGresku(document.getElementById("greskaKorisnici"), err); }
        };
        tr.lastElementChild.appendChild(btnObrisi);
        tbody.appendChild(tr);
    }
}

document.getElementById("formaKorisnik").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaKorisnici");
    g.classList.add("d-none");

    const dto = {
        polisaId: odabrana.polisaId,
        klijentId: parseInt(document.getElementById("kiKlijentId").value),
        procenatUdela: parseFloat(document.getElementById("kiProcenat").value)
    };

    try {
        await apiFetch("/korisniciisplate", { method: "POST", body: JSON.stringify(dto) });
        e.target.reset();
        await ucitajKorisnike(odabrana.polisaId);
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Dodatna pokrica (dostupno za svaku polisu) ----------

document.getElementById("btnDetaljiPokrica").addEventListener("click", () => {
    if (!odabrana) return;
    otvoriPokrica(odabrana);
});

async function otvoriPokrica(p) {
    document.getElementById("greskaPokrica").classList.add("d-none");
    document.getElementById("pokricaNaslov").textContent = `Dodatna pokrića — ${p.brojPolise ?? ""}`;
    document.getElementById("formaPokrice").reset();
    document.getElementById("pkFransiza").value = "0";
    document.getElementById("pkPremija").value = "0";
    modalPokrica.show();
    await ucitajPokrica(p.polisaId);
}

async function ucitajPokrica(polisaId) {
    const g = document.getElementById("greskaPokrica");
    try {
        const lista = await apiFetch(`/dodatnapokrica/polisa/${polisaId}`);
        prikaziPokrica(lista);
    } catch (err) {
        prikaziGresku(g, err);
    }
}

function prikaziPokrica(lista) {
    const tbody = document.getElementById("tbodyPokrica");
    tbody.innerHTML = "";
    for (const dp of lista) {
        const tr = document.createElement("tr");
        tr.innerHTML = `
            <td>${dp.naziv ?? ""}</td>
            <td>${dp.opis ?? ""}</td>
            <td>${dp.limitPokrića != null ? dp.limitPokrića.toFixed(2) : "/"}</td>
            <td>${dp.fransiza != null ? dp.fransiza.toFixed(2) + "%" : "0%"}</td>
            <td>${dp.dodatnaPremija?.toFixed(2) ?? "0.00"}</td>
            <td class="text-end"></td>
        `;
        const btnObrisi = document.createElement("button");
        btnObrisi.className = "btn btn-sm btn-osig-crvena";
        btnObrisi.textContent = "Obriši";
        btnObrisi.onclick = async () => {
            if (!confirm(`Obrisati pokriće "${dp.naziv}"?`)) return;
            try { await apiFetch(`/dodatnapokrica/${dp.pokriceId}`, { method: "DELETE" }); await ucitajPokrica(odabrana.polisaId); }
            catch (err) { prikaziGresku(document.getElementById("greskaPokrica"), err); }
        };
        tr.lastElementChild.appendChild(btnObrisi);
        tbody.appendChild(tr);
    }
}

document.getElementById("formaPokrice").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaPokrica");
    g.classList.add("d-none");

    const naziv = document.getElementById("pkNaziv").value.trim();
    if (!naziv) { prikaziGresku(g, new Error("Naziv je obavezan.")); return; }

    const fransiza = parseFloat(document.getElementById("pkFransiza").value) || 0;
    if (fransiza < 0 || fransiza > 100) { prikaziGresku(g, new Error("Franšiza je procenat, mora biti između 0 i 100.")); return; }

    const limitRaw = document.getElementById("pkLimit").value;
    const dto = {
        polisaId: odabrana.polisaId,
        naziv,
        opis: document.getElementById("pkOpis").value.trim(),
        limitPokrića: limitRaw ? parseFloat(limitRaw) : null,
        fransiza,
        dodatnaPremija: parseFloat(document.getElementById("pkPremija").value) || 0
    };

    try {
        await apiFetch("/dodatnapokrica", { method: "POST", body: JSON.stringify(dto) });
        e.target.reset();
        document.getElementById("pkFransiza").value = "0";
        document.getElementById("pkPremija").value = "0";
        await ucitajPokrica(odabrana.polisaId);
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Istorija polise (izmene/obnove/raskidi/mirovanja - samo za citanje) ----------

document.getElementById("btnDetaljiIstorija").addEventListener("click", () => {
    if (!odabrana) return;
    otvoriIstoriju(odabrana);
});

async function otvoriIstoriju(p) {
    const g = document.getElementById("greskaIstorija");
    g.classList.add("d-none");
    document.getElementById("istorijaNaslov").textContent = `Istorija — ${p.brojPolise ?? ""}`;
    modalIstorija.show();
    try {
        const lista = await apiFetch(`/istorijapolise/polisa/${p.polisaId}`);
        const tbody = document.getElementById("tbodyIstorija");
        tbody.innerHTML = "";
        document.getElementById("istorijaPrazno").style.display = lista.length === 0 ? "block" : "none";
        for (const h of lista) {
            const tr = document.createElement("tr");
            tr.innerHTML = `
                <td>${new Date(h.datumPromene).toLocaleString("sr-RS")}</td>
                <td>${pill(h.tipPromene)}</td>
                <td>${h.opis ?? ""}</td>
                <td>${h.korisnikIme ?? "/"}</td>
            `;
            tbody.appendChild(tr);
        }
    } catch (err) {
        prikaziGresku(g, err);
    }
}

// ---------- Combo-ovi i pickeri - deljeni za "Nova polisa" i "Izmeni" ----------

async function ucitajComboove() {
    try {
        sveKlijenti = await apiFetch("/klijenti");
        const agenti = await apiFetch("/osoblje/agenti");
        svaVozila = await apiFetch("/vozila");
        sveNekretnine = await apiFetch("/nekretnine");
        svaPokretnaImovina = await apiFetch("/pokretnaimovina");
        sviUsevi = await apiFetch("/usevi");
        sveZivotinje = await apiFetch("/zivotinje");

        for (const sel of [document.getElementById("ugovaracId"), document.getElementById("ezUgovaracId"), document.getElementById("kiKlijentId")]) {
            for (const k of sveKlijenti) {
                const opt = document.createElement("option");
                opt.value = k.klijentId;
                opt.textContent = k.naziv;
                sel.appendChild(opt);
            }
        }
        for (const sel of [document.getElementById("agentId"), document.getElementById("ezAgentId")]) {
            for (const a of agenti) {
                const opt = document.createElement("option");
                opt.value = a.osobljeId;
                opt.textContent = `${a.ime} ${a.prezime}`;
                sel.appendChild(opt);
            }
        }

        for (const sel of [document.getElementById("vozaci"), document.getElementById("osiguranaLica"),
                            document.getElementById("ezVozaci"), document.getElementById("ezOsiguranaLica")])
            popuniSelect(sel, sveKlijenti, "klijentId", k => k.naziv);

        for (const sel of [document.getElementById("voziloId"), document.getElementById("ezVoziloId")])
            popuniSelect(sel, svaVozila, "voziloId", v => `${v.registracija} — ${v.marka} ${v.model}`);

        for (const sel of [document.getElementById("nekretnine"), document.getElementById("ezNekretnine")])
            popuniSelect(sel, sveNekretnine, "nekretninaId", n => n.adresa);

        for (const sel of [document.getElementById("pokretnaImovina"), document.getElementById("ezPokretnaImovina")])
            popuniSelect(sel, svaPokretnaImovina, "pokretnaImovinaId", pi => pi.naziv);

        for (const sel of [document.getElementById("usevi"), document.getElementById("ezUsevi")])
            popuniSelect(sel, sviUsevi, "usevId", u => `${u.vrsta} (${u.lokacija ?? ""})`);

        for (const sel of [document.getElementById("zivotinje"), document.getElementById("ezZivotinje")])
            popuniSelect(sel, sveZivotinje, "zivotinjaId", z => `${z.vrsta} (${z.lokacija ?? ""})`);
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

// ---------- Nova polisa: prikaz tip-specificnih polja ----------

// Sufiksi id-jeva "redX" (dodaj forma) / "ezRedX" (izmeni forma) sekcija po tipu polise.
const TIP_SUFIKSI = {
    ZIVOTNO: "Zivotno", ZDRAVSTVENO: "Zdravstveno", AUTO: "Auto", PUTNO: "Putno",
    IMOVINSKO: "Imovinsko", POLJOPRIVREDNO: "Poljoprivredno",
    ODGOVORNOST: "Odgovornost", SPECIJALIZOVANO: "Specijalizacija"
};

function azurirajVidljivostTipa(tip, prefiks) {
    for (const [t, sufiks] of Object.entries(TIP_SUFIKSI)) {
        const el = document.getElementById(prefiks ? `${prefiks}Red${sufiks}` : `red${sufiks}`);
        if (el) el.classList.toggle("d-none", t !== tip);
    }
}

document.getElementById("tip").addEventListener("change", (e) => azurirajVidljivostTipa(e.target.value, ""));

function napraviTipSpecificnoTelo(tip, prefiks, g) {
    // Bez prefiksa (Dodaj forma) id pocinje malim slovom (npr. "sumaOsiguranja"),
    // sa "ez" prefiksom (Izmeni forma) je "ezSumaOsiguranja" - otud uslovna promena prvog slova.
    const id = (ime) => document.getElementById(prefiks ? prefiks + ime : ime.charAt(0).toLowerCase() + ime.slice(1));
    switch (tip) {
        case "ZIVOTNO": {
            const suma = id("SumaOsiguranja").value;
            if (!suma) { prikaziGresku(g, new Error("Suma osiguranja je obavezna.")); return null; }
            return { sumaOsiguranja: parseFloat(suma), tipIsplate: id("TipIsplate").value };
        }
        case "ZDRAVSTVENO":
            return {
                mrezaUstanova: id("MrezaUstanova").value.trim(),
                pokrica: id("Pokrica").value.trim(),
                limitSpecijalista: parseFloat(id("LimitSpecijalista").value) || 0,
                limitStomatologa: parseFloat(id("LimitStomatologa").value) || 0,
                limitBolnickih: parseFloat(id("LimitBolnickih").value) || 0,
                limitBolnickiDan: parseFloat(id("LimitBolnickiDan").value) || 0
            };
        case "AUTO": {
            const voziloId = id("VoziloId").value;
            if (!voziloId) { prikaziGresku(g, new Error("Izaberite vozilo.")); return null; }
            return {
                voziloId: parseInt(voziloId),
                bonusMalusKlasa: id("BonusMalusKlasa").value.trim(),
                teritorijanoVazenje: id("TeritorijalnoVazenje").value.trim(),
                voziciIds: selektovaneVrednosti(id("Vozaci"))
            };
        }
        case "PUTNO": {
            const polazak = id("DatumPolaska").value;
            const povratak = id("DatumPovratka").value;
            if (!polazak || !povratak) { prikaziGresku(g, new Error("Datum polaska i povratka su obavezni.")); return null; }
            return {
                destinacije: id("Destinacije").value.trim(),
                datumPolaska: polazak, datumPovratka: povratak,
                osiguranaLicaIds: selektovaneVrednosti(id("OsiguranaLica"))
            };
        }
        case "IMOVINSKO":
            return {
                vrsteRizika: id("VrsteRizika").value.trim(),
                nekretnineIds: selektovaneVrednosti(id("Nekretnine")),
                pokretneImovineIds: selektovaneVrednosti(id("PokretnaImovina"))
            };
        case "POLJOPRIVREDNO":
            return {
                useviIds: selektovaneVrednosti(id("Usevi")),
                zivotinjeIds: selektovaneVrednosti(id("Zivotinje"))
            };
        case "ODGOVORNOST":
            return {
                vrstaOdgovornosti: id("VrstaOdgovornosti").value.trim(),
                limitOdgovornosti: parseFloat(id("LimitOdgovornosti").value) || 0
            };
        case "SPECIJALIZOVANO": {
            const naziv = id("NazivSpecijalizacije").value.trim();
            if (!naziv) { prikaziGresku(g, new Error("Naziv specijalizacije je obavezan.")); return null; }
            return { nazivSpecijalizacije: naziv, opisUslova: id("OpisUslova").value.trim() };
        }
        default:
            return {};
    }
}

document.getElementById("formaPolisa").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaPolisa");
    g.classList.add("d-none");

    const tip = document.getElementById("tip").value;
    const baza = {
        brojPolise: document.getElementById("brojPolise").value.trim(),
        tipOsiguranja: tip,
        ugovaracId: parseInt(document.getElementById("ugovaracId").value),
        agentId: document.getElementById("agentId").value ? parseInt(document.getElementById("agentId").value) : null,
        datumPocetka: document.getElementById("datumPocetka").value,
        datumIsteka: document.getElementById("datumIsteka").value,
        osnovnaPremija: parseFloat(document.getElementById("premija").value),
        valuta: document.getElementById("valuta").value
    };

    const dodatno = napraviTipSpecificnoTelo(tip, "", g);
    if (dodatno === null) return;

    try {
        await apiFetch(putEndpointZaTip(tip), { method: "POST", body: JSON.stringify({ ...baza, ...dodatno }) });
        bootstrap.Modal.getInstance(document.getElementById("modalPolisa")).hide();
        e.target.reset();
        azurirajVidljivostTipa(null, "");
        ucitajPolise();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Izmeni (isto kao WinForms IzmeniPolisaForma, dopunjeno tip-specificnim poljima) ----------

function popuniTipSpecificnaPoljaZaIzmenu(p) {
    switch (p.tipOsiguranja) {
        case "ZIVOTNO":
            document.getElementById("ezSumaOsiguranja").value = p.sumaOsiguranja ?? "";
            document.getElementById("ezTipIsplate").value = p.tipIsplate ?? "JEDNOKRATNA";
            break;
        case "ZDRAVSTVENO":
            document.getElementById("ezMrezaUstanova").value = p.mrezaUstanova ?? "";
            document.getElementById("ezPokrica").value = p.pokrica ?? "";
            document.getElementById("ezLimitSpecijalista").value = p.limitSpecijalista ?? "";
            document.getElementById("ezLimitStomatologa").value = p.limitStomatologa ?? "";
            document.getElementById("ezLimitBolnickih").value = p.limitBolnickih ?? "";
            document.getElementById("ezLimitBolnickiDan").value = p.limitBolnickiDan ?? "";
            break;
        case "AUTO":
            document.getElementById("ezVoziloId").value = p.voziloId ?? "";
            document.getElementById("ezBonusMalusKlasa").value = p.bonusMalusKlasa ?? "";
            document.getElementById("ezTeritorijalnoVazenje").value = p.teritorijanoVazenje ?? "";
            postaviSelektovane(document.getElementById("ezVozaci"), p.voziciIds);
            break;
        case "PUTNO":
            document.getElementById("ezDestinacije").value = p.destinacije ?? "";
            document.getElementById("ezDatumPolaska").value = (p.datumPolaska ?? "").slice(0, 10);
            document.getElementById("ezDatumPovratka").value = (p.datumPovratka ?? "").slice(0, 10);
            postaviSelektovane(document.getElementById("ezOsiguranaLica"), p.osiguranaLicaIds);
            break;
        case "IMOVINSKO":
            document.getElementById("ezVrsteRizika").value = p.vrsteRizika ?? "";
            postaviSelektovane(document.getElementById("ezNekretnine"), p.nekretnineIds);
            postaviSelektovane(document.getElementById("ezPokretnaImovina"), p.pokretneImovineIds);
            break;
        case "POLJOPRIVREDNO":
            postaviSelektovane(document.getElementById("ezUsevi"), p.useviIds);
            postaviSelektovane(document.getElementById("ezZivotinje"), p.zivotinjeIds);
            break;
        case "ODGOVORNOST":
            document.getElementById("ezVrstaOdgovornosti").value = p.vrstaOdgovornosti ?? "";
            document.getElementById("ezLimitOdgovornosti").value = p.limitOdgovornosti ?? "";
            break;
        case "SPECIJALIZOVANO":
            document.getElementById("ezNazivSpecijalizacije").value = p.nazivSpecijalizacije ?? "";
            document.getElementById("ezOpisUslova").value = p.opisUslova ?? "";
            break;
    }
}

function otvoriIzmeni(p) {
    document.getElementById("greskaIzmeni").classList.add("d-none");
    document.getElementById("izmeniNaslov").textContent = `Izmeni polisu — ${p.brojPolise ?? ""}`;
    document.getElementById("ezId").value = p.polisaId;
    document.getElementById("ezUgovaracId").value = p.ugovaracId ?? "";
    document.getElementById("ezAgentId").value = p.agentId ?? "";
    document.getElementById("ezDatumPocetka").value = (p.datumPocetka ?? "").slice(0, 10);
    document.getElementById("ezDatumIsteka").value = (p.datumIsteka ?? "").slice(0, 10);
    document.getElementById("ezPremija").value = p.osnovnaPremija ?? "";
    document.getElementById("ezValuta").value = p.valuta ?? "RSD";
    document.getElementById("ezNacin").value = p.nacinPlacanja ?? "MESECNO";
    document.getElementById("ezStatus").value = p.status ?? "AKTIVNA";

    azurirajVidljivostTipa(p.tipOsiguranja, "ez");
    popuniTipSpecificnaPoljaZaIzmenu(p);

    modalIzmeni.show();
}

document.getElementById("formaIzmeni").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaIzmeni");
    g.classList.add("d-none");

    const id = document.getElementById("ezId").value;
    const ugovaracId = parseInt(document.getElementById("ezUgovaracId").value);
    if (!ugovaracId) { prikaziGresku(g, new Error("Izaberite ugovarača.")); return; }

    const datumPocetka = document.getElementById("ezDatumPocetka").value;
    const datumIsteka = document.getElementById("ezDatumIsteka").value;
    if (datumIsteka <= datumPocetka) { prikaziGresku(g, new Error("Datum isteka mora biti posle početka.")); return; }

    const tip = punaOdabrana.tipOsiguranja;
    const dodatno = napraviTipSpecificnoTelo(tip, "ez", g);
    if (dodatno === null) return;

    // Salje se ceo objekat (kao WinForms IzmeniPolisaForma) da se ne izgubi brojPolise/tipOsiguranja,
    // a tip-specificni PUT endpoint (isti kao pri dodavanju) azurira i zajednicka i tip-specificna polja.
    const dto = {
        ...punaOdabrana,
        ...dodatno,
        ugovaracId,
        agentId: document.getElementById("ezAgentId").value ? parseInt(document.getElementById("ezAgentId").value) : null,
        datumPocetka,
        datumIsteka,
        osnovnaPremija: parseFloat(document.getElementById("ezPremija").value),
        valuta: document.getElementById("ezValuta").value,
        nacinPlacanja: document.getElementById("ezNacin").value,
        status: document.getElementById("ezStatus").value
    };

    try {
        await apiFetch(`${putEndpointZaTip(tip)}/${id}`, { method: "PUT", body: JSON.stringify(dto) });
        modalIzmeni.hide();
        offDetalji.hide();
        ucitajPolise();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

ucitajComboove();
ucitajPolise();

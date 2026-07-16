zahtevajPrijavu();
ucitajNavigaciju("stete");

const greska = document.getElementById("greska");
// Isto kao WinForms SteteForma._samoPregled: samo ADMIN/AGENT smeju da prijave/obrisu stetu.
// LEKAR/PRAVNIK/PROCENITELJ mogu da azuriraju status/faze obrade, zato Izmeni ostaje dostupan.
const smeUpis = Auth.imaUlogu("ADMIN", "AGENT");
// Isto kao DTOManager.dodajProcenu/azurirajProcenu (ADMIN, PROCENITELJ) i obrisiProcenu (ADMIN).
const smeProcena = Auth.imaUlogu("ADMIN", "PROCENITELJ");
const smeBrisanjeProcena = Auth.imaUlogu("ADMIN");
let svePolise = [];
let sveOsoblje = [];
let sveKlijenti = [];
let svaVozila = [];
let odabrana = null;
let punaOdabrana = null;
let trenutnaLista = [];
let faze = [];
let pendingOstecenaLica = [];
let pendingOsteceniPredmeti = [];
let pendingProcene = [];
const offDetalji = new bootstrap.Offcanvas(document.getElementById("offDetalji"));
const modalIzmeni = new bootstrap.Modal(document.getElementById("modalIzmeni"));
const modalFaze = new bootstrap.Modal(document.getElementById("modalFaze"));
const modalFazaForma = new bootstrap.Modal(document.getElementById("modalFazaForma"));
const modalFotografije = new bootstrap.Modal(document.getElementById("modalFotografije"));

// Podtipovi stete koji imaju sopstvenu tabelu/dodatna polja (isti obrazac kao kod polisa).
const TIP_SUFIKSI = { AUTO: "Auto", ZDRAVSTVENA: "Zdravstvena", IMOVINSKA: "Imovinska" };

function endpointZaTip(vrsta) {
    return { AUTO: "/stete/auto", ZDRAVSTVENA: "/stete/zdravstvena", IMOVINSKA: "/stete/imovinska" }[vrsta] ?? null;
}

function azurirajVidljivostTipa(vrsta, prefiks) {
    for (const sufiks of Object.values(TIP_SUFIKSI)) {
        const baznoIme = `Red${sufiks}`;
        const id = prefiks ? prefiks + baznoIme : baznoIme.charAt(0).toLowerCase() + baznoIme.slice(1);
        document.getElementById(id).classList.toggle("d-none", TIP_SUFIKSI[vrsta] !== sufiks);
    }
}

if (!smeUpis) {
    document.getElementById("btnDodaj").style.display = "none";
    document.getElementById("btnDetaljiObrisi").style.display = "none";
}
// Sekcije imaju "d-none" u markupu po default-u (sakrivene dok se ne utvrdi uloga) -
// za smeProcena korisnike se klasa mora ukloniti, ne samo uslovno dodati kad NEMA dozvole.
document.getElementById("redProceneNovi").classList.toggle("d-none", !smeProcena);
document.getElementById("ezRedProcene").classList.toggle("d-none", !smeProcena);

function pill(status) {
    const klasa = "status-" + (status || "").toLowerCase();
    return `<span class="status-pill ${klasa}">${status ?? ""}</span>`;
}

function datum(iso) {
    return iso ? new Date(iso).toLocaleDateString("sr-RS") : "";
}

async function ucitajStete() {
    const status = document.getElementById("filterStatus").value;
    try {
        const qs = new URLSearchParams();
        if (status) qs.set("status", status);
        trenutnaLista = await apiFetch(`/stete?${qs.toString()}`);
        prikaziStete(trenutnaLista);
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

function prikaziStete(lista) {
    const tbody = document.getElementById("tbodyStete");
    tbody.innerHTML = "";
    for (const s of lista) {
        const tr = document.createElement("tr");
        tr.style.cursor = "pointer";
        tr.innerHTML = `
            <td>${s.brojStete ?? ""}</td>
            <td>${s.vrstaStete ?? ""}</td>
            <td>${s.podnosilacNaziv ?? ""}</td>
            <td>${s.brojPolise ?? ""}</td>
            <td>${s.procenjeniIznos != null ? s.procenjeniIznos.toFixed(2) + " " + (s.valuta ?? "RSD") : "/"}</td>
            <td>${pill(s.status)}</td>
            <td>${datum(s.datumPrijave)}</td>
            <td class="text-end"></td>
        `;

        const btnIzmeni = document.createElement("button");
        btnIzmeni.className = "btn btn-sm btn-osig-plava me-1";
        btnIzmeni.textContent = "Izmeni";
        btnIzmeni.onclick = async (e) => {
            e.stopPropagation();
            const puna = await apiFetch(`/stete/${s.stetaId}`);
            punaOdabrana = puna;
            otvoriIzmeni(puna);
        };
        tr.lastElementChild.appendChild(btnIzmeni);

        if (smeUpis) {
            const btnObrisi = document.createElement("button");
            btnObrisi.className = "btn btn-sm btn-osig-crvena";
            btnObrisi.textContent = "Obriši";
            btnObrisi.onclick = async (e) => {
                e.stopPropagation();
                if (!confirm(`Obrisati štetu "${s.brojStete}"?`)) return;
                try { await apiFetch(`/stete/${s.stetaId}`, { method: "DELETE" }); ucitajStete(); }
                catch (err) { prikaziGresku(greska, err); }
            };
            tr.lastElementChild.appendChild(btnObrisi);
        }
        tr.addEventListener("click", () => otvoriDetalje(s));
        tbody.appendChild(tr);
    }
    document.getElementById("lblBroj").textContent = `Ukupno: ${lista.length}`;
}

omoguciSortiranje(document.querySelector("thead"), () => trenutnaLista, prikaziStete);

document.getElementById("filterStatus").addEventListener("change", ucitajStete);
document.getElementById("btnOsvezi").addEventListener("click", ucitajStete);

// ---------- Detalji + akcije (offcanvas) ----------

async function otvoriDetalje(s) {
    odabrana = s;
    const naslov = document.getElementById("detaljiNaziv");
    const sadrzaj = document.getElementById("detaljiSadrzaj");
    naslov.textContent = s.brojStete ?? "Detalji štete";
    sadrzaj.innerHTML = '<p class="text-muted">Učitavanje...</p>';
    offDetalji.show();
    try {
        const puna = await apiFetch(`/stete/${s.stetaId}`);
        punaOdabrana = puna;

        let html = `
            <table class="table table-sm mb-2">
                <tr><th style="width:42%">Vrsta</th><td>${s.vrstaStete ?? ""}</td></tr>
                <tr><th>Status</th><td>${pill(s.status)}</td></tr>
                <tr><th>Polisa</th><td>${s.brojPolise ?? ""}</td></tr>
                <tr><th>Podnosilac</th><td>${s.podnosilacNaziv ?? ""}</td></tr>
            </table>
            <h6 class="text-muted mt-3">Događaj</h6>
            <table class="table table-sm mb-2">
                <tr><th style="width:42%">Datum nastanka</th><td>${datum(s.datumNastanka)}</td></tr>
                <tr><th>Datum prijave</th><td>${datum(s.datumPrijave)}</td></tr>
                <tr><th>Lokacija</th><td>${s.lokacija ?? ""}</td></tr>
            </table>
            <h6 class="text-muted mt-3">Opis</h6>
            <p>${s.opisDogodjaja ?? ""}</p>`;
        if (s.procenjeniIznos != null)
            html += `<p><strong>Procenjeni iznos:</strong> ${s.procenjeniIznos.toFixed(2)} ${s.valuta ?? "RSD"}</p>`;

        if (puna.fazeObrade && puna.fazeObrade.length > 0) {
            html += `<h6 class="text-muted mt-3">Faze obrade (${puna.fazeObrade.length})</h6><ul class="mb-0">`;
            for (const f of puna.fazeObrade) {
                const odgovorno = f.odgovornoLiceIme ? `, ${f.odgovornoLiceIme}` : "";
                html += `<li>${f.nazivFaze ?? ""} — ${f.odluka ?? "u toku"} (${datum(f.datumPocetka)}${odgovorno})</li>`;
            }
            html += `</ul>`;
        }

        const lica = await apiFetch(`/ostecenalica/steta/${s.stetaId}`);
        if (lica.length > 0) {
            html += `<h6 class="text-muted mt-3">Oštećena lica (${lica.length})</h6><ul class="mb-0">`;
            for (const o of lica) {
                const opis = o.opisPovrede ? ` — ${o.opisPovrede}` : "";
                const iznos = o.iznosNaknade != null ? ` (${o.iznosNaknade.toFixed(2)} ${s.valuta ?? "RSD"})` : "";
                html += `<li>${o.klijentNaziv ?? o.imePrezime ?? ""}${opis}${iznos}</li>`;
            }
            html += `</ul>`;
        }

        const predmeti = await apiFetch(`/osteceniPredmeti/steta/${s.stetaId}`);
        if (predmeti.length > 0) {
            html += `<h6 class="text-muted mt-3">Oštećeni predmeti (${predmeti.length})</h6><ul class="mb-0">`;
            for (const o of predmeti) {
                const opis = o.opisOstecenja ? ` — ${o.opisOstecenja}` : "";
                const iznos = o.procenjeniIznos != null ? ` (${o.procenjeniIznos.toFixed(2)} ${s.valuta ?? "RSD"})` : "";
                html += `<li>${o.tipPredmeta ?? ""}${opis}${iznos}</li>`;
            }
            html += `</ul>`;
        }

        if (puna.proceneSteta && puna.proceneSteta.length > 0) {
            html += `<h6 class="text-muted mt-3">Procene štete (${puna.proceneSteta.length})</h6><ul class="mb-0">`;
            for (const p of puna.proceneSteta)
                html += `<li>${datum(p.datumProc)} — ${p.proceniteljIme ?? ""}: ${(p.procenjeniIznos ?? 0).toFixed(2)} ${s.valuta ?? "RSD"}</li>`;
            html += `</ul>`;
        }

        sadrzaj.innerHTML = html;
    } catch (err) {
        sadrzaj.innerHTML = `<div class="greska-box">${err.message}</div>`;
    }
}

document.getElementById("btnDetaljiObrisi").addEventListener("click", async () => {
    if (!odabrana) return;
    if (!confirm(`Obrisati štetu "${odabrana.brojStete}"?`)) return;
    try {
        await apiFetch(`/stete/${odabrana.stetaId}`, { method: "DELETE" });
        offDetalji.hide();
        ucitajStete();
    } catch (err) {
        prikaziGresku(greska, err);
    }
});

document.getElementById("btnDetaljiIzmeni").addEventListener("click", () => {
    if (!punaOdabrana) return;
    otvoriIzmeni(punaOdabrana);
});

// ---------- Faze obrade (isto kao WinForms FazeObradeForma / DodajFazuForma) ----------

document.getElementById("btnDetaljiFaze").addEventListener("click", () => {
    if (!odabrana) return;
    otvoriFaze(odabrana);
});

async function otvoriFaze(s) {
    document.getElementById("greskaFaze").classList.add("d-none");
    document.getElementById("fazeNaslov").textContent = `Faze obrade — ${s.brojStete ?? ""}`;
    modalFaze.show();
    await ucitajFaze(s.stetaId);
}

async function ucitajFaze(stetaId) {
    const g = document.getElementById("greskaFaze");
    try {
        const puna = await apiFetch(`/stete/${stetaId}`);
        punaOdabrana = puna;
        faze = puna.fazeObrade ?? [];
        prikaziFaze();
    } catch (err) {
        prikaziGresku(g, err);
    }
}

function prikaziFaze() {
    const tbody = document.getElementById("tbodyFaze");
    tbody.innerHTML = "";
    for (const f of [...faze].sort((a, b) => a.redniBrojFaze - b.redniBrojFaze)) {
        const tr = document.createElement("tr");
        tr.innerHTML = `
            <td>${f.redniBrojFaze ?? ""}</td>
            <td>${f.nazivFaze ?? ""}</td>
            <td>${datum(f.datumPocetka)}</td>
            <td>${f.datumZavrsetka ? datum(f.datumZavrsetka) : "—"}</td>
            <td>${f.odgovornoLiceIme ?? "/"}</td>
            <td>${pill(f.odluka)}</td>
            <td class="text-end"></td>
        `;
        const btnIzmeni = document.createElement("button");
        btnIzmeni.className = "btn btn-sm btn-osig-plava";
        btnIzmeni.textContent = "Izmeni";
        btnIzmeni.onclick = () => otvoriFazaForma(f);
        tr.lastElementChild.appendChild(btnIzmeni);
        tbody.appendChild(tr);
    }
}

function otvoriFazaForma(f) {
    document.getElementById("greskaFazaForma").classList.add("d-none");
    document.getElementById("fazaFormaNaslov").textContent = f ? `Izmeni fazu — ${f.nazivFaze ?? ""}` : "Dodaj fazu";
    document.getElementById("fzId").value = f ? f.fazaId : "";
    document.getElementById("fzNaziv").value = f?.nazivFaze ?? "";
    document.getElementById("fzDatumPocetka").value = f ? (f.datumPocetka ?? "").slice(0, 10) : new Date().toISOString().slice(0, 10);
    document.getElementById("fzZavrsena").checked = !!f?.datumZavrsetka;
    document.getElementById("fzDatumZavrsetka").value = f?.datumZavrsetka ? f.datumZavrsetka.slice(0, 10) : "";
    document.getElementById("fzRedZavrsetak").classList.toggle("d-none", !f?.datumZavrsetka);
    document.getElementById("fzOdgovornoLice").value = f?.odgovornoLiceId ?? "";
    document.getElementById("fzOdluka").value = f?.odluka ?? "U_TOKU";
    document.getElementById("fzDokumentacija").value = f?.dokumentacija ?? "";
    document.getElementById("fzNapomena").value = f?.napomena ?? "";
    modalFazaForma.show();
}

document.getElementById("btnDodajFazu").addEventListener("click", () => otvoriFazaForma(null));

document.getElementById("fzZavrsena").addEventListener("change", (e) => {
    document.getElementById("fzRedZavrsetak").classList.toggle("d-none", !e.target.checked);
});

document.getElementById("formaFaza").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaFazaForma");
    g.classList.add("d-none");

    const id = document.getElementById("fzId").value;
    const zavrsena = document.getElementById("fzZavrsena").checked;
    const odgovornoLiceId = document.getElementById("fzOdgovornoLice").value;

    const dto = {
        stetaId: punaOdabrana.stetaId,
        redniBrojFaze: id ? faze.find(x => x.fazaId === parseInt(id)).redniBrojFaze : faze.length + 1,
        nazivFaze: document.getElementById("fzNaziv").value.trim(),
        datumPocetka: document.getElementById("fzDatumPocetka").value,
        datumZavrsetka: zavrsena ? document.getElementById("fzDatumZavrsetka").value : null,
        odgovornoLiceId: odgovornoLiceId ? parseInt(odgovornoLiceId) : null,
        odluka: document.getElementById("fzOdluka").value,
        dokumentacija: document.getElementById("fzDokumentacija").value.trim(),
        napomena: document.getElementById("fzNapomena").value.trim()
    };

    try {
        if (id) await apiFetch(`/stete/faze/${id}`, { method: "PUT", body: JSON.stringify({ ...dto, fazaId: parseInt(id) }) });
        else await apiFetch("/stete/faze", { method: "POST", body: JSON.stringify(dto) });
        modalFazaForma.hide();
        await ucitajFaze(punaOdabrana.stetaId);
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Fotografije (isto kao WinForms FotografijeSteteForma) ----------

document.getElementById("btnDetaljiFoto").addEventListener("click", () => {
    if (!odabrana) return;
    otvoriFotografije(odabrana);
});

async function otvoriFotografije(s) {
    document.getElementById("greskaFoto").classList.add("d-none");
    document.getElementById("fotoNaslov").textContent = `Fotografije — ${s.brojStete ?? ""}`;
    document.getElementById("formaFoto").reset();
    modalFotografije.show();
    await ucitajFotografije(s.stetaId);
}

async function ucitajFotografije(stetaId) {
    const g = document.getElementById("greskaFoto");
    try {
        const lista = await apiFetch(`/fotografije/steta/${stetaId}`);
        prikaziFotografije(lista);
    } catch (err) {
        prikaziGresku(g, err);
    }
}

function prikaziFotografije(lista) {
    const tbody = document.getElementById("tbodyFoto");
    tbody.innerHTML = "";
    for (const f of lista) {
        const tr = document.createElement("tr");
        const jeUpload = f.putanja && f.putanja.startsWith("/uploads/");
        const celijaFoto = jeUpload
            ? `<a href="${API_ORIGIN}${f.putanja}" target="_blank"><img src="${API_ORIGIN}${f.putanja}" alt="fotografija" style="height:48px;width:48px;object-fit:cover;border-radius:4px;"></a>`
            : (f.putanja ?? "");
        tr.innerHTML = `
            <td>${celijaFoto}</td>
            <td>${f.opis ?? ""}</td>
            <td>${datum(f.datumDodavanja)}</td>
            <td class="text-end"></td>
        `;
        if (smeUpis) {
            const btnObrisi = document.createElement("button");
            btnObrisi.className = "btn btn-sm btn-osig-crvena";
            btnObrisi.textContent = "Obriši";
            btnObrisi.onclick = async () => {
                if (!confirm(`Obrisati fotografiju "${f.putanja}"?`)) return;
                try { await apiFetch(`/fotografije/${f.fotografijaId}`, { method: "DELETE" }); await ucitajFotografije(odabrana.stetaId); }
                catch (err) { prikaziGresku(document.getElementById("greskaFoto"), err); }
            };
            tr.lastElementChild.appendChild(btnObrisi);
        }
        tbody.appendChild(tr);
    }
}

document.getElementById("formaFoto").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaFoto");
    g.classList.add("d-none");

    const fajl = document.getElementById("ftFajl").files[0];
    if (!fajl) { prikaziGresku(g, new Error("Izaberite fajl (JPG, PNG ili WEBP).")); return; }

    const podaciForme = new FormData();
    podaciForme.append("stetaId", odabrana.stetaId);
    podaciForme.append("opis", document.getElementById("ftOpis").value.trim());
    podaciForme.append("fajl", fajl);

    try {
        // Rucni fetch (ne apiFetch) - multipart/form-data ne sme imati rucno postavljen Content-Type,
        // browser sam dodaje boundary; apiFetch uvek forsira "application/json".
        const headers = {};
        if (Auth.token) headers["Authorization"] = `Bearer ${Auth.token}`;
        const resp = await fetch(`${API_BASE}/fotografije/upload`, { method: "POST", headers, body: podaciForme });
        if (!resp.ok) {
            const podaci = await resp.json().catch(() => null);
            throw new Error(podaci?.title ?? `Greška ${resp.status}`);
        }
        e.target.reset();
        await ucitajFotografije(odabrana.stetaId);
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Ostecena lica (ugradjeno u Dodaj/Izmeni; klijent moze biti registrovan ili slobodan unos imena) ----------

function citajOstecenoLiceInpute(prefiks, g) {
    const id = (ime) => document.getElementById(prefiks + ime);
    const klijentId = id("KlijentId").value;
    const imePrezime = id("ImePrezime").value.trim();
    if (!klijentId && !imePrezime) { prikaziGresku(g, new Error("Izaberite registrovanog klijenta ili unesite ime i prezime.")); return null; }

    const iznosRaw = id("IznosNaknade").value;
    const rez = {
        klijentId: klijentId ? parseInt(klijentId) : null,
        imePrezime,
        opisPovrede: id("OpisPovrede").value.trim(),
        iznosNaknade: iznosRaw ? parseFloat(iznosRaw) : null,
        naziv: klijentId ? sveKlijenti.find(k => k.klijentId === parseInt(klijentId))?.naziv : imePrezime
    };
    id("KlijentId").value = ""; id("ImePrezime").value = ""; id("OpisPovrede").value = ""; id("IznosNaknade").value = "";
    return rez;
}

// Oštećena lica/predmeti/procene nemaju svoje polje za valutu - iznosi se podrazumevaju
// u valuti same štete, pa se ta valuta dohvata iz forme (Dodaj) ili iz ucitane stete (Izmeni).
function trenutnaValutaNova() {
    return document.getElementById("valutaSteta").value || "RSD";
}
function trenutnaValutaIzmena() {
    return document.getElementById("ezValuta").value || punaOdabrana?.valuta || "RSD";
}

function prikaziOstecenaLicaNovi() {
    const tbody = document.getElementById("tbodyOstecenaLicaNovi");
    tbody.innerHTML = "";
    const valuta = trenutnaValutaNova();
    pendingOstecenaLica.forEach((o, i) => {
        const tr = document.createElement("tr");
        tr.innerHTML = `<td style="font-size:.85rem;">${o.naziv}${o.opisPovrede ? " — " + o.opisPovrede : ""}${o.iznosNaknade != null ? ` (${o.iznosNaknade.toFixed(2)} ${valuta})` : ""}</td><td class="text-end" style="width:70px;"></td>`;
        const btnObrisi = document.createElement("button");
        btnObrisi.className = "btn btn-sm btn-osig-crvena";
        btnObrisi.textContent = "Ukloni";
        btnObrisi.onclick = () => { pendingOstecenaLica.splice(i, 1); prikaziOstecenaLicaNovi(); };
        tr.lastElementChild.appendChild(btnObrisi);
        tbody.appendChild(tr);
    });
}

document.getElementById("btnDodajOstecenoLiceNovi").addEventListener("click", () => {
    const o = citajOstecenoLiceInpute("ol", document.getElementById("greskaSteta"));
    if (o === null) return;
    pendingOstecenaLica.push(o);
    prikaziOstecenaLicaNovi();
});

async function ucitajOstecenaLicaIzmena(stetaId) {
    const g = document.getElementById("greskaOsteceni");
    g.classList.add("d-none");
    try {
        const lista = await apiFetch(`/ostecenalica/steta/${stetaId}`);
        prikaziOstecenaLicaIzmena(lista);
    } catch (err) {
        prikaziGresku(g, err);
    }
}

function prikaziOstecenaLicaIzmena(lista) {
    const tbody = document.getElementById("tbodyOstecenaLicaIzmena");
    tbody.innerHTML = "";
    const valuta = trenutnaValutaIzmena();
    for (const o of lista) {
        const naziv = o.klijentNaziv ?? o.imePrezime ?? "";
        const tr = document.createElement("tr");
        tr.innerHTML = `<td style="font-size:.85rem;">${naziv}${o.opisPovrede ? " — " + o.opisPovrede : ""}${o.iznosNaknade != null ? ` (${o.iznosNaknade.toFixed(2)} ${valuta})` : ""}</td><td class="text-end" style="width:70px;"></td>`;
        const btnObrisi = document.createElement("button");
        btnObrisi.className = "btn btn-sm btn-osig-crvena";
        btnObrisi.textContent = "Ukloni";
        btnObrisi.onclick = async () => {
            if (!confirm(`Ukloniti "${naziv}" kao oštećeno lice?`)) return;
            try { await apiFetch(`/ostecenalica/${o.ostecenLiceId}`, { method: "DELETE" }); await ucitajOstecenaLicaIzmena(punaOdabrana.stetaId); }
            catch (err) { prikaziGresku(document.getElementById("greskaOsteceni"), err); }
        };
        tr.lastElementChild.appendChild(btnObrisi);
        tbody.appendChild(tr);
    }
}

document.getElementById("btnDodajOstecenoLiceIzmena").addEventListener("click", async () => {
    const g = document.getElementById("greskaOsteceni");
    const o = citajOstecenoLiceInpute("ezOl", g);
    if (o === null) return;
    try {
        await apiFetch("/ostecenalica", {
            method: "POST",
            body: JSON.stringify({ stetaId: punaOdabrana.stetaId, klijentId: o.klijentId, imePrezime: o.imePrezime, opisPovrede: o.opisPovrede, iznosNaknade: o.iznosNaknade })
        });
        await ucitajOstecenaLicaIzmena(punaOdabrana.stetaId);
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Osteceni predmeti (ugradjeno u Dodaj/Izmeni; jedna steta moze imati vise ostecenih predmeta) ----------

function citajOsteceniPredmetInpute(prefiks, g) {
    const id = (ime) => document.getElementById(prefiks + ime);
    const tipPredmeta = id("TipPredmeta").value.trim();
    if (!tipPredmeta) { prikaziGresku(g, new Error("Unesite tip predmeta.")); return null; }

    const iznosRaw = id("ProcenjeniIznos").value;
    const rez = {
        tipPredmeta,
        opisOstecenja: id("OpisOstecenja").value.trim(),
        procenjeniIznos: iznosRaw ? parseFloat(iznosRaw) : null
    };
    id("TipPredmeta").value = ""; id("OpisOstecenja").value = ""; id("ProcenjeniIznos").value = "";
    return rez;
}

function prikaziOsteceniPredmetiNovi() {
    const tbody = document.getElementById("tbodyOsteceniPredmetiNovi");
    tbody.innerHTML = "";
    const valuta = trenutnaValutaNova();
    pendingOsteceniPredmeti.forEach((o, i) => {
        const tr = document.createElement("tr");
        tr.innerHTML = `<td style="font-size:.85rem;">${o.tipPredmeta}${o.opisOstecenja ? " — " + o.opisOstecenja : ""}${o.procenjeniIznos != null ? ` (${o.procenjeniIznos.toFixed(2)} ${valuta})` : ""}</td><td class="text-end" style="width:70px;"></td>`;
        const btnObrisi = document.createElement("button");
        btnObrisi.className = "btn btn-sm btn-osig-crvena";
        btnObrisi.textContent = "Ukloni";
        btnObrisi.onclick = () => { pendingOsteceniPredmeti.splice(i, 1); prikaziOsteceniPredmetiNovi(); };
        tr.lastElementChild.appendChild(btnObrisi);
        tbody.appendChild(tr);
    });
}

document.getElementById("btnDodajOsteceniPredmetNovi").addEventListener("click", () => {
    const o = citajOsteceniPredmetInpute("op", document.getElementById("greskaSteta"));
    if (o === null) return;
    pendingOsteceniPredmeti.push(o);
    prikaziOsteceniPredmetiNovi();
});

async function ucitajOsteceniPredmetiIzmena(stetaId) {
    const g = document.getElementById("greskaPredmeti");
    g.classList.add("d-none");
    try {
        const lista = await apiFetch(`/osteceniPredmeti/steta/${stetaId}`);
        prikaziOsteceniPredmetiIzmena(lista);
    } catch (err) {
        prikaziGresku(g, err);
    }
}

function prikaziOsteceniPredmetiIzmena(lista) {
    const tbody = document.getElementById("tbodyOsteceniPredmetiIzmena");
    tbody.innerHTML = "";
    const valuta = trenutnaValutaIzmena();
    for (const o of lista) {
        const tr = document.createElement("tr");
        tr.innerHTML = `<td style="font-size:.85rem;">${o.tipPredmeta ?? ""}${o.opisOstecenja ? " — " + o.opisOstecenja : ""}${o.procenjeniIznos != null ? ` (${o.procenjeniIznos.toFixed(2)} ${valuta})` : ""}</td><td class="text-end" style="width:70px;"></td>`;
        const btnObrisi = document.createElement("button");
        btnObrisi.className = "btn btn-sm btn-osig-crvena";
        btnObrisi.textContent = "Ukloni";
        btnObrisi.onclick = async () => {
            if (!confirm(`Ukloniti oštećeni predmet "${o.tipPredmeta}"?`)) return;
            try { await apiFetch(`/osteceniPredmeti/${o.osteceniPredmetId}`, { method: "DELETE" }); await ucitajOsteceniPredmetiIzmena(punaOdabrana.stetaId); }
            catch (err) { prikaziGresku(document.getElementById("greskaPredmeti"), err); }
        };
        tr.lastElementChild.appendChild(btnObrisi);
        tbody.appendChild(tr);
    }
}

document.getElementById("btnDodajOsteceniPredmetIzmena").addEventListener("click", async () => {
    const g = document.getElementById("greskaPredmeti");
    const o = citajOsteceniPredmetInpute("ezOp", g);
    if (o === null) return;
    try {
        await apiFetch("/osteceniPredmeti", { method: "POST", body: JSON.stringify({ stetaId: punaOdabrana.stetaId, ...o }) });
        await ucitajOsteceniPredmetiIzmena(punaOdabrana.stetaId);
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Procene stete (ugradjeno u Dodaj/Izmeni, samo ADMIN/PROCENITELJ; datum, procenitelj, metod, nalaz, iznos, preporuka) ----------

function citajProcenaInpute(prefiks, g) {
    const id = (ime) => document.getElementById(prefiks + ime);
    const proceniteljId = id("ProceniteljId").value;
    if (!proceniteljId) { prikaziGresku(g, new Error("Izaberite procenitelja.")); return null; }

    const iznosRaw = id("ProcenjeniIznos").value;
    const proceniteljNaziv = id("ProceniteljId").selectedOptions[0]?.textContent ?? "";
    const rez = {
        proceniteljId: parseInt(proceniteljId),
        metodProc: id("MetodProc").value.trim(),
        nalaz: id("Nalaz").value.trim(),
        procenjeniIznos: iznosRaw ? parseFloat(iznosRaw) : 0,
        preporuka: id("Preporuka").value.trim(),
        proceniteljNaziv
    };
    id("ProceniteljId").value = ""; id("MetodProc").value = ""; id("Nalaz").value = ""; id("ProcenjeniIznos").value = ""; id("Preporuka").value = "";
    return rez;
}

function prikaziProceneNovi() {
    const tbody = document.getElementById("tbodyProceneNovi");
    tbody.innerHTML = "";
    const valuta = trenutnaValutaNova();
    pendingProcene.forEach((p, i) => {
        const tr = document.createElement("tr");
        tr.innerHTML = `<td style="font-size:.85rem;">${p.proceniteljNaziv} — ${p.metodProc || "/"}: ${p.procenjeniIznos.toFixed(2)} ${valuta}</td><td class="text-end" style="width:70px;"></td>`;
        const btnObrisi = document.createElement("button");
        btnObrisi.className = "btn btn-sm btn-osig-crvena";
        btnObrisi.textContent = "Ukloni";
        btnObrisi.onclick = () => { pendingProcene.splice(i, 1); prikaziProceneNovi(); };
        tr.lastElementChild.appendChild(btnObrisi);
        tbody.appendChild(tr);
    });
}

document.getElementById("btnDodajProcenuNovi").addEventListener("click", () => {
    const p = citajProcenaInpute("pr", document.getElementById("greskaSteta"));
    if (p === null) return;
    pendingProcene.push(p);
    prikaziProceneNovi();
});

async function ucitajProceneIzmena(stetaId) {
    const g = document.getElementById("greskaProcene");
    g.classList.add("d-none");
    try {
        const lista = await apiFetch(`/procenesteta/steta/${stetaId}`);
        prikaziProceneIzmena(lista);
    } catch (err) {
        prikaziGresku(g, err);
    }
}

function prikaziProceneIzmena(lista) {
    const tbody = document.getElementById("tbodyProceneIzmena");
    tbody.innerHTML = "";
    const valuta = trenutnaValutaIzmena();
    for (const p of [...lista].sort((a, b) => new Date(b.datumProc) - new Date(a.datumProc))) {
        const proceniteljOpis = `${p.proceniteljIme ?? ""}`
            + (p.proceniteljBrojLicence ? ` (licenca ${p.proceniteljBrojLicence})` : "")
            + (p.proceniteljOblasti && p.proceniteljOblasti.length ? ` — ${p.proceniteljOblasti.join(", ")}` : "");
        const tr = document.createElement("tr");
        tr.innerHTML = `<td style="font-size:.85rem;">${datum(p.datumProc)} — ${proceniteljOpis}: ${p.metodProc || "/"}, ${(p.procenjeniIznos ?? 0).toFixed(2)} ${valuta}${p.preporuka ? " — " + p.preporuka : ""}</td><td class="text-end" style="width:70px;"></td>`;
        if (smeBrisanjeProcena) {
            const btnObrisi = document.createElement("button");
            btnObrisi.className = "btn btn-sm btn-osig-crvena";
            btnObrisi.textContent = "Ukloni";
            btnObrisi.onclick = async () => {
                if (!confirm("Obrisati ovu procenu štete?")) return;
                try { await apiFetch(`/procenesteta/${p.procenaId}`, { method: "DELETE" }); await ucitajProceneIzmena(punaOdabrana.stetaId); }
                catch (err) { prikaziGresku(document.getElementById("greskaProcene"), err); }
            };
            tr.lastElementChild.appendChild(btnObrisi);
        }
        tbody.appendChild(tr);
    }
}

document.getElementById("btnDodajProcenuIzmena").addEventListener("click", async () => {
    const g = document.getElementById("greskaProcene");
    const p = citajProcenaInpute("ezPr", g);
    if (p === null) return;
    try {
        await apiFetch("/procenesteta", {
            method: "POST",
            body: JSON.stringify({ stetaId: punaOdabrana.stetaId, proceniteljId: p.proceniteljId, metodProc: p.metodProc, nalaz: p.nalaz, procenjeniIznos: p.procenjeniIznos, preporuka: p.preporuka })
        });
        await ucitajProceneIzmena(punaOdabrana.stetaId);
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Combo-ovi (klijenti/polise za "Prijavi stetu") ----------

async function ucitajComboove() {
    const selPodnosilac = document.getElementById("podnosilacId");
    try {
        sveKlijenti = await apiFetch("/klijenti");
        for (const k of sveKlijenti) {
            const opt = document.createElement("option");
            opt.value = k.klijentId;
            opt.textContent = k.naziv;
            selPodnosilac.appendChild(opt);
        }
        for (const sel of [document.getElementById("olKlijentId"), document.getElementById("ezOlKlijentId")]) {
            popuniSelect(sel, sveKlijenti, "klijentId", k => k.naziv);
            sel.insertAdjacentHTML("afterbegin", '<option value="">-- nije registrovan klijent --</option>');
        }

        svePolise = await apiFetch("/polise");
        osveziPoliseZaKlijenta();

        sveOsoblje = await apiFetch("/osoblje");
        const selOdgovorno = document.getElementById("fzOdgovornoLice");
        for (const o of sveOsoblje) {
            const opt = document.createElement("option");
            opt.value = o.osobljeId;
            opt.textContent = `${o.ime} ${o.prezime}`;
            selOdgovorno.appendChild(opt);
        }

        const sviLekari = sveOsoblje.filter(o => o.tipOsoblja === "LEKAR");
        popuniSelect(document.getElementById("lekarId"), sviLekari, "osobljeId", l => `${l.ime} ${l.prezime}`);
        document.getElementById("lekarId").insertAdjacentHTML("afterbegin", '<option value="">-- nije poznato --</option>');
        popuniSelect(document.getElementById("ezLekarId"), sviLekari, "osobljeId", l => `${l.ime} ${l.prezime}`);
        document.getElementById("ezLekarId").insertAdjacentHTML("afterbegin", '<option value="">-- nije poznato --</option>');

        svaVozila = await apiFetch("/vozila");
        popuniSelect(document.getElementById("voziloId"), svaVozila, "voziloId", v => `${v.registracija} — ${v.marka} ${v.model}`);
        document.getElementById("voziloId").insertAdjacentHTML("afterbegin", '<option value="">-- nije poznato --</option>');
        popuniSelect(document.getElementById("ezVoziloId"), svaVozila, "voziloId", v => `${v.registracija} — ${v.marka} ${v.model}`);
        document.getElementById("ezVoziloId").insertAdjacentHTML("afterbegin", '<option value="">-- nije poznato --</option>');

        const sviProcenitelji = sveOsoblje.filter(o => o.tipOsoblja === "PROCENITELJ");
        for (const sel of [document.getElementById("prProceniteljId"), document.getElementById("ezPrProceniteljId")]) {
            popuniSelect(sel, sviProcenitelji, "osobljeId",
                p => `${p.ime} ${p.prezime}${p.brojLicence ? " — licenca " + p.brojLicence : ""}${p.oblasti && p.oblasti.length ? " (" + p.oblasti.join(", ") + ")" : ""}`);
            sel.insertAdjacentHTML("afterbegin", '<option value="">-- izaberite procenitelja --</option>');
        }
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

// Koja vrsta stete odgovara kojem tipu polise -- nema smisla prijaviti npr. zdravstvenu stetu na putno osiguranje.
const MAPA_VRSTA_STETE_TIP_POLISE = {
    AUTO: ["AUTO"],
    ZDRAVSTVENA: ["ZDRAVSTVENO"],
    IMOVINSKA: ["IMOVINSKO"],
    PUTNA: ["PUTNO"],
    ZIVOTNA: ["ZIVOTNO"],
    OSTALO: ["POLJOPRIVREDNO", "ODGOVORNOST", "SPECIJALIZOVANO"]
};

function osveziPoliseZaKlijenta() {
    const klijentId = parseInt(document.getElementById("podnosilacId").value) || 0;
    const vrsta = document.getElementById("vrstaStete").value;
    const dozvoljeniTipovi = MAPA_VRSTA_STETE_TIP_POLISE[vrsta] ?? [];
    const sel = document.getElementById("polisaId");
    sel.innerHTML = "";
    const filtrirane = svePolise.filter(p => p.ugovaracId === klijentId && dozvoljeniTipovi.includes(p.tipOsiguranja));
    if (filtrirane.length === 0) {
        sel.innerHTML = '<option value="">-- klijent nema polisu odgovarajućeg tipa --</option>';
        return;
    }
    for (const p of filtrirane) {
        const opt = document.createElement("option");
        opt.value = p.polisaId;
        opt.textContent = `${p.brojPolise} (${p.tipOsiguranja})`;
        sel.appendChild(opt);
    }
}

document.getElementById("podnosilacId").addEventListener("change", osveziPoliseZaKlijenta);

document.getElementById("vrstaStete").addEventListener("change", (e) => {
    azurirajVidljivostTipa(e.target.value, "");
    osveziPoliseZaKlijenta();
});
azurirajVidljivostTipa(document.getElementById("vrstaStete").value, "");

document.getElementById("modalSteta").addEventListener("show.bs.modal", () => {
    pendingOstecenaLica = [];
    prikaziOstecenaLicaNovi();
    pendingOsteceniPredmeti = [];
    prikaziOsteceniPredmetiNovi();
    pendingProcene = [];
    prikaziProceneNovi();
});

function tipSpecificnoTeloSteta(vrsta, prefiks) {
    const el = (ime) => document.getElementById(prefiks ? prefiks + ime : ime.charAt(0).toLowerCase() + ime.slice(1));

    if (vrsta === "AUTO") {
        const voziloId = el("VoziloId").value;
        return {
            zapisnikPolicije: el("ZapisnikPolicije").value.trim(),
            servis: el("Servis").value.trim(),
            voziloId: voziloId ? parseInt(voziloId) : null
        };
    }
    if (vrsta === "ZDRAVSTVENA") {
        const lekarId = el("LekarId").value;
        return {
            dijagnoza: el("Dijagnoza").value.trim(),
            medicinskaDocumentacija: el("MedicinskaDocumentacija").value.trim(),
            zdravstvenaUstanova: el("ZdravstvenaUstanova").value.trim(),
            lekarId: lekarId ? parseInt(lekarId) : null
        };
    }
    if (vrsta === "IMOVINSKA") {
        return {
            procenaOstecenja: el("ProcenaOstecenja").value.trim(),
            izvodjacSanacije: el("IzvodjacSanacije").value.trim()
        };
    }
    return {};
}

document.getElementById("formaSteta").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaSteta");
    g.classList.add("d-none");

    const polisaId = parseInt(document.getElementById("polisaId").value);
    if (!polisaId) { prikaziGresku(g, new Error("Izaberite polisu.")); return; }

    const vrsta = document.getElementById("vrstaStete").value;
    const iznosRaw = document.getElementById("procenjeniIznos").value;
    const dto = {
        brojStete: document.getElementById("brojStete").value.trim(),
        vrstaStete: vrsta,
        podnosilacId: parseInt(document.getElementById("podnosilacId").value),
        polisaId,
        datumNastanka: document.getElementById("datumNastanka").value,
        lokacija: document.getElementById("lokacija").value.trim(),
        opisDogodjaja: document.getElementById("opisDogodjaja").value.trim(),
        procenjeniIznos: iznosRaw ? parseFloat(iznosRaw) : null,
        valuta: document.getElementById("valutaSteta").value,
        ...tipSpecificnoTeloSteta(vrsta, "")
    };

    try {
        const endpoint = endpointZaTip(vrsta) ?? "/stete";
        const rezultat = await apiFetch(endpoint, { method: "POST", body: JSON.stringify(dto) });

        for (const o of pendingOstecenaLica) {
            try {
                await apiFetch("/ostecenalica", {
                    method: "POST",
                    body: JSON.stringify({ stetaId: rezultat.stetaId, klijentId: o.klijentId, imePrezime: o.imePrezime, opisPovrede: o.opisPovrede, iznosNaknade: o.iznosNaknade })
                });
            } catch (err) { prikaziGresku(g, err); }
        }
        for (const o of pendingOsteceniPredmeti) {
            try {
                await apiFetch("/osteceniPredmeti", { method: "POST", body: JSON.stringify({ stetaId: rezultat.stetaId, ...o }) });
            } catch (err) { prikaziGresku(g, err); }
        }
        for (const p of pendingProcene) {
            try {
                await apiFetch("/procenesteta", {
                    method: "POST",
                    body: JSON.stringify({ stetaId: rezultat.stetaId, proceniteljId: p.proceniteljId, metodProc: p.metodProc, nalaz: p.nalaz, procenjeniIznos: p.procenjeniIznos, preporuka: p.preporuka })
                });
            } catch (err) { prikaziGresku(g, err); }
        }
        pendingOstecenaLica = []; prikaziOstecenaLicaNovi();
        pendingOsteceniPredmeti = []; prikaziOsteceniPredmetiNovi();
        pendingProcene = []; prikaziProceneNovi();

        bootstrap.Modal.getInstance(document.getElementById("modalSteta")).hide();
        e.target.reset();
        azurirajVidljivostTipa(document.getElementById("vrstaStete").value, "");
        osveziPoliseZaKlijenta();
        ucitajStete();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Izmeni (isto kao WinForms IzmeniSteteForma) ----------

function otvoriIzmeni(s) {
    document.getElementById("greskaIzmeni").classList.add("d-none");
    document.getElementById("izmeniNaslov").textContent = `Izmeni štetu — ${s.brojStete ?? ""}`;
    document.getElementById("ezId").value = s.stetaId;
    document.getElementById("ezDatumNastanka").value = (s.datumNastanka ?? "").slice(0, 10);
    document.getElementById("ezLokacija").value = s.lokacija ?? "";
    document.getElementById("ezOpisDogodjaja").value = s.opisDogodjaja ?? "";
    document.getElementById("ezProcenjeniIznos").value = s.procenjeniIznos ?? "";
    document.getElementById("ezValuta").value = s.valuta ?? "RSD";
    document.getElementById("ezStatus").value = s.status ?? "PRIJAVLJENA";

    azurirajVidljivostTipa(s.vrstaStete, "ez");
    if (s.vrstaStete === "AUTO") {
        document.getElementById("ezVoziloId").value = s.voziloId ?? "";
        document.getElementById("ezZapisnikPolicije").value = s.zapisnikPolicije ?? "";
        document.getElementById("ezServis").value = s.servis ?? "";
    } else if (s.vrstaStete === "ZDRAVSTVENA") {
        document.getElementById("ezDijagnoza").value = s.dijagnoza ?? "";
        document.getElementById("ezMedicinskaDocumentacija").value = s.medicinskaDocumentacija ?? "";
        document.getElementById("ezZdravstvenaUstanova").value = s.zdravstvenaUstanova ?? "";
        document.getElementById("ezLekarId").value = s.lekarId ?? "";
    } else if (s.vrstaStete === "IMOVINSKA") {
        document.getElementById("ezProcenaOstecenja").value = s.procenaOstecenja ?? "";
        document.getElementById("ezIzvodjacSanacije").value = s.izvodjacSanacije ?? "";
    }

    document.getElementById("ezOlKlijentId").value = "";
    document.getElementById("ezOlImePrezime").value = "";
    document.getElementById("ezOlOpisPovrede").value = "";
    document.getElementById("ezOlIznosNaknade").value = "";
    ucitajOstecenaLicaIzmena(s.stetaId);

    document.getElementById("ezOpTipPredmeta").value = "";
    document.getElementById("ezOpOpisOstecenja").value = "";
    document.getElementById("ezOpProcenjeniIznos").value = "";
    ucitajOsteceniPredmetiIzmena(s.stetaId);

    if (smeProcena) {
        document.getElementById("ezPrProceniteljId").value = "";
        document.getElementById("ezPrMetodProc").value = "";
        document.getElementById("ezPrNalaz").value = "";
        document.getElementById("ezPrProcenjeniIznos").value = "";
        document.getElementById("ezPrPreporuka").value = "";
        ucitajProceneIzmena(s.stetaId);
    }

    modalIzmeni.show();
}

document.getElementById("formaIzmeni").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaIzmeni");
    g.classList.add("d-none");

    const id = document.getElementById("ezId").value;
    const iznosRaw = document.getElementById("ezProcenjeniIznos").value;
    const vrsta = punaOdabrana.vrstaStete;

    // Salje se ceo objekat (kao WinForms IzmeniSteteForma) da se ne izgube
    // brojStete/vrstaStete/polisaId/podnosilacId koje azurirajStetu prepisuje bukvalno.
    const dto = {
        ...punaOdabrana,
        datumNastanka: document.getElementById("ezDatumNastanka").value,
        lokacija: document.getElementById("ezLokacija").value.trim(),
        opisDogodjaja: document.getElementById("ezOpisDogodjaja").value.trim(),
        procenjeniIznos: iznosRaw ? parseFloat(iznosRaw) : null,
        valuta: document.getElementById("ezValuta").value,
        status: document.getElementById("ezStatus").value,
        ...tipSpecificnoTeloSteta(vrsta, "ez")
    };

    try {
        const endpoint = endpointZaTip(vrsta) ? `${endpointZaTip(vrsta)}/${id}` : `/stete/${id}`;
        await apiFetch(endpoint, { method: "PUT", body: JSON.stringify(dto) });
        modalIzmeni.hide();
        offDetalji.hide();
        ucitajStete();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

ucitajComboove();
ucitajStete();

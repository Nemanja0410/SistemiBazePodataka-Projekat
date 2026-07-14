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
const offDetalji = new bootstrap.Offcanvas(document.getElementById("offDetalji"));
const modalIzmeni = new bootstrap.Modal(document.getElementById("modalIzmeni"));
const modalFaze = new bootstrap.Modal(document.getElementById("modalFaze"));
const modalFazaForma = new bootstrap.Modal(document.getElementById("modalFazaForma"));
const modalFotografije = new bootstrap.Modal(document.getElementById("modalFotografije"));
const modalOsteceni = new bootstrap.Modal(document.getElementById("modalOsteceni"));
const modalOstecenoForma = new bootstrap.Modal(document.getElementById("modalOstecenoForma"));
const modalPredmeti = new bootstrap.Modal(document.getElementById("modalPredmeti"));
const modalPredmetForma = new bootstrap.Modal(document.getElementById("modalPredmetForma"));
const modalProcene = new bootstrap.Modal(document.getElementById("modalProcene"));
const modalProcenaForma = new bootstrap.Modal(document.getElementById("modalProcenaForma"));

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
if (!smeProcena) document.getElementById("btnDodajProcenu").style.display = "none";

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
            for (const f of puna.fazeObrade)
                html += `<li>${f.nazivFaze ?? ""} — ${f.odluka ?? "u toku"}</li>`;
            html += `</ul>`;
        }

        if (puna.proceneSteta && puna.proceneSteta.length > 0) {
            html += `<h6 class="text-muted mt-3">Procene štete (${puna.proceneSteta.length})</h6><ul class="mb-0">`;
            for (const p of puna.proceneSteta)
                html += `<li>${datum(p.datumProc)} — ${p.proceniteljIme ?? ""}: ${(p.procenjeniIznos ?? 0).toFixed(2)}</li>`;
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
        tr.innerHTML = `
            <td>${f.putanja ?? ""}</td>
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

    const dto = {
        stetaId: odabrana.stetaId,
        putanja: document.getElementById("ftPutanja").value.trim(),
        opis: document.getElementById("ftOpis").value.trim()
    };

    try {
        await apiFetch("/fotografije", { method: "POST", body: JSON.stringify(dto) });
        e.target.reset();
        await ucitajFotografije(odabrana.stetaId);
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Ostecena lica (klijent moze biti registrovan ili slobodan unos imena) ----------

document.getElementById("btnDetaljiOsteceni").addEventListener("click", () => {
    if (!odabrana) return;
    otvoriOsteceni(odabrana);
});

async function otvoriOsteceni(s) {
    document.getElementById("greskaOsteceni").classList.add("d-none");
    document.getElementById("ostecenNaslov").textContent = `Oštećena lica — ${s.brojStete ?? ""}`;
    modalOsteceni.show();
    await ucitajOsteceni(s.stetaId);
}

async function ucitajOsteceni(stetaId) {
    const g = document.getElementById("greskaOsteceni");
    try {
        const lista = await apiFetch(`/ostecenalica/steta/${stetaId}`);
        prikaziOsteceni(lista);
    } catch (err) {
        prikaziGresku(g, err);
    }
}

function prikaziOsteceni(lista) {
    const tbody = document.getElementById("tbodyOsteceni");
    tbody.innerHTML = "";
    for (const o of lista) {
        const tr = document.createElement("tr");
        tr.innerHTML = `
            <td>${o.klijentNaziv ?? o.imePrezime ?? ""}</td>
            <td>${o.opisPovrede ?? ""}</td>
            <td>${o.iznosNaknade != null ? o.iznosNaknade.toFixed(2) + " RSD" : "/"}</td>
            <td class="text-end"></td>
        `;
        const btnIzmeni = document.createElement("button");
        btnIzmeni.className = "btn btn-sm btn-osig-plava me-1";
        btnIzmeni.textContent = "Izmeni";
        btnIzmeni.onclick = () => otvoriOstecenoForma(o);
        tr.lastElementChild.appendChild(btnIzmeni);

        if (smeUpis) {
            const btnObrisi = document.createElement("button");
            btnObrisi.className = "btn btn-sm btn-osig-crvena";
            btnObrisi.textContent = "Obriši";
            btnObrisi.onclick = async () => {
                if (!confirm(`Obrisati oštećeno lice "${o.klijentNaziv ?? o.imePrezime}"?`)) return;
                try { await apiFetch(`/ostecenalica/${o.ostecenLiceId}`, { method: "DELETE" }); await ucitajOsteceni(odabrana.stetaId); }
                catch (err) { prikaziGresku(document.getElementById("greskaOsteceni"), err); }
            };
            tr.lastElementChild.appendChild(btnObrisi);
        }
        tbody.appendChild(tr);
    }
}

function otvoriOstecenoForma(o) {
    document.getElementById("greskaOstecenoForma").classList.add("d-none");
    document.getElementById("ostecenoFormaNaslov").textContent = o ? "Izmeni oštećeno lice" : "Dodaj oštećeno lice";
    document.getElementById("olId").value = o ? o.ostecenLiceId : "";
    document.getElementById("olKlijentId").value = o?.klijentId ?? "";
    document.getElementById("olImePrezime").value = o?.imePrezime ?? "";
    document.getElementById("olOpisPovrede").value = o?.opisPovrede ?? "";
    document.getElementById("olIznosNaknade").value = o?.iznosNaknade ?? "";
    modalOstecenoForma.show();
}

document.getElementById("btnDodajOsteceno").addEventListener("click", () => otvoriOstecenoForma(null));

document.getElementById("formaOsteceno").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaOstecenoForma");
    g.classList.add("d-none");

    const id = document.getElementById("olId").value;
    const klijentId = document.getElementById("olKlijentId").value;
    const imePrezime = document.getElementById("olImePrezime").value.trim();
    if (!klijentId && !imePrezime) {
        prikaziGresku(g, new Error("Izaberite registrovanog klijenta ili unesite ime i prezime."));
        return;
    }

    const iznosRaw = document.getElementById("olIznosNaknade").value;
    const dto = {
        stetaId: odabrana.stetaId,
        klijentId: klijentId ? parseInt(klijentId) : null,
        imePrezime,
        opisPovrede: document.getElementById("olOpisPovrede").value.trim(),
        iznosNaknade: iznosRaw ? parseFloat(iznosRaw) : null
    };

    try {
        if (id) await apiFetch(`/ostecenalica/${id}`, { method: "PUT", body: JSON.stringify(dto) });
        else await apiFetch("/ostecenalica", { method: "POST", body: JSON.stringify(dto) });
        modalOstecenoForma.hide();
        await ucitajOsteceni(odabrana.stetaId);
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Osteceni predmeti (jedna steta moze imati vise ostecenih predmeta) ----------

document.getElementById("btnDetaljiPredmeti").addEventListener("click", () => {
    if (!odabrana) return;
    otvoriPredmeti(odabrana);
});

async function otvoriPredmeti(s) {
    document.getElementById("greskaPredmeti").classList.add("d-none");
    document.getElementById("predmetiNaslov").textContent = `Oštećeni predmeti — ${s.brojStete ?? ""}`;
    modalPredmeti.show();
    await ucitajPredmete(s.stetaId);
}

async function ucitajPredmete(stetaId) {
    const g = document.getElementById("greskaPredmeti");
    try {
        const lista = await apiFetch(`/osteceniPredmeti/steta/${stetaId}`);
        prikaziPredmete(lista);
    } catch (err) {
        prikaziGresku(g, err);
    }
}

function prikaziPredmete(lista) {
    const tbody = document.getElementById("tbodyPredmeti");
    tbody.innerHTML = "";
    for (const o of lista) {
        const tr = document.createElement("tr");
        tr.innerHTML = `
            <td>${o.tipPredmeta ?? ""}</td>
            <td>${o.opisOstecenja ?? ""}</td>
            <td>${o.procenjeniIznos != null ? o.procenjeniIznos.toFixed(2) + " RSD" : "/"}</td>
            <td class="text-end"></td>
        `;
        const btnIzmeni = document.createElement("button");
        btnIzmeni.className = "btn btn-sm btn-osig-plava me-1";
        btnIzmeni.textContent = "Izmeni";
        btnIzmeni.onclick = () => otvoriPredmetForma(o);
        tr.lastElementChild.appendChild(btnIzmeni);

        if (smeUpis) {
            const btnObrisi = document.createElement("button");
            btnObrisi.className = "btn btn-sm btn-osig-crvena";
            btnObrisi.textContent = "Obriši";
            btnObrisi.onclick = async () => {
                if (!confirm(`Obrisati oštećeni predmet "${o.tipPredmeta}"?`)) return;
                try { await apiFetch(`/osteceniPredmeti/${o.osteceniPredmetId}`, { method: "DELETE" }); await ucitajPredmete(odabrana.stetaId); }
                catch (err) { prikaziGresku(document.getElementById("greskaPredmeti"), err); }
            };
            tr.lastElementChild.appendChild(btnObrisi);
        }
        tbody.appendChild(tr);
    }
}

function otvoriPredmetForma(o) {
    document.getElementById("greskaPredmetForma").classList.add("d-none");
    document.getElementById("predmetFormaNaslov").textContent = o ? "Izmeni oštećeni predmet" : "Dodaj oštećeni predmet";
    document.getElementById("opId").value = o ? o.osteceniPredmetId : "";
    document.getElementById("opTipPredmeta").value = o?.tipPredmeta ?? "";
    document.getElementById("opOpisOstecenja").value = o?.opisOstecenja ?? "";
    document.getElementById("opProcenjeniIznos").value = o?.procenjeniIznos ?? "";
    modalPredmetForma.show();
}

document.getElementById("btnDodajPredmet").addEventListener("click", () => otvoriPredmetForma(null));

document.getElementById("formaPredmet").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaPredmetForma");
    g.classList.add("d-none");

    const id = document.getElementById("opId").value;
    const iznosRaw = document.getElementById("opProcenjeniIznos").value;
    const dto = {
        stetaId: odabrana.stetaId,
        tipPredmeta: document.getElementById("opTipPredmeta").value.trim(),
        opisOstecenja: document.getElementById("opOpisOstecenja").value.trim(),
        procenjeniIznos: iznosRaw ? parseFloat(iznosRaw) : null
    };

    try {
        if (id) await apiFetch(`/osteceniPredmeti/${id}`, { method: "PUT", body: JSON.stringify(dto) });
        else await apiFetch("/osteceniPredmeti", { method: "POST", body: JSON.stringify(dto) });
        modalPredmetForma.hide();
        await ucitajPredmete(odabrana.stetaId);
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Procene stete (datum, procenitelj, metod, nalaz, iznos, preporuka) ----------

document.getElementById("btnDetaljiProcene").addEventListener("click", () => {
    if (!odabrana) return;
    otvoriProcene(odabrana);
});

async function otvoriProcene(s) {
    document.getElementById("greskaProcene").classList.add("d-none");
    document.getElementById("proceneNaslov").textContent = `Procene štete — ${s.brojStete ?? ""}`;
    modalProcene.show();
    await ucitajProcene(s.stetaId);
}

async function ucitajProcene(stetaId) {
    const g = document.getElementById("greskaProcene");
    try {
        const lista = await apiFetch(`/procenesteta/steta/${stetaId}`);
        prikaziProcene(lista);
    } catch (err) {
        prikaziGresku(g, err);
    }
}

function prikaziProcene(lista) {
    const tbody = document.getElementById("tbodyProcene");
    tbody.innerHTML = "";
    for (const p of [...lista].sort((a, b) => new Date(b.datumProc) - new Date(a.datumProc))) {
        const tr = document.createElement("tr");
        const proceniteljOpis = `${p.proceniteljIme ?? ""}`
            + (p.proceniteljBrojLicence ? ` (licenca ${p.proceniteljBrojLicence})` : "")
            + (p.proceniteljOblasti && p.proceniteljOblasti.length ? ` — ${p.proceniteljOblasti.join(", ")}` : "");
        tr.innerHTML = `
            <td>${datum(p.datumProc)}</td>
            <td>${proceniteljOpis}</td>
            <td>${p.metodProc ?? ""}</td>
            <td>${p.nalaz ?? ""}</td>
            <td>${p.procenjeniIznos != null ? p.procenjeniIznos.toFixed(2) : "/"}</td>
            <td>${p.preporuka ?? ""}</td>
            <td class="text-end"></td>
        `;
        if (smeProcena) {
            const btnIzmeni = document.createElement("button");
            btnIzmeni.className = "btn btn-sm btn-osig-plava me-1";
            btnIzmeni.textContent = "Izmeni";
            btnIzmeni.onclick = () => otvoriProcenaForma(p);
            tr.lastElementChild.appendChild(btnIzmeni);
        }
        if (smeBrisanjeProcena) {
            const btnObrisi = document.createElement("button");
            btnObrisi.className = "btn btn-sm btn-osig-crvena";
            btnObrisi.textContent = "Obriši";
            btnObrisi.onclick = async () => {
                if (!confirm("Obrisati ovu procenu štete?")) return;
                try { await apiFetch(`/procenesteta/${p.procenaId}`, { method: "DELETE" }); await ucitajProcene(odabrana.stetaId); }
                catch (err) { prikaziGresku(document.getElementById("greskaProcene"), err); }
            };
            tr.lastElementChild.appendChild(btnObrisi);
        }
        tbody.appendChild(tr);
    }
}

function otvoriProcenaForma(p) {
    document.getElementById("greskaProcenaForma").classList.add("d-none");
    document.getElementById("procenaFormaNaslov").textContent = p ? "Izmeni procenu" : "Dodaj procenu";
    document.getElementById("prId").value = p ? p.procenaId : "";
    document.getElementById("prProceniteljId").value = p?.proceniteljId ?? "";
    document.getElementById("prMetodProc").value = p?.metodProc ?? "";
    document.getElementById("prNalaz").value = p?.nalaz ?? "";
    document.getElementById("prProcenjeniIznos").value = p?.procenjeniIznos ?? "";
    document.getElementById("prPreporuka").value = p?.preporuka ?? "";
    modalProcenaForma.show();
}

document.getElementById("btnDodajProcenu").addEventListener("click", () => otvoriProcenaForma(null));

document.getElementById("formaProcena").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaProcenaForma");
    g.classList.add("d-none");

    const id = document.getElementById("prId").value;
    const proceniteljId = document.getElementById("prProceniteljId").value;
    if (!proceniteljId) { prikaziGresku(g, new Error("Izaberite procenitelja.")); return; }

    const iznosRaw = document.getElementById("prProcenjeniIznos").value;
    const dto = {
        stetaId: odabrana.stetaId,
        proceniteljId: parseInt(proceniteljId),
        metodProc: document.getElementById("prMetodProc").value.trim(),
        nalaz: document.getElementById("prNalaz").value.trim(),
        procenjeniIznos: iznosRaw ? parseFloat(iznosRaw) : 0,
        preporuka: document.getElementById("prPreporuka").value.trim()
    };

    try {
        if (id) await apiFetch(`/procenesteta/${id}`, { method: "PUT", body: JSON.stringify(dto) });
        else await apiFetch("/procenesteta", { method: "POST", body: JSON.stringify(dto) });
        modalProcenaForma.hide();
        await ucitajProcene(odabrana.stetaId);
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Combo-ovi (klijenti/polise za "Prijavi stetu") ----------

async function ucitajComboove() {
    const selPodnosilac = document.getElementById("podnosilacId");
    const selOsteceni = document.getElementById("olKlijentId");
    try {
        sveKlijenti = await apiFetch("/klijenti");
        for (const k of sveKlijenti) {
            const opt = document.createElement("option");
            opt.value = k.klijentId;
            opt.textContent = k.naziv;
            selPodnosilac.appendChild(opt);

            const opt2 = opt.cloneNode(true);
            selOsteceni.appendChild(opt2);
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
        popuniSelect(document.getElementById("prProceniteljId"), sviProcenitelji, "osobljeId",
            p => `${p.ime} ${p.prezime}${p.brojLicence ? " — licenca " + p.brojLicence : ""}${p.oblasti && p.oblasti.length ? " (" + p.oblasti.join(", ") + ")" : ""}`);
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

function osveziPoliseZaKlijenta() {
    const klijentId = parseInt(document.getElementById("podnosilacId").value) || 0;
    const sel = document.getElementById("polisaId");
    sel.innerHTML = "";
    const filtrirane = svePolise.filter(p => p.ugovaracId === klijentId);
    if (filtrirane.length === 0) {
        sel.innerHTML = '<option value="">-- klijent nema polisa --</option>';
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

document.getElementById("vrstaStete").addEventListener("change", (e) => azurirajVidljivostTipa(e.target.value, ""));
azurirajVidljivostTipa(document.getElementById("vrstaStete").value, "");

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
        await apiFetch(endpoint, { method: "POST", body: JSON.stringify(dto) });
        bootstrap.Modal.getInstance(document.getElementById("modalSteta")).hide();
        e.target.reset();
        azurirajVidljivostTipa(document.getElementById("vrstaStete").value, "");
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

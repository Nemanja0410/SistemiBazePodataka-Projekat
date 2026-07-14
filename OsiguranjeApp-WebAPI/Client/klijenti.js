zahtevajPrijavu();
ucitajNavigaciju("klijenti");

const greska = document.getElementById("greska");
// Isto kao WinForms KlijentiForma._samoPregled: samo ADMIN/AGENT smeju da menjaju, ostali su read-only.
const smeUpis = Auth.imaUlogu("ADMIN", "AGENT");
let debounceId = null;
let odabran = null;
let trenutnaLista = [];
const offDetalji = new bootstrap.Offcanvas(document.getElementById("offDetalji"));
const modalIzmeni = new bootstrap.Modal(document.getElementById("modalIzmeni"));

if (!smeUpis) {
    document.getElementById("dropDodaj").style.display = "none";
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

async function ucitajKlijente() {
    const naziv = document.getElementById("pretraga").value.trim();
    const tip = document.getElementById("filterTip").value;
    try {
        const qs = new URLSearchParams();
        if (naziv) qs.set("naziv", naziv);
        if (tip) qs.set("tip", tip);
        trenutnaLista = await apiFetch(`/klijenti?${qs.toString()}`);
        prikaziKlijente(trenutnaLista);
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

function prikaziKlijente(lista) {
    const tbody = document.getElementById("tbodyKlijenti");
    tbody.innerHTML = "";
    for (const k of lista) {
        const tr = document.createElement("tr");
        tr.style.cursor = "pointer";
        tr.innerHTML = `
            <td>${k.naziv ?? ""}</td>
            <td>${(k.tipKlijenta ?? "").replace("_", " ")}</td>
            <td>${k.telefon ?? ""}</td>
            <td>${k.email ?? ""}</td>
            <td>${pill(k.status)}</td>
            <td>${datum(k.datumRegistracije)}</td>
            <td class="text-end"></td>
        `;

        if (smeUpis) {
            const btnIzmeni = document.createElement("button");
            btnIzmeni.className = "btn btn-sm btn-osig-plava me-1";
            btnIzmeni.textContent = "Izmeni";
            btnIzmeni.onclick = (e) => { e.stopPropagation(); otvoriIzmeni(k); };

            const btnObrisi = document.createElement("button");
            btnObrisi.className = "btn btn-sm btn-osig-crvena";
            btnObrisi.textContent = "Obriši";
            btnObrisi.onclick = async (e) => {
                e.stopPropagation();
                if (!confirm(`Obrisati klijenta "${k.naziv}"? Ovo briše i sve njegove polise i štete.`)) return;
                try { await apiFetch(`/klijenti/${k.klijentId}`, { method: "DELETE" }); ucitajKlijente(); }
                catch (err) { prikaziGresku(greska, err); }
            };

            tr.lastElementChild.appendChild(btnIzmeni);
            tr.lastElementChild.appendChild(btnObrisi);
        }
        tr.addEventListener("click", () => otvoriDetalje(k));
        tbody.appendChild(tr);
    }
    document.getElementById("lblBroj").textContent = `Ukupno: ${lista.length}`;
}

omoguciSortiranje(document.querySelector("thead"), () => trenutnaLista, prikaziKlijente);

document.getElementById("pretraga").addEventListener("input", () => {
    clearTimeout(debounceId);
    debounceId = setTimeout(ucitajKlijente, 300);
});
document.getElementById("filterTip").addEventListener("change", ucitajKlijente);
document.getElementById("btnOsvezi").addEventListener("click", ucitajKlijente);

// ---------- Detalji + akcije (offcanvas) ----------

async function otvoriDetalje(k) {
    odabran = k;
    const naslov = document.getElementById("detaljiNaziv");
    const sadrzaj = document.getElementById("detaljiSadrzaj");
    sadrzaj.innerHTML = '<p class="text-muted">Učitavanje...</p>';
    offDetalji.show();
    try {
        const puni = await apiFetch(`/klijenti/${k.klijentId}/detalji`);
        odabran = puni;
        naslov.textContent = puni.naziv ?? "Detalji klijenta";

        let html = `
            <table class="table table-sm">
                <tr><th style="width:45%">ID</th><td>${puni.klijentId}</td></tr>
                <tr><th>Tip</th><td>${(puni.tipKlijenta ?? "").replace("_", " ")}</td></tr>
                <tr><th>Adresa</th><td>${puni.adresa ?? ""}</td></tr>
                <tr><th>Telefon</th><td>${puni.telefon ?? ""}</td></tr>
                <tr><th>Email</th><td>${puni.email ?? ""}</td></tr>
                <tr><th>Status</th><td>${pill(puni.status)}</td></tr>
                <tr><th>Registrovan</th><td>${datum(puni.datumRegistracije)}</td></tr>
            </table>`;

        if (puni.tipKlijenta === "FIZICKO_LICE") {
            html += `
                <h6 class="mt-3 text-muted">Lični podaci</h6>
                <table class="table table-sm">
                    <tr><th style="width:45%">JMBG</th><td>${puni.jmbg ?? ""}</td></tr>
                    <tr><th>Datum rođenja</th><td>${datum(puni.datumRodjenja)}</td></tr>
                    <tr><th>Zanimanje</th><td>${puni.zanimanje ?? ""}</td></tr>
                </table>`;
        } else if (puni.tipKlijenta === "PRAVNO_LICE") {
            html += `
                <h6 class="mt-3 text-muted">Poslovni podaci</h6>
                <table class="table table-sm">
                    <tr><th style="width:45%">PIB</th><td>${puni.pib ?? ""}</td></tr>
                    <tr><th>Matični broj</th><td>${puni.maticniBroj ?? ""}</td></tr>
                    <tr><th>Delatnost</th><td>${puni.delatnost ?? ""}</td></tr>
                </table>`;
        } else if (puni.tipKlijenta === "JAVNA_INSTITUCIJA") {
            html += `
                <h6 class="mt-3 text-muted">Institucija</h6>
                <table class="table table-sm">
                    <tr><th style="width:45%">PIB</th><td>${puni.pib ?? ""}</td></tr>
                    <tr><th>Matični broj</th><td>${puni.maticniBroj ?? ""}</td></tr>
                    <tr><th>Delatnost</th><td>${puni.delatnost ?? ""}</td></tr>
                    <tr><th>Nivo</th><td>${puni.nivoInstitucije ?? ""}</td></tr>
                </table>`;
        }

        sadrzaj.innerHTML = html;
    } catch (err) {
        sadrzaj.innerHTML = `<div class="greska-box">${err.message}</div>`;
    }
}

document.getElementById("btnDetaljiObrisi").addEventListener("click", async () => {
    if (!odabran) return;
    if (!confirm(`Obrisati klijenta "${odabran.naziv}"? Ovo briše i sve njegove polise i štete.`)) return;
    try {
        await apiFetch(`/klijenti/${odabran.klijentId}`, { method: "DELETE" });
        offDetalji.hide();
        ucitajKlijente();
    } catch (err) {
        prikaziGresku(greska, err);
    }
});

document.getElementById("btnDetaljiIzmeni").addEventListener("click", () => {
    if (!odabran) return;
    otvoriIzmeni(odabran);
});

// ---------- Izmeni (samo zajednicka polja, isto kao WinForms) ----------

function otvoriIzmeni(k) {
    document.getElementById("greskaIzmeni").classList.add("d-none");
    document.getElementById("izmeniNaslov").textContent = `Izmeni klijenta — ${k.naziv ?? ""}`;
    document.getElementById("ezId").value = k.klijentId;
    document.getElementById("ezNaziv").value = k.naziv ?? "";
    document.getElementById("ezAdresa").value = k.adresa ?? "";
    document.getElementById("ezTelefon").value = k.telefon ?? "";
    document.getElementById("ezEmail").value = k.email ?? "";
    document.getElementById("ezStatus").value = k.status ?? "AKTIVAN";
    modalIzmeni.show();
}

document.getElementById("formaIzmeni").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaIzmeni");
    g.classList.add("d-none");

    const id = document.getElementById("ezId").value;
    const naziv = document.getElementById("ezNaziv").value.trim();
    if (!naziv) { prikaziGresku(g, new Error("Naziv je obavezan.")); return; }

    const dto = {
        naziv,
        adresa: document.getElementById("ezAdresa").value.trim(),
        telefon: document.getElementById("ezTelefon").value.trim(),
        email: document.getElementById("ezEmail").value.trim(),
        status: document.getElementById("ezStatus").value
    };

    try {
        await apiFetch(`/klijenti/${id}`, { method: "PUT", body: JSON.stringify(dto) });
        modalIzmeni.hide();
        offDetalji.hide();
        ucitajKlijente();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Dodaj fizicko lice ----------

document.getElementById("formaFizicko").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaFizicko");
    g.classList.add("d-none");
    const dto = {
        naziv: document.getElementById("fNaziv").value.trim(),
        jmbg: document.getElementById("fJmbg").value.trim(),
        datumRodjenja: document.getElementById("fDatumRodjenja").value || null,
        zanimanje: document.getElementById("fZanimanje").value.trim(),
        adresa: document.getElementById("fAdresa").value.trim(),
        telefon: document.getElementById("fTelefon").value.trim(),
        email: document.getElementById("fEmail").value.trim()
    };
    try {
        await apiFetch("/klijenti/fizicko-lice", { method: "POST", body: JSON.stringify(dto) });
        bootstrap.Modal.getInstance(document.getElementById("modalFizicko")).hide();
        e.target.reset();
        ucitajKlijente();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Dodaj pravno lice ----------

document.getElementById("formaPravno").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaPravno");
    g.classList.add("d-none");
    const dto = {
        naziv: document.getElementById("pNaziv").value.trim(),
        pib: document.getElementById("pPib").value.trim(),
        maticniBroj: document.getElementById("pMaticniBroj").value.trim(),
        delatnost: document.getElementById("pDelatnost").value.trim(),
        adresa: document.getElementById("pAdresa").value.trim(),
        telefon: document.getElementById("pTelefon").value.trim(),
        email: document.getElementById("pEmail").value.trim()
    };
    try {
        await apiFetch("/klijenti/pravno-lice", { method: "POST", body: JSON.stringify(dto) });
        bootstrap.Modal.getInstance(document.getElementById("modalPravno")).hide();
        e.target.reset();
        ucitajKlijente();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Dodaj javnu instituciju ----------

document.getElementById("formaInstitucija").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaInstitucija");
    g.classList.add("d-none");
    const dto = {
        naziv: document.getElementById("iNaziv").value.trim(),
        pib: document.getElementById("iPib").value.trim(),
        maticniBroj: document.getElementById("iMaticniBroj").value.trim(),
        delatnost: document.getElementById("iDelatnost").value.trim(),
        nivoInstitucije: document.getElementById("iNivo").value,
        adresa: document.getElementById("iAdresa").value.trim(),
        telefon: document.getElementById("iTelefon").value.trim(),
        email: document.getElementById("iEmail").value.trim()
    };
    if (!dto.naziv) { prikaziGresku(g, new Error("Naziv je obavezan.")); return; }
    if (!dto.pib) { prikaziGresku(g, new Error("PIB je obavezan.")); return; }
    try {
        await apiFetch("/klijenti/javna-institucija", { method: "POST", body: JSON.stringify(dto) });
        bootstrap.Modal.getInstance(document.getElementById("modalInstitucija")).hide();
        e.target.reset();
        document.getElementById("iNivo").value = "GRADSKA";
        ucitajKlijente();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

ucitajKlijente();

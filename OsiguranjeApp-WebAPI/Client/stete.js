zahtevajPrijavu();
ucitajNavigaciju("stete");

const greska = document.getElementById("greska");
let svePolise = [];
let odabrana = null;
let punaOdabrana = null;
let trenutnaLista = [];
const offDetalji = new bootstrap.Offcanvas(document.getElementById("offDetalji"));
const modalIzmeni = new bootstrap.Modal(document.getElementById("modalIzmeni"));

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
            <td>${s.procenjeniIznos != null ? s.procenjeniIznos.toFixed(2) + " RSD" : "/"}</td>
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

        const btnObrisi = document.createElement("button");
        btnObrisi.className = "btn btn-sm btn-osig-crvena";
        btnObrisi.textContent = "Obriši";
        btnObrisi.onclick = async (e) => {
            e.stopPropagation();
            if (!confirm(`Obrisati štetu "${s.brojStete}"?`)) return;
            try { await apiFetch(`/stete/${s.stetaId}`, { method: "DELETE" }); ucitajStete(); }
            catch (err) { prikaziGresku(greska, err); }
        };

        tr.lastElementChild.appendChild(btnIzmeni);
        tr.lastElementChild.appendChild(btnObrisi);
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
            html += `<p><strong>Procenjeni iznos:</strong> ${s.procenjeniIznos.toFixed(2)} RSD</p>`;

        if (puna.fazeObrade && puna.fazeObrade.length > 0) {
            html += `<h6 class="text-muted mt-3">Faze obrade (${puna.fazeObrade.length})</h6><ul class="mb-0">`;
            for (const f of puna.fazeObrade)
                html += `<li>${f.nazivFaze ?? ""} — ${f.odluka ?? "u toku"}</li>`;
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

// ---------- Combo-ovi (klijenti/polise za "Prijavi stetu") ----------

async function ucitajComboove() {
    const selPodnosilac = document.getElementById("podnosilacId");
    try {
        const klijenti = await apiFetch("/klijenti");
        for (const k of klijenti) {
            const opt = document.createElement("option");
            opt.value = k.klijentId;
            opt.textContent = k.naziv;
            selPodnosilac.appendChild(opt);
        }
        svePolise = await apiFetch("/polise");
        osveziPoliseZaKlijenta();
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

document.getElementById("formaSteta").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaSteta");
    g.classList.add("d-none");

    const polisaId = parseInt(document.getElementById("polisaId").value);
    if (!polisaId) { prikaziGresku(g, new Error("Izaberite polisu.")); return; }

    const iznosRaw = document.getElementById("procenjeniIznos").value;
    const dto = {
        brojStete: document.getElementById("brojStete").value.trim(),
        vrstaStete: document.getElementById("vrstaStete").value,
        podnosilacId: parseInt(document.getElementById("podnosilacId").value),
        polisaId,
        datumNastanka: document.getElementById("datumNastanka").value,
        lokacija: document.getElementById("lokacija").value.trim(),
        opisDogodjaja: document.getElementById("opisDogodjaja").value.trim(),
        procenjeniIznos: iznosRaw ? parseFloat(iznosRaw) : null
    };

    try {
        await apiFetch("/stete", { method: "POST", body: JSON.stringify(dto) });
        bootstrap.Modal.getInstance(document.getElementById("modalSteta")).hide();
        e.target.reset();
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
    document.getElementById("ezStatus").value = s.status ?? "PRIJAVLJENA";
    modalIzmeni.show();
}

document.getElementById("formaIzmeni").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaIzmeni");
    g.classList.add("d-none");

    const id = document.getElementById("ezId").value;
    const iznosRaw = document.getElementById("ezProcenjeniIznos").value;

    // Salje se ceo objekat (kao WinForms IzmeniSteteForma) da se ne izgube
    // brojStete/vrstaStete/polisaId/podnosilacId koje azurirajStetu prepisuje bukvalno.
    const dto = {
        ...punaOdabrana,
        datumNastanka: document.getElementById("ezDatumNastanka").value,
        lokacija: document.getElementById("ezLokacija").value.trim(),
        opisDogodjaja: document.getElementById("ezOpisDogodjaja").value.trim(),
        procenjeniIznos: iznosRaw ? parseFloat(iznosRaw) : null,
        status: document.getElementById("ezStatus").value
    };

    try {
        await apiFetch(`/stete/${id}`, { method: "PUT", body: JSON.stringify(dto) });
        modalIzmeni.hide();
        offDetalji.hide();
        ucitajStete();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

ucitajComboove();
ucitajStete();

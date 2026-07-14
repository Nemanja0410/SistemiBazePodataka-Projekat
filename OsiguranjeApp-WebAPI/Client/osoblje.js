zahtevajPrijavu();
ucitajNavigaciju("osoblje");

// Isto kao WinForms MainForm.PrimeniUlogu(): btnOsoblje.Visible = jeAdmin - ova
// stranica uopste nije dostupna nikome sem ADMIN-u, ne samo dugmad za upis.
if (!Auth.imaUlogu("ADMIN")) {
    document.body.innerHTML = '<div class="container pt-5"><div class="greska-box">Ova stranica je dostupna samo administratorima.</div></div>';
    throw new Error("Pristup odbijen");
}

const greska = document.getElementById("greska");
const jeAdmin = true;
let odabran = null;
let trenutnaLista = [];
const offDetalji = new bootstrap.Offcanvas(document.getElementById("offDetalji"));
const modalIzmeni = new bootstrap.Modal(document.getElementById("modalIzmeni"));
const modalOblasti = new bootstrap.Modal(document.getElementById("modalOblasti"));

const NAZIVI_OBLASTI = { VOZILO: "Vozilo", IMOVINA: "Imovina", ZDRAVSTVO: "Zdravstvo", SPECIJALNE_STETE: "Specijalne štete" };

function pill(status) {
    const klasa = "status-" + (status || "").toLowerCase();
    return `<span class="status-pill ${klasa}">${status ?? ""}</span>`;
}

function datum(iso) {
    return iso ? new Date(iso).toLocaleDateString("sr-RS") : "";
}

function tipNaziv(t) {
    return { AGENT: "Agent", PROCENITELJ: "Procenitelj", LEKAR: "Lekar", PRAVNIK: "Pravnik" }[t] ?? (t ?? "");
}

function dodatno(o) {
    return o.tipOsoblja === "AGENT" ? (o.regionRada || "/") : "/";
}

async function ucitajOsoblje() {
    const tip = document.getElementById("filterTip").value;
    try {
        trenutnaLista = await apiFetch(`/osoblje?tip=${tip}`);
        prikaziOsoblje(trenutnaLista);
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

function prikaziOsoblje(lista) {
    const tbody = document.getElementById("tbodyOsoblje");
    tbody.innerHTML = "";
    for (const o of lista) {
        const tr = document.createElement("tr");
        tr.style.cursor = "pointer";
        tr.innerHTML = `
            <td>${o.ime ?? ""} ${o.prezime ?? ""}</td>
            <td>${tipNaziv(o.tipOsoblja)}</td>
            <td>${o.telefon ?? ""}</td>
            <td>${o.email ?? ""}</td>
            <td>${dodatno(o)}</td>
            <td>${pill(o.status)}</td>
            <td>${datum(o.datumAngazovanja)}</td>
            <td class="text-end"></td>
        `;

        if (jeAdmin) {
            const btnIzmeni = document.createElement("button");
            btnIzmeni.className = "btn btn-sm btn-osig-plava me-1";
            btnIzmeni.textContent = "Izmeni";
            btnIzmeni.onclick = (e) => { e.stopPropagation(); otvoriIzmeni(o); };

            const btnObrisi = document.createElement("button");
            btnObrisi.className = "btn btn-sm btn-osig-crvena";
            btnObrisi.textContent = "Obriši";
            btnObrisi.onclick = async (e) => {
                e.stopPropagation();
                if (!confirm(`Obrisati zaposlenog "${o.ime} ${o.prezime}"?`)) return;
                try { await apiFetch(`/osoblje/${o.osobljeId}`, { method: "DELETE" }); ucitajOsoblje(); }
                catch (err) { prikaziGresku(greska, err); }
            };

            tr.lastElementChild.appendChild(btnIzmeni);
            tr.lastElementChild.appendChild(btnObrisi);
        }

        tr.addEventListener("click", () => otvoriDetalje(o));
        tbody.appendChild(tr);
    }
    document.getElementById("lblBroj").textContent = `Ukupno: ${lista.length}`;
}

omoguciSortiranje(document.querySelector("thead"), () => trenutnaLista, prikaziOsoblje);

document.getElementById("filterTip").addEventListener("change", ucitajOsoblje);
document.getElementById("btnOsvezi").addEventListener("click", ucitajOsoblje);

// ---------- Detalji (offcanvas) ----------

function otvoriDetalje(o) {
    odabran = o;
    document.getElementById("detaljiNaziv").textContent = `${o.ime ?? ""} ${o.prezime ?? ""}`;
    let html = `
        <table class="table table-sm mb-2">
            <tr><th style="width:42%">Tip</th><td>${tipNaziv(o.tipOsoblja)}</td></tr>
            <tr><th>Status</th><td>${pill(o.status)}</td></tr>
            <tr><th>Adresa</th><td>${o.adresa ?? ""}</td></tr>
            <tr><th>Telefon</th><td>${o.telefon ?? ""}</td></tr>
            <tr><th>Email</th><td>${o.email ?? ""}</td></tr>
            <tr><th>Angažovan od</th><td>${datum(o.datumAngazovanja)}</td></tr>
        </table>`;

    document.getElementById("btnDetaljiOblasti").classList.toggle("d-none", o.tipOsoblja !== "PROCENITELJ");

    if (o.tipOsoblja === "AGENT") {
        html += `
            <h6 class="text-muted mt-3">Agent — detalji</h6>
            <table class="table table-sm mb-2">
                <tr><th style="width:42%">Tip agenta</th><td>${o.tipAgenta ?? ""}</td></tr>
                <tr><th>Licenca</th><td>${o.licenca ?? ""}</td></tr>
                <tr><th>Region rada</th><td>${o.regionRada ?? ""}</td></tr>
                <tr><th>Provizija</th><td>${(o.provizijaProcenat ?? 0).toFixed(2)}%</td></tr>
            </table>
            <h6 class="text-muted mt-3">Prodajni rezultati</h6>
            <table class="table table-sm mb-2">
                <tr><th style="width:42%">Prodato polisa</th><td>${o.brojPolisa ?? 0}</td></tr>
                <tr><th>Ukupna premija</th><td>${(o.ukupnaPremija ?? 0).toFixed(2)}</td></tr>
            </table>`;
        if (o.polise && o.polise.length > 0) {
            html += `<h6 class="text-muted mt-3">Polise (${o.polise.length})</h6><ul class="mb-0">`;
            for (const p of o.polise)
                html += `<li>${p.brojPolise ?? ""} — ${p.tipOsiguranja ?? ""} (${p.status ?? ""})</li>`;
            html += `</ul>`;
        }
    } else if (o.tipOsoblja === "PROCENITELJ") {
        html += `
            <h6 class="text-muted mt-3">Procenitelj — detalji</h6>
            <table class="table table-sm mb-2">
                <tr><th style="width:42%">Broj licence</th><td>${o.brojLicence ?? ""}</td></tr>
            </table>`;
        if (o.oblasti && o.oblasti.length > 0) {
            html += `<h6 class="text-muted mt-3">Oblasti procene</h6><ul class="mb-0">`;
            for (const ob of o.oblasti) html += `<li>${ob}</li>`;
            html += `</ul>`;
        }
    } else if (o.tipOsoblja === "LEKAR") {
        html += `
            <h6 class="text-muted mt-3">Lekar — detalji</h6>
            <table class="table table-sm mb-2">
                <tr><th style="width:42%">Specijalizacija</th><td>${o.specijalizacija ?? ""}</td></tr>
                <tr><th>Broj licence</th><td>${o.licencaBroj ?? ""}</td></tr>
            </table>`;
    } else if (o.tipOsoblja === "PRAVNIK") {
        html += `
            <h6 class="text-muted mt-3">Pravnik — detalji</h6>
            <table class="table table-sm mb-2">
                <tr><th style="width:42%">Tip pravnika</th><td>${o.tipPravnika ?? ""}</td></tr>
                <tr><th>Broj advokata</th><td>${o.barBroj ?? ""}</td></tr>
            </table>`;
    }

    document.getElementById("detaljiSadrzaj").innerHTML = html;
    offDetalji.show();
}

document.getElementById("btnDetaljiObrisi")?.addEventListener("click", async () => {
    if (!odabran) return;
    if (!confirm(`Obrisati zaposlenog "${odabran.ime} ${odabran.prezime}"?`)) return;
    try {
        await apiFetch(`/osoblje/${odabran.osobljeId}`, { method: "DELETE" });
        offDetalji.hide();
        ucitajOsoblje();
    } catch (err) {
        prikaziGresku(greska, err);
    }
});

document.getElementById("btnDetaljiIzmeni")?.addEventListener("click", () => {
    if (!odabran) return;
    otvoriIzmeni(odabran);
});

// ---------- Oblasti procene (samo za Procenitelja) ----------

async function ucitajOblasti() {
    if (!odabran) return;
    const g = document.getElementById("greskaOblasti");
    g.classList.add("d-none");
    try {
        const lista = await apiFetch(`/oblastiprocene/procenitelj/${odabran.osobljeId}`);

        const ul = document.getElementById("listaOblasti");
        ul.innerHTML = "";
        if (lista.length === 0) {
            ul.innerHTML = `<li class="list-group-item text-muted">Nijedna oblast nije dodeljena.</li>`;
        }
        for (const ob of lista) {
            const li = document.createElement("li");
            li.className = "list-group-item d-flex justify-content-between align-items-center";
            li.innerHTML = `<span>${NAZIVI_OBLASTI[ob.oblast] ?? ob.oblast}</span>`;
            const btn = document.createElement("button");
            btn.type = "button";
            btn.className = "btn btn-sm btn-osig-crvena";
            btn.textContent = "Ukloni";
            btn.onclick = async () => {
                try { await apiFetch(`/oblastiprocene/${ob.oblastId}`, { method: "DELETE" }); ucitajOblasti(); }
                catch (err) { prikaziGresku(g, err); }
            };
            li.appendChild(btn);
            ul.appendChild(li);
        }

        const preostale = Object.keys(NAZIVI_OBLASTI).filter(k => !lista.some(ob => ob.oblast === k));
        const sel = document.getElementById("novaOblast");
        sel.innerHTML = "";
        for (const k of preostale) {
            const opt = document.createElement("option");
            opt.value = k;
            opt.textContent = NAZIVI_OBLASTI[k];
            sel.appendChild(opt);
        }
        document.getElementById("formaOblasti").classList.toggle("d-none", preostale.length === 0);
    } catch (err) {
        prikaziGresku(g, err);
    }
}

document.getElementById("btnDetaljiOblasti")?.addEventListener("click", () => {
    if (!odabran) return;
    document.getElementById("oblastiNaslov").textContent = `Oblasti procene — ${odabran.ime ?? ""} ${odabran.prezime ?? ""}`;
    ucitajOblasti();
    modalOblasti.show();
});

document.getElementById("modalOblasti").addEventListener("hidden.bs.modal", async () => {
    if (!odabran) return;
    await ucitajOsoblje();
    const svez = trenutnaLista.find(o => o.osobljeId === odabran.osobljeId);
    if (svez) otvoriDetalje(svez);
});

document.getElementById("formaOblasti").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaOblasti");
    g.classList.add("d-none");
    try {
        await apiFetch("/oblastiprocene", {
            method: "POST",
            body: JSON.stringify({ proceniteljId: odabran.osobljeId, oblast: document.getElementById("novaOblast").value })
        });
        ucitajOblasti();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Dodaj agenta ----------

document.getElementById("formaAgent").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaAgent");
    g.classList.add("d-none");

    const jmbg = document.getElementById("aJmbg").value.trim();
    if (jmbg.length !== 13) { prikaziGresku(g, new Error("JMBG mora imati 13 cifara.")); return; }
    const licenca = document.getElementById("aLicenca").value.trim();
    if (!licenca) { prikaziGresku(g, new Error("Licenca je obavezna za agente.")); return; }

    const dto = {
        ime: document.getElementById("aIme").value.trim(),
        prezime: document.getElementById("aPrezime").value.trim(),
        jmbg,
        adresa: document.getElementById("aAdresa").value.trim(),
        telefon: document.getElementById("aTelefon").value.trim(),
        email: document.getElementById("aEmail").value.trim(),
        tipAgenta: document.getElementById("aTipAgenta").value,
        licenca,
        regionRada: document.getElementById("aRegion").value.trim(),
        provizijaProcenat: parseFloat(document.getElementById("aProvizija").value) || 0,
        status: document.getElementById("aStatus").value,
        tipOsoblja: "AGENT"
    };

    try {
        await apiFetch("/osoblje/agenti", { method: "POST", body: JSON.stringify(dto) });
        bootstrap.Modal.getInstance(document.getElementById("modalAgent")).hide();
        e.target.reset();
        ucitajOsoblje();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Dodaj ostalo (procenitelj/lekar/pravnik/ostalo) ----------

function azurirajVidljivostOstalo() {
    const tip = document.getElementById("oTip").value;
    document.getElementById("oRedPravnik1").classList.toggle("d-none", tip !== "PRAVNIK");
    document.getElementById("oRedPravnik2").classList.toggle("d-none", tip !== "PRAVNIK");
    document.getElementById("oRedProcenitelj").classList.toggle("d-none", tip !== "PROCENITELJ");
}
document.getElementById("oTip").addEventListener("change", azurirajVidljivostOstalo);
azurirajVidljivostOstalo();

document.getElementById("formaOstalo").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaOstalo");
    g.classList.add("d-none");

    const jmbg = document.getElementById("oJmbg").value.trim();
    if (jmbg.length !== 13) { prikaziGresku(g, new Error("JMBG mora imati 13 cifara.")); return; }

    const tip = document.getElementById("oTip").value;
    const dto = {
        tipOsoblja: tip,
        ime: document.getElementById("oIme").value.trim(),
        prezime: document.getElementById("oPrezime").value.trim(),
        jmbg,
        adresa: document.getElementById("oAdresa").value.trim(),
        telefon: document.getElementById("oTelefon").value.trim(),
        email: document.getElementById("oEmail").value.trim(),
        status: document.getElementById("oStatus").value,
        tipPravnika: tip === "PRAVNIK" ? document.getElementById("oTipPravnika").value : null,
        barBroj: tip === "PRAVNIK" ? document.getElementById("oBarBroj").value.trim() : null,
        brojLicence: tip === "PROCENITELJ" ? document.getElementById("oBrojLicence").value.trim() : null
    };

    try {
        await apiFetch("/osoblje", { method: "POST", body: JSON.stringify(dto) });
        bootstrap.Modal.getInstance(document.getElementById("modalOstalo")).hide();
        e.target.reset();
        azurirajVidljivostOstalo();
        ucitajOsoblje();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Izmeni (samo zajednicka polja, isto kao WinForms IzmeniOsobljeForma) ----------

function otvoriIzmeni(o) {
    document.getElementById("greskaIzmeni").classList.add("d-none");
    document.getElementById("izmeniNaslov").textContent = `Izmeni — ${o.ime ?? ""} ${o.prezime ?? ""}`;
    document.getElementById("ezId").value = o.osobljeId;
    document.getElementById("ezIme").value = o.ime ?? "";
    document.getElementById("ezPrezime").value = o.prezime ?? "";
    document.getElementById("ezAdresa").value = o.adresa ?? "";
    document.getElementById("ezTelefon").value = o.telefon ?? "";
    document.getElementById("ezEmail").value = o.email ?? "";
    document.getElementById("ezStatus").value = o.status ?? "AKTIVAN";

    const jePravnik = o.tipOsoblja === "PRAVNIK";
    const jeProcenitelj = o.tipOsoblja === "PROCENITELJ";
    document.getElementById("ezRedPravnik1").classList.toggle("d-none", !jePravnik);
    document.getElementById("ezRedPravnik2").classList.toggle("d-none", !jePravnik);
    document.getElementById("ezRedProcenitelj").classList.toggle("d-none", !jeProcenitelj);
    if (jePravnik) {
        document.getElementById("ezTipPravnika").value = o.tipPravnika ?? "INTERNI";
        document.getElementById("ezBarBroj").value = o.barBroj ?? "";
    }
    if (jeProcenitelj) {
        document.getElementById("ezBrojLicence").value = o.brojLicence ?? "";
    }

    offDetalji.hide();
    modalIzmeni.show();
}

document.getElementById("formaIzmeni").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaIzmeni");
    g.classList.add("d-none");

    const id = document.getElementById("ezId").value;
    const jePravnik = !document.getElementById("ezRedPravnik1").classList.contains("d-none");
    const jeProcenitelj = !document.getElementById("ezRedProcenitelj").classList.contains("d-none");

    const dto = {
        ime: document.getElementById("ezIme").value.trim(),
        prezime: document.getElementById("ezPrezime").value.trim(),
        adresa: document.getElementById("ezAdresa").value.trim(),
        telefon: document.getElementById("ezTelefon").value.trim(),
        email: document.getElementById("ezEmail").value.trim(),
        status: document.getElementById("ezStatus").value,
        tipPravnika: jePravnik ? document.getElementById("ezTipPravnika").value : null,
        barBroj: jePravnik ? document.getElementById("ezBarBroj").value.trim() : null,
        brojLicence: jeProcenitelj ? document.getElementById("ezBrojLicence").value.trim() : null
    };

    try {
        await apiFetch(`/osoblje/${id}`, { method: "PUT", body: JSON.stringify(dto) });
        modalIzmeni.hide();
        ucitajOsoblje();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

ucitajOsoblje();

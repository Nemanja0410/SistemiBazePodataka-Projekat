zahtevajPrijavu();
ucitajNavigaciju("polise");

const greska = document.getElementById("greska");
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

// ---------- Combo-ovi (klijenti/agenti) - deljeni za "Nova polisa" i "Izmeni" ----------

async function ucitajComboove() {
    try {
        const klijenti = await apiFetch("/klijenti");
        const agenti = await apiFetch("/osoblje/agenti");

        for (const sel of [document.getElementById("ugovaracId"), document.getElementById("ezUgovaracId")]) {
            for (const k of klijenti) {
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
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

document.getElementById("tip").addEventListener("change", (e) => {
    document.getElementById("redSpecijalizacija").classList.toggle("d-none", e.target.value !== "SPECIJALIZOVANO");
});

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

    let put = "/polise";
    let telo = baza;
    if (tip === "POLJOPRIVREDNO") {
        put = "/polise/poljoprivredno";
        telo = { ...baza, useviIds: [], zivotinjeIds: [] };
    } else if (tip === "ODGOVORNOST") {
        put = "/polise/odgovornost";
    } else if (tip === "SPECIJALIZOVANO") {
        const naziv = document.getElementById("nazivSpecijalizacije").value.trim();
        if (!naziv) { prikaziGresku(g, new Error("Naziv specijalizacije je obavezan.")); return; }
        put = "/polise/specijalizovano";
        telo = { ...baza, nazivSpecijalizacije: naziv };
    }

    try {
        await apiFetch(put, { method: "POST", body: JSON.stringify(telo) });
        bootstrap.Modal.getInstance(document.getElementById("modalPolisa")).hide();
        e.target.reset();
        document.getElementById("redSpecijalizacija").classList.add("d-none");
        ucitajPolise();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Izmeni (isto kao WinForms IzmeniPolisaForma) ----------

function otvoriIzmeni(p) {
    document.getElementById("greskaIzmeni").classList.add("d-none");
    document.getElementById("izmeniNaslov").textContent = `Izmeni polisu — ${p.brojPolise ?? ""}`;
    document.getElementById("ezId").value = p.polisaId;
    document.getElementById("ezUgovaracId").value = p.ugovaracId ?? "";
    document.getElementById("ezAgentId").value = p.agentId ?? "";
    document.getElementById("ezDatumPocetka").value = (p.datumPocetka ?? "").slice(0, 10);
    document.getElementById("ezDatumIsteka").value = (p.datumIsteka ?? "").slice(0, 10);
    document.getElementById("ezPremija").value = p.osnovnaPremija ?? "";
    document.getElementById("ezNacin").value = p.nacinPlacanja ?? "MESECNO";
    document.getElementById("ezStatus").value = p.status ?? "AKTIVNA";
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

    // Salje se ceo objekat (kao WinForms IzmeniPolisaForma) da se ne izgube brojPolise/tipOsiguranja/valuta
    // koje azurirajPolisu na backendu prepisuje bukvalno onim sto stigne u telu zahteva.
    const dto = {
        ...punaOdabrana,
        ugovaracId,
        agentId: document.getElementById("ezAgentId").value ? parseInt(document.getElementById("ezAgentId").value) : null,
        datumPocetka,
        datumIsteka,
        osnovnaPremija: parseFloat(document.getElementById("ezPremija").value),
        nacinPlacanja: document.getElementById("ezNacin").value,
        status: document.getElementById("ezStatus").value
    };

    try {
        await apiFetch(`/polise/${id}`, { method: "PUT", body: JSON.stringify(dto) });
        modalIzmeni.hide();
        offDetalji.hide();
        ucitajPolise();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

ucitajComboove();
ucitajPolise();

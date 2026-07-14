zahtevajPrijavu();
ucitajNavigaciju("nalozi");

if (!Auth.imaUlogu("ADMIN")) {
    document.body.innerHTML = '<div class="container pt-5"><div class="greska-box">Ova stranica je dostupna samo administratorima.</div></div>';
}

const greska = document.getElementById("greska");
const modalReset = new bootstrap.Modal(document.getElementById("modalReset"));

document.getElementById("btnOsvezi").addEventListener("click", () => {
    ucitajNaCekanju();
    ucitajSve();
});

function pill(status) {
    const klasa = "status-" + (status || "").toLowerCase();
    return `<span class="status-pill ${klasa}">${status ?? ""}</span>`;
}

async function ucitajNaCekanju() {
    const tbody = document.getElementById("tbodyNaCekanju");
    const prazno = document.getElementById("prazNaCekanju");
    try {
        const lista = await apiFetch("/nalozi/na-cekanju");
        tbody.innerHTML = "";
        prazno.style.display = lista.length === 0 ? "block" : "none";

        for (const n of lista) {
            const tr = document.createElement("tr");

            const tdUloga = document.createElement("td");
            const sel = document.createElement("select");
            sel.className = "form-select form-select-sm";
            for (const u of ["ADMIN", "AGENT", "PROCENITELJ", "LEKAR", "PRAVNIK"]) {
                const opt = document.createElement("option");
                opt.value = u;
                opt.textContent = u;
                if (u === n.tipOsoblja) opt.selected = true;
                sel.appendChild(opt);
            }
            tdUloga.appendChild(sel);

            const tdAkcije = document.createElement("td");
            const btnOdobri = document.createElement("button");
            btnOdobri.className = "btn btn-sm btn-osig me-1";
            btnOdobri.textContent = "Odobri";
            btnOdobri.onclick = async () => {
                try {
                    await apiFetch(`/nalozi/${n.nalogId}/odobri`, {
                        method: "POST",
                        body: JSON.stringify({ dodeljenaUloga: sel.value })
                    });
                    ucitajNaCekanju(); ucitajSve();
                } catch (err) { prikaziGresku(greska, err); }
            };
            const btnOdbij = document.createElement("button");
            btnOdbij.className = "btn btn-sm btn-osig-crvena";
            btnOdbij.textContent = "Odbij";
            btnOdbij.onclick = async () => {
                if (!confirm(`Odbiti zahtev za nalog "${n.korisnickoIme}"?`)) return;
                try {
                    await apiFetch(`/nalozi/${n.nalogId}/odbij`, { method: "POST" });
                    ucitajNaCekanju(); ucitajSve();
                } catch (err) { prikaziGresku(greska, err); }
            };
            tdAkcije.appendChild(btnOdobri);
            tdAkcije.appendChild(btnOdbij);

            tr.innerHTML = `
                <td>${n.korisnickoIme}</td>
                <td>${n.imeOsoblja ?? ""} ${n.prezimeOsoblja ?? ""}</td>
                <td>${n.tipOsoblja ?? ""}</td>
                <td>${n.datumRegistracije ? new Date(n.datumRegistracije).toLocaleDateString("sr-RS") : ""}</td>
            `;
            tr.appendChild(tdUloga);
            tr.appendChild(tdAkcije);
            tbody.appendChild(tr);
        }
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

// napraviAkcijeDugmad: gradi istu listu akcija (Zaključaj/Otključaj/Reset/Obriši) i za
// red u tabeli (kompaktan "..." dropdown) i za offcanvas detalje (pune dugmadi), da se
// logika akcija ne duplira na dva mesta.
function napraviAkcijeDugmad(n, kontejner, rezim) {
    kontejner.innerHTML = "";
    if (n.uloga === "ADMIN") return;

    const akcije = [];
    if (n.statusNaloga === "ODOBREN") {
        akcije.push({ tekst: "Zaključaj", klasa: "btn-osig-narandzasta", onclick: async () => {
            try { await apiFetch(`/nalozi/${n.nalogId}/zakljucaj`, { method: "POST" }); ucitajSve(); }
            catch (err) { prikaziGresku(greska, err); }
        }});
    }
    if (n.statusNaloga === "ZAKLJUCAN") {
        akcije.push({ tekst: "Otključaj", klasa: "btn-osig-plava", onclick: async () => {
            try { await apiFetch(`/nalozi/${n.nalogId}/otkljucaj`, { method: "POST" }); ucitajSve(); }
            catch (err) { prikaziGresku(greska, err); }
        }});
    }
    akcije.push({ tekst: "🔑 Reset lozinke", klasa: "btn-osig-siva", onclick: () => otvoriReset(n) });
    akcije.push({ tekst: "Obriši", klasa: "btn-osig-crvena", onclick: async () => {
        if (!confirm(`Obrisati nalog "${n.korisnickoIme}"?`)) return;
        try {
            await apiFetch(`/nalozi/${n.nalogId}`, { method: "DELETE" });
            offDetalji.hide();
            ucitajSve();
        } catch (err) { prikaziGresku(greska, err); }
    }});

    if (rezim === "dropdown") {
        const wrap = document.createElement("div");
        wrap.className = "dropend";
        wrap.addEventListener("click", (e) => e.stopPropagation());

        const toggle = document.createElement("button");
        toggle.className = "btn btn-sm btn-outline-secondary";
        toggle.setAttribute("data-bs-toggle", "dropdown");
        toggle.textContent = "⋮";
        wrap.appendChild(toggle);

        const menu = document.createElement("ul");
        menu.className = "dropdown-menu dropdown-menu-akcije";
        for (const a of akcije) {
            const li = document.createElement("li");
            const stavka = document.createElement("button");
            stavka.type = "button";
            stavka.className = `dropdown-item ${a.klasa}`;
            stavka.textContent = a.tekst;
            stavka.onclick = a.onclick;
            li.appendChild(stavka);
            menu.appendChild(li);
        }
        wrap.appendChild(menu);
        kontejner.appendChild(wrap);
    } else {
        for (const a of akcije) {
            const btn = document.createElement("button");
            btn.className = `btn ${a.klasa} flex-fill`;
            btn.textContent = a.tekst;
            btn.onclick = a.onclick;
            kontejner.appendChild(btn);
        }
    }
}

async function ucitajSve() {
    const tbody = document.getElementById("tbodySvi");
    try {
        const lista = await apiFetch("/nalozi");
        tbody.innerHTML = "";

        // Ako je offcanvas trenutno otvoren za neki nalog, osvezi mu sadrzaj/dugmad
        // svezim podacima (npr. posle Zakljucaj klika, status/dugmad se menjaju u letu).
        if (trenutniNalogId != null) {
            const azuriran = lista.find(x => x.nalogId === trenutniNalogId);
            if (azuriran) renderDetalje(azuriran);
        }

        for (const n of lista) {
            const tr = document.createElement("tr");
            tr.style.cursor = "pointer";
            // Isto kao WinForms (Color.FromArgb(230,230,230)) - admin red se blago izdvaja.
            // Bootstrap boji svaku celiju posebno preko --bs-table-bg promenljive, pa obican
            // background-color na <tr> ne bi bio vidljiv - mora se prepisati ta promenljiva.
            if (n.uloga === "ADMIN") tr.style.setProperty("--bs-table-bg", "#e6e6e6");
            const tdAkcije = document.createElement("td");
            tdAkcije.className = "text-end";
            napraviAkcijeDugmad(n, tdAkcije, "dropdown");

            tr.innerHTML = `
                <td>${n.korisnickoIme}</td>
                <td>${n.imeOsoblja ?? ""} ${n.prezimeOsoblja ?? ""}</td>
                <td>${n.uloga ?? ""}</td>
                <td>${pill(n.statusNaloga)}</td>
                <td>${n.neuspesnihPrijava}</td>
                <td>${n.zadnjaPrijava ? new Date(n.zadnjaPrijava).toLocaleString("sr-RS") : "/"}</td>
            `;
            tr.appendChild(tdAkcije);
            tr.addEventListener("click", () => otvoriDetalje(n));
            tbody.appendChild(tr);
        }
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

// ---------- Detalji (offcanvas, isto kao WinForms dgvNalozi_SelectionChanged) ----------

const offDetalji = new bootstrap.Offcanvas(document.getElementById("offDetalji"));
let trenutniNalogId = null;
document.getElementById("offDetalji").addEventListener("hidden.bs.offcanvas", () => { trenutniNalogId = null; });

function datumVreme(iso) {
    return iso ? new Date(iso).toLocaleString("sr-RS", { day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit" }) : "/";
}

// renderDetalje: samo puni sadrzaj panela (bez otvaranja) - koristi se i pri prvom
// otvaranju i za osvezavanje vec otvorenog panela posle neke akcije.
function renderDetalje(n) {
    trenutniNalogId = n.nalogId;
    document.getElementById("detaljiNaziv").textContent = n.korisnickoIme;
    napraviAkcijeDugmad(n, document.getElementById("detaljiAkcije"), "dugmad");

    document.getElementById("detaljiSadrzaj").innerHTML = `
        <table class="table table-sm mb-0">
            <tr><th style="width:48%">Zaposleni</th><td>${n.imeOsoblja ? `${n.imeOsoblja} ${n.prezimeOsoblja}` : "/ (admin nalog)"}</td></tr>
            <tr><th>Uloga</th><td>${n.uloga ?? ""}</td></tr>
            <tr><th>Status naloga</th><td>${pill(n.statusNaloga)}</td></tr>
            <tr><th>Neuspešnih prijava</th><td>${n.neuspesnihPrijava}</td></tr>
            <tr><th>Datum registracije</th><td>${datumVreme(n.datumRegistracije)}</td></tr>
            <tr><th>Datum odobrenja</th><td>${datumVreme(n.datumOdobrenja)}</td></tr>
            <tr><th>Zadnja prijava</th><td>${datumVreme(n.zadnjaPrijava)}</td></tr>
            <tr><th>Prisilna promena lozinke</th><td>${n.moraPromenitiLozinku ? "DA" : "NE"}</td></tr>
        </table>`;
}

function otvoriDetalje(n) {
    renderDetalje(n);
    offDetalji.show();
}

// ---------- Reset lozinke (isto kao WinForms UnesiLozinkuForma) ----------

function otvoriReset(n) {
    document.getElementById("greskaReset").classList.add("d-none");
    document.getElementById("resetNaslov").textContent = `Reset lozinke — ${n.korisnickoIme}`;
    document.getElementById("rzNalogId").value = n.nalogId;
    document.getElementById("rzLozinka").value = "";
    modalReset.show();
}

document.getElementById("formaReset").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaReset");
    g.classList.add("d-none");

    const lozinka = document.getElementById("rzLozinka").value;
    if (lozinka.length < 8 || !/\d/.test(lozinka)) {
        prikaziGresku(g, new Error("Lozinka mora imati najmanje 8 karaktera i bar jednu cifru."));
        return;
    }

    const nalogId = document.getElementById("rzNalogId").value;
    try {
        await apiFetch(`/nalozi/${nalogId}/resetuj-lozinku`, {
            method: "POST",
            body: JSON.stringify({ privremenaLozinka: lozinka })
        });
        modalReset.hide();
        alert("Privremena lozinka je postavljena. Korisnik mora da je promeni pri sledećoj prijavi.");
        ucitajSve();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

ucitajNaCekanju();
ucitajSve();

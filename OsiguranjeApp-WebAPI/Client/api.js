// Zajednicki helper za sve stranice - poziva OsiguranjApp Web API i
// upravlja JWT tokenom dobijenim sa POST /api/nalozi/prijava.

const API_BASE = "http://localhost:5000/api";
// Bazni URL bez "/api" - za pristup statickim fajlovima (npr. uploadovane fotografije steta).
const API_ORIGIN = API_BASE.replace(/\/api$/, "");

const Auth = {
    get token() { return localStorage.getItem("token"); },
    get nalog() {
        const raw = localStorage.getItem("nalog");
        return raw ? JSON.parse(raw) : null;
    },
    setSesija(token, nalog) {
        localStorage.setItem("token", token);
        localStorage.setItem("nalog", JSON.stringify(nalog));
    },
    odjava() {
        localStorage.removeItem("token");
        localStorage.removeItem("nalog");
    },
    jeUlogovan() { return !!this.token; },
    imaUlogu(...uloge) {
        const n = this.nalog;
        return !!n && uloge.includes(n.uloga);
    }
};

// apiFetch: kao fetch(), samo automatski dodaje bazni URL, JSON header i
// Authorization header (ako je korisnik ulogovan), i baca citljivu gresku na ne-2xx odgovor.
async function apiFetch(path, options = {}) {
    const headers = { "Content-Type": "application/json", ...(options.headers || {}) };
    if (Auth.token) headers["Authorization"] = `Bearer ${Auth.token}`;

    const resp = await fetch(`${API_BASE}${path}`, { ...options, headers });

    if (resp.status === 204) return null;

    const tekst = await resp.text();
    const podaci = tekst ? JSON.parse(tekst) : null;

    if (!resp.ok) {
        const poruka = podaci?.title || podaci?.poruka || `Greška ${resp.status}`;
        throw new Error(poruka);
    }
    return podaci;
}

function ucitajNavigaciju(aktivna) {
    const nalog = Auth.nalog;
    const stavke = [
        { href: "index.html", tekst: "Početna", id: "pocetna" },
        { href: "klijenti.html", tekst: "Klijenti", id: "klijenti" },
        { href: "polise.html", tekst: "Polise", id: "polise" },
        { href: "stete.html", tekst: "Štete", id: "stete" },
        { href: "predmeti.html", tekst: "Predmeti", id: "predmeti" },
        { href: "izvestaji.html", tekst: "Izveštaji", id: "izvestaji" },
    ];
    // Osoblje i Nalozi - isto kao WinForms MainForm.PrimeniUlogu(): btnOsoblje/btnNalozi
    // vidljivi samo za ADMIN-a, sve ostale uloge ih uopste ne vide.
    if (Auth.imaUlogu("ADMIN")) {
        stavke.push({ href: "osoblje.html", tekst: "Osoblje", id: "osoblje" });
        stavke.push({ href: "nalozi.html", tekst: "Nalozi", id: "nalozi" });
    }

    const nav = document.createElement("nav");
    nav.className = "navbar navbar-expand-lg navbar-dark bg-dark mb-4";
    const container = document.createElement("div");
    container.className = "container-fluid";
    nav.appendChild(container);

    const brand = document.createElement("span");
    brand.className = "navbar-brand me-4";
    brand.textContent = "🔒 Osiguravajuća kompanija";
    container.appendChild(brand);

    const ul = document.createElement("ul");
    ul.className = "navbar-nav me-auto";
    container.appendChild(ul);

    for (const s of stavke) {
        const li = document.createElement("li");
        li.className = "nav-item";
        const a = document.createElement("a");
        a.className = "nav-link" + (s.id === aktivna ? " active fw-bold" : "");
        a.href = s.href;
        a.textContent = s.tekst;
        li.appendChild(a);
        ul.appendChild(li);
    }

    const desno = document.createElement("span");
    desno.className = "navbar-text text-white";
    if (nalog) {
        desno.textContent = `${nalog.imeOsoblja ? nalog.imeOsoblja + " " + nalog.prezimeOsoblja : nalog.korisnickoIme} (${nalog.uloga})  `;
        const btn = document.createElement("button");
        btn.className = "btn btn-sm btn-outline-light ms-2";
        btn.textContent = "Odjava";
        btn.onclick = () => { Auth.odjava(); location.href = "login.html"; };
        desno.appendChild(btn);
    } else {
        const a = document.createElement("a");
        a.className = "btn btn-sm btn-outline-light";
        a.href = "login.html";
        a.textContent = "Prijava";
        desno.appendChild(a);
    }
    container.appendChild(desno);

    document.body.prepend(nav);
}

function zahtevajPrijavu() {
    if (!Auth.jeUlogovan()) {
        location.href = "login.html";
    }
}

// izvezuCsv: redovi = niz nizova (uklj. zaglavlje kao prvi red), naziv bez ekstenzije.
function izvezuCsv(redovi, naziv) {
    const tekst = redovi
        .map(red => red.map(polje => `"${String(polje ?? "").replace(/"/g, '""')}"`).join(";"))
        .join("\r\n");
    const blob = new Blob(["﻿" + tekst], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `${naziv}_${new Date().toISOString().slice(0, 10)}.csv`;
    document.body.appendChild(a);
    a.click();
    a.remove();
    URL.revokeObjectURL(url);
}

// omoguciSortiranje: kad se klikne na <th class="sortable" data-sort="polje">,
// sortira poslednju ucitanu listu (dobaviListu()) po tom polju i ponovo je iscrtava (prikaziListu).
function omoguciSortiranje(theadEl, dobaviListu, prikaziListu) {
    let polje = null, smer = 1;
    theadEl.querySelectorAll("th.sortable").forEach(th => {
        const strelica = document.createElement("span");
        strelica.className = "strelica";
        th.appendChild(strelica);

        th.addEventListener("click", () => {
            const novoPolje = th.dataset.sort;
            if (polje === novoPolje) smer *= -1; else { polje = novoPolje; smer = 1; }

            theadEl.querySelectorAll("th.sortable").forEach(t => {
                t.classList.remove("sort-aktivno");
                t.querySelector(".strelica").textContent = "";
            });
            th.classList.add("sort-aktivno");
            strelica.textContent = smer === 1 ? "▲" : "▼";

            const sortirano = [...dobaviListu()].sort((a, b) => {
                let va = a[polje], vb = b[polje];
                if (va == null) va = "";
                if (vb == null) vb = "";
                if (typeof va === "string") { va = va.toLowerCase(); vb = String(vb).toLowerCase(); }
                if (va < vb) return -1 * smer;
                if (va > vb) return 1 * smer;
                return 0;
            });
            prikaziListu(sortirano);
        });
    });
}

function popuniSelect(sel, lista, idPolje, tekstFn) {
    sel.innerHTML = "";
    for (const x of lista) {
        const opt = document.createElement("option");
        opt.value = x[idPolje];
        opt.textContent = tekstFn(x);
        sel.appendChild(opt);
    }
}

function prikaziGresku(el, err) {
    el.textContent = err.message || String(err);
    el.classList.remove("d-none");
}

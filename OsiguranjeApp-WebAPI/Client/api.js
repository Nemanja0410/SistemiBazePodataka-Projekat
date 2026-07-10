// Zajednicki helper za sve stranice - poziva OsiguranjApp Web API i
// upravlja JWT tokenom dobijenim sa POST /api/nalozi/prijava.

const API_BASE = "http://localhost:5080/api";

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
    ];
    if (Auth.imaUlogu("ADMIN")) stavke.push({ href: "nalozi.html", tekst: "Nalozi", id: "nalozi" });

    const nav = document.createElement("nav");
    nav.className = "navbar navbar-expand-lg navbar-dark bg-dark mb-4";
    const container = document.createElement("div");
    container.className = "container-fluid";
    nav.appendChild(container);

    const brand = document.createElement("span");
    brand.className = "navbar-brand";
    brand.textContent = "🔒 OsiguranjApp";
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

function prikaziGresku(el, err) {
    el.textContent = err.message || String(err);
    el.classList.remove("d-none");
}

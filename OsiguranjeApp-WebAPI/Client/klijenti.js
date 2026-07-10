zahtevajPrijavu();
ucitajNavigaciju("klijenti");

const greska = document.getElementById("greska");
let debounceId = null;

function pill(status) {
    const klasa = "status-" + (status || "").toLowerCase();
    return `<span class="status-pill ${klasa}">${status ?? ""}</span>`;
}

async function ucitajKlijente() {
    const naziv = document.getElementById("pretraga").value.trim();
    const tip = document.getElementById("filterTip").value;
    const tbody = document.getElementById("tbodyKlijenti");
    try {
        const qs = new URLSearchParams();
        if (naziv) qs.set("naziv", naziv);
        if (tip) qs.set("tip", tip);
        const lista = await apiFetch(`/klijenti?${qs.toString()}`);
        tbody.innerHTML = "";
        for (const k of lista) {
            const tr = document.createElement("tr");
            tr.innerHTML = `
                <td>${k.naziv ?? ""}</td>
                <td>${(k.tipKlijenta ?? "").replace("_", " ")}</td>
                <td>${k.telefon ?? ""}</td>
                <td>${k.email ?? ""}</td>
                <td>${pill(k.status)}</td>
                <td>${k.datumRegistracije ? new Date(k.datumRegistracije).toLocaleDateString("sr-RS") : ""}</td>
                <td></td>
            `;
            const btn = document.createElement("button");
            btn.className = "btn btn-sm btn-osig-crvena";
            btn.textContent = "Obriši";
            btn.onclick = async () => {
                if (!confirm(`Obrisati klijenta "${k.naziv}"? Ovo briše i sve njegove polise i štete.`)) return;
                try { await apiFetch(`/klijenti/${k.klijentId}`, { method: "DELETE" }); ucitajKlijente(); }
                catch (err) { prikaziGresku(greska, err); }
            };
            tr.lastElementChild.appendChild(btn);
            tbody.appendChild(tr);
        }
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

document.getElementById("pretraga").addEventListener("input", () => {
    clearTimeout(debounceId);
    debounceId = setTimeout(ucitajKlijente, 300);
});
document.getElementById("filterTip").addEventListener("change", ucitajKlijente);

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

ucitajKlijente();

zahtevajPrijavu();
ucitajNavigaciju("predmeti");

const greska = document.getElementById("greska");
const smeUpis = Auth.imaUlogu("ADMIN", "AGENT");
const smeBrisanje = Auth.imaUlogu("ADMIN");
const modalPredmet = new bootstrap.Modal(document.getElementById("modalPredmet"));
const modalImovina = new bootstrap.Modal(document.getElementById("modalImovina"));
const modalVozilo = new bootstrap.Modal(document.getElementById("modalVozilo"));
const modalNekretnina = new bootstrap.Modal(document.getElementById("modalNekretnina"));

// ---------- Vozila ----------

let sviKlijenti = [];
let listaVozila = [];

async function ucitajVozila() {
    try {
        listaVozila = await apiFetch("/vozila");
        prikaziVozila(listaVozila);
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

function prikaziVozila(lista) {
    const tbody = document.getElementById("tbodyVozila");
    tbody.innerHTML = "";
    for (const x of lista) {
        const tr = document.createElement("tr");
        tr.innerHTML = `
            <td>${x.registracija ?? ""}</td>
            <td>${x.marka ?? ""}</td>
            <td>${x.model ?? ""}</td>
            <td>${x.godinaProizvodnje ?? ""}</td>
            <td>${x.vlasnikNaziv ?? ""}</td>
            <td class="text-end"></td>
        `;
        if (smeUpis) {
            const btnIzmeni = document.createElement("button");
            btnIzmeni.className = "btn btn-sm btn-osig-plava me-1";
            btnIzmeni.textContent = "Izmeni";
            btnIzmeni.onclick = () => otvoriModalVozilo(x);
            tr.lastElementChild.appendChild(btnIzmeni);
        }
        if (smeBrisanje) {
            const btnObrisi = document.createElement("button");
            btnObrisi.className = "btn btn-sm btn-osig-crvena";
            btnObrisi.textContent = "Obriši";
            btnObrisi.onclick = async () => {
                if (!confirm(`Obrisati vozilo "${x.registracija}"?`)) return;
                try { await apiFetch(`/vozila/${x.voziloId}`, { method: "DELETE" }); ucitajVozila(); }
                catch (err) { prikaziGresku(greska, err); }
            };
            tr.lastElementChild.appendChild(btnObrisi);
        }
        tbody.appendChild(tr);
    }
    document.getElementById("lblBrojVozila").textContent = `Ukupno: ${lista.length}`;
}

function otvoriModalVozilo(x) {
    document.getElementById("greskaVozilo").classList.add("d-none");
    document.getElementById("voziloNaslov").textContent = x ? "Izmeni — Vozilo" : "Dodaj — Vozilo";
    document.getElementById("vId").value = x ? x.voziloId : "";
    document.getElementById("vRegistracija").value = x?.registracija ?? "";
    document.getElementById("vMarka").value = x?.marka ?? "";
    document.getElementById("vModel").value = x?.model ?? "";
    document.getElementById("vGodina").value = x?.godinaProizvodnje ?? "";
    popuniSelect(document.getElementById("vVlasnik"), sviKlijenti, "klijentId", k => k.naziv);
    document.getElementById("vVlasnik").value = x?.vlasnikId ?? "";
    modalVozilo.show();
}

omoguciSortiranje(document.querySelector("#tbodyVozila").closest("table").querySelector("thead"), () => listaVozila, prikaziVozila);

document.getElementById("btnDodajVozila").addEventListener("click", () => otvoriModalVozilo(null));
document.getElementById("btnOsveziVozila").addEventListener("click", ucitajVozila);
if (!smeUpis) document.getElementById("btnDodajVozila").style.display = "none";

document.getElementById("formaVozilo").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaVozilo");
    g.classList.add("d-none");

    const id = document.getElementById("vId").value;
    const dto = {
        registracija: document.getElementById("vRegistracija").value.trim(),
        marka: document.getElementById("vMarka").value.trim(),
        model: document.getElementById("vModel").value.trim(),
        godinaProizvodnje: parseInt(document.getElementById("vGodina").value),
        vlasnikId: parseInt(document.getElementById("vVlasnik").value)
    };

    try {
        if (id) await apiFetch(`/vozila/${id}`, { method: "PUT", body: JSON.stringify(dto) });
        else await apiFetch("/vozila", { method: "POST", body: JSON.stringify(dto) });
        modalVozilo.hide();
        ucitajVozila();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Nekretnine ----------

let listaNekretnine = [];

async function ucitajNekretnine() {
    try {
        listaNekretnine = await apiFetch("/nekretnine");
        prikaziNekretnine(listaNekretnine);
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

function prikaziNekretnine(lista) {
    const tbody = document.getElementById("tbodyNekretnine");
    tbody.innerHTML = "";
    for (const x of lista) {
        const tr = document.createElement("tr");
        tr.innerHTML = `
            <td>${x.adresa ?? ""}</td>
            <td>${x.tipObjekta ?? ""}</td>
            <td>${x.povrsina != null ? x.povrsina.toFixed(2) : "/"}</td>
            <td>${x.godinaIzgradnje ?? ""}</td>
            <td>${x.procenjenaVrednost != null ? x.procenjenaVrednost.toFixed(2) : "/"}</td>
            <td class="text-end"></td>
        `;
        if (smeUpis) {
            const btnIzmeni = document.createElement("button");
            btnIzmeni.className = "btn btn-sm btn-osig-plava me-1";
            btnIzmeni.textContent = "Izmeni";
            btnIzmeni.onclick = () => otvoriModalNekretnina(x);
            tr.lastElementChild.appendChild(btnIzmeni);
        }
        if (smeBrisanje) {
            const btnObrisi = document.createElement("button");
            btnObrisi.className = "btn btn-sm btn-osig-crvena";
            btnObrisi.textContent = "Obriši";
            btnObrisi.onclick = async () => {
                if (!confirm(`Obrisati nekretninu "${x.adresa}"?`)) return;
                try { await apiFetch(`/nekretnine/${x.nekretninaId}`, { method: "DELETE" }); ucitajNekretnine(); }
                catch (err) { prikaziGresku(greska, err); }
            };
            tr.lastElementChild.appendChild(btnObrisi);
        }
        tbody.appendChild(tr);
    }
    document.getElementById("lblBrojNekretnine").textContent = `Ukupno: ${lista.length}`;
}

function otvoriModalNekretnina(x) {
    document.getElementById("greskaNekretnina").classList.add("d-none");
    document.getElementById("nekretninaNaslov").textContent = x ? "Izmeni — Nekretnina" : "Dodaj — Nekretnina";
    document.getElementById("nId").value = x ? x.nekretninaId : "";
    document.getElementById("nAdresa").value = x?.adresa ?? "";
    document.getElementById("nTip").value = x?.tipObjekta ?? "STAN";
    document.getElementById("nPovrsina").value = x?.povrsina ?? "";
    document.getElementById("nGodina").value = x?.godinaIzgradnje ?? "";
    document.getElementById("nVrednost").value = x?.procenjenaVrednost ?? "";
    modalNekretnina.show();
}

omoguciSortiranje(document.querySelector("#tbodyNekretnine").closest("table").querySelector("thead"), () => listaNekretnine, prikaziNekretnine);

document.getElementById("btnDodajNekretnine").addEventListener("click", () => otvoriModalNekretnina(null));
document.getElementById("btnOsveziNekretnine").addEventListener("click", ucitajNekretnine);
if (!smeUpis) document.getElementById("btnDodajNekretnine").style.display = "none";

document.getElementById("formaNekretnina").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaNekretnina");
    g.classList.add("d-none");

    const id = document.getElementById("nId").value;
    const povrsinaRaw = document.getElementById("nPovrsina").value;
    const vrednostRaw = document.getElementById("nVrednost").value;
    const godinaRaw = document.getElementById("nGodina").value;
    const dto = {
        adresa: document.getElementById("nAdresa").value.trim(),
        tipObjekta: document.getElementById("nTip").value,
        povrsina: povrsinaRaw ? parseFloat(povrsinaRaw) : null,
        godinaIzgradnje: godinaRaw ? parseInt(godinaRaw) : null,
        procenjenaVrednost: vrednostRaw ? parseFloat(vrednostRaw) : null
    };

    try {
        if (id) await apiFetch(`/nekretnine/${id}`, { method: "PUT", body: JSON.stringify(dto) });
        else await apiFetch("/nekretnine", { method: "POST", body: JSON.stringify(dto) });
        modalNekretnina.hide();
        ucitajNekretnine();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

async function ucitajKlijenteZaVozila() {
    try {
        sviKlijenti = await apiFetch("/klijenti");
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

// ---------- Usevi i Zivotinje (dele istu strukturu: Vrsta/Lokacija/ProcenjenaVrednost) ----------

function napraviModulUsevZivotinja(cfg) {
    let trenutnaLista = [];

    async function ucitaj() {
        try {
            trenutnaLista = await apiFetch(cfg.endpoint);
            prikazi(trenutnaLista);
        } catch (err) {
            prikaziGresku(greska, err);
        }
    }

    function prikazi(lista) {
        const tbody = document.getElementById(cfg.tbodyId);
        tbody.innerHTML = "";
        for (const x of lista) {
            const tr = document.createElement("tr");
            tr.innerHTML = `
                <td>${x.vrsta ?? ""}</td>
                <td>${x.lokacija ?? ""}</td>
                <td>${x.procenjenaVrednost != null ? x.procenjenaVrednost.toFixed(2) : "/"}</td>
                <td class="text-end"></td>
            `;
            if (smeUpis) {
                const btnIzmeni = document.createElement("button");
                btnIzmeni.className = "btn btn-sm btn-osig-plava me-1";
                btnIzmeni.textContent = "Izmeni";
                btnIzmeni.onclick = () => otvoriModal(x);
                tr.lastElementChild.appendChild(btnIzmeni);
            }
            if (smeBrisanje) {
                const btnObrisi = document.createElement("button");
                btnObrisi.className = "btn btn-sm btn-osig-crvena";
                btnObrisi.textContent = "Obriši";
                btnObrisi.onclick = async () => {
                    if (!confirm(`Obrisati "${x.vrsta}"?`)) return;
                    try { await apiFetch(`${cfg.endpoint}/${x[cfg.idPolje]}`, { method: "DELETE" }); ucitaj(); }
                    catch (err) { prikaziGresku(greska, err); }
                };
                tr.lastElementChild.appendChild(btnObrisi);
            }
            tbody.appendChild(tr);
        }
        document.getElementById(cfg.lblBrojId).textContent = `Ukupno: ${lista.length}`;
    }

    function otvoriModal(x) {
        document.getElementById("greskaPredmet").classList.add("d-none");
        document.getElementById("predmetNaslov").textContent = x ? `Izmeni — ${cfg.naslov}` : `Dodaj — ${cfg.naslov}`;
        document.getElementById("pId").value = x ? x[cfg.idPolje] : "";
        document.getElementById("pVrsta").value = x?.vrsta ?? "";
        document.getElementById("pLokacija").value = x?.lokacija ?? "";
        document.getElementById("pVrednost").value = x?.procenjenaVrednost ?? "";
        document.getElementById("formaPredmet").dataset.aktivniModul = cfg.endpoint;
        modalPredmet.show();
    }

    omoguciSortiranje(document.querySelector(`#${cfg.tbodyId}`).closest("table").querySelector("thead"), () => trenutnaLista, prikazi);

    document.getElementById(cfg.btnDodajId).addEventListener("click", () => otvoriModal(null));
    document.getElementById(cfg.btnOsveziId).addEventListener("click", ucitaj);
    if (!smeUpis) document.getElementById(cfg.btnDodajId).style.display = "none";

    ucitaj();
    return { ucitaj, endpoint: cfg.endpoint, idPolje: cfg.idPolje };
}

const moduli = {};
moduli["/usevi"] = napraviModulUsevZivotinja({
    endpoint: "/usevi", idPolje: "usevId", tbodyId: "tbodyUsevi", lblBrojId: "lblBrojUsevi",
    btnDodajId: "btnDodajUsevi", btnOsveziId: "btnOsveziUsevi", naslov: "Usev"
});
moduli["/zivotinje"] = napraviModulUsevZivotinja({
    endpoint: "/zivotinje", idPolje: "zivotinjaId", tbodyId: "tbodyZivotinje", lblBrojId: "lblBrojZivotinje",
    btnDodajId: "btnDodajZivotinje", btnOsveziId: "btnOsveziZivotinje", naslov: "Životinja"
});

document.getElementById("formaPredmet").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaPredmet");
    g.classList.add("d-none");

    const endpoint = e.target.dataset.aktivniModul;
    const modul = moduli[endpoint];
    const id = document.getElementById("pId").value;
    const vrednostRaw = document.getElementById("pVrednost").value;
    const dto = {
        vrsta: document.getElementById("pVrsta").value.trim(),
        lokacija: document.getElementById("pLokacija").value.trim(),
        procenjenaVrednost: vrednostRaw ? parseFloat(vrednostRaw) : null
    };

    try {
        if (id) await apiFetch(`${endpoint}/${id}`, { method: "PUT", body: JSON.stringify(dto) });
        else await apiFetch(endpoint, { method: "POST", body: JSON.stringify(dto) });
        modalPredmet.hide();
        modul.ucitaj();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

// ---------- Pokretna imovina (Naziv/Opis/ProcenjenaVrednost) ----------

let listaImovina = [];

async function ucitajImovinu() {
    try {
        listaImovina = await apiFetch("/pokretnaimovina");
        prikaziImovinu(listaImovina);
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

function prikaziImovinu(lista) {
    const tbody = document.getElementById("tbodyImovina");
    tbody.innerHTML = "";
    for (const x of lista) {
        const tr = document.createElement("tr");
        tr.innerHTML = `
            <td>${x.naziv ?? ""}</td>
            <td>${x.opis ?? ""}</td>
            <td>${x.procenjenaVrednost != null ? x.procenjenaVrednost.toFixed(2) : "/"}</td>
            <td class="text-end"></td>
        `;
        if (smeUpis) {
            const btnIzmeni = document.createElement("button");
            btnIzmeni.className = "btn btn-sm btn-osig-plava me-1";
            btnIzmeni.textContent = "Izmeni";
            btnIzmeni.onclick = () => otvoriModalImovina(x);
            tr.lastElementChild.appendChild(btnIzmeni);
        }
        if (smeBrisanje) {
            const btnObrisi = document.createElement("button");
            btnObrisi.className = "btn btn-sm btn-osig-crvena";
            btnObrisi.textContent = "Obriši";
            btnObrisi.onclick = async () => {
                if (!confirm(`Obrisati "${x.naziv}"?`)) return;
                try { await apiFetch(`/pokretnaimovina/${x.pokretnaImovinaId}`, { method: "DELETE" }); ucitajImovinu(); }
                catch (err) { prikaziGresku(greska, err); }
            };
            tr.lastElementChild.appendChild(btnObrisi);
        }
        tbody.appendChild(tr);
    }
    document.getElementById("lblBrojImovina").textContent = `Ukupno: ${lista.length}`;
}

function otvoriModalImovina(x) {
    document.getElementById("greskaImovina").classList.add("d-none");
    document.getElementById("imovinaNaslov").textContent = x ? "Izmeni — Pokretna imovina" : "Dodaj — Pokretna imovina";
    document.getElementById("iId").value = x ? x.pokretnaImovinaId : "";
    document.getElementById("iNaziv").value = x?.naziv ?? "";
    document.getElementById("iOpis").value = x?.opis ?? "";
    document.getElementById("iVrednost").value = x?.procenjenaVrednost ?? "";
    modalImovina.show();
}

omoguciSortiranje(document.querySelector("#tbodyImovina").closest("table").querySelector("thead"), () => listaImovina, prikaziImovinu);

document.getElementById("btnDodajImovina").addEventListener("click", () => otvoriModalImovina(null));
document.getElementById("btnOsveziImovina").addEventListener("click", ucitajImovinu);
if (!smeUpis) document.getElementById("btnDodajImovina").style.display = "none";

document.getElementById("formaImovina").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaImovina");
    g.classList.add("d-none");

    const id = document.getElementById("iId").value;
    const vrednostRaw = document.getElementById("iVrednost").value;
    const dto = {
        naziv: document.getElementById("iNaziv").value.trim(),
        opis: document.getElementById("iOpis").value.trim(),
        procenjenaVrednost: vrednostRaw ? parseFloat(vrednostRaw) : null
    };

    try {
        if (id) await apiFetch(`/pokretnaimovina/${id}`, { method: "PUT", body: JSON.stringify(dto) });
        else await apiFetch("/pokretnaimovina", { method: "POST", body: JSON.stringify(dto) });
        modalImovina.hide();
        ucitajImovinu();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

ucitajImovinu();
ucitajKlijenteZaVozila().then(ucitajVozila);
ucitajNekretnine();

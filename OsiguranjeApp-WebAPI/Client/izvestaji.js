zahtevajPrijavu();
ucitajNavigaciju("izvestaji");

const greska = document.getElementById("greska");

function novac(broj) {
    return (broj ?? 0).toLocaleString("sr-RS", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function pill(status) {
    const klasa = "status-" + (status || "").toLowerCase();
    return `<span class="status-pill ${klasa}">${status ?? ""}</span>`;
}

function datumSr(iso) {
    return iso ? new Date(iso).toLocaleDateString("sr-RS") : "";
}

function grupisi(lista, kljucFn) {
    const mapa = new Map();
    for (const stavka of lista) {
        const kljuc = kljucFn(stavka) ?? "/";
        if (!mapa.has(kljuc)) mapa.set(kljuc, []);
        mapa.get(kljuc).push(stavka);
    }
    return mapa;
}

let poslednjiIzvozPolise = [];
let poslednjiIzvozVrsta = [];
let poslednjiIzvozStatus = [];
let poslednjiIzvozAgenti = [];
let poslednjiIzvozIsticu = [];
let poslednjiIzvozSveP = [];
let poslednjiIzvozSviK = [];

// Iznosi u razlicitim valutama se ne smeju sabirati kao da su isti broj (RSD+EUR+USD nema smisla),
// zato se svugde grupise po (kategorija, valuta) - svaka valuta dobija svoj red/podzbir.
async function ucitajPolise() {
    const tbody = document.getElementById("tbodyPolise");
    try {
        const lista = await apiFetch("/polise");
        const grupe = grupisi(lista, p => `${p.tipOsiguranja}|${p.valuta ?? "RSD"}`);

        const redovi = [...grupe.entries()]
            .map(([kljuc, stavke]) => {
                const [tip, valuta] = kljuc.split("|");
                return {
                    tip, valuta,
                    broj: stavke.length,
                    ukupno: stavke.reduce((s, p) => s + (p.osnovnaPremija ?? 0), 0)
                };
            })
            .sort((a, b) => a.tip.localeCompare(b.tip) || b.valuta.localeCompare(b.valuta));

        tbody.innerHTML = "";
        for (const r of redovi) {
            const prosek = r.broj > 0 ? r.ukupno / r.broj : 0;
            tbody.innerHTML += `<tr><td>${r.tip}</td><td>${r.valuta}</td><td>${r.broj}</td><td>${novac(r.ukupno)}</td><td>${novac(prosek)}</td></tr>`;
        }
        const poValuti = grupisi(redovi, r => r.valuta);
        for (const [valuta, stavke] of poValuti) {
            const svBroj = stavke.reduce((s, r) => s + r.broj, 0);
            const svUkupno = stavke.reduce((s, r) => s + r.ukupno, 0);
            tbody.innerHTML += `<tr class="table-primary fw-bold"><td colspan="2">UKUPNO (${valuta})</td><td>${svBroj}</td><td>${novac(svUkupno)}</td><td></td></tr>`;
        }

        poslednjiIzvozPolise = [
            ["Tip osiguranja", "Valuta", "Broj polisa", "Ukupna premija", "Prosečna premija"],
            ...redovi.map(r => [r.tip, r.valuta, r.broj, novac(r.ukupno), novac(r.broj > 0 ? r.ukupno / r.broj : 0)])
        ];
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

async function ucitajStete() {
    try {
        const stete = await apiFetch("/stete");

        const grupeVrsta = grupisi(stete, s => `${s.vrstaStete}|${s.valuta ?? "RSD"}`);
        const tbodyVrsta = document.getElementById("tbodyVrsta");
        tbodyVrsta.innerHTML = "";
        const redoviVrsta = [...grupeVrsta.entries()]
            .map(([kljuc, stavke]) => {
                const [vrsta, valuta] = kljuc.split("|");
                return {
                    vrsta, valuta, broj: stavke.length,
                    ukupno: stavke.reduce((s, x) => s + (x.procenjeniIznos ?? 0), 0)
                };
            })
            .sort((a, b) => a.vrsta.localeCompare(b.vrsta) || a.valuta.localeCompare(b.valuta));
        for (const r of redoviVrsta)
            tbodyVrsta.innerHTML += `<tr><td>${r.vrsta}</td><td>${r.valuta}</td><td>${r.broj}</td><td>${novac(r.ukupno)}</td></tr>`;

        poslednjiIzvozVrsta = [
            ["Vrsta štete", "Valuta", "Broj", "Ukupno"],
            ...redoviVrsta.map(r => [r.vrsta, r.valuta, r.broj, novac(r.ukupno)])
        ];

        const grupeStatus = grupisi(stete, s => `${s.status}|${s.valuta ?? "RSD"}`);
        const tbodyStatus = document.getElementById("tbodyStatus");
        tbodyStatus.innerHTML = "";
        const redoviStatus = [...grupeStatus.entries()]
            .map(([kljuc, stavke]) => {
                const [status, valuta] = kljuc.split("|");
                const saIznosom = stavke.filter(x => x.procenjeniIznos != null);
                const ukupno = saIznosom.reduce((s, x) => s + x.procenjeniIznos, 0);
                return { status, valuta, broj: stavke.length, ukupno, prosek: saIznosom.length > 0 ? ukupno / saIznosom.length : 0 };
            })
            .sort((a, b) => b.broj - a.broj);
        for (const r of redoviStatus)
            tbodyStatus.innerHTML += `<tr><td>${pill(r.status)}</td><td>${r.valuta}</td><td>${r.broj}</td><td>${novac(r.ukupno)}</td><td>${novac(r.prosek)}</td></tr>`;

        poslednjiIzvozStatus = [
            ["Status", "Valuta", "Broj šteta", "Ukupno", "Prosek"],
            ...redoviStatus.map(r => [r.status, r.valuta, r.broj, novac(r.ukupno), novac(r.prosek)])
        ];
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

async function ucitajIsticu() {
    const tbody = document.getElementById("tbodyIsticu");
    try {
        const lista = await apiFetch("/polise");
        const danas = new Date();
        const za30dana = new Date(danas.getTime() + 30 * 86400000);
        const isticu = lista
            .filter(p => p.status === "AKTIVNA" && new Date(p.datumIsteka) >= danas && new Date(p.datumIsteka) <= za30dana)
            .map(p => ({ ...p, danaDoIsteka: Math.round((new Date(p.datumIsteka) - danas) / 86400000) }))
            .sort((a, b) => a.danaDoIsteka - b.danaDoIsteka);

        tbody.innerHTML = "";
        if (isticu.length === 0) tbody.innerHTML = `<tr><td colspan="5" class="text-muted text-center">Nema polisa koje ističu u narednih 30 dana.</td></tr>`;
        for (const p of isticu) {
            tbody.innerHTML += `<tr>
                <td>${p.brojPolise ?? ""}</td>
                <td>${p.tipOsiguranja ?? ""}</td>
                <td>${p.ugovaracNaziv ?? ""}</td>
                <td>${new Date(p.datumIsteka).toLocaleDateString("sr-RS")}</td>
                <td>${p.danaDoIsteka} dana</td>
            </tr>`;
        }

        poslednjiIzvozIsticu = [
            ["Broj polise", "Tip", "Ugovarač", "Datum isteka", "Dana preostalo"],
            ...isticu.map(p => [p.brojPolise, p.tipOsiguranja, p.ugovaracNaziv, new Date(p.datumIsteka).toLocaleDateString("sr-RS"), p.danaDoIsteka])
        ];
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

async function ucitajAgente() {
    const tbody = document.getElementById("tbodyAgenti");
    try {
        const agenti = await apiFetch("/osoblje/agenti");
        tbody.innerHTML = "";
        for (const a of agenti) {
            tbody.innerHTML += `<tr>
                <td>${a.ime} ${a.prezime}</td>
                <td>${a.tipAgenta ?? ""}</td>
                <td>${a.regionRada ?? "/"}</td>
                <td>${a.polise?.length ?? 0}</td>
                <td>${(a.provizijaProcenat ?? 0).toFixed(2)}%</td>
            </tr>`;
        }
        poslednjiIzvozAgenti = [
            ["Agent", "Tip", "Region", "Br. polisa", "Provizija %"],
            ...agenti.map(a => [`${a.ime} ${a.prezime}`, a.tipAgenta, a.regionRada ?? "/", a.polise?.length ?? 0, (a.provizijaProcenat ?? 0).toFixed(2)])
        ];
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

async function ucitajSveP() {
    const tbody = document.getElementById("tbodySveP");
    try {
        const lista = await apiFetch("/polise");
        tbody.innerHTML = "";
        for (const p of lista)
            tbody.innerHTML += `<tr><td>${p.brojPolise ?? ""}</td><td>${p.tipOsiguranja ?? ""}</td><td>${p.ugovaracNaziv ?? ""}</td><td>${novac(p.osnovnaPremija)} ${p.valuta ?? ""}</td><td>${pill(p.status)}</td></tr>`;

        poslednjiIzvozSveP = [
            ["Broj polise", "Tip", "Ugovarač", "Agent", "Premija", "Valuta", "Status", "Datum početka", "Datum isteka"],
            ...lista.map(p => [p.brojPolise, p.tipOsiguranja, p.ugovaracNaziv, p.agentIme ?? "", (p.osnovnaPremija ?? 0).toFixed(2), p.valuta, p.status, datumSr(p.datumPocetka), datumSr(p.datumIsteka)])
        ];
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

async function ucitajSviK() {
    const tbody = document.getElementById("tbodySviK");
    try {
        const lista = await apiFetch("/klijenti");
        tbody.innerHTML = "";
        for (const k of lista)
            tbody.innerHTML += `<tr><td>${k.naziv ?? ""}</td><td>${(k.tipKlijenta ?? "").replace("_", " ")}</td><td>${k.telefon ?? ""}</td><td>${pill(k.status)}</td></tr>`;

        poslednjiIzvozSviK = [
            ["Naziv", "Tip", "Telefon", "Email", "Status", "Datum registracije"],
            ...lista.map(k => [k.naziv, (k.tipKlijenta ?? "").replace("_", " "), k.telefon ?? "", k.email ?? "", k.status, datumSr(k.datumRegistracije)])
        ];
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

document.getElementById("btnExportPolise").addEventListener("click", () => izvezuCsv(poslednjiIzvozPolise, "polise_statistika"));
document.getElementById("btnExportVrsta").addEventListener("click", () => izvezuCsv(poslednjiIzvozVrsta, "stete_po_vrsti"));
document.getElementById("btnExportStete").addEventListener("click", () => izvezuCsv(poslednjiIzvozStatus, "stete_statistika"));
document.getElementById("btnExportAgenti").addEventListener("click", () => izvezuCsv(poslednjiIzvozAgenti, "agenti_statistika"));
document.getElementById("btnExportIsticu").addEventListener("click", () => izvezuCsv(poslednjiIzvozIsticu, "polise_isticu"));
document.getElementById("btnExportSveP").addEventListener("click", () => izvezuCsv(poslednjiIzvozSveP, "polise"));
document.getElementById("btnExportSviK").addEventListener("click", () => izvezuCsv(poslednjiIzvozSviK, "klijenti"));

ucitajPolise();
ucitajStete();
ucitajAgente();
ucitajIsticu();
ucitajSveP();
ucitajSviK();

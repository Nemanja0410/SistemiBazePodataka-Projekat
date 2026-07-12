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
let poslednjiIzvozStatus = [];
let poslednjiIzvozAgenti = [];

async function ucitajPolise() {
    const tbody = document.getElementById("tbodyPolise");
    try {
        const lista = await apiFetch("/polise");
        const grupe = grupisi(lista, p => p.tipOsiguranja);

        const redovi = [...grupe.entries()]
            .map(([tip, stavke]) => ({
                tip,
                broj: stavke.length,
                ukupno: stavke.reduce((s, p) => s + (p.osnovnaPremija ?? 0), 0)
            }))
            .sort((a, b) => b.ukupno - a.ukupno);

        tbody.innerHTML = "";
        let svBroj = 0, svUkupno = 0;
        for (const r of redovi) {
            const prosek = r.broj > 0 ? r.ukupno / r.broj : 0;
            tbody.innerHTML += `<tr><td>${r.tip}</td><td>${r.broj}</td><td>${novac(r.ukupno)}</td><td>${novac(prosek)}</td></tr>`;
            svBroj += r.broj; svUkupno += r.ukupno;
        }
        tbody.innerHTML += `<tr class="table-primary fw-bold"><td>UKUPNO</td><td>${svBroj}</td><td>${novac(svUkupno)}</td><td></td></tr>`;

        poslednjiIzvozPolise = [
            ["Tip osiguranja", "Broj polisa", "Ukupna premija", "Prosečna premija"],
            ...redovi.map(r => [r.tip, r.broj, novac(r.ukupno), novac(r.broj > 0 ? r.ukupno / r.broj : 0)]),
            ["UKUPNO", svBroj, novac(svUkupno), ""]
        ];
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

async function ucitajStete() {
    try {
        const stete = await apiFetch("/stete");

        const grupeVrsta = grupisi(stete, s => s.vrstaStete);
        const tbodyVrsta = document.getElementById("tbodyVrsta");
        tbodyVrsta.innerHTML = "";
        const redoviVrsta = [...grupeVrsta.entries()]
            .map(([vrsta, stavke]) => ({
                vrsta, broj: stavke.length,
                ukupno: stavke.reduce((s, x) => s + (x.procenjeniIznos ?? 0), 0)
            }))
            .sort((a, b) => b.ukupno - a.ukupno);
        for (const r of redoviVrsta)
            tbodyVrsta.innerHTML += `<tr><td>${r.vrsta}</td><td>${r.broj}</td><td>${novac(r.ukupno)}</td></tr>`;

        const grupeStatus = grupisi(stete, s => s.status);
        const tbodyStatus = document.getElementById("tbodyStatus");
        tbodyStatus.innerHTML = "";
        const redoviStatus = [...grupeStatus.entries()]
            .map(([status, stavke]) => {
                const saIznosom = stavke.filter(x => x.procenjeniIznos != null);
                const ukupno = saIznosom.reduce((s, x) => s + x.procenjeniIznos, 0);
                return { status, broj: stavke.length, ukupno, prosek: saIznosom.length > 0 ? ukupno / saIznosom.length : 0 };
            })
            .sort((a, b) => b.broj - a.broj);
        for (const r of redoviStatus)
            tbodyStatus.innerHTML += `<tr><td>${pill(r.status)}</td><td>${r.broj}</td><td>${novac(r.ukupno)}</td><td>${novac(r.prosek)}</td></tr>`;

        poslednjiIzvozStatus = [
            ["Status", "Broj šteta", "Ukupno", "Prosek"],
            ...redoviStatus.map(r => [r.status, r.broj, novac(r.ukupno), novac(r.prosek)])
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

document.getElementById("btnExportPolise").addEventListener("click", () => izvezuCsv(poslednjiIzvozPolise, "polise_statistika"));
document.getElementById("btnExportStete").addEventListener("click", () => izvezuCsv(poslednjiIzvozStatus, "stete_statistika"));
document.getElementById("btnExportAgenti").addEventListener("click", () => izvezuCsv(poslednjiIzvozAgenti, "agenti_statistika"));

ucitajPolise();
ucitajStete();
ucitajAgente();

zahtevajPrijavu();
ucitajNavigaciju("pocetna");

if (Auth.imaUlogu("ADMIN")) document.getElementById("kartNalozi").style.display = "block";

async function ucitajStatistiku() {
    const greska = document.getElementById("greska");
    const kontejner = document.getElementById("statKartice");

    async function bezbedno(fn, podrazumevano = 0) {
        try { return await fn(); } catch { return podrazumevano; }
    }

    try {
        const [klijenti, polise, stete] = await Promise.all([
            bezbedno(() => apiFetch("/klijenti"), []),
            bezbedno(() => apiFetch("/polise"), []),
            bezbedno(() => apiFetch("/stete"), [])
        ]);

        const kartice = [
            { naziv: "Klijenata", broj: klijenti.length },
            { naziv: "Polisa", broj: polise.length },
            { naziv: "Prijavljenih šteta", broj: stete.length },
            { naziv: "Aktivnih polisa", broj: polise.filter(p => p.status === "AKTIVNA").length }
        ];

        kontejner.innerHTML = "";
        for (const k of kartice) {
            const col = document.createElement("div");
            col.className = "col-6 col-md-3";
            col.innerHTML = `<div class="stat-card"><div class="broj">${k.broj}</div><div class="naziv">${k.naziv}</div></div>`;
            kontejner.appendChild(col);
        }
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

ucitajStatistiku();

zahtevajPrijavu();
ucitajNavigaciju("polise");

const greska = document.getElementById("greska");

function pill(status) {
    const klasa = "status-" + (status || "").toLowerCase();
    return `<span class="status-pill ${klasa}">${status ?? ""}</span>`;
}

async function ucitajPolise() {
    const tip = document.getElementById("filterTip").value;
    const status = document.getElementById("filterStatus").value;
    const tbody = document.getElementById("tbodyPolise");
    try {
        const qs = new URLSearchParams();
        if (tip) qs.set("tip", tip);
        if (status) qs.set("status", status);
        const lista = await apiFetch(`/polise?${qs.toString()}`);
        tbody.innerHTML = "";
        for (const p of lista) {
            const tr = document.createElement("tr");
            tr.innerHTML = `
                <td>${p.brojPolise ?? ""}</td>
                <td>${p.tipOsiguranja ?? ""}</td>
                <td>${p.ugovaracNaziv ?? ""}</td>
                <td>${p.osnovnaPremija?.toFixed(2)} ${p.valuta ?? ""}</td>
                <td>${pill(p.status)}</td>
                <td>${new Date(p.datumPocetka).toLocaleDateString("sr-RS")} – ${new Date(p.datumIsteka).toLocaleDateString("sr-RS")}</td>
                <td></td>
            `;
            const btn = document.createElement("button");
            btn.className = "btn btn-sm btn-osig-crvena";
            btn.textContent = "Obriši";
            btn.onclick = async () => {
                if (!confirm(`Obrisati polisu "${p.brojPolise}"?`)) return;
                try { await apiFetch(`/polise/${p.polisaId}`, { method: "DELETE" }); ucitajPolise(); }
                catch (err) { prikaziGresku(greska, err); }
            };
            tr.lastElementChild.appendChild(btn);
            tbody.appendChild(tr);
        }
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

document.getElementById("filterTip").addEventListener("change", ucitajPolise);
document.getElementById("filterStatus").addEventListener("change", ucitajPolise);

async function ucitajComboove() {
    const selUgovarac = document.getElementById("ugovaracId");
    const selAgent = document.getElementById("agentId");
    try {
        const klijenti = await apiFetch("/klijenti");
        for (const k of klijenti) {
            const opt = document.createElement("option");
            opt.value = k.klijentId;
            opt.textContent = k.naziv;
            selUgovarac.appendChild(opt);
        }
        const agenti = await apiFetch("/osoblje/agenti");
        for (const a of agenti) {
            const opt = document.createElement("option");
            opt.value = a.osobljeId;
            opt.textContent = `${a.ime} ${a.prezime}`;
            selAgent.appendChild(opt);
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

ucitajComboove();
ucitajPolise();

zahtevajPrijavu();
ucitajNavigaciju("stete");

const greska = document.getElementById("greska");
let svePolise = [];

function pill(status) {
    const klasa = "status-" + (status || "").toLowerCase();
    return `<span class="status-pill ${klasa}">${status ?? ""}</span>`;
}

async function ucitajStete() {
    const status = document.getElementById("filterStatus").value;
    const tbody = document.getElementById("tbodyStete");
    try {
        const qs = new URLSearchParams();
        if (status) qs.set("status", status);
        const lista = await apiFetch(`/stete?${qs.toString()}`);
        tbody.innerHTML = "";
        for (const s of lista) {
            const tr = document.createElement("tr");
            tr.innerHTML = `
                <td>${s.brojStete ?? ""}</td>
                <td>${s.vrstaStete ?? ""}</td>
                <td>${s.podnosilacNaziv ?? ""}</td>
                <td>${s.brojPolise ?? ""}</td>
                <td>${s.procenjeniIznos != null ? s.procenjeniIznos.toFixed(2) + " RSD" : "/"}</td>
                <td>${pill(s.status)}</td>
                <td>${new Date(s.datumPrijave).toLocaleDateString("sr-RS")}</td>
                <td></td>
            `;
            const btn = document.createElement("button");
            btn.className = "btn btn-sm btn-osig-crvena";
            btn.textContent = "Obriši";
            btn.onclick = async () => {
                if (!confirm(`Obrisati štetu "${s.brojStete}"?`)) return;
                try { await apiFetch(`/stete/${s.stetaId}`, { method: "DELETE" }); ucitajStete(); }
                catch (err) { prikaziGresku(greska, err); }
            };
            tr.lastElementChild.appendChild(btn);
            tbody.appendChild(tr);
        }
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

document.getElementById("filterStatus").addEventListener("change", ucitajStete);

async function ucitajComboove() {
    const selPodnosilac = document.getElementById("podnosilacId");
    try {
        const klijenti = await apiFetch("/klijenti");
        for (const k of klijenti) {
            const opt = document.createElement("option");
            opt.value = k.klijentId;
            opt.textContent = k.naziv;
            selPodnosilac.appendChild(opt);
        }
        svePolise = await apiFetch("/polise");
        osveziPoliseZaKlijenta();
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

function osveziPoliseZaKlijenta() {
    const klijentId = parseInt(document.getElementById("podnosilacId").value) || 0;
    const sel = document.getElementById("polisaId");
    sel.innerHTML = "";
    const filtrirane = svePolise.filter(p => p.ugovaracId === klijentId);
    if (filtrirane.length === 0) {
        sel.innerHTML = '<option value="">-- klijent nema polisa --</option>';
        return;
    }
    for (const p of filtrirane) {
        const opt = document.createElement("option");
        opt.value = p.polisaId;
        opt.textContent = `${p.brojPolise} (${p.tipOsiguranja})`;
        sel.appendChild(opt);
    }
}

document.getElementById("podnosilacId").addEventListener("change", osveziPoliseZaKlijenta);

document.getElementById("formaSteta").addEventListener("submit", async (e) => {
    e.preventDefault();
    const g = document.getElementById("greskaSteta");
    g.classList.add("d-none");

    const polisaId = parseInt(document.getElementById("polisaId").value);
    if (!polisaId) { prikaziGresku(g, new Error("Izaberite polisu.")); return; }

    const iznosRaw = document.getElementById("procenjeniIznos").value;
    const dto = {
        brojStete: document.getElementById("brojStete").value.trim(),
        vrstaStete: document.getElementById("vrstaStete").value,
        podnosilacId: parseInt(document.getElementById("podnosilacId").value),
        polisaId,
        datumNastanka: document.getElementById("datumNastanka").value,
        lokacija: document.getElementById("lokacija").value.trim(),
        opisDogodjaja: document.getElementById("opisDogodjaja").value.trim(),
        procenjeniIznos: iznosRaw ? parseFloat(iznosRaw) : null
    };

    try {
        await apiFetch("/stete", { method: "POST", body: JSON.stringify(dto) });
        bootstrap.Modal.getInstance(document.getElementById("modalSteta")).hide();
        e.target.reset();
        ucitajStete();
    } catch (err) {
        prikaziGresku(g, err);
    }
});

ucitajComboove();
ucitajStete();

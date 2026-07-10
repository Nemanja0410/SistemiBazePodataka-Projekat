zahtevajPrijavu();
ucitajNavigaciju("nalozi");

if (!Auth.imaUlogu("ADMIN")) {
    document.body.innerHTML = '<div class="container pt-5"><div class="greska-box">Ova stranica je dostupna samo administratorima.</div></div>';
}

const greska = document.getElementById("greska");

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

async function ucitajSve() {
    const tbody = document.getElementById("tbodySvi");
    try {
        const lista = await apiFetch("/nalozi");
        tbody.innerHTML = "";
        for (const n of lista) {
            const tr = document.createElement("tr");
            const tdAkcije = document.createElement("td");

            if (n.uloga !== "ADMIN") {
                if (n.statusNaloga === "ODOBREN") {
                    const btn = document.createElement("button");
                    btn.className = "btn btn-sm btn-osig-plava me-1";
                    btn.textContent = "Zaključaj";
                    btn.onclick = async () => {
                        try { await apiFetch(`/nalozi/${n.nalogId}/zakljucaj`, { method: "POST" }); ucitajSve(); }
                        catch (err) { prikaziGresku(greska, err); }
                    };
                    tdAkcije.appendChild(btn);
                }
                if (n.statusNaloga === "ZAKLJUCAN") {
                    const btn = document.createElement("button");
                    btn.className = "btn btn-sm btn-osig me-1";
                    btn.textContent = "Otključaj";
                    btn.onclick = async () => {
                        try { await apiFetch(`/nalozi/${n.nalogId}/otkljucaj`, { method: "POST" }); ucitajSve(); }
                        catch (err) { prikaziGresku(greska, err); }
                    };
                    tdAkcije.appendChild(btn);
                }
                const btnBrisi = document.createElement("button");
                btnBrisi.className = "btn btn-sm btn-osig-crvena";
                btnBrisi.textContent = "Obriši";
                btnBrisi.onclick = async () => {
                    if (!confirm(`Obrisati nalog "${n.korisnickoIme}"?`)) return;
                    try { await apiFetch(`/nalozi/${n.nalogId}`, { method: "DELETE" }); ucitajSve(); }
                    catch (err) { prikaziGresku(greska, err); }
                };
                tdAkcije.appendChild(btnBrisi);
            }

            tr.innerHTML = `
                <td>${n.korisnickoIme}</td>
                <td>${n.imeOsoblja ?? ""} ${n.prezimeOsoblja ?? ""}</td>
                <td>${n.uloga ?? ""}</td>
                <td>${pill(n.statusNaloga)}</td>
                <td>${n.neuspesnihPrijava}</td>
                <td>${n.zadnjaPrijava ? new Date(n.zadnjaPrijava).toLocaleString("sr-RS") : "/"}</td>
            `;
            tr.appendChild(tdAkcije);
            tbody.appendChild(tr);
        }
    } catch (err) {
        prikaziGresku(greska, err);
    }
}

ucitajNaCekanju();
ucitajSve();

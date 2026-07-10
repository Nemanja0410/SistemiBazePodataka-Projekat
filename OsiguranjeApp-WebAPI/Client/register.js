async function ucitajOsoblje() {
    const sel = document.getElementById("osobljeId");
    try {
        const lista = await apiFetch("/nalozi/osoblje-za-registraciju");
        sel.innerHTML = "";
        if (lista.length === 0) {
            sel.innerHTML = '<option value="">-- Nema slobodnih zaposlenih --</option>';
            return;
        }
        for (const o of lista) {
            const opt = document.createElement("option");
            opt.value = o.osobljeId;
            opt.textContent = `${o.ime} ${o.prezime} (${o.tipOsoblja})`;
            sel.appendChild(opt);
        }
    } catch (err) {
        sel.innerHTML = `<option value="">Greška pri učitavanju: ${err.message}</option>`;
    }
}

ucitajOsoblje();

document.getElementById("formaRegistracija").addEventListener("submit", async (e) => {
    e.preventDefault();
    const greska = document.getElementById("greska");
    const poruka = document.getElementById("poruka");
    greska.classList.add("d-none");
    poruka.classList.add("d-none");

    const osobljeId = parseInt(document.getElementById("osobljeId").value);
    const korisnickoIme = document.getElementById("korisnickoIme").value.trim();
    const lozinka = document.getElementById("lozinka").value;

    if (!osobljeId) { prikaziGresku(greska, new Error("Izaberite zaposlenog.")); return; }

    try {
        const resp = await apiFetch("/nalozi/registracija", {
            method: "POST",
            body: JSON.stringify({ osobljeId, korisnickoIme, lozinka })
        });
        poruka.textContent = resp.poruka;
        poruka.classList.remove("d-none");
        document.getElementById("formaRegistracija").reset();
        ucitajOsoblje();
    } catch (err) {
        prikaziGresku(greska, err);
    }
});

if (Auth.jeUlogovan()) location.href = "index.html";

let staraLozinkaPrijava = null;

document.getElementById("formaPrijava").addEventListener("submit", async (e) => {
    e.preventDefault();
    const greska = document.getElementById("greska");
    greska.classList.add("d-none");

    const korisnickoIme = document.getElementById("korisnickoIme").value.trim();
    const lozinka = document.getElementById("lozinka").value;

    try {
        const resp = await apiFetch("/nalozi/prijava", {
            method: "POST",
            body: JSON.stringify({ korisnickoIme, lozinka })
        });
        Auth.setSesija(resp.token, resp.nalog);

        if (resp.nalog?.moraPromenitiLozinku) {
            // Admin je resetovao lozinku - korisnik mora da postavi novu pre nego sto nastavi dalje.
            staraLozinkaPrijava = lozinka;
            document.getElementById("formaPrijava").classList.add("d-none");
            document.getElementById("formaPromenaLozinke").classList.remove("d-none");
            return;
        }
        location.href = "index.html";
    } catch (err) {
        prikaziGresku(greska, err);
    }
});

document.getElementById("formaPromenaLozinke").addEventListener("submit", async (e) => {
    e.preventDefault();
    const greska = document.getElementById("greska");
    greska.classList.add("d-none");

    const novaLozinka = document.getElementById("novaLozinka").value;
    if (novaLozinka.length < 8 || !/\d/.test(novaLozinka)) {
        prikaziGresku(greska, new Error("Lozinka mora imati najmanje 8 karaktera i bar jednu cifru."));
        return;
    }

    try {
        await apiFetch(`/nalozi/${Auth.nalog.nalogId}/promeni-lozinku`, {
            method: "POST",
            body: JSON.stringify({ staraLozinka: staraLozinkaPrijava, novaLozinka })
        });
        location.href = "index.html";
    } catch (err) {
        prikaziGresku(greska, err);
    }
});

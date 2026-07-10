if (Auth.jeUlogovan()) location.href = "index.html";

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
        location.href = "index.html";
    } catch (err) {
        prikaziGresku(greska, err);
    }
});

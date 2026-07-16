using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    // Fotografije uz stetu - pravi lokalni fajl (OpenFileDialog + kopiranje na disk), isto
    // nacelo kao WebAPI upload: generisano ime fajla (bez kolizija), u bazi se pamti putanja.
    public partial class FotografijeSteteForma : Form
    {
        private static readonly string[] DozvoljeneEkstenzije = { ".jpg", ".jpeg", ".png", ".webp" };

        private readonly int _stetaId;
        private readonly string _brojStete;
        private string? _izabranaPutanja;

        public FotografijeSteteForma(int stetaId, string brojStete)
        {
            _stetaId = stetaId;
            _brojStete = brojStete;
            InitializeComponent();
            UcitajFotografije();
        }

        private static string UploadsRoot =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OsiguranjApp", "Uploads", "stete");

        private void UcitajFotografije()
        {
            dgvFoto.Rows.Clear();
            foreach (var f in DTOManager.vratiFotografijeZaStetu(_stetaId))
            {
                int r = dgvFoto.Rows.Add(f.FotografijaId, Path.GetFileName(f.Putanja), f.Opis, f.DatumDodavanja.ToString("dd.MM.yyyy HH:mm"));
                dgvFoto.Rows[r].Tag = f;
            }
        }

        private void BtnIzaberi_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Izaberite fotografiju",
                Filter = "Slike (*.jpg;*.jpeg;*.png;*.webp)|*.jpg;*.jpeg;*.png;*.webp"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            _izabranaPutanja = dlg.FileName;
            txtIzabranFajl.Text = Path.GetFileName(dlg.FileName);
        }

        private void BtnDodaj_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_izabranaPutanja))
            { MessageBox.Show("Izaberite fajl.", "Validacija"); return; }

            string ekstenzija = Path.GetExtension(_izabranaPutanja).ToLowerInvariant();
            if (Array.IndexOf(DozvoljeneEkstenzije, ekstenzija) < 0)
            { MessageBox.Show("Dozvoljeni formati: JPG, PNG, WEBP.", "Validacija"); return; }

            try
            {
                string folder = Path.Combine(UploadsRoot, _stetaId.ToString());
                Directory.CreateDirectory(folder);
                string imeFajla = $"{Guid.NewGuid()}{ekstenzija}";
                string odrediste = Path.Combine(folder, imeFajla);
                File.Copy(_izabranaPutanja, odrediste, overwrite: false);

                var dto = new FotografijaBasic
                {
                    StetaId = _stetaId,
                    Putanja = odrediste,
                    Opis = txtOpis.Text.Trim()
                };
                if (!UiHelper.PokusajAkciju(() => DTOManager.dodajFotografiju(dto))) return;

                _izabranaPutanja = null;
                txtIzabranFajl.Clear();
                txtOpis.Clear();
                UcitajFotografije();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri kopiranju fajla: {ex.Message}", "Greška");
            }
        }

        private void OtvoriIzabranu()
        {
            if (dgvFoto.CurrentRow?.Tag is not FotografijaBasic f) return;
            if (string.IsNullOrEmpty(f.Putanja) || !File.Exists(f.Putanja))
            { MessageBox.Show("Fajl nije pronađen na disku.", "Greška"); return; }
            Process.Start(new ProcessStartInfo(f.Putanja) { UseShellExecute = true });
        }

        private void BtnObrisi_Click(object? sender, EventArgs e)
        {
            if (dgvFoto.CurrentRow?.Tag is not FotografijaBasic f)
            { MessageBox.Show("Izaberite fotografiju za brisanje.", "Info"); return; }
            if (MessageBox.Show("Obrisati izabranu fotografiju?", "Potvrda", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            if (!UiHelper.PokusajAkciju(() => DTOManager.obrisiFotografiju(f.FotografijaId))) return;
            if (!string.IsNullOrEmpty(f.Putanja) && File.Exists(f.Putanja))
            {
                try { File.Delete(f.Putanja); } catch { /* fajl ostaje siroce na disku, baza je izvor istine */ }
            }
            UcitajFotografije();
        }
    }
}

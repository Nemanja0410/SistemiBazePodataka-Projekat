using System;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    // Fotografije uz stetu (zapisnik policije/servis kod auto stete, sanacija kod imovinske) -
    // cuva se samo putanja/opis fajla, bez stvarnog upload-a fajla.
    public class FotografijeSteteForma : Form
    {
        private readonly int _stetaId;
        private readonly string _brojStete;
        private DataGridView dgvFoto = null!;
        private TextBox txtPutanja = null!, txtOpis = null!;
        private Button btnDodaj = null!, btnObrisi = null!, btnZatvori = null!;

        public FotografijeSteteForma(int stetaId, string brojStete)
        {
            _stetaId = stetaId;
            _brojStete = brojStete;
            InitializeComponent();
            UcitajFotografije();
        }

        private void InitializeComponent()
        {
            this.Text          = $"Fotografije — {_brojStete}";
            this.Size          = new Size(700, 460);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font          = new Font("Segoe UI", 9f);
            this.BackColor     = UiHelper.PozadinaForm;

            var naslov = UiHelper.NapraviNaslov($"📷  Fotografije — {_brojStete}");

            dgvFoto = new DataGridView { Dock = DockStyle.Fill };
            UiHelper.StilizirajGrid(dgvFoto);
            dgvFoto.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", HeaderText = "ID", Visible = false });
            dgvFoto.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Putanja / naziv fajla", FillWeight = 50 });
            dgvFoto.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Opis", FillWeight = 30 });
            dgvFoto.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Dodato", FillWeight = 20 });

            var pnlUnos = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 76, ColumnCount = 3, Padding = new Padding(8) };
            pnlUnos.BackColor = Color.White;
            pnlUnos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            pnlUnos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            pnlUnos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            pnlUnos.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            pnlUnos.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));

            pnlUnos.Controls.Add(new Label { Text = "Putanja / naziv fajla *", AutoSize = true }, 0, 0);
            pnlUnos.Controls.Add(new Label { Text = "Opis", AutoSize = true }, 1, 0);
            txtPutanja = new TextBox { Dock = DockStyle.Fill };
            txtOpis    = new TextBox { Dock = DockStyle.Fill };
            pnlUnos.Controls.Add(txtPutanja, 0, 1);
            pnlUnos.Controls.Add(txtOpis, 1, 1);

            btnDodaj = UiHelper.NapraviDugme("➕  Dodaj", UiHelper.Zelena, 95);
            btnDodaj.Dock = DockStyle.Bottom;
            pnlUnos.Controls.Add(btnDodaj, 2, 1);
            btnDodaj.Click += BtnDodaj_Click;

            var pnlBtn = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.White, Padding = new Padding(8, 6, 8, 0) };
            btnObrisi  = UiHelper.NapraviDugme("🗑️  Obriši izabranu", UiHelper.Crvena, 150);
            btnZatvori = UiHelper.NapraviDugme("Zatvori", UiHelper.Siva, 90);
            btnObrisi.Location  = new Point(8, 4);
            btnZatvori.Location = new Point(166, 4);
            btnObrisi.Click  += BtnObrisi_Click;
            btnZatvori.Click += (s, e) => Close();
            pnlBtn.Controls.AddRange(new Control[] { btnObrisi, btnZatvori });

            var pnlGrid = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 4, 8, 4) };
            pnlGrid.Controls.Add(dgvFoto);

            this.Controls.Add(pnlGrid);
            this.Controls.Add(pnlBtn);
            this.Controls.Add(pnlUnos);
            this.Controls.Add(naslov);
        }

        private void UcitajFotografije()
        {
            dgvFoto.Rows.Clear();
            foreach (var f in DTOManager.vratiFotografijeZaStetu(_stetaId))
            {
                int r = dgvFoto.Rows.Add(f.FotografijaId, f.Putanja, f.Opis, f.DatumDodavanja.ToString("dd.MM.yyyy HH:mm"));
                dgvFoto.Rows[r].Tag = f;
            }
        }

        private void BtnDodaj_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPutanja.Text))
            { MessageBox.Show("Putanja / naziv fajla je obavezna.", "Validacija"); return; }

            var dto = new FotografijaBasic
            {
                StetaId = _stetaId,
                Putanja = txtPutanja.Text.Trim(),
                Opis = txtOpis.Text.Trim()
            };
            if (!UiHelper.PokusajAkciju(() => DTOManager.dodajFotografiju(dto))) return;
            txtPutanja.Clear();
            txtOpis.Clear();
            UcitajFotografije();
        }

        private void BtnObrisi_Click(object? sender, EventArgs e)
        {
            if (dgvFoto.CurrentRow?.Tag is not FotografijaBasic f)
            { MessageBox.Show("Izaberite fotografiju za brisanje.", "Info"); return; }
            if (MessageBox.Show("Obrisati izabranu fotografiju?", "Potvrda", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            if (!UiHelper.PokusajAkciju(() => DTOManager.obrisiFotografiju(f.FotografijaId))) return;
            UcitajFotografije();
        }
    }
}

using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    partial class IzmeniSteteForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private TextBox        txtLokacija = null!, txtOpis = null!, txtIznos = null!;
        private ComboBox       cmbStatus = null!, cmbValuta = null!;
        private DateTimePicker dtpNastanka = null!;
        private Button         btnSacuvaj = null!, btnOdustani = null!;
        private FlowLayoutPanel root = null!;

        private ComboBox? cmbVozilo; private TextBox? txtZapisnik, txtServis;
        private TextBox? txtDijagnoza, txtMedDok, txtZdravUstanova; private ComboBox? cmbLekar;
        private TextBox? txtProcenaOstecenja, txtIzvodjacSanacije;

        private ListBox lstLica = null!, lstPredmeti = null!, lstProcene = null!;
        private ComboBox cmbOlKlijent = null!; private TextBox txtOlImePrezime = null!, txtOlOpis = null!, txtOlIznos = null!;
        private TextBox txtOpTip = null!, txtOpOpis = null!, txtOpIznos = null!;
        private ComboBox cmbPrProcenitelj = null!; private TextBox txtPrMetod = null!, txtPrNalaz = null!, txtPrIznos = null!, txtPrPreporuka = null!;

        private void InitializeComponent()
        {
            this.Text          = $"Izmeni štetu — {_steta.BrojStete}";
            this.Size          = new System.Drawing.Size(560, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font          = new System.Drawing.Font("Segoe UI", 9f);
            this.BackColor     = System.Drawing.Color.White;
            this.MinimizeBox   = false;

            root = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, AutoScroll = true, Padding = new Padding(12)
            };

            root.Controls.Add(NapraviBaznaPoljaGrupa());
            root.Controls.Add(NapraviTipSpecificnaGrupa());
            root.Controls.Add(NapraviOstecenaLicaGrupa());
            root.Controls.Add(NapraviOsteceniPredmetiGrupa());

            var grpProcene = NapraviProceneGrupa();
            grpProcene.Visible = _smeProcena;
            root.Controls.Add(grpProcene);

            var pnlDugmad = new FlowLayoutPanel { Width = 500, Height = 44, FlowDirection = FlowDirection.RightToLeft };
            btnSacuvaj  = UiHelper.NapraviDugme("✔  Sačuvaj", UiHelper.Zelena, 110);
            btnOdustani = UiHelper.NapraviDugme("✖  Zatvori", UiHelper.Siva, 110);
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };
            pnlDugmad.Controls.Add(btnOdustani);
            pnlDugmad.Controls.Add(btnSacuvaj);
            root.Controls.Add(pnlDugmad);

            this.Controls.Add(root);
        }
    }
}

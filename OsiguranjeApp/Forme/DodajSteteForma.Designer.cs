using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    partial class DodajSteteForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private TextBox        txtBroj = null!, txtLokacija = null!, txtOpis = null!, txtIznos = null!;
        private ComboBox       cmbVrsta = null!, cmbStatus = null!, cmbPolisa = null!, cmbPodnosilac = null!, cmbValuta = null!;
        private DateTimePicker dtpNastanka = null!;
        private Button         btnSacuvaj = null!, btnOdustani = null!;
        private FlowLayoutPanel root = null!;
        private Panel           pnlTipSpecificno = null!;
        private GroupBox        grpProcene = null!;

        private ComboBox? cmbVozilo; private TextBox? txtZapisnik, txtServis;
        private TextBox? txtDijagnoza, txtMedDok, txtZdravUstanova; private ComboBox? cmbLekar;
        private TextBox? txtProcenaOstecenja, txtIzvodjacSanacije;

        private ListBox lstPendingLica = null!, lstPendingPredmeti = null!, lstPendingProcene = null!;
        private ComboBox cmbOlKlijent = null!; private TextBox txtOlImePrezime = null!, txtOlOpis = null!, txtOlIznos = null!;
        private TextBox txtOpTip = null!, txtOpOpis = null!, txtOpIznos = null!;
        private ComboBox cmbPrProcenitelj = null!; private TextBox txtPrMetod = null!, txtPrNalaz = null!, txtPrIznos = null!, txtPrPreporuka = null!;

        private void InitializeComponent()
        {
            this.Text          = "Nova šteta";
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

            pnlTipSpecificno = new Panel { Width = 500, AutoSize = true, MinimumSize = new System.Drawing.Size(500, 0) };
            root.Controls.Add(pnlTipSpecificno);

            root.Controls.Add(NapraviOstecenaLicaGrupa());
            root.Controls.Add(NapraviOsteceniPredmetiGrupa());

            grpProcene = NapraviProceneGrupa();
            grpProcene.Visible = _smeProcena;
            root.Controls.Add(grpProcene);

            var pnlDugmad = new FlowLayoutPanel { Width = 500, Height = 44, FlowDirection = FlowDirection.RightToLeft };
            btnSacuvaj  = UiHelper.NapraviDugme("✔  Sačuvaj", UiHelper.Zelena, 110);
            btnOdustani = UiHelper.NapraviDugme("✖  Odustani", UiHelper.Siva, 110);
            btnSacuvaj.Click  += BtnSacuvaj_Click;
            btnOdustani.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            pnlDugmad.Controls.Add(btnOdustani);
            pnlDugmad.Controls.Add(btnSacuvaj);
            root.Controls.Add(pnlDugmad);

            this.Controls.Add(root);
        }
    }
}

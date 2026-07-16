using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OsiguranjApp.DTOs;

namespace OsiguranjApp.Forme
{
    partial class IzmeniPolisaForma
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

        private TextBox        txtPremija = null!;
        private ComboBox       cmbStatus = null!, cmbNacin = null!, cmbUgovarac = null!, cmbAgent = null!;
        private DateTimePicker dtpPocetka = null!, dtpIsteka = null!;
        private Button         btnSacuvaj = null!, btnOdustani = null!;
        private FlowLayoutPanel root = null!;
        private GroupBox        grpOsiguranici = null!, grpKorisnici = null!;

        private List<KlijentPregled> _klijenti = new();
        private List<VoziloBasic> _vozila = new();
        private List<NekretninaBasic> _nekretnine = new();
        private List<PokretnaImovinaBasic> _pokretnaImovina = new();
        private List<UsevBasic> _usevi = new();
        private List<ZivotinjaBasic> _zivotinje = new();

        private TextBox? txtSumaOsiguranja; private ComboBox? cmbTipIsplate;
        private TextBox? txtMrezaUstanova, txtPokrica, txtLimitSpecijalista, txtLimitStomatologa, txtLimitBolnickih, txtLimitBolnickiDan;
        private ComboBox? cmbVozilo; private TextBox? txtBonusMalus, txtTeritorijalno; private ListBox? lstVozaci;
        private TextBox? txtVrsteRizika; private ListBox? lstNekretnine, lstPokretnaImovina;
        private TextBox? txtDestinacije; private DateTimePicker? dtpPolazak, dtpPovratak; private ListBox? lstOsiguranaLica;
        private ListBox? lstUsevi, lstZivotinje;
        private TextBox? txtVrstaOdgovornosti, txtLimitOdgovornosti;
        private TextBox? txtNazivSpecijalizacije, txtOpisUslova;

        private ListBox lstOsiguranici = null!, lstPokrica = null!, lstKorisnici = null!;
        private ComboBox cmbOsigKlijent = null!; private TextBox txtOsigImePrezime = null!;
        private TextBox txtPkNaziv = null!, txtPkOpis = null!, txtPkLimit = null!, txtPkFransiza = null!, txtPkPremija = null!;
        private ComboBox cmbKiKlijent = null!; private TextBox txtKiImePrezime = null!, txtKiProcenat = null!;

        private void InitializeComponent()
        {
            this.Text            = $"Izmeni polisu — {_polisa.BrojPolise}";
            this.Size            = new Size(560, 720);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.Font            = new Font("Segoe UI", 9f);
            this.BackColor       = Color.White;
            this.MinimizeBox     = false;

            root = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, AutoScroll = true, Padding = new Padding(12)
            };

            root.Controls.Add(NapraviBaznaPoljaGrupa());
            root.Controls.Add(NapraviTipSpecificnaGrupa());

            grpOsiguranici = NapraviOsiguraniciGrupa();
            grpOsiguranici.Visible = _tip != "PUTNO";
            root.Controls.Add(grpOsiguranici);

            root.Controls.Add(NapraviDodatnaPokricaGrupa());

            grpKorisnici = NapraviKorisniciIsplateGrupa();
            grpKorisnici.Visible = _tip == "ZIVOTNO";
            root.Controls.Add(grpKorisnici);

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

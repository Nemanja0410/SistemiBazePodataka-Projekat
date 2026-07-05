using System.Drawing;
using System.Windows.Forms;

namespace OsiguranjApp.Forme
{
    /// <summary>
    /// Stavka za ComboBox koja čuva ID i tekst.
    /// Koristi se u svim formama umesto anonimnih objekata.
    /// </summary>
    public class ComboItem
    {
        public int     Id    { get; }
        public string? Tekst { get; }

        public ComboItem(int id, string? tekst)
        {
            Id    = id;
            Tekst = tekst;
        }

        public override string ToString() => Tekst ?? "";
    }

    /// <summary>
    /// Centralizovani UI helpers — boje, stilizovanje grida, dugmad.
    /// </summary>
    public static class UiHelper
    {
        // ---- Boje ----
        public static readonly Color Plava        = Color.FromArgb(30,  64,  103);
        public static readonly Color PlavaSvetla  = Color.FromArgb(52,  120, 186);
        public static readonly Color Zelena       = Color.FromArgb(39,  174, 96);
        public static readonly Color Narandzasta  = Color.FromArgb(230, 126, 34);
        public static readonly Color Crvena       = Color.FromArgb(192, 57,  43);
        public static readonly Color Siva         = Color.FromArgb(120, 120, 120);
        public static readonly Color PozadinaForm = Color.FromArgb(240, 242, 245);

        // ---- Grid ----
        public static void StilizirajGrid(DataGridView dgv)
        {
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, dgv, new object[] { true });

            dgv.AllowUserToAddRows    = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly              = true;
            dgv.MultiSelect           = false;
            dgv.SelectionMode         = DataGridViewSelectionMode.FullRowSelect;
            dgv.RowHeadersVisible     = false;
            dgv.AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor       = Color.White;
            dgv.BorderStyle           = BorderStyle.None;
            dgv.CellBorderStyle       = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor             = Color.FromArgb(220, 220, 220);
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.ColumnHeadersHeight   = 36;
            dgv.RowTemplate.Height    = 28;
            dgv.RowTemplate.Resizable = DataGridViewTriState.False;
            dgv.AutoSizeRowsMode      = DataGridViewAutoSizeRowsMode.None;
            dgv.EnableHeadersVisualStyles = false;

            dgv.ColumnHeadersDefaultCellStyle.BackColor  = Plava;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor  = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font       = new Font("Segoe UI Semibold", 9F);
            dgv.ColumnHeadersDefaultCellStyle.Padding    = new Padding(6, 0, 0, 0);

            dgv.DefaultCellStyle.Font               = new Font("Segoe UI", 9F);
            dgv.DefaultCellStyle.SelectionBackColor = PlavaSvetla;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        }

        // ---- Dugme ----
        public static Button NapraviDugme(string tekst, Color boja, int sirina = 105)
        {
            var btn = new Button
            {
                Text      = tekst,
                BackColor = boja,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(sirina, 30),
                Cursor    = Cursors.Hand,
                Font      = new Font("Segoe UI", 9F)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // ---- Naslov panel ----
        public static Panel NapraviNaslov(string tekst)
        {
            var pnl = new Panel
            {
                BackColor = Plava,
                Dock      = DockStyle.Top,
                Height    = 50
            };
            pnl.Controls.Add(new Label
            {
                Text      = tekst,
                Font      = new Font("Segoe UI", 14F),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(16, 12)
            });
            return pnl;
        }

        // ---- Boja statusa ----
        public static Color StatusBoja(string? status) => status switch
        {
            "AKTIVAN"    or "AKTIVNA"    or "ISPLACENA"   => Zelena,
            "NEAKTIVAN"  or "ISTEKLA"    or "ODBIJENA"    => Crvena,
            "BLOKIRAN"   or "RASKINUTA"                   => Crvena,
            "PRIJAVLJENA" or "U_OBRADI"  or "U_PROCENI"  => Narandzasta,
            "MIROVANJE"  or "OBNOVLJENA"                  => PlavaSvetla,
            _                                             => Siva
        };

        // ---- Red u TableLayoutPanel ----
        public static TextBox DodajRed(TableLayoutPanel tbl, int red, string labelTekst)
        {
            tbl.Controls.Add(new Label
            {
                Text      = labelTekst,
                Dock      = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                Padding   = new Padding(0, 0, 8, 0)
            }, 0, red);
            var txt = new TextBox { Dock = DockStyle.Fill };
            tbl.Controls.Add(txt, 1, red);
            return txt;
        }

        public static ComboBox DodajComboRed(TableLayoutPanel tbl, int red, string labelTekst)
        {
            tbl.Controls.Add(new Label
            {
                Text      = labelTekst,
                Dock      = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                Padding   = new Padding(0, 0, 8, 0)
            }, 0, red);
            var cmb = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            tbl.Controls.Add(cmb, 1, red);
            return cmb;
        }

        public static DateTimePicker DodajDTPRed(TableLayoutPanel tbl, int red, string labelTekst)
        {
            tbl.Controls.Add(new Label
            {
                Text      = labelTekst,
                Dock      = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                Padding   = new Padding(0, 0, 8, 0)
            }, 0, red);
            var dtp = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };
            tbl.Controls.Add(dtp, 1, red);
            return dtp;
        }

        // ---- Standardni layout ----
        public static TableLayoutPanel NapraviLayout(int rows)
        {
            var tbl = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = rows,
                Padding     = new Padding(16)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < rows; i++)
                tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            return tbl;
        }

        // ---- Panel sa dugmadima Sacuvaj/Odustani ----
        public static (Button btnOk, Button btnCancel) DodajDugmadPanel(
            TableLayoutPanel tbl, int red,
            string tekstOk = "✔  Sačuvaj",
            string tekstCancel = "✖  Odustani")
        {
            var pnl = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft
            };
            var btnOk     = NapraviDugme(tekstOk,     Zelena,  105);
            var btnCancel = NapraviDugme(tekstCancel, Siva,    105);
            pnl.Controls.AddRange(new Control[] { btnOk, btnCancel });
            tbl.Controls.Add(pnl, 0, red);
            tbl.SetColumnSpan(pnl, 2);
            return (btnOk, btnCancel);
        }
    }
}

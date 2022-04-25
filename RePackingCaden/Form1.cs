using RePackingCaden.Message;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Deployment.Application;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RePackingCaden
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            var list = new List<string>();
            foreach (var item in PrinterSettings.InstalledPrinters)
                list.Add(item.ToString());

            foreach (var item in list)
            {
                listBox1.Items.Add(item);
            }

            printName = list.Where(c => c.Contains("ZDesigner")).FirstOrDefault();
            LbPrinter.Text = $"Выбранный текущий принтер \n {printName}";

            RB1.Checked = true;
           
        }

        string printName;
        int LotID = 5154;
        int CountSerial = 28;


        private void Form1_Load(object sender, EventArgs e)
        {
            if (ApplicationDeployment.IsNetworkDeployed) //Показывает версию публикации
            {
                this.Text = "Verison Product - " + ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                //label2.Visible = true;
            }
        }
        // SN Модель ДатаЭтикетки  Паллет Коробка  Номер  ДатаУпаковки Литер
        private void SN_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //if (!CheckRePack(SN.Text))
                //{
                //    MessageBox.Show("Номер не входит в диапазон на перепечатку 07.07.19 или не найден");
                //    return;
                //}

                GetBox(SN,Grid1,LBEr);

                if (Grid1.Rows.Count == 0)              
                    return;

                for (int i = 0; i < Grid1.Rows.Count; i++)
                {
                    var SN = Grid1[0, i].Value.ToString();
                    var model = Grid1[1, i].Value.ToString();

                    //var labelDate = UpdateRepack(SN);
                    var labelDate = Grid1[2, i].Value.ToString();
                    var lbda = DateTime.Parse(labelDate).ToString("dd.MM.yyyy");
                    print(LabelSN(SN, model, lbda));
                }

                var liter = Grid1[7, 0].Value.ToString();
                var boxnum = Grid1[4, 0].Value.ToString();
                print(LabelBox(liter, boxnum));

                GetLB("Успешно отсканировано и распечатано!",Color.Green,LBEr);

                SN.Clear();
                SN.Select();
            }
        }

        string UpdateRepack(string TRID)
        {
            var fas = new FASEntities();
            var r = fas.M_CadenaID.Where(c => c.TRID == TRID & c.Repack != null);
            r.FirstOrDefault().LabelDate = DateTime.Parse("07.07.2019");
            fas.SaveChanges();
            return r.FirstOrDefault().LabelDate.ToString();
        }

        bool CheckRePack(string TRID)
        {
            var fas = new FASEntities();
            var _r = fas.M_CadenaID.Where(c => c.TRID == TRID & c.Repack != null).Select(b => b.TRID).FirstOrDefault();
            if (_r != null)
                return true;
            return false;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                GetSN(textBox1);

                if (Grid1.Rows.Count == 0)
                    return;

                for (int i = 0; i < Grid1.Rows.Count; i++)
                {
                    var SN = Grid1[0, i].Value.ToString();
                    var model = Grid1[1, i].Value.ToString();
                    var labelDate = Grid1[2, i].Value.ToString();
                    var lbda = DateTime.Parse(labelDate).ToString("dd.MM.yyyy");
                    print(LabelSN(SN, model, lbda));
                }

                var liter = Grid1[7, 0].Value.ToString();
                var boxnum = Grid1[4, 0].Value.ToString();
                print(LabelBox(liter, boxnum));

                GetLB("Успешно отсканировано и распечатано!", Color.Green, LBEr);

                textBox1.Clear();
                textBox1.Select();
            }
        }

        private void TBBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                GetBoxForLang(TBBox);

            }
        }

        void print(string content)
        {            
            PrintHelper.SendStringToPrinter(printName, content); //Нужно получать ответ от принтера Для Кости 
        }

        void GetSN(TextBox TB)
        {
            using (var fas = new FASEntities())
            {
                var r = fas.M_CadenaID.Where(c => c.TRID == TB.Text).Select(c => c.TRID == c.TRID).FirstOrDefault();

                if (!r)
                {
                    GetLB($"Номер не найден в M_Caden {TB.Text}", Color.Red, LBEr); SN.Clear(); SN.Select();
                    return;
                }

                var boxnum = fas.M_CadenaID.Where(c => c.TRID == TB.Text).Select(c => c.BoxNumber).FirstOrDefault();
                var liter = fas.M_CadenaID.Where(c => c.TRID == TB.Text).Select(c => c.Liter).FirstOrDefault();
                var lotid = fas.M_CadenaID.Where(c => c.TRID == TB.Text).Select(c => c.LOTID).FirstOrDefault();
                var modelid =

                Grid1.DataSource = fas.M_CadenaID.Where(c => c.BoxNumber == boxnum & c.LOTID == lotid & c.Liter == liter & c.TRID == TB.Text).
                    Select(c => new {
                        SN = c.TRID,
                        Модель = fas.M_Models.Where(d => fas.M_LOT_Cadena.Where(b => b.ID == lotid).Select(v => v.ModelID).FirstOrDefault() == d.ModelID).Select(d => d.ModelName).FirstOrDefault(),
                        ДатаЭтикетки = c.LabelDate,
                        Паллет = c.PalletNumber,
                        Коробка = c.BoxNumber,
                        Номер = c.UnitNumber,
                        ДатаУпаковки = c.PackDate,
                        Литер = c.Liter

                    }).ToList();

            }
        }

        void GetBox(TextBox TB,DataGridView grid,Label LBer)
        {
            using (var fas = new FASEntities())
            {
                var r = fas.M_CadenaID.Where(c => c.TRID == TB.Text).Select(c => c.TRID == c.TRID).FirstOrDefault();

                if (!r)
                {
                    GetLB($"Номер не найден в M_Caden {TB.Text}",Color.Red, LBer); TB.Clear(); TB.Select();
                    return;
                }

                var boxnum = fas.M_CadenaID.Where(c => c.TRID == TB.Text).Select(c => c.BoxNumber).FirstOrDefault();
                var liter = fas.M_CadenaID.Where(c => c.TRID == TB.Text).Select(c => c.Liter).FirstOrDefault();
                var lotid = fas.M_CadenaID.Where(c => c.TRID == TB.Text).Select(c => c.LOTID).FirstOrDefault();
                var modelid =

                grid.DataSource = fas.M_CadenaID.Where(c => c.BoxNumber == boxnum & c.LOTID == lotid & c.Liter == liter).OrderByDescending(c=>c.UnitNumber).
                    Select(c => new {
                        SN = c.TRID, 
                        Модель = fas.M_Models.Where(d => fas.M_LOT_Cadena.Where(b=>b.ID == lotid).Select(v=>v.ModelID).FirstOrDefault() == d.ModelID).Select(d=>d.ModelName).FirstOrDefault(),
                        ДатаЭтикетки = c.LabelDate, 
                        Паллет = c.PalletNumber,
                        Коробка = c.BoxNumber, 
                        Номер = c.UnitNumber, 
                        ДатаУпаковки = c.PackDate,
                        Литер = c.Liter
                        
                    }).ToList();

            }
        }

        void GetBoxVeryfi(TextBox TB, DataGridView grid, Label LBer)
        {
            using (var fas = new FASEntities())
            {
                if (!int.TryParse(TB.Text.Substring(6), out int id))
                {
                    return;
                }

                var r = fas.M_CadenaID.Where(c => c.ID == id).Select(c => c.TRID == c.TRID).FirstOrDefault();

                if (!r)
                {
                    GetLB($"Номер не найден в M_Caden {TB.Text}", Color.Red, LBer); TB.Clear(); TB.Select();
                    return;
                }

                var listdata = fas.M_CadenaID.Where(c =>  c.ID == id).Select(c => new { c.BoxNumber,c.Liter,c.LOTID }).ToList();
                var box = listdata[0].BoxNumber;
                var lotid = listdata[0].LOTID;
                var liter = listdata[0].Liter;

                grid.DataSource = fas.M_CadenaID.Where(c =>  c.BoxNumber == box & c.LOTID == lotid & c.Liter == liter).OrderByDescending(c => c.UnitNumber).
                    Select(c => new {
                        SN = c.TRID,
                        Модель = fas.M_Models.Where(d => fas.M_LOT_Cadena.Where(b => b.ID == lotid).Select(v => v.ModelID).FirstOrDefault() == d.ModelID).Select(d => d.ModelName).FirstOrDefault(),
                        ДатаЭтикетки = c.LabelDate,
                        Паллет = c.PalletNumber,
                        Коробка = c.BoxNumber,
                        Номер = c.UnitNumber,
                        ДатаУпаковки = c.PackDate,
                        Литер = c.Liter

                    }).ToList();

            }
        }

        void GetBoxForLang(TextBox TB)
        {
            using (var fas = new FASEntities())
            {
                var r = fas.M_CadenaID.Where(c => c.TRID == TB.Text).Select(c => c.TRID == c.TRID).FirstOrDefault();

                if (!r)
                {
                    GetLB($"Номер не найден в M_Caden {TB.Text}", Color.Red,LbErrLang); TBBox.Clear(); TBBox.Select();
                    return;
                }

                var boxnum = fas.M_CadenaID.Where(c => c.TRID == TB.Text).Select(c => c.BoxNumber).FirstOrDefault();
                var liter = fas.M_CadenaID.Where(c => c.TRID == TB.Text).Select(c => c.Liter).FirstOrDefault();
                var lotid = fas.M_CadenaID.Where(c => c.TRID == TB.Text).Select(c => c.LOTID).FirstOrDefault();
               

                GridLang.DataSource = fas.M_CadenaID.Where(c => c.BoxNumber == boxnum & c.LOTID == lotid & c.Liter == liter).OrderByDescending(c => c.UnitNumber).
                    Select(c => new {
                        ВерсияПО = c.LangPO,
                        SN = c.TRID,
                        Модель = fas.M_Models.Where(d => fas.M_LOT_Cadena.Where(b => b.ID == lotid).Select(v => v.ModelID).FirstOrDefault() == d.ModelID).Select(d => d.ModelName).FirstOrDefault(),
                        ДатаЭтикетки = c.LabelDate,
                        Паллет = c.PalletNumber,
                        Коробка = c.BoxNumber,
                        Номер = c.UnitNumber,
                        ДатаУпаковки = c.PackDate,
                        Литер = c.Liter,                        

                    }).ToList();

            }

            TB.Enabled = false;
        }

        void GetLB(string text, Color color,Label lb)
        {
            lb.Visible = true;
            lb.BackColor = color;
            lb.Text = text;
        }

        string LabelBox(string liter,string boxnum)
        {
  return$@"
^XA~TA000~JSN^LT0^MNW^MTT^PON^PMN^LH0,0^JMA^JUS^LRN^CI0^XZ
^XA
^MMT
^PW650
^LL0201
^LS0
^FT39,83^A0N,54,52^FH\^FDLit^FS
^FT39,146^A0N,54,52^FH\^FDBoxNum^FS
^FT110,83^A0N,54,52^FH\^FD{liter}^FS
^FT262,146^A0N,54,52^FH\^FD{boxnum}^FS
^PQ1,0,1,Y^XZ";
        }

        string LabelSN(string printTextSN, string model,string labeldate)
        {
            var x = Xn.Value;
            var y = Yn.Value;
  return $@"
^XA~TA000~JSN^LT0^MNW^MTT^PON^PMN^LH0,0^JMA^JUS^LRN^CI0^XZ
^XA
^MMT
^PW650
^LL0201
^LS0
^FO{96 + x},{128 + y}^GFA,02560,02560,00040,:Z64:
eJztkz2unTAQhX1F4dJlujdLyAYiO0tKeQsEXN2CkiW8nbwYUVBmC0YU73VxOYXlyTE/bwORkoYpGBkdPp0zY5S66qqr/rZa6aUKlm6eFIn/8p2S0qN4ZXUv8X6L2seiExnkGRoaggTohEk6vQgrS4Mwm2w877xfMkJHo8SiS0U3bjrwEmV96gZZQ3a0FtQkGbpq4znouJVDBwP9tGQiHRLpMTakPVqtrEW71zjioFQVzXNdM72ZkGmYkGjweuGsbDILw+DgOW080y/Q1drj2znQJ69GS2xmzwevX9+zy6aD8zU4GuAvWuVYr1HY9IF59yfTeybM6Rt4nra8RRfR2qjn5fQn0+83SoU3zOCV+bEVx/AH3ixHXiMPeSlzqg+eKrFbl9Ak6mnXgZerbJ01vqG+5M2q+GttA3/I+zx5uquSLTHBKzpVeHXRYX5suh/x4G261z4kN0ycqQOvzM+W+WXXqSOv6W7phQh8wWLTxpOE/RLm14pS5/y6iq0zmy5gyXi37/fnM7LL6uTBX7Sw14YGj7jzInivj3jHbVTxk1e2Gr52bLzx8If71ylr8gNxP8BL/+yvueqqq/5n/QGBgl71:2AB9
^FO{64 + x},{128 + y}^GFA,00256,00256,00004,:Z64:
eJxjYKAEsP9gYOD/AMUPGBjkDzAw2ANxPYhuANJA/J+BgRGIGf4B6T8MDMxAzA6igVrZoVrlkbU3QLSDtNZDtf2nyJF4AQCESRoM:9F12
^FO{0+ x},{128 + y}^GFA,00768,00768,00012,:Z64:
eJxjYBjZgP2AHOMBKNv+hz3zAyi7/g+C/eOPfTOM/aFE/iBM/Q8G+8cw8f8M9p/h7AcIdv0P+Y9w8//Z/YeJM/6xq4exGezs7KFsxgIbGXmoesYfCHGGvzV2/HDz/yDY9R/k2GHm6z+wg7t5FBAHAOLPMJc=:B706
^FO{384 + x},{0 + y}^GFA,00768,00768,00012,:Z64:
eJztjjEKAjEQRSeskHKOMBeRzbVSLLiQwjJH8QoDW2zpFQJ7AC1j4zfZuGAjdmLhL8Ln8XkZon++HofXPn7ke6HBwly59F4CIuKdx8otZgikco6T5vKunIL6Hbf9yCmTq7ZB6KDerL8MHJAyo3E7qZc6pywxpOYxwHxU30vxd8Cl7B2e/Fb2Z175shTPqXI1up3cpS5t/R3/hTwAHyFPIw==:0A21
^BY5,3,81^FT{41 + x},{137 + y}^BCN,,N,N
^FD>;{printTextSN}^FS
^FT{30 + x},{51 + y}^A0N,33,33^FH\^FD{model}^FS
^FT{395 + x},{169 + y}^A0N,33,33^FH\^FD{printTextSN}^FS
^FT{471 + x},{51 + y}^A0N,33,33^FH\^FD{labeldate}^FS
^PQ{Num.Value},0,1,Y^XZ";

            //            string X = "0";
            //            string Y =  "0";
            //            return $@"
            //^ XA~TA000~JSN^LT0^MNW^MTT^PON^PMN^LH20,30^JMA^PR2,2^MD10^JUS^LRN^CI0^XZ
            //^XA
            //^MMT
            //^PW685
            //^LL0354
            //^LS0
            //положение и контент сервисная служба
            //^FO{44 + X},{185 + Y}^GFA,04352,04352,00068,:Z64:
            //eJzt1D9v1DAUAPBnMpgpb2UINt8AxlS4DR+Fj5CKAZ8uOlOdBBv9AsDnYEDgKBIdb2VA4KhCbJUjFg/hzHOuFCRaKIIBpHtjfPrd8/tjgG1sYxvb+C+i+XOCjxef4fpyBv4FQ4afnF3SED85w8sRoP6CUV54kpGRRc661hkwFkbuZl56nWfO6cpWDioWFVdKlRYdOPBl4eod50uo/ALyfeuAi8nIuraPB9GytfTzUAUtuPfaOOPAZLFIxpzokgytvCZDgxki5EMyim9GF222lsGM1aj3MDgdXXQQl1FgodT9sQqqBt0oH8ioIZKBycgVjnzFORld11n+WinTKKUX8prXbvCODctVMrQZb+ni1NADGX3XnholGYccqR6sW1oOZdHsqUIvFDgNg3OZzVaYC6WbPSjzkoyyJiPU4DoLeEBGoWUy5JmhCy0oj7Wim58ahygkFUGQoaAcy9lYD8GzPhnMelAegYzrXSSjswhmJCPMyIgB/FcDo0sGL0Ct9extvR8c9G2AnFEek4F8lwxDNd0YlZ+9S4b0kQz6i4Ing24tQJ3UyRgdDO2Y8qC71JOx08U+2jNjnE+G8WZjvJiMKqQ8Tuqdt27+hBpmF5s8hJaAyPfJ6CdDQ6qHeZYMf2bgLtVDpXpcX9W779z8qYX4QG7qQX2BHPnQtcdte/zVEEakvtAkJQOz59QXLXTqi1z56rG7/dQyC3yTR5oxgbxPRpqPKxpoPJJxlQDvyTjELKTeFlrRfMgjJw9dJe1ARo6sn2b9hrjJWzKmGXuori4IWQhRpjycY/0rZIHmlBquaU7xERm2QjvEjGPOPpGRy7WJR2T0H5Oxrmggd5tGyOD1QPPBIhmehGpsQukn49HG6N5LZNEDo3eMjINpX5bTvqSda8S0L2RAfLkxZKjewB0yrORW4gMfj9cyh3g3vR/JSLv/+Whp2Yhh2v3N3t6jihlFhlYCnXySfr60OBnDh2FEhObu9IbUVOLvQv/4yrhzXp7U1W/xS4P9K4b94RN9jN/LvzSy8wzal/O+/p5hLia2sY1tbOP/ii8gSQz/:A196
            //положение и контент general satellite
            //^FO{224 + X},{217 + Y}^GFA,02688,02688,00028,:Z64:
            //eJzt1M1KxDAQB/CEHPYYwatsHmFfoDT4RF4rVLOwh30LH0UKHjzuIxjpxYNgZBEjhIwzST+268dREDpdevkxZPJPs4zNNdc/qLP0PsGHfl1xwPI8MBWMd6UOYBe2zCbInAhMo3k04waTyWRgBi2QedmbJrMKjdUsliW7CtJ1ZsgaHVjorKpVb5DMBObRINnSl+OYsEZzzDy93OpntGpigFYY/35Ls5SdpS0A7y2SFXW2xYHVbB9LZlxdHFgUnb2SWRMPLCzQfGeXrjc5GOXicL0PB/gezMvernWonFfHRhNazLP6sHKVTKWoFZry2LdEu3JSj2Y1Gl8fn/doX0vlqH+29W8GPxv73mh/kZPpJhgw+6YQO3gY9h5PyUxn9cSCIINv+zBqtLfzaKAWuW9YzydzPAqLVoimUsP5YdRkIls9MYwaLYidsKbNfWk9kb5qsphsn/tGW5FdbHebaR/PUdP53W830z6Wo0aT97CZzkl3hTywxQ7upnPiHYsTA70fDD8VngwexB2uR7H160nwItuNaMn0aByazpa8PerDu5zNl/yxMS3odrTx72Suueb60/oEc4oKnw==:460E
            //модель
            //^FT{20 + X},{32 + Y}^A0N,33,33^FH\^FD11^FS
            //дата
            //^FT{320 + X},{32 + Y}^A0N,33,33^FH\^FD22^FS
            //время
            //^FT{490 + X},{32 + Y}^A0N,33,33^FH\^FD33^FS
            //сн_текст 
            //^FT{86 + X},{68 + Y}^A0N,33,36^FH\^FD{printTextSN}^FS
            //штрихкод
            //^BY3,3,125^FT{45 + X},{197 + Y}^BCN,,N,N
            //^FD>;44^FS
            //made_QC
            //^FT{20 + X},{282 + Y}^A0N,33,31^FH\^FDMade in Russia^FS
            //^FT{448 + X},{282 + Y}^A0N,33,33^FH\^FDQC Passed^FS
            //количество этикеток
            //^PQ1,0,1,Y^XZ";




        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            { MessageBox.Show("Принтер не выбран"); return; }

            printName  = listBox1.SelectedItem.ToString();
            LbPrinter.Text = "Текущий принтер \n" + printName;
            listBox1.ClearSelected();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
           
            //var Result = MessageBox.Show($"Желаете перепечатать SN - {dataGridView1[0,e.RowIndex].Value} \nМодель - {dataGridView1[1, e.RowIndex].Value}?"
            //    ,"Перепечатка Серийного номера",MessageBoxButtons.YesNo,MessageBoxIcon.Information );
            //if (Result != DialogResult.OK)
            //{
            //    return;
            //}

            //var labelDate = dataGridView1[2, e.RowIndex].Value.ToString();
            //var lbda = DateTime.Parse(labelDate).ToString("dd.MM.yyyy");
            //print(LabelSN(dataGridView1[0, e.RowIndex].Value.ToString(), dataGridView1[1, e.RowIndex].Value.ToString(), lbda));
            //GetLB($"SN - {dataGridView1[0, e.RowIndex].Value} Успешено перепечатан!", Color.Green);

        }

        private void RB1_CheckedChanged(object sender, EventArgs e)
        {
            if (RB1.Checked)
            {
                GetGB(new Size(767, 630), new Point(400, 18),GB1);
                GB2.Visible = false;
                Grid1.DataSource = null;
                textBox1.Clear();
                SN.Clear();
                SN.Select();
            }          
           
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (RB2.Checked)
            {
                GetGB(new Size(962, 574), new Point(400, 18), GB2);
                Grid2.Rows.Clear();
                GB1.Visible = false;

                Identify();

                //var list = GetPalletBox();
                //numericUpDown1.Value = int.Parse(list[0]) + 1;
                //LabelDate.Text = list[2];
                //LiterLB.Text = list[3];              

                //SetDate();
            }
        }

        private void RB3_CheckedChanged(object sender, EventArgs e)
        {
            if (RB3.Checked)
            {
                GetGB(new Size(897, 813), new Point(408, 12), GB3);
                GridLang.DataSource = null;
                TBBox.Clear();
                TBBox.Enabled = true;
                ChLang2.Checked = false;
                ChLang1.Checked = false;
                GetDate();
                TBBox.Select();
            }
        }

        void SetDate()
        {
            var countDate = CheckCountInDate(LabelDate.Text);

            //if (countDate == 5200 TH
            //TA 3024
            if (countDate == 5200) //TA
            {
                LabelDate.Text = DateTime.Parse(LabelDate.Text).AddDays(1).ToString("yyyy-MM-dd");

                if (LabelDate.Text == "2020-02-23" || LabelDate.Text == "2020-03-08")
                {
                    LabelDate.Text = DateTime.Parse(LabelDate.Text).AddDays(1).ToString();
                }
            }
        }


        void SetDateD51()
        {
            var countDate = CheckCountInDateD51(LabelDate.Text);

            //if (countDate == 5200 TH
            //TA 3024
            //D51 = 10400
            if (countDate == 10400) //TA
            {
                LabelDate.Text = DateTime.Parse(LabelDate.Text).AddDays(-1).ToString("yyyy-MM-dd");

                if (LabelDate.Text == "2020-02-23" || LabelDate.Text == "2020-03-08")
                {
                    LabelDate.Text = DateTime.Parse(LabelDate.Text).AddDays(-1).ToString();
                }
            }
        }

        void SetDateB50()
        {
            var countDate = CheckCountInDateB50(LabelDate.Text);

            //if (countDate == 5200 TH
            //TA 3024
            //D51 = 10400
            if (countDate == 3024) //TA
            {
                LabelDate.Text = DateTime.Parse(LabelDate.Text).AddDays(-1).ToString("yyyy-MM-dd");

                if (LabelDate.Text == "2020-02-23" || LabelDate.Text == "2020-03-08")
                {
                    LabelDate.Text = DateTime.Parse(LabelDate.Text).AddDays(-1).ToString();
                }
            }
        }

        void SetDateA50()
        {
            var countDate = CheckCountInDateA50(LabelDate.Text);

            //if (countDate == 5200 TH
            //TA 3024
            //D51 = 10400
            if (countDate >= 3024) //TA
            {
                LabelDate.Text = DateTime.Parse(LabelDate.Text).AddDays(1).ToString("yyyy-MM-dd");

                if (LabelDate.Text == "2020-02-23" || LabelDate.Text == "2020-03-08")
                {
                    LabelDate.Text = DateTime.Parse(LabelDate.Text).AddDays(1).ToString();
                }
            }
        }


        int CheckCountInDate(string Date)
        {
            var D = DateTime.Parse(Date);
            using (var fas = new FASEntities())
            {
                return fas.M_CadenaID.Where(c => c.LOTID == LotID & c.IsPacked != null & c.LabelDate == D).Count();
            }
        }

        int CheckCountInDateD51(string Date)
        {
            var D = DateTime.Parse(Date);
            using (var fas = new FASEntities())
            {
                return fas.M_CadenaID.Where(c => c.LOTID == LotID & c.IsPacked != null & c.LabelDate == D).Count();
            }
        }

        int CheckCountInDateB50(string Date)
        {
            var D = DateTime.Parse(Date);
            using (var fas = new FASEntities())
            {
                return fas.M_CadenaID.Where(c => c.LOTID == 5156 & c.IsPacked != null & c.LabelDate == D).Count();
            }
        }

        int CheckCountInDateA50(string Date)
        {
            var D = DateTime.Parse(Date);
            using (var fas = new FASEntities())
            {
                return fas.M_CadenaID.Where(c => c.LOTID == 5154 & c.IsPacked != null & c.LabelDate == D).Count();
            }
        }


        List<string> GetPalletBox()
        {
            using (var fas = new FASEntities())
            {
                var list = fas.M_CadenaID.OrderByDescending(c => c.PackDate).Where(c=> c.LOTID == LotID & c.Liter != "D51 "& c.IsUsed == true ).
                   Select(c=> new {c.BoxNumber, c.PalletNumber, c.LabelDate , c.Liter}).ToList();

                var l = new List<string>();
                foreach (var item in list)
                {                    
                    l.Add(item.BoxNumber.ToString());
                    l.Add(item.PalletNumber.ToString());
                    l.Add(((DateTime)item.LabelDate).ToString("yyyy-MM-dd"));
                    l.Add(item.Liter);
                    break;
                }

                return l;
            }
        
        }


        List<string> GetPalletBoxD51()
        {
            using (var fas = new FASEntities())
            {
                var list = fas.M_CadenaID.OrderByDescending(c => c.PackDate).Where(c => c.LOTID == LotID & c.Liter == "D51" & c.IsUsed == true).
                   Select(c => new { c.BoxNumber, c.PalletNumber, c.LabelDate, c.Liter }).Take(1).ToList();

                var l = new List<string>();
                foreach (var item in list)
                {
                    l.Add(item.BoxNumber.ToString());
                    l.Add(item.PalletNumber.ToString());
                    l.Add(((DateTime)item.LabelDate).ToString("yyyy-MM-dd"));
                    l.Add(item.Liter);
                    break;
                }

                return l;
            }

        }


        List<string> GetPalletBoxB50()
        {
            using (var fas = new FASEntities())
            {
                var list = fas.M_CadenaID.OrderByDescending(c => c.BoxNumber ).Where(c => c.LOTID == 5156 & c.Liter == "B50" & c.IsUsed == true).
                   Select(c => new { c.BoxNumber, c.PalletNumber, c.LabelDate, c.Liter }).Take(1).ToList();

                var l = new List<string>();
                foreach (var item in list)
                {
                    l.Add(item.BoxNumber.ToString());
                    l.Add(item.PalletNumber.ToString());
                    l.Add(((DateTime)item.LabelDate).ToString("yyyy-MM-dd"));
                    l.Add(item.Liter);
                    break;
                }

                return l;
            }

        }

        List<string> GetPalletBoxA50()
        {
            using (var fas = new FASEntities())
            {
                var list = fas.M_CadenaID.OrderByDescending(c => c.BoxNumber).Where(c => c.BoxNumber >= 33000 & c.LOTID == 5154 & c.Liter == "A50" & c.IsUsed == true).
                   Select(c => new { c.BoxNumber, c.PalletNumber, c.LabelDate, c.Liter }).Take(1).ToList();

                var l = new List<string>();
                foreach (var item in list)
                {
                    l.Add(item.BoxNumber.ToString());
                    l.Add(item.PalletNumber.ToString());
                    l.Add(((DateTime)item.LabelDate).ToString("yyyy-MM-dd"));
                    l.Add(item.Liter);
                    break;
                }

                return l;
            }

        }
        void GetGB(Size size, Point point, GroupBox GB)
        {
            GBVisibleoff();
            GB.Visible = true;
            GB.Location = point;
            GB.Size = size;

        }

        void GBVisibleoff()
        {
            foreach (var item in this.Controls.OfType<GroupBox>())            
                item.Visible = false;
           
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (RB2.Checked)
            {
                if (e.KeyCode == Keys.Space)
                {
                    if (CBLiter.Text == "D50")
                        Run();
                    else if (CBLiter.Text == "B50")
                    {
                        RunB50();
                    }
                    else
                    { }
                }
            }
            else if (RB3.Checked)
            {
                if (e.KeyCode == Keys.Space)
                {
                    SetNewDate();
                }
            }
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SetNewDate();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (CBLiter.Text == "D50")
                Run();
            else if (CBLiter.Text == "B50")
            {
                RunB50();
            }
            else if (CBLiter.Text == "A50")
            {
                RunA50();
            }
            else
            { RunD51(); }

        }

        string GetLangCH()
        {
            if (ChLang1.Checked)         
                return "Английская версия ПО";         
            else           
                return "Русская версия ПО";
           
        }

        void SetNewDate()
        {
           
            if (GridLang == null) { MessageBox.Show("Список пустой"); return; }
               
            if (GridLang.RowCount == 0) { MessageBox.Show("Список пустой"); return; }

            var date = GetDate();

            #region
                //if (!ChLang1.Checked & !ChLang2.Checked)
                //{ MessageBox.Show("Не выбрана версия ПО"); return; }

                //var lang = GetLangCH();

                //           0 ВерсияПО = c.LangPO,
                //           1 SN = c.TRID,
                //           2 Модель = fas.M_Models.Where(d => fas.M_LOT_Cadena.Where(b => b.ID == lotid).Select(v => v.ModelID).FirstOrDefault() == d.ModelID).Select(d => d.ModelName).FirstOrDefault(),
                //           3 ДатаЭтикетки = c.LabelDate,
                //           4 Паллет = c.PalletNumber,
                //           5 Коробка = c.BoxNumber,
                //           6 Номер = c.UnitNumber,
                //           7 ДатаУпаковки = c.PackDate,
                //           8 Литер = c.Liter,
            #endregion

            for (int i = 0; i < GridLang.RowCount; i++) { 
                UpdatePackingDate(GridLang[1, i].Value.ToString(), date);
                print(LabelSN(GridLang[1,i].Value.ToString(), GridLang[2, i].Value.ToString(), date));
            }

            print(LabelBox(GridLang[8,1].Value.ToString(), GridLang[5, 1].Value.ToString()));
            GetLB($"Дата {date} присвоена успешно!", Color.Green, LbErrLang);
            GridLang.DataSource = null;
            TBBox.Clear();
            TBBox.Enabled = true;
            TBBox.Select();

        }

        string GetDate()
        {
            string date = "";          

            using (var fas = new FASEntities())
            {
                var LBDate = fas.M_CadenaID.Where(c => c.LOTID == LotID).Select(c => c.LabelDate).Max();
                if (LBDate < DateTime.Parse("2020-05-01"))
                    date = "2020-05-01";
                else
                {
                    var ResultCount = CheckCountInDateD51(LBDate.ToString());
                    if (ResultCount >= 10400)
                    {
                        LBDate = LBDate.Value.AddDays(1);

                    }

                    date = LBDate.Value.ToString("yyyy-MM-dd");

                }


                if (date == "2020-05-01" || date == "2020-05-09")
                {
                    date = DateTime.Parse(date).AddDays(1).ToString("yyyy-MM-dd");
                }

                CurrDateLB.Text = $"Текущая Дата {date} ";
                return DateTime.Parse(date).ToString("dd.MM.yyyy");

                //CurrDateLB.Text = $"Текущая Дата {"2020-05-03"} ";
                //return DateTime.Parse("2020-05-03").ToString("dd.MM.yyyy");
            }
        }



        void RunD51()
        {
            if (numericUpDown1.Value == 0)
            {
                return;
            }

            if (CBLiter.Text == string.Empty)
                return;

            string lang = "Английская версия ПО";

            Grid2.Rows.Clear();

            var ListTrid = GetFreeSN();
            int i = 1;
            foreach (var item in ListTrid)
            {
                UpdatePacking(item, i, "BarTon TH-562", lang);
                i += 1;
            }
            print(LabelBox(CBLiter.Text, numericUpDown1.Value.ToString()));
            numericUpDown1.Value -= 1;

            

            SetDateD51();

        }

        void RunB50()
        {
            if (numericUpDown1.Value == 0)
            {
                return;
            }

            if (CBLiter.Text == string.Empty)
                return;

            //string lang = "Английская версия ПО";

            Grid2.Rows.Clear();

            var ListTrid = GetFreeSNB50();
            int i = 1;
            foreach (var item in ListTrid)
            {
                UpdatePacking(item,  i, "BarTon TA-561");
                i += 1;
            }
            print(LabelBox(CBLiter.Text, numericUpDown1.Value.ToString()));
            numericUpDown1.Value += 1;



            SetDateB50();

        }

        void RunA50()
        {
            if (numericUpDown1.Value == 0)
            {
                return;
            }

            if (CBLiter.Text == string.Empty)
                return;

            //string lang = "Английская версия ПО";

            Grid2.Rows.Clear();

            var ListTrid = GetFreeSNA50();
            int i = 1;
            foreach (var item in ListTrid)
            {
                UpdatePacking(item, i, "BarTon TH-562");
                i += 1;
            }
            print(LabelBox(CBLiter.Text, numericUpDown1.Value.ToString()));
            numericUpDown1.Value += 1;



            SetDateA50();

        }
        void Run()
        {
            if (CBLiter.Text == string.Empty)          
                return;          
                        string lang = "Русскоязычная версия ПО";

        


            Grid2.Rows.Clear();
            var ListTrid = GetFreeSN();
            int i = 1;
            foreach (var item in ListTrid)
            {
                UpdatePacking(item,i, "BarTon TH-562", lang);
                i += 1;
            }
            print(LabelBox(CBLiter.Text, numericUpDown1.Value.ToString()));
            numericUpDown1.Value += 1;

            SetDate();

        }

        void UpdatePacking(string Trid, int i, string Model , string lang = "")
        {
            using (var fas = new FASEntities())
            {
                var pac = fas.M_CadenaID.Where(c => c.TRID == Trid).FirstOrDefault();
                pac.IsUsed = true;
                pac.IsPacked = true;
                pac.LabelDate = DateTime.Parse(DateTime.Parse(LabelDate.Text).ToString("dd.MM.yyyy"));
                pac.PackDate = DateTime.UtcNow.AddHours(2);
                pac.LOTID = LotID;
                pac.BoxNumber = (int)numericUpDown1.Value;                
                pac.Liter = CBLiter.Text;
                pac.UnitNumber = (short)i;
                pac.LangPO = lang;

                fas.SaveChanges();
            }

            print(LabelSN(Trid, Model, DateTime.Parse(LabelDate.Text).ToString("dd.MM.yyyy")));
            Grid2.Rows.Add(i,Trid,LabelDate.Text, DateTime.UtcNow.AddHours(2),LotID, numericUpDown1.Value, CBLiter.Text,lang);
        }

        void UpdatePackingDate(string Trid,  string LabelDate)
        {
            using (var fas = new FASEntities())
            {
                var pac = fas.M_CadenaID.Where(c => c.TRID == Trid).FirstOrDefault();
                pac.LabelDate = DateTime.Parse(LabelDate);
                fas.SaveChanges();
            }          
        }


        List<string> GetFreeSN()
        {
            using (var fas = new FASEntities())
            {
                return fas.M_CadenaID.Where(c => c.LOTID == 5156 & c.IsUsed == false).Select(c => c.TRID).Take(CountSerial).ToList();
            }
        }

        List<string> GetFreeSNB50()
        {
            using (var fas = new FASEntities())
            {
                return fas.M_CadenaID.Where(c => c.IsUsed == false).Select(c => c.TRID).Take(CountSerial).ToList();
            }
        }

        List<string> GetFreeSNA50()
        {
            using (var fas = new FASEntities())
            {
                return fas.M_CadenaID.Where(c => c.IsUsed == false).Select(c => c.TRID).Take(10).ToList();
            }
        }


        private void GB3_Enter(object sender, EventArgs e)
        {

        }

        private void ChLang1_CheckedChanged(object sender, EventArgs e)
        {
            if (ChLang1.Checked)
            {
                ChLang2.Checked = false;
                ChLang1.BackColor = Color.Coral;
            }
            else
            {
                
                ChLang1.BackColor = Color.Transparent;
            }
        }

        private void ChLang2_CheckedChanged(object sender, EventArgs e)
        {
            if (ChLang2.Checked)
            {
                ChLang1.Checked = false;
                ChLang2.BackColor = Color.Lime;
            }
            else
            {
               
                ChLang2.BackColor = Color.Transparent;
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            GridLang.DataSource = null;
            TBBox.Enabled = true;
            TBBox.Clear();
            TBBox.Select();
        }

        private void RB4_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void CBLiter_SelectedIndexChanged(object sender, EventArgs e)
        {
            Identify();
        }

        void Identify()
        {
            if (CBLiter.Text == "D51")
            {
                var list = GetPalletBoxD51();
                if (list.Count == 0)
                {
                    numericUpDown1.Value = 20000;
                    LabelDate.Text = "2020-04-30";
                    LiterLB.Text = "D51";
                }
                else
                {
                    numericUpDown1.Value = int.Parse(list[0]) - 1;
                    LabelDate.Text = list[2];
                    LiterLB.Text = list[3];
                }

                SetDateD51();
            }
            else if (CBLiter.Text == "B50")
            {
                var list = GetPalletBoxB50();
             
                    numericUpDown1.Value = int.Parse(list[0]) + 1;
                    LabelDate.Text = list[2];
                    LiterLB.Text = list[3];
                SetDateB50();


            }
            else if (CBLiter.Text == "A50")
            {
                var list = GetPalletBoxA50();

                numericUpDown1.Value = int.Parse(list[0]) + 1;
                LabelDate.Text = list[2];
                LiterLB.Text = list[3];
                SetDateA50();
            }
            else
            {
                var list = GetPalletBox();
                numericUpDown1.Value = int.Parse(list[0]) + 1;
                LabelDate.Text = list[2];
                LiterLB.Text = list[3];

                SetDate();
            }
        }

        int UserID;
        private void RBVerify_CheckedChanged(object sender, EventArgs e)
        {
            if (RBVerify.Checked)
            {
                ConfirmUser conf = new ConfirmUser();
                var r = conf.ShowDialog();
                if (r != DialogResult.OK) //Логин
                {
                    
                    RB1.Checked = true;
                    return;
                }

                UserID = conf.UserID;
                UserLabel.Text = conf.UserName;

                GetGB(new Size(774, 836), new Point(413,13), GBVeryfy);
                TBVerify.Clear();
                GridVerify.DataSource = null;
                TBVerifyUnit.Visible = false;
                LBunit.Visible = false;
                TBVerify.Enabled = true;
                button6.Visible = false;
                ListVerify.Clear();
                TBVerify.Select();
            }
        }

        private void TBVerify_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var result = CheckVerify(TBVerify.Text);

                if (result != "true")
                {
                    TBVerify.Clear(); TBVerify.Select();
                    MessageBox.Show(result); return;
                }

                ListVerify.Clear();
                GridVerify.DataSource = null;

                GetBoxVeryfi(TBVerify, GridVerify, LBErrror);

                if (GridVerify.Rows.Count == 0)
                    return;               

                string message = $" Коробка №{GridVerify[4,0].Value};\n Дата этикетки {GridVerify[2,0].Value};\n Литер {GridVerify[7,0].Value};\n Данные коробки совпадают с вашими?";

                YesNo yesno =  new YesNo(message);
                var Dialog = yesno.ShowDialog();

                if (Dialog == DialogResult.No)
                {
                    TBVerify.Clear();
                    GridVerify.DataSource = null;
                    TBVerifyUnit.Visible = false;
                    LBunit.Visible = false;
                    TBVerify.Select();
                    return;
                }

                TBVerifyUnit.Visible = true;
                LBunit.Visible = true;
                ListVerify.Clear();
                TBVerify.Clear();
                GridVerify.ClearSelection();
                TBVerify.Enabled = false;
                button6.Visible = true;
                TBVerifyUnit.Select();
            }
        }

        List<string> ListVerify = new List<string>();
        private void TBVerifyUnit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LBErrror.Text = "";
                var SN = TBVerifyUnit.Text;
                for (int i = 0; i < GridVerify.RowCount; i++)
                {
                    if (GridVerify[0,i].Value.ToString() == SN)
                    {
                        if (ListVerify.Contains(SN))
                        {
                            GetLB($"Приемник с номером {SN} был отсканирован ранее", Color.Yellow, LBErrror); TBVerifyUnit.Clear(); TBVerifyUnit.Select();                     
                            return;
                        }

                        GridVerify.Rows[i].DefaultCellStyle.BackColor = Color.GreenYellow;
                        ListVerify.Add(SN);
                        var count = GridVerify.RowCount - ListVerify.Count;
                        if (count == 0)
                        {
                            VeryfiGot();
                            GetLB($"Коробка {GridVerify[4,0].Value} Верифицирована!", Color.GreenYellow, LBErrror);
                            GridVerify.DataSource = null; 
                            TBVerifyUnit.Visible = false; LBunit.Visible = false; TBVerify.Clear(); TBVerify.Enabled = true;  LBErrror.Text = ""; TBVerify.Select();
                            return;
                        }
                        else                     
                            GetLB($"Приемник №{SN} подтвержден, осталось подтвердить {count}шт.", Color.GreenYellow, LBErrror); TBVerifyUnit.Clear(); TBVerifyUnit.Select();
                        return;

                    }
                }
                Error er = new Error();
                er.ShowDialog();

                var list = GetData(SN);
                if (list.Count == 0)
                {
                    GetLB($"Приемник с номером {SN} не найден в базе M_CADEN", Color.Coral, LBErrror); TBVerifyUnit.Clear(); TBVerifyUnit.Select();
                    return;
                }

                GetLB($"Приемник №{SN} не подтвержден в коробке {GridVerify[4,0].Value} \n по данным базы, приемник с номером {SN} находится в коробке {list[0]}\nДатаЭтикетки {list[1]}\nЛитер {list[2]}", Color.Coral, LBErrror);
                TBVerifyUnit.Clear(); TBVerifyUnit.Select();
                return;
            }
        }

        void VeryfiGot()
        {
            using (var fas = new FASEntities())
            {
                
                for (int i = 0; i < GridVerify.RowCount; i++)
                {
                    var sn = GridVerify[0, i].Value.ToString();

                    if (!int.TryParse(sn.Substring(6), out int id))
                    {
                        return;
                    }

                    var R = fas.M_CadenaID.Where(c =>  c.ID == id);
                    R.FirstOrDefault().Verify = true;
                    R.FirstOrDefault().UserID = UserID;
                    R.FirstOrDefault().DateVerify = DateTime.UtcNow.AddHours(2);
                }

                fas.SaveChanges();

            }
        }

        List<string> GetData(string sn)
        {
            using ( var fas = new FASEntities())
            {
                List<string> list = new List<string>();
                if (!int.TryParse(sn.Substring(6), out int id))
                {
                    return list;
                }
                var result = fas.M_CadenaID.Where(c =>  c.ID == id).Select(c => new { c.BoxNumber, c.LabelDate, c.Liter }).ToList();
                

                result.ForEach(c => list.AddRange(new List<string>() { c.BoxNumber.ToString(), c.LabelDate.ToString(), c.Liter })   );
                return list;
            }
        }        

        string CheckVerify(string sn)
        {
            using (var fas  =new FASEntities())
            {

                if (!int.TryParse(sn.Substring(6), out int id))
                {
                    return "Ошибка в преобразовании";
                }
                

                var result = fas.M_CadenaID.Where(c =>   c.ID == id).Select(c => c.TRID).FirstOrDefault();
                if (result == null)               
                    return $"Номер {sn} не найден в базе M_CADEN";   
                if (result == "")               
                    return $"Номер {sn} не найден в базе M_CADEN";

                var R = fas.M_CadenaID.Where(c => c.ID == id).Select(c => c.Verify).FirstOrDefault();
                if (R == null)
                {
                    return "true";
                }

                if (R == false)
                {
                    return "true";
                }

                var list = GetData(result);


                return $"Номер приемника {result} был ранее верифицирован\nКоробка {list[0]}\nДатаЭтикетки {list[1]}\nЛитер {list[2]}";


            }

           
        }

        private void GBVeryfy_Enter(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            TBVerify.Enabled = true; TBVerifyUnit.Visible = false; LBunit.Visible = false; button6.Visible = false; LBErrror.Text = ""; TBVerify.Clear(); TBVerify.Select();
            GridVerify.DataSource = null;
        }

        private void TBBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

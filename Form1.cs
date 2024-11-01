using System;
using System.Collections.Generic;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        Universo universe = new Universo();
        List<Body> corpos = new List<Body>();
        List<Body> corpos_removidos = new List<Body>();


        public Form1()
        {
            bool ExceptionMass = false;
            corpos = universe.ReadBodies();

            if (corpos.Count > 500)
                MessageBox.Show("Quantidade maxima atingida   (MAXIMO: 200) ");
            else if (ExceptionMass)
                MessageBox.Show("A massa dos corpos devem estar entre 1 e 499");
            else
            {
                InitializeComponent();
                this.Paint += Form1_Load;
            }
        }

        async private void Form1_Load(object sender, EventArgs e)
        {
            Graphics g = this.CreateGraphics();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Pen pen = new Pen(Color.Black, 10);

            for (int i = 0; i < universe.getInterations(); i++)
            {
                foreach (Body corpo_1 in corpos)
                {
                    //Calcular força de todos os corpos
                    if (i > 0)
                    {
                        foreach (Body corpo_2 in corpos)
                        {
                            if (corpo_1 == corpo_2) continue;
                            if (this.corpos_removidos.Contains(corpo_1) || this.corpos_removidos.Contains(corpo_2)) continue;

                            universe.GravitationalForceBodies(corpo_1, corpo_2);

                            RectangleF rect = new RectangleF(new PointF((float)corpo_2.getPosX(), (float)corpo_2.getPosY()), new Size(Convert.ToInt32(corpo_2.getRadius()), (int)corpo_2.getRadius()));
                            g.FillEllipse(Brushes.Black, rect);
                            g.DrawEllipse(pen, rect);
                        }
                    }

                    universe.InteractionForceBodies(corpos);
                    universe.InteractionBodies(corpos);
                    universe.ClearForce(corpos);

                    await Task.Delay(10);
                    g.Clear(SystemColors.Control);
                }
                this.corpos_removidos = universe.CheckColision(corpos);
            }
        }

    }
}

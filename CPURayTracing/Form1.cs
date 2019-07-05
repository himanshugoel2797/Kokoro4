using CPURayTracing.RayTracer;
using CPURayTracing.RayTracer.Materials;
using CPURayTracing.RayTracer.Primitives;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CPURayTracing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            pos = Vector3.UnitZ * 2;
        }

        int click_cnt = 2;
        Vector3 pos;

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            int w = this.Width / 8;
            int h = this.Height / 8;

            var scene = new Scene(w, h, 90, pos, Vector3.UnitY, Vector3.UnitZ, 1e10f);
            scene.ClearColor = new Vector4(0, 0.5f, 1.0f, 0);
            scene.AddPrimitive(new Sphere(Vector3.UnitY * click_cnt, 1, 0) { Material = new DiffuseMaterial() { Color = Vector4.One } });
            scene.AddPrimitive(new Sphere(Vector3.UnitY * click_cnt + Vector3.UnitZ * 4, 1, 1) { Material = new DiffuseMaterial() { Color = Vector4.UnitX } });
            //scene.AddPrimitive(new Sphere(Vector3.UnitY * click_cnt - Vector3.UnitZ * 2, 1f, 2) { Material = new EmissiveMaterial() { EmissiveColor = Vector4.One } });
            scene.AddPrimitive(new Sphere(Vector3.UnitY * click_cnt + Vector3.UnitZ * 2, 0.75f, 3) { Material = new EmissiveMaterial() { EmissiveColor = Vector4.UnitX + Vector4.UnitW * 10 } });

            Bitmap bmp = new Bitmap(w, h);
            scene.Render(bmp, 3);

            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.DrawImage(bmp, 0, 0, pictureBox1.Width, pictureBox1.Height);
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == 'w')
            {
                pos += Vector3.UnitY * 0.5f;
            }
            if (e.KeyChar == 's')
            {
                pos -= Vector3.UnitY * 0.5f;
            }
            if (e.KeyChar == 'a')
            {
                pos += Vector3.UnitX * 0.5f;
            }
            if (e.KeyChar == 'd')
            {
                pos -= Vector3.UnitX * 0.5f;
            }
            if (e.KeyChar == 'q')
            {
                pos -= Vector3.UnitZ * 0.5f;
            }
            if (e.KeyChar == 'e')
            {
                pos += Vector3.UnitZ * 0.5f;
            }
            if(e.KeyChar == '+')
            {
                click_cnt++;
            }
            if (e.KeyChar == '-')
            {
                click_cnt--;
            }
            pictureBox1.Invalidate();
        }
    }
}

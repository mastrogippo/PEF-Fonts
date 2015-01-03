using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace fonts
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        class FontStore
        {
            public string name;
            public int numfonts;
            public Chr[] fonts;

            public void Parse(byte[] inp)
            {
                name = System.Text.Encoding.Default.GetString(inp,0,16).Trim() + System.Text.Encoding.Default.GetString(inp,16,8).Trim();
                numfonts =  inp[0x1C];
                fonts =new Chr[numfonts];

                int off = 0x1E;
                for(int i = 0; i < numfonts; i++)
                {
                    fonts[i] = new Chr();
                    fonts[i].CharCode = inp[off+1];
                    fonts[i].CharCode <<= 8;
                    fonts[i].CharCode += inp[off];
                    fonts[i].ASCII = inp[off];

                    fonts[i].height = inp[off + 3];

                    fonts[i].width = inp[off + 2];
                    //if (fonts[i].height > 8) fonts[i].width *= 2;

                    fonts[i].data = new UInt16[fonts[i].width];

                    off += 4;
                    for(int j = 0; j < fonts[i].width; j++)
                    {
                        fonts[i].data[j] = 0;
                        if (j >= 8)
                        {
                            for (int k = 1; k <= (fonts[i].height * 2); k += 2)
                            {
                                fonts[i].data[j] <<= 1;
                                fonts[i].data[j] += (byte)((inp[k + off] >> (8-j)) & 1);
                            }

                        }
                        else
                        {
                            for (int k = 0; k < (fonts[i].height * 2); k += 2)
                            {
                                fonts[i].data[j] <<= 1;
                                fonts[i].data[j] += (byte)((inp[k + off] >> j) & 1);
                            }
                        }
                    }
                    off += (fonts[i].height * 2);

                    if (fonts[i].ASCII == 0x76)
                        fonts[i].ASCII = 0x76;
                    //WARNING! THIS IS TO REMOVE SPACES AFTER FONTS!!
                    if(fonts[i].width > 1)
                        if (fonts[i].data[fonts[i].width-1] == 0) 
                            fonts[i].width--;
                }
            }
        }
        class Chr
        {
            public int CharCode; //PROBLEM WITH UNICODE! SOME FONTS ARE 2BYTES W
            public byte ASCII;
            public byte width;
            public byte height;
            public UInt16[] data;
        }

        FontStore[] fnt;

        byte[] bu;
        byte[][] fonts;
        private void Form1_Load(object sender, EventArgs e)
        {
            int size;
            bu = File.ReadAllBytes("font.pef");
            
            fonts = new byte[bu[0x08]][];
            fnt = new FontStore[bu[0x08]];

            int offset = 0x0C;
            for(int i = 0; i < bu[0x08]; i++)
            {
                size = getint(bu, offset + 24);

                fonts[i] = new byte[size];

                int j;
                for (j = 0; j < size; j++ )
                    fonts[i][j] = bu[offset++];

                fnt[i] = new FontStore();
                try { fnt[i].Parse(fonts[i]); }
                catch { }// GESTIREE!!!!
            }

            int Nf = 0;
            prova(Nf);

            AddFonts(Nf);
            CalcWidth(Nf, 0x20, 0x3f);
            CalcWidth(Nf, 0x40, 0x5f);
            CalcWidth(Nf, 0x60, 0x7f);


            //GenMemory_high(Nf);
            GenMemory_line(Nf);
            //GenMemory(Nf);
        }

        void AddFonts(int Nfont)
        {
            for(int i = 0; i < fnt[Nfont].numfonts; i++)
            {
                checkedListBox1.Items.Add("\" " + (char)fnt[Nfont].fonts[i].ASCII + " \" " + (fnt[Nfont].fonts[i].CharCode).ToString(), ((fnt[Nfont].fonts[i].CharCode >= 0x20) && (fnt[Nfont].fonts[i].CharCode <= 0x7B)));
//                if((fnt[Nfont].fonts[i].ASCII >= 0x20) &&(fnt[Nfont].fonts[i].ASCII <= 0x7B))
//                    checkedListBox1.Items[checkedListBox1.Items.Count]
            }

        }
       /* void GenMemory_high(int Nfont)
        {
            int[] Offsets = new int[7];
            int[] IndF = new int[0x60];
            byte[] DataSec = new byte[5000];
            int min = 0x20;
            int max = 0x7F;

            int index = 0;
            for (int i = 0; i < fnt[Nfont].numfonts; i++)
            {
                if ((fnt[Nfont].fonts[i].CharCode >= min) && (fnt[Nfont].fonts[i].CharCode <= max))
                {
                    if (fnt[Nfont].fonts[i].CharCode >= 0x60)
                        IndF[fnt[Nfont].fonts[i].CharCode - 0x20] = index - Offsets[6];
                    else if (fnt[Nfont].fonts[i].CharCode >= 0x50)
                        IndF[fnt[Nfont].fonts[i].CharCode - 0x20] = index - Offsets[5];
                    else if (fnt[Nfont].fonts[i].CharCode >= 0x40)
                        IndF[fnt[Nfont].fonts[i].CharCode - 0x20] = index - Offsets[4];
                    else if (fnt[Nfont].fonts[i].CharCode >= 0x30)
                        IndF[fnt[Nfont].fonts[i].CharCode - 0x20] = index - Offsets[3];
                    else if (fnt[Nfont].fonts[i].CharCode >= 0x20)
                        IndF[fnt[Nfont].fonts[i].CharCode - 0x20] = index - Offsets[2];
                    else if (fnt[Nfont].fonts[i].CharCode >= 0x10)
                        IndF[fnt[Nfont].fonts[i].CharCode - 0x20] = index - Offsets[1];
                    else
                        IndF[fnt[Nfont].fonts[i].CharCode - 0x20] = index;

                    DataSec[index++] = fnt[Nfont].fonts[i].width;
                    for (int j = 0; j < fnt[Nfont].fonts[i].width; j++)
                    {
                        if (fnt[Nfont].fonts[i].height <= 8)
                            DataSec[index++] = ReverseBits((byte)fnt[Nfont].fonts[i].data[j]); //INVERT BYTES
                        else
                        {
                            DataSec[index++] = ReverseBits((byte)fnt[Nfont].fonts[i].data[j]); //INVERT BYTES
                            DataSec[index++] = ReverseBits((byte)(fnt[Nfont].fonts[i].data[j] >> 8)); //INVERT BYTES
                        }
                        //DataSec[index++] = fnt[Nfont].fonts[i].data[j];
                    }

                    Console.WriteLine((char)fnt[Nfont].fonts[i].ASCII);
                }
                if (fnt[Nfont].fonts[i].CharCode == 0x3F)
                    Offsets[1] = index;
                else if (fnt[Nfont].fonts[i].CharCode == 0x5F)
                    Offsets[2] = index;
            }

            textBox1.Text = "";
            textBox1.Text += "uint8_t Offsets[2] = {" + Offsets[1].ToString() + ", " + (Offsets[2] - Offsets[1]).ToString() + "};\r\n";

            textBox1.Text += "uint8_t fnt1[0x20] = {";
            for (int i = 0; i < 0x10; i++) textBox1.Text += IndF[i] + ", ";

            textBox1.Text += "};\r\nuint8_t fnt2[0x20] = {";
            for (int i = 0x10; i < 0x20; i++) textBox1.Text += IndF[i] + ", ";

            textBox1.Text += "};\r\nuint8_t fnt3[0x20] = {";
            for (int i = 0x20; i < 0x30; i++) textBox1.Text += IndF[i] + ", ";

            textBox1.Text += "};\r\nuint8_t fnt4[0x20] = {";
            for (int i = 0x30; i < 0x40; i++) textBox1.Text += IndF[i] + ", ";

            textBox1.Text += "};\r\nuint8_t fnt5[0x20] = {";
            for (int i = 0x40; i < 0x50; i++) textBox1.Text += IndF[i] + ", ";

            textBox1.Text += "};\r\nuint8_t fnt6[0x20] = {";
            for (int i = 0x50; i < 0x60; i++) textBox1.Text += IndF[i] + ", ";

            textBox1.Text += "};\r\n";

            byte[] tmp = new byte[index];
            Array.Copy(DataSec, tmp, index);
            File.WriteAllBytes("font.bin", tmp);
           
        }
    */

        void GenMemory_line(int Nfont)
        {
            int[] IndF = new int[0x50];
            byte[] DataSec = new byte[5000];
            int min = 0x30;
            int max = 0x7A;

            int index = 0;
            for (int i = 0; i < fnt[Nfont].numfonts; i++)
            {
                if ((fnt[Nfont].fonts[i].CharCode >= min) && (fnt[Nfont].fonts[i].CharCode <= max))
                {
                    IndF[fnt[Nfont].fonts[i].CharCode - min] = index;

                    DataSec[index++] = fnt[Nfont].fonts[i].width;
                    for (int j = 0; j < fnt[Nfont].fonts[i].width; j++)
                    {
                        if (fnt[Nfont].fonts[i].height <= 8)
                            DataSec[index++] = ReverseBits((byte)fnt[Nfont].fonts[i].data[j]); //INVERT BYTES
                        else
                        {
                            fnt[Nfont].fonts[i].data[j] <<= 2;
                            DataSec[index++] = ReverseBits((byte)fnt[Nfont].fonts[i].data[j]); //INVERT BYTES
                            DataSec[index++] = ReverseBits((byte)(fnt[Nfont].fonts[i].data[j] >> 8)); //INVERT BYTES
                        }
                        //DataSec[index++] = fnt[Nfont].fonts[i].data[j];
                    }

                    Console.WriteLine((char)fnt[Nfont].fonts[i].ASCII);
                }
            }

            textBox1.Text = "";
            textBox1.Text += "uint16_t fnt1[] = {";
            for (int i = 0; i < (max-min); i++) textBox1.Text += IndF[i] + ", ";
            textBox1.Text += "};\r\n";

            byte[] tmp = new byte[index];
            Array.Copy(DataSec, tmp, index);
            File.WriteAllBytes("font.bin", tmp);
            /*
            for(int i = 0; i < 0x60; i++)
            {


            }*/
        }
        void GenMemory(int Nfont)
        {
            int[] Offsets = new int[3];
            int[] IndF = new int[0x60];
            byte[] DataSec = new byte[5000];
            int min = 0x20;
            int max = 0x7F;

            int index = 0;
            for(int i = 0; i < fnt[Nfont].numfonts; i++)
            {
                if ((fnt[Nfont].fonts[i].CharCode >= min) && (fnt[Nfont].fonts[i].CharCode <= max))
                {
                    if (fnt[Nfont].fonts[i].CharCode >= 0x60)
                        IndF[fnt[Nfont].fonts[i].CharCode - 0x20] = index - Offsets[2];
                    else if (fnt[Nfont].fonts[i].CharCode >= 0x40)
                        IndF[fnt[Nfont].fonts[i].CharCode - 0x20] = index - Offsets[1];
                    else
                        IndF[fnt[Nfont].fonts[i].CharCode - 0x20] = index;

                    DataSec[index++] = fnt[Nfont].fonts[i].width;
                    for(int j = 0; j < fnt[Nfont].fonts[i].width; j++)
                    {
                        if( fnt[Nfont].fonts[i].height <= 8)
                            DataSec[index++] = ReverseBits((byte)fnt[Nfont].fonts[i].data[j]); //INVERT BYTES
                        else
                        {
                            DataSec[index++] = ReverseBits((byte)fnt[Nfont].fonts[i].data[j]); //INVERT BYTES
                            DataSec[index++] = ReverseBits((byte)(fnt[Nfont].fonts[i].data[j] >> 8)); //INVERT BYTES
                        }
                        //DataSec[index++] = fnt[Nfont].fonts[i].data[j];
                    }
                        
                    Console.WriteLine((char)fnt[Nfont].fonts[i].ASCII);
                }
                if (fnt[Nfont].fonts[i].CharCode == 0x3F)
                    Offsets[1] = index;
                else if (fnt[Nfont].fonts[i].CharCode == 0x5F)
                    Offsets[2] = index;
            }

            textBox1.Text = "";
            textBox1.Text += "uint8_t Offsets[2] = {" + Offsets[1].ToString() + ", " + (Offsets[2] - Offsets[1]).ToString() + "};\r\n";
            textBox1.Text += "uint8_t fnt1[0x20] = {";
            for (int i = 0; i < 0x20; i++) textBox1.Text += IndF[i] + ", ";
            textBox1.Text += "};\r\nuint8_t fnt2[0x20] = {";
            for (int i = 0x20; i < 0x40; i++) textBox1.Text += IndF[i] + ", ";
            textBox1.Text += "};\r\nuint8_t fnt3[0x20] = {";
            for (int i = 0x40; i < 0x60; i++) textBox1.Text += IndF[i] + ", ";
            textBox1.Text += "};\r\n";

            byte[] tmp = new byte[index];
            Array.Copy(DataSec, tmp, index);
            File.WriteAllBytes("font.bin", tmp);
            /*
            for(int i = 0; i < 0x60; i++)
            {


            }*/
        }

        byte ReverseBits(byte c)
        {
            byte g = c;
            byte b = 0;
            for (int i = 0; i < 7; i++)
            {
                b += (byte)(c & 1);
                c >>= 1;
                b <<= 1;
            }
            b += (byte)(c & 1);
            return b;
        }

        int CalcWidth(int Nfont, int min, int max)
        {
            int wi = 0;
            for(int i = 0; i < fnt[Nfont].numfonts; i++)
            {
                if ((fnt[Nfont].fonts[i].CharCode >= min) && (fnt[Nfont].fonts[i].CharCode <= max))
                {
                    wi += fnt[Nfont].fonts[i].width;
                }
            }
            return wi;

        }

        void prova(int Nfont)
        {
            Bitmap gg = new Bitmap(2000, 15);
            int ind = 0;
            for(int i = 0; i < fnt[Nfont].numfonts; i++)
            {
                for (int j = 0; j < fnt[Nfont].fonts[i].width; j++)
                {
                    for(int k = 0; k < fnt[Nfont].fonts[i].height; k++)
                    {
                        int bit = (((fnt[Nfont].fonts[i].data[j] >> ((fnt[Nfont].fonts[i].height-1) - k)) & 1)/*== 1*/);
                        if (bit!= 0) gg.SetPixel(ind, k+1, Color.Black);
                        //gg.SetPixel(ind, k, Color.Red);// Color.FromArgb(bit==1 ? 0xFFFFFF : 0xFF0000));

                    }
                    ind++;
                }
            }
            pictureBox1.Image = gg; 
            //pictureBox1.Width = gg.Width * 2;
            //pictureBox1.Height = gg.Height * 2;
        }

        int getint(byte[] data, int offs)
        {
            int gg = data[offs+1];
            gg <<= 8;
            gg += data[offs];
            return gg;
        }
    }
}

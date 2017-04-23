using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;

namespace AIS_Decoder
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string [] paths;
        public string [] safe_file_names;
        ais_decode_lib decode = new ais_decode_lib();
        //text_form t_f = new text_form();

        // 瀏覽
        private void browse_button_Click(object sender, EventArgs e)
        {
            // 讀檔
            decode.read_data(ref path_comboBox,ref paths,ref safe_file_names);
        }

        // 執行
        private void action_button_Click(object sender, EventArgs e)
        {           

            // 資料處理
            decode.initialize(paths,safe_file_names);

            decode.start_decode();

            decode.decoding();

            decode.output();

            //decode.Show_error_code(ref t_f);
                
            
            MessageBox.Show("Finish！！！");
        }

        private void description_button_Click(object sender, EventArgs e)
        {
            pic_form p_f = new pic_form();
            p_f.Show();
        }

        /******************************************************/
        /*
        // 更新進度條
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.progressBar1.Value = decode.temp_data_count;
        }
        */
    }
}

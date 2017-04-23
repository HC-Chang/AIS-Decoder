using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.IO;

namespace AIS_Decoder
{
    public class ais_decode_lib
    {
        string[] input_paths;
        string input;
        string[] output_paths;
        string output_path;
        List<List<many_data>> l_data = new List<List<many_data>>();
        string temp_ais_message = "";
        int type = 0;

        string[] error_codes;
        string error_code = "";
        
        // 讀檔
        public void read_data(ref ComboBox p_c_b,ref string[] ps,ref string[] s_f_ns)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            ofd.Multiselect = true;
            p_c_b.Items.Clear();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i<ofd.SafeFileNames.Length;i++ )
                {
                    p_c_b.Items.Add(ofd.SafeFileNames[i]);
                }
                p_c_b.SelectedIndex = 0;
                ps = ofd.FileNames;
                s_f_ns = ofd.SafeFileNames;
                MessageBox.Show("Finish");
            }       
        }

    

        #region 執行



        // 初始化
        public void initialize(string [] input_data_paths, string [] safe_data_paths)
        {
            input_paths = input_data_paths;
            output_paths = safe_data_paths;

        }

        // 開始解碼
        public void start_decode()
        {
            StreamReader sr;
            
            for (int i = 0; i < input_paths.Length; i++)
            {
                l_data.Add(new List<many_data>());
                sr = new StreamReader(input_paths[i]);
                string[] row_data;
                string[] temp_s_array;
                while (!sr.EndOfStream)
                {
                    input = sr.ReadLine();
                    
                    row_data = input.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);             
                    
                    // 將資料加入 list - data
                    temp_s_array = new string[2];
                    temp_s_array = row_data[0].Split('\t');
                    temp_s_array[1] = temp_s_array[1].TrimStart('\\').TrimStart('"').TrimEnd('"').TrimEnd('\\');
                    l_data[i].Add(new many_data { times = temp_s_array[0], datas = temp_s_array[1] });

                    // 排序
                    /******************************************************************
                    l_data.Sort((x, y) => { return x.times.CompareTo(y.times); });
                    l_data.Sort((x, y) => { return -x.datas.CompareTo(y.datas);});
                    *******************************************************************/
                }
               
            }         
        }

        // 解碼中
        public void decoding()
        {
            string[] sort;
            error_codes = new string[l_data.Count];
            for (int i = 0; i < l_data.Count; i++)
            {
                sort = new string[l_data[i].Count];
                for (Int32 a = 0; a < l_data[i].Count; a++)
                {
                    // 選擇解碼方式
                    string[] sort_decoding = l_data[i][a].datas.Split(',');
                    switch (sort_decoding[0])
                    {
                        case "$GPRMC":
                            GPRMC_decode(l_data[i][a].datas);
                            break;
                        case "!AIVDO":
                        case "!AIVDM":
                            temp_ais_message = l_data[i][a].datas;
                            AIVDM_AIVDO_decode(l_data[i][a].times, l_data[i][a].datas);
                            l_data[i][a].datas = temp_ais_message;
                            break;
                    }

                    if (a % 100 == 0 && a / 500 > 0)
                    {
                        error_codes[i] += error_code;
                        error_code = "";
                    }
                }

                error_codes[i] += error_code;
                error_code = "";
            }
            
        }

        // GPS 解碼
        private void GPRMC_decode(string s_gps_decoding)
        {

        }

        #region AIS

        // AIS 解碼
        private void AIVDM_AIVDO_decode(string a ,string b)
        {
            // 檢驗信文
            bool accurated = check_sum();
            if (accurated)
            {
                // temp_ais_message 指定為信文加密內容
                temp_ais_message = temp_ais_message.Split(',')[5];
				
                // 判斷信文是否為空信文
                if (temp_ais_message == "")
                {
                    return;
                }

                // temp_ais_message 轉為2進制6位元
                temp_ais_message = decimal2bianry();
                type = message_type();

//                MessageBox.Show(type.ToString());
                
                try
                {
                    switch (type)
                    {
                        case 1:
                        case 2:
                        case 3:
                            temp_ais_message = decode_123();
                            break;
                        case 5:
                            temp_ais_message = decode_5();
                            break;
                        case 8:
                            temp_ais_message = decode_8();
                            break;
//                        case 15:
//                            break;
                        case 18:
                            temp_ais_message = decode_18();
                            break;
                        case 19:
                            temp_ais_message = decode_19();
                            break;
                        case 24:
                            temp_ais_message = decode_24();
                            break;
                        default:
                            temp_ais_message = "";
                            error_code += type.ToString() + "\t" + a + "\t" + b +"\r\n";
                            break;
                    }
                }
                catch
                {
                    temp_ais_message = type.ToString() + ",message error !," + a + "," + b;
                    error_code += type.ToString() + "\tmessage error !\t" + a + "\t" + b +"\r\n";
                }
            }
            else
            {
                temp_ais_message = type.ToString() + ",check sum error !," + a + "," + b ;
                error_code += type.ToString() + "\tcheck sum error !\t" + a + "\t" + b +"\r\n";
            }
        }

 

        #region 解碼方式

        // 檢驗信文
        private bool check_sum()
        {
            string[] compare_s = new string[2];
            compare_s = temp_ais_message.Split('*');
            int sum = 0;

            // Not Or 處理
            for (int i = 1; i < compare_s[0].Length; i++)
            {
                sum ^= temp_ais_message[i];
            }
            // 16進制轉換
            compare_s[0] = Convert.ToString(sum, 16).ToUpper();

            // 不足兩位數時例外處理
            if (compare_s[0].Length < 2)
            {
                compare_s[0] = "0" + compare_s[0];
            }

            // 檢驗比對
            if (string.Compare(compare_s[0], compare_s[1].Trim()) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // 信文種類
        private int message_type()
        {
            return decode_u(0, 6);
        }

        // 十進制轉2進制(6位元)
        private string decimal2bianry()
        {
            int [] number = new int[temp_ais_message.Length];
            string[] cut = new string[temp_ais_message.Length];
            string all = "";
            
            for (int a = 0; a < number.Length; a++)
            {
                number[a] = (int)temp_ais_message[a];
                if (number[a] < 48 || number[a] > 119 || (96 > number[a] && number[a] > 87))
                {
                    MessageBox.Show("信文錯誤 !!!", "警告");
          //          return "000000";
                }
                if (number[a] < 88)
                    number[a] = number[a] - 48;
                if (number[a] > 95)
                    number[a] = number[a] - 56;
            }

            // 10進制轉2進制
            for (int a = 0; a < number.Length; a++)
            {
                cut[a] = Convert.ToString(number[a], 2);
                //將2進制補足6bit
                cut[a] = cut[a].PadLeft(6, '0');
            }

            //將所有區間的6bit彙整
            for (int a = 0; a < number.Length; a++)
            {
                all += cut[a];
            }
            return all;
        }

        private int decode_u(int c, int d)
        {
            string a="";
            Int32 b = 0;
            a = temp_ais_message.Substring(c, d);
            b = Convert.ToInt32(a, 2);
            return b;
        }

        private void decode_e(ref string a, ref int b, int c, int d)
        {
            a = temp_ais_message.Substring(c, d);
            b = Convert.ToInt32(a, 2);
            if (b == 60)
                a = "Noncombatant ship according to RR Resolution No. 18";
        }

        private string decode_t(int a)
        {
            string b;
            if (0 <= a && a <= 31)
                a += 64;
            //  if (32 <= a && a <= 63)
            //    a = a;
            b = Convert.ToString((Char)a);
            return b;
        }

        private double decode_lon(int c, int d)
        {
            string a, e, f = null;
            double b = 0;
            a = temp_ais_message.Substring(c, d);
            if (a[0] == '0')//代表正數
            {
                for (int k = 0; k < 28; k++)
                {
                    b += (a[k] - 48) * (Math.Pow(2, 27 - k));
                }
            }
            if (a[0] == '1')//代表負數
            {
                e = a.Substring(0, 27) + '0';
                for (int k = 0; k < 28; k++)
                {
                    if (e[k] == '1')
                    {
                        f += '0';
                    }
                    if (e[k] == '0')
                    {
                        f += '1';
                    }
                }
                for (int k = 0; k < 28; k++)
                {
                    b += (f[k] - 48) * (Math.Pow(2, 27 - k));
                }
                b = -b;
            }
            return b / 600000;
        }

        private double decode_lat(int c, int d)
        {
            string a, e, f = null;
            double b = 0;
            a = temp_ais_message.Substring(c, d);
            if (a[0] == '0')//代表正數
            {
                for (int k = 0; k < 27; k++)
                {
                    b += (a[k] - 48) * (Math.Pow(2, 26 - k));
                }
            }
            if (a[0] == '1')//代表負數
            {
                e = a.Substring(0, 26) + '0';
                for (int k = 0; k < 27; k++)
                {
                    if (e[k] == '1')
                    {
                        f += '0';
                    }
                    if (e[k] == '0')
                    {
                        f += '1';
                    }
                }
                for (int k = 0; k < 27; k++)
                {
                    b += (f[k] - 48) * (Math.Pow(2, 26 - k));
                }
                b = -b;
            }
            return b / 600000;
        }

        // 解碼 call sign, ship name, destination
        private string decode_name(int start, int count)
        {
            // int start 起始字元
            // int count 字元個數
            int I = 0;
            string S = "";
            string [] cut = new string [count];
            for ( int a = 0; a < count; a++)
            {
                I = decode_u(start + a * 6, 6);
                cut[a] = decode_t(I);
                if (cut[a] == "@")
                {
                    S += " ";
                }
                else
                {
                    S += cut[a];
                }
            }
            return S;
        }

        #endregion

        #region 解碼種類

            private string decode_123()
            {
                string[] all_temp_s_array = new string[14];
                bool[] display = new bool[14];
                display = new bool[] {false,true,false,false,true,
                                        false,true,true,false,true};

                Int32 temp_i;
                string temp_s = "";

                 // 1. repeat    u
                all_temp_s_array[0] = decode_u(6, 2).ToString();              

                // 2. mmsi      u
                all_temp_s_array[1] = decode_u(8, 30).ToString().PadLeft(9,'0');               

                // 3. status    e
                temp_i = decode_u(38,4);
                switch (temp_i)
                {
                    case 0:
                        all_temp_s_array[2] = "Under way using engine";
                        break;
                    case 1:
                        all_temp_s_array[2] = "At anchor";
                        break;
                    case 2:
                        all_temp_s_array[2] = "Not under command";
                        break;
                    case 3:
                        all_temp_s_array[2] = "Restricted manoeuverability";
                        break;
                    case 4:
                        all_temp_s_array[2] = "Constrained by her draught";
                        break;
                    case 5:
                        all_temp_s_array[2] = "Moored";
                        break;
                    case 6:
                        all_temp_s_array[2] = "Aground";
                        break;
                    case 7:
                        all_temp_s_array[2] = "Engaged in Fishing";
                        break;
                    case 8:
                        all_temp_s_array[2] = "Under way sailing";
                        break;
                    case 9:
                        all_temp_s_array[2] = "Reserved for future amendment of Navigational Status for HSC";
                        break;
                    case 10:
                        all_temp_s_array[2] = "Reserved for future amendment of Navigational Status for WIG";
                        break;
                    case 11:
                        all_temp_s_array[2] = "Reserved for future use";
                        break;
                    case 12:
                        all_temp_s_array[2] = "Reserved for future use";
                        break;
                    case 13:
                        all_temp_s_array[2] = "Reserved for future use";
                        break;
                    case 14:
                        all_temp_s_array[2] = "Reserved for future use";
                        break;
                    case 15:
                        all_temp_s_array[2] = "Not defined (default)";
                        break;
                }

                // 4. turn          I3
                temp_i = decode_u(42, 8);
                switch (temp_i)
                {
                    case 0:
                        all_temp_s_array[3] = "not turning";
                        break;
                    case 127:
                        all_temp_s_array[3] = "turning right at more than 5deg/30s (No TI available)";
                        break;
                    case -127:
                        all_temp_s_array[3] = "turning left at more than 5deg/30s (No TI available)";
                        break;
                    case 128:
                        all_temp_s_array[3] = "no turn information available (default)";
                        break;
                    default:
                        if (temp_i > 0)
                        {
                            all_temp_s_array[3] = "turning right at up to 708 degrees per minute or higher";
                        }
                        else
                        {
                            all_temp_s_array[3] = "turning left at up to 708 degrees per minute or higher";
                        }
                        break;
                }

                // 5. speed         U1
                all_temp_s_array[4] = (((float)decode_u(50, 10)) / 10).ToString();

                // 6. accuracy      b
                temp_i = decode_u(60, 1);
                switch (temp_i)
                {
                    case 0:
                        all_temp_s_array[5] = "使用DGPS修正，精確度 < 10 m";
                        break;
                    case 1:
                        all_temp_s_array[5] = "使用GNSS修正，精確度 > 10 m";
                        break;
                }

                // 7. lon           I4
                all_temp_s_array[6] = decode_lon(61,28).ToString();

                // 8. lat           I4
                all_temp_s_array[7] = decode_lat(89,27).ToString();

                // 9. course        U1
                all_temp_s_array[8] = (decode_u(116, 12) / 10).ToString();

                // 10.heading       u
                temp_i = decode_u(128, 9);
                if (temp_i == 511)
                {
                    all_temp_s_array[9] = "Not Available ";
                }
                else all_temp_s_array[9] = temp_i.ToString() + "degree";

                // 11.Time Stamp    u
                all_temp_s_array[10] = decode_u(137, 6).ToString();

                // 12.maneuver      e
                temp_i = decode_u(143, 2);
                switch (temp_i)
                {
                    case 0:
                        all_temp_s_array[11] = "Not available (default)";
                        break;
                    case 1:
                        all_temp_s_array[11] = "No special maneuver";
                        break;
                    case 2:
                        all_temp_s_array[11] = "Special maneuver (such as regional passing arrangement)";
                        break;
                }

                // 13.raim          b
                temp_i = decode_u(148, 1);
                switch (temp_i)
                {
                    case 0:
                        all_temp_s_array[12] = "RAIM not in use(default)";
                        break;
                    case 1:
                        all_temp_s_array[12] = "RAIM in use";
                        break;
                }

                // 14.radio         u
                all_temp_s_array[13] = decode_u(149, 19).ToString();



                for (int a = 0; a < display.Length; a++)
                {
                    if (display[a])
                    {
                        temp_s += all_temp_s_array[a] + ",";
                    }
                }
                temp_s = temp_s.TrimEnd(',');

                return type + "," +temp_s;

            }

            /*type 5 多信文整合*/
            private string decode_5()
        {
            string[] all_temp_s_array = new string[19];
            bool[] display = new bool[19];
            // 設定布林值

            Int32 temp_i;
            string temp_s = "";

            // 1. repeat    u
            all_temp_s_array[0] = decode_u(6, 2).ToString();

            // 2. mmsi      u
            all_temp_s_array[1] = decode_u(8, 30).ToString();

            // 3. ais_version   u
            temp_i = decode_u(38, 2);
            switch (temp_i)
            {
                case 0:
                    all_temp_s_array[2] = "[ITU1371]";
                    break;
                case 1:
                case 2:
                case 3:
                    all_temp_s_array[2] = " future editions";
                    break;
            }

            // 4. imo           u
            all_temp_s_array[3] = decode_u(40, 30).ToString();

            // 5. callsign      t
            all_temp_s_array[4] = decode_name(70,7);

            // 6. shipname      t
            /********************************************************/
            #region error
                
            all_temp_s_array[5] = decode_name(112, 20);

            // 7. shiptype  u
            temp_i = decode_u(232, 8);
            if (temp_i == 0)
            {
                all_temp_s_array[6] = "Not available (default)";
            }
            if (temp_i >= 1 && temp_i <= 19)
            {
                all_temp_s_array[6] = "Reserved for future use";
            }
            if (temp_i == 20)
            {
                all_temp_s_array[6] = "Wing in ground (WIG),all ships of this type";
            }
            if (temp_i == 21)
            {
                all_temp_s_array[6] = "Wing in ground (WIG),Hazardous category A";
            }
            if (temp_i == 22)
            {
                all_temp_s_array[6] = "Wing in ground (WIG),Hazardous category B";
            }
            if (temp_i == 23)
            {
                all_temp_s_array[6] = "Wing in ground (WIG),Hazardous category C";
            }
            if (temp_i == 24)
            {
                all_temp_s_array[6] = "Wing in ground (WIG),Hazardous category D";
            }
            if (temp_i >= 25 && temp_i <= 29)
            {
                all_temp_s_array[6] = "Wing in ground (WIG), Reserved for future use";
            }
            if (temp_i == 30)
            {
                all_temp_s_array[6] = "Fishing";
            }
            if (temp_i == 31)
            {
                all_temp_s_array[6] = "Towing";
            }
            if (temp_i == 32)
            {
                all_temp_s_array[6] = "Towing: length exceeds 200m or breadth exceeds 25m";
            }
            if (temp_i == 33)
            {
                all_temp_s_array[6] = "Dredging or underwater ops";
            }
            if (temp_i == 34)
            {
                all_temp_s_array[6] = "Diving ops";
            }
            if (temp_i == 35)
            {
                all_temp_s_array[6] = "Military ops";
            }
            if (temp_i == 36)
            {
                all_temp_s_array[6] = "Sailing";
            }
            if (temp_i == 37)
            {
                all_temp_s_array[6] = "Pleasure Craft";
            }
            if (temp_i == 38 || temp_i == 39)
            {
                all_temp_s_array[6] = "Reserved";
            }
            if (temp_i == 40)
            {
                all_temp_s_array[6] = "High speed craft (HSC), all ships of this type";
            }
            if (temp_i == 41)
            {
                all_temp_s_array[6] = "High speed craft (HSC), Hazardous category A";
            }
            if (temp_i == 42)
            {
                all_temp_s_array[6] = "High speed craft (HSC), Hazardous category B";
            }
            if (temp_i == 43)
            {
                all_temp_s_array[6] = "High speed craft (HSC), Hazardous category C";
            }
            if (temp_i == 44)
            {
                all_temp_s_array[6] = "High speed craft (HSC), Hazardous category D";
            }
            if (temp_i >= 45 && temp_i <= 48)
            {
                all_temp_s_array[6] = "High speed craft (HSC), Reserved for future use";
            }
            if (temp_i == 49)
            {
                all_temp_s_array[6] = "High speed craft (HSC), No additional information";
            }
            if (temp_i == 50)
            {
                all_temp_s_array[6] = "Pilot Vessel";
            }
            if (temp_i == 51)
            {
                all_temp_s_array[6] = "Search and Rescue vessel";
            }
            if (temp_i == 52)
            {
                all_temp_s_array[6] = "Tug";
            }
            if (temp_i == 53)
            {
                all_temp_s_array[6] = "Port Tender";
            }
            if (temp_i == 54)
            {
                all_temp_s_array[6] = "Anti-pollution equipment";
            }
            if (temp_i == 55)
            {
                all_temp_s_array[6] = "Law Enforcement";
            }
            if (temp_i == 56 || temp_i == 57)
            {
                all_temp_s_array[6] = "Spare - Local Vessel";
            }
            if (temp_i == 58)
            {
                all_temp_s_array[6] = "Medical Transport";
            }
            if (temp_i == 59)
            {
                all_temp_s_array[6] = "Noncombatant Ship according to RR Resolution No.18";
            }
            if (temp_i == 60)
            {
                all_temp_s_array[6] = "Passenger,all ships of this type";
            }
            if (temp_i == 61)
            {
                all_temp_s_array[6] = "Passenger, Hazardous category A";
            }
            if (temp_i == 62)
            {
                all_temp_s_array[6] = "Passenger, Hazardous category B";
            }
            if (temp_i == 63)
            {
                all_temp_s_array[6] = "Passenger, Hazardous category C";
            }
            if (temp_i == 64)
            {
                all_temp_s_array[6] = "Passenger, Hazardous category D";
            }
            if (temp_i >= 65 && temp_i <= 68)
            {
                all_temp_s_array[6] = "Passenger, Reserved for future use";
            }
            if (temp_i == 69)
            {
                all_temp_s_array[6] = "Passenger, No additional information";
            }
            if (temp_i == 70)
            {
                all_temp_s_array[6] = "Cargo, all ships of this type";
            }
            if (temp_i == 71)
            {
                all_temp_s_array[6] = "Cargo, Hazardous category A";
            }
            if (temp_i == 72)
            {
                all_temp_s_array[6] = "Cargo, Hazardous category B";
            }
            if (temp_i == 73)
            {
                all_temp_s_array[6] = "Cargo, Hazardous category C";
            }
            if (temp_i == 74)
            {
                all_temp_s_array[6] = "Cargo, Hazardous category D";
            }
            if (temp_i >= 75 && temp_i <= 78)
            {
                all_temp_s_array[6] = "Cargo, Reserved for future use";
            }
            if (temp_i == 79)
            {
                all_temp_s_array[6] = "Cargo, No additional information";
            }
            if (temp_i == 80)
            {
                all_temp_s_array[6] = "Tanker, all ships of this type";
            }
            if (temp_i == 81)
            {
                all_temp_s_array[6] = "Tanker, Hazardous category A";
            }
            if (temp_i == 82)
            {
                all_temp_s_array[6] = "Tanker, Hazardous category B";
            }
            if (temp_i == 83)
            {
                all_temp_s_array[6] = "Tanker, Hazardous category C";
            }
            if (temp_i == 84)
            {
                all_temp_s_array[6] = "Tanker, Hazardous category D";
            }
            if (temp_i >= 85 && temp_i <= 88)
            {
                all_temp_s_array[6] = "Tanker, Reserved for future use";
            }
            if (temp_i == 89)
            {
                all_temp_s_array[6] = "Tanker, No additional information";
            }
            if (temp_i == 90)
            {
                all_temp_s_array[6] = "Other Type, all ships of this type";
            }
            if (temp_i == 91)
            {
                all_temp_s_array[6] = "Other Type, Hazardous category A";
            }
            if (temp_i == 92)
            {
                all_temp_s_array[6] = "Other Type, Hazardous category B";
            }
            if (temp_i == 93)
            {
                all_temp_s_array[6] = "Other Type, Hazardous category C";
            }
            if (temp_i == 94)
            {
                all_temp_s_array[6] = "Other Type, Hazardous category D";
            }
            if (temp_i >= 95 && temp_i <= 98)
            {
                all_temp_s_array[6] = "Other Type, Reserved for future use";
            }
            if (temp_i == 99)
            {
                all_temp_s_array[6] = "Other Type, No additional information";
            }

            // 8. bow       u
            all_temp_s_array[7] = decode_u(240, 9).ToString();

            // 9. stern     u
            all_temp_s_array[8] = decode_u(249, 9).ToString();

            // 10. port      u
            all_temp_s_array[9] = decode_u(258, 6).ToString();

            // 11. starboard u
            all_temp_s_array[10] = decode_u(264, 6).ToString();

            // 12. epfd      e
            temp_i = decode_u(270, 4);
            switch (temp_i)
            {
                case 0:
                    all_temp_s_array[11] = "Undefined (default)";
                    break;
                case 1:
                    all_temp_s_array[11] = "GPS";
                    break;
                case 2:
                    all_temp_s_array[11] = "GLONASS";
                    break;
                case 3:
                    all_temp_s_array[11] = "Combined GPS/GLONASS";
                    break;
                case 4:
                   all_temp_s_array[11] = "Loran-C";
                    break;
                case 5:
                    all_temp_s_array[11]= "Chayka";
                    break;
                case 6:
                    all_temp_s_array[11] = "Integrated navigation system";
                    break;
                case 7:
                    all_temp_s_array[11] = "Surveyed";
                    break;
                case 8:
                    all_temp_s_array[11] = "Galileo";
                    break;
            }

            // 13. month         u
            all_temp_s_array[12] = decode_u(274, 4).ToString();

            // 14. day           u
            all_temp_s_array[13] = decode_u(278, 5).ToString();

            // 15. hour          u
            all_temp_s_array[14] = decode_u(283, 5).ToString();

            // 16. minute        u
            all_temp_s_array[15] = decode_u(288, 6).ToString();

            // 17. draught       U1
            all_temp_s_array[16] = (decode_u(294, 8)/10).ToString();

            // 18. destination   t
           all_temp_s_array[17] = decode_name(302,20);

            // 19. dte           b
            temp_i = decode_u(422, 1);
            switch (temp_i)
            {
                case 0:
                    all_temp_s_array[18] = "Data terminal ready";
                    break;
                case 1:
                    all_temp_s_array[18] = "Not ready (default)";
                    break;
            }
                
            #endregion


            for (int a = 0; a < display.Length; a++)
            {
                if (display[a])
                {
                    temp_s += all_temp_s_array[a] + ",";
                }
            }
            temp_s = temp_s.TrimEnd(',');

            return type + "," + temp_s;
        }

            private string decode_8()
            {
                string[] all_temp_s_array = new string[5];
                bool[] display = new bool[5];
                display = new bool[] { false, true, true,true, };

                string temp_s = "";

                // 1. repeat    u
                all_temp_s_array[0] = decode_u(6, 2).ToString();

                // 2. source mmsi      u
                all_temp_s_array[1] = decode_u(8, 30).ToString().PadLeft(9, '0');

                // 3. Designated Area Code      u
                all_temp_s_array[2] = decode_u(40, 10).ToString();

                // 4. Functional ID     u
                all_temp_s_array[3] = decode_u(50, 6).ToString();

                /*********************************************************************/
                // 5. Data      d
                

                for (int a = 0; a < display.Length; a++)
                {
                    if (display[a])
                    {
                        temp_s += all_temp_s_array[a] + ",";
                    }
                }
                temp_s = temp_s.TrimEnd(',');

                return type + "," + temp_s;
            }

            private string decode_18()
        {
            string [] all_temp_s_array = new string [18];
            bool [] display = new bool [18];
            display = new bool[]{false,true,true,false,true,
                                    true,false,true,false,false,
                                    false,false,false,false,false,
                                    false,false,false};
            Int32 temp_i = 0;
            string temp_s = "";

            // 1. repeat    u
            all_temp_s_array[0] = decode_u(6, 2).ToString();

            // 2. mmsi      u
            all_temp_s_array[1] = decode_u(8, 30).ToString();

            // 3. speed     u
            all_temp_s_array[2] = (((float)decode_u(46, 10))/10).ToString();

            // 4. accuracy  b
            temp_i = decode_u(56, 1);
            switch (temp_i)
            {
                case 0:
                    all_temp_s_array[3] = "使用DGPS修正，精確度 < 10 m";
                    break;
                case 1:
                    all_temp_s_array[3] = "使用GNSS修正，精確度 > 10 m";
                    break;
            }

            // 5. lon       I4
            all_temp_s_array[4] = decode_lon(57, 28).ToString();           

            // 6. lat       I4
            all_temp_s_array[5] = decode_lat(85, 27).ToString();

            // 7. course    U1
            all_temp_s_array[6] = (decode_u(112, 12)/10).ToString();

            // 8. heading   u
            temp_i = decode_u(124, 9);
            if (temp_i == 511)
            {
                all_temp_s_array[7] = "Not Available ";
            }
            else all_temp_s_array[7] = temp_i.ToString() + "degree";

            // 9. second    u
            all_temp_s_array[8] = decode_u(133, 6).ToString();

            // 10. regional  u
            all_temp_s_array[9] = decode_u(139, 2).ToString();

            // 11. cs        b
            temp_i = decode_u(141, 1);
            switch (temp_i)
            {
                case 0:
                    all_temp_s_array[10] = "Class B SOTDMA unit";
                    break;
                case 1:
                    all_temp_s_array[10] = "Class B CS (Carrier Sense) unit";
                    break;
            }

            // 12. display   b
            temp_i = decode_u(142, 1);
            switch (temp_i)
            {
                case 0:
                    all_temp_s_array[11] = "No visual display";
                    break;
                case 1:
                    all_temp_s_array[11] = "Has display, (Probably not reliable)";
                    break;
            }

            // 13. dsc       b
            temp_i = decode_u(143, 1);
            switch (temp_i)
            {
                case 0:
                    all_temp_s_array[12] = temp_i.ToString();
                    break;
                case 1:
                    all_temp_s_array[12] = "unit is attached to a VHF voice radio with DSC capability";
                    break;
            }

            // 14. band      b
            temp_i = decode_u(144, 1);
            switch (temp_i)
            {
                case 0:
                    all_temp_s_array[13] = temp_i.ToString();
                    break;
                case 1:
                    all_temp_s_array[13] = "the unit can use any part of the marine channel";
                    break;
            }

            // 15. msg22     b
            temp_i = decode_u(145, 1);
            switch (temp_i)
            {
                case 0:
                    all_temp_s_array[14] = temp_i.ToString();
                    break;
                case 1:
                    all_temp_s_array[14] = "unit can accept a channel assignment via Message Type 22";
                    break;
            }

            // 16. assigned  b
            temp_i = decode_u(146, 1);
            switch (temp_i)
            {
                case 0:
                    all_temp_s_array[15] = "autonomous mode (default)";
                    break;
                case 1:
                    all_temp_s_array[15] = "assigned mode";
                    break;
            }

            // 17. raim      b
            temp_i = decode_u(147, 1);
            switch (temp_i)
            {
                case 0:
                    all_temp_s_array[16] = "RAIM not in use(default)";
                    break;
                case 1:
                    all_temp_s_array[16] = "RAIM in use";
                    break;
            }

            // 18. radio     u
            all_temp_s_array[17] = decode_u(148, 20).ToString();



            for (int a = 0; a < display.Length; a++)
            {
                if (display[a])
                {
                    temp_s += all_temp_s_array[a] + ",";
                }
            }
            temp_s = temp_s.TrimEnd(',');

            return type + "," + temp_s;
        }

            private string decode_19()
            {
                string[] all_temp_s_array = new string[20];
                bool[] display = new bool[20];
                display = new bool[]{false,true,true,false,true,
                                    true,false,true,false,false,
                                    true,true,false,false,false,
                                    false,false,false,false,false};
                Int32 temp_i = 0;
                string temp_s = "";

                // 1. repeat    u
                all_temp_s_array[0] = decode_u(6, 2).ToString();

                // 2. mmsi      u
                all_temp_s_array[1] = decode_u(8, 30).ToString();

                // 3. speed     u
                all_temp_s_array[2] = (((float)decode_u(46, 10)) / 10).ToString();

                // 4. accuracy  b
                temp_i = decode_u(56, 1);
                switch (temp_i)
                {
                    case 0:
                        all_temp_s_array[3] = "使用DGPS修正，精確度 < 10 m";
                        break;
                    case 1:
                        all_temp_s_array[3] = "使用GNSS修正，精確度 > 10 m";
                        break;
                }

                // 5. lon       I4
                all_temp_s_array[4] = decode_lon(57, 28).ToString();

                // 6. lat       I4
                all_temp_s_array[5] = decode_lat(85, 27).ToString();

                // 7. course    U1
                all_temp_s_array[6] = (decode_u(112, 12) / 10).ToString();

                // 8. heading   u
                temp_i = decode_u(124, 9);
                if (temp_i == 511)
                {
                    all_temp_s_array[7] = "Not Available ";
                }
                else all_temp_s_array[7] = temp_i.ToString() + "degree";

                // 9. second    u
                all_temp_s_array[8] = decode_u(133, 6).ToString();

                // 10. Regional reserved    u
                all_temp_s_array[9] = decode_u(139,4).ToString();

                // 11. shipname     s
                all_temp_s_array[10] = decode_name(143, 120);

                // 12. shiptype     u
                temp_i = decode_u(263, 8);
                if (temp_i == 0)
                {
                    all_temp_s_array[11] = "Not available (default)";
                }
                if (temp_i >= 1 && temp_i <= 19)
                {
                    all_temp_s_array[11] = "Reserved for future use";
                }
                if (temp_i == 20)
                {
                    all_temp_s_array[11] = "Wing in ground (WIG),all ships of this type";
                }
                if (temp_i == 21)
                {
                    all_temp_s_array[11] = "Wing in ground (WIG),Hazardous category A";
                }
                if (temp_i == 22)
                {
                    all_temp_s_array[11] = "Wing in ground (WIG),Hazardous category B";
                }
                if (temp_i == 23)
                {
                    all_temp_s_array[11] = "Wing in ground (WIG),Hazardous category C";
                }
                if (temp_i == 24)
                {
                    all_temp_s_array[11] = "Wing in ground (WIG),Hazardous category D";
                }
                if (temp_i >= 25 && temp_i <= 29)
                {
                    all_temp_s_array[11] = "Wing in ground (WIG), Reserved for future use";
                }
                if (temp_i == 30)
                {
                    all_temp_s_array[11] = "Fishing";
                }
                if (temp_i == 31)
                {
                    all_temp_s_array[11] = "Towing";
                }
                if (temp_i == 32)
                {
                    all_temp_s_array[11] = "Towing: length exceeds 200m or breadth exceeds 25m";
                }
                if (temp_i == 33)
                {
                    all_temp_s_array[11] = "Dredging or underwater ops";
                }
                if (temp_i == 34)
                {
                    all_temp_s_array[11] = "Diving ops";
                }
                if (temp_i == 35)
                {
                    all_temp_s_array[11] = "Military ops";
                }
                if (temp_i == 36)
                {
                    all_temp_s_array[11] = "Sailing";
                }
                if (temp_i == 37)
                {
                    all_temp_s_array[11] = "Pleasure Craft";
                }
                if (temp_i == 38 || temp_i == 39)
                {
                    all_temp_s_array[11] = "Reserved";
                }
                if (temp_i == 40)
                {
                    all_temp_s_array[11] = "High speed craft (HSC), all ships of this type";
                }
                if (temp_i == 41)
                {
                    all_temp_s_array[11] = "High speed craft (HSC), Hazardous category A";
                }
                if (temp_i == 42)
                {
                    all_temp_s_array[11] = "High speed craft (HSC), Hazardous category B";
                }
                if (temp_i == 43)
                {
                    all_temp_s_array[11] = "High speed craft (HSC), Hazardous category C";
                }
                if (temp_i == 44)
                {
                    all_temp_s_array[11] = "High speed craft (HSC), Hazardous category D";
                }
                if (temp_i >= 45 && temp_i <= 48)
                {
                    all_temp_s_array[11] = "High speed craft (HSC), Reserved for future use";
                }
                if (temp_i == 49)
                {
                    all_temp_s_array[11] = "High speed craft (HSC), No additional information";
                }
                if (temp_i == 50)
                {
                    all_temp_s_array[11] = "Pilot Vessel";
                }
                if (temp_i == 51)
                {
                    all_temp_s_array[11] = "Search and Rescue vessel";
                }
                if (temp_i == 52)
                {
                    all_temp_s_array[11] = "Tug";
                }
                if (temp_i == 53)
                {
                    all_temp_s_array[11] = "Port Tender";
                }
                if (temp_i == 54)
                {
                    all_temp_s_array[11] = "Anti-pollution equipment";
                }
                if (temp_i == 55)
                {
                    all_temp_s_array[11] = "Law Enforcement";
                }
                if (temp_i == 56 || temp_i == 57)
                {
                    all_temp_s_array[11] = "Spare - Local Vessel";
                }
                if (temp_i == 58)
                {
                    all_temp_s_array[11] = "Medical Transport";
                }
                if (temp_i == 59)
                {
                    all_temp_s_array[11] = "Noncombatant Ship according to RR Resolution No.18";
                }
                if (temp_i == 60)
                {
                    all_temp_s_array[11] = "Passenger,all ships of this type";
                }
                if (temp_i == 61)
                {
                    all_temp_s_array[11] = "Passenger, Hazardous category A";
                }
                if (temp_i == 62)
                {
                    all_temp_s_array[11] = "Passenger, Hazardous category B";
                }
                if (temp_i == 63)
                {
                    all_temp_s_array[11] = "Passenger, Hazardous category C";
                }
                if (temp_i == 64)
                {
                    all_temp_s_array[11] = "Passenger, Hazardous category D";
                }
                if (temp_i >= 65 && temp_i <= 68)
                {
                    all_temp_s_array[11] = "Passenger, Reserved for future use";
                }
                if (temp_i == 69)
                {
                    all_temp_s_array[11] = "Passenger, No additional information";
                }
                if (temp_i == 70)
                {
                    all_temp_s_array[11] = "Cargo, all ships of this type";
                }
                if (temp_i == 71)
                {
                    all_temp_s_array[11] = "Cargo, Hazardous category A";
                }
                if (temp_i == 72)
                {
                    all_temp_s_array[11] = "Cargo, Hazardous category B";
                }
                if (temp_i == 73)
                {
                    all_temp_s_array[11] = "Cargo, Hazardous category C";
                }
                if (temp_i == 74)
                {
                    all_temp_s_array[11] = "Cargo, Hazardous category D";
                }
                if (temp_i >= 75 && temp_i <= 78)
                {
                    all_temp_s_array[11] = "Cargo, Reserved for future use";
                }
                if (temp_i == 79)
                {
                    all_temp_s_array[11] = "Cargo, No additional information";
                }
                if (temp_i == 80)
                {
                    all_temp_s_array[11] = "Tanker, all ships of this type";
                }
                if (temp_i == 81)
                {
                    all_temp_s_array[11] = "Tanker, Hazardous category A";
                }
                if (temp_i == 82)
                {
                    all_temp_s_array[11] = "Tanker, Hazardous category B";
                }
                if (temp_i == 83)
                {
                    all_temp_s_array[11] = "Tanker, Hazardous category C";
                }
                if (temp_i == 84)
                {
                    all_temp_s_array[11] = "Tanker, Hazardous category D";
                }
                if (temp_i >= 85 && temp_i <= 88)
                {
                    all_temp_s_array[11] = "Tanker, Reserved for future use";
                }
                if (temp_i == 89)
                {
                    all_temp_s_array[11] = "Tanker, No additional information";
                }
                if (temp_i == 90)
                {
                    all_temp_s_array[11] = "Other Type, all ships of this type";
                }
                if (temp_i == 91)
                {
                    all_temp_s_array[11] = "Other Type, Hazardous category A";
                }
                if (temp_i == 92)
                {
                    all_temp_s_array[11] = "Other Type, Hazardous category B";
                }
                if (temp_i == 93)
                {
                    all_temp_s_array[11] = "Other Type, Hazardous category C";
                }
                if (temp_i == 94)
                {
                    all_temp_s_array[11] = "Other Type, Hazardous category D";
                }
                if (temp_i >= 95 && temp_i <= 98)
                {
                    all_temp_s_array[11] = "Other Type, Reserved for future use";
                }
                if (temp_i == 99)
                {
                    all_temp_s_array[11] = "Other Type, No additional information";
                }

                // 13. Dimension to Bow     u
                all_temp_s_array[12] = decode_u(271,9).ToString();

                // 14. Dimension to Stern     u
                all_temp_s_array[13] = decode_u(280, 9).ToString();

                // 15. Dimension to Port     u
                all_temp_s_array[14] = decode_u(289, 6).ToString();

                // 16. Dimension to Starboard     u
                all_temp_s_array[15] = decode_u(295, 6).ToString();

                // 17. Position Fix Type    e
                temp_i = decode_u(301, 4);
                switch (temp_i)
                {
                    case 0:
                        all_temp_s_array[16] = "Undefined (default)";
                        break;
                    case 1:
                        all_temp_s_array[16] = "GPS";
                        break;
                    case 2:
                        all_temp_s_array[16] = "GLONASS";
                        break;
                    case 3:
                        all_temp_s_array[16] = "Combined GPS/GLONASS";
                        break;
                    case 4:
                        all_temp_s_array[16] = "Loran-C";
                        break;
                    case 5:
                        all_temp_s_array[16] = "Chayka";
                        break;
                    case 6:
                        all_temp_s_array[16] = "Integrated navigation system";
                        break;
                    case 7:
                        all_temp_s_array[16] = "Surveyed";
                        break;
                    case 8:
                        all_temp_s_array[16] = "Galileo";
                        break;
                }

                // 18. RAIM flag    b
                temp_i = decode_u(305, 1);
                switch (temp_i)
                {
                    case 0:
                        all_temp_s_array[17] = "RAIM not in use(default)";
                        break;
                    case 1:
                        all_temp_s_array[17] = "RAIM in use";
                        break;
                }

                // 19. DTE  b
                temp_i = decode_u(306, 1);
                switch (temp_i)
                {
                    case 0:
                        all_temp_s_array[18] = "Data terminal ready";
                        break;
                    case 1:
                        all_temp_s_array[18] = "Not ready (default)";
                        break;
                }

                // 20. Assigned mode flag   u
                all_temp_s_array[19] = decode_u(307,1).ToString();



                for (int a = 0; a < display.Length; a++)
                {
                    if (display[a])
                    {
                        temp_s += all_temp_s_array[a] + ",";
                    }
                }
                temp_s = temp_s.TrimEnd(',');

                return type + "," + temp_s;
            }

            private string decode_24()
            {
                string[] all_temp_s_array = new string[13];
                bool[] display = new bool[13];
                display = new bool[] {false,true,false,true,false,
                                        false,false,true,false,false,
                                        false,false,false};

                Int32 temp_i = 0;
                string temp_s = "";

                // 1. repeat    u
                all_temp_s_array[0] = decode_u(6, 2).ToString();

                // 2. mmsi      u
                all_temp_s_array[1] = decode_u(8, 30).ToString().PadLeft(9,'0');

                // 3. part      u
                temp_i = decode_u(38, 2);

                switch (temp_i)
                {
                    // Type A
                    case 0:

                        // 3. part      u
                        all_temp_s_array[2] = "Type A";

                        // 4. ship name     t
                        all_temp_s_array[3] = decode_name(40,20);
                        break;

                    // Type B
                    case 1:
                        // 3. part      u
                        all_temp_s_array[2] = "Type B";

                        // 4. shiptype     e
                        temp_i = decode_u(40, 8);
                        if (temp_i == 0)
                        {
                            all_temp_s_array[3] = "Not available (default)";
                        }
                        if (temp_i >= 1 && temp_i <= 19)
                        {
                            all_temp_s_array[3] = "Reserved for future use";
                        }
                        if (temp_i == 20)
                        {
                            all_temp_s_array[3] = "Wing in ground (WIG),all ships of this type";
                        }
                        if (temp_i == 21)
                        {
                            all_temp_s_array[3] = "Wing in ground (WIG),Hazardous category A";
                        }
                        if (temp_i == 22)
                        {
                            all_temp_s_array[3] = "Wing in ground (WIG),Hazardous category B";
                        }
                        if (temp_i == 23)
                        {
                            all_temp_s_array[3] = "Wing in ground (WIG),Hazardous category C";
                        }
                        if (temp_i == 24)
                        {
                            all_temp_s_array[3] = "Wing in ground (WIG),Hazardous category D";
                        }
                        if (temp_i >= 25 && temp_i <= 29)
                        {
                            all_temp_s_array[3] = "Wing in ground (WIG), Reserved for future use";
                        }
                        if (temp_i == 30)
                        {
                            all_temp_s_array[3] = "Fishing";
                        }
                        if (temp_i == 31)
                        {
                            all_temp_s_array[3] = "Towing";
                        }
                        if (temp_i == 32)
                        {
                            all_temp_s_array[3] = "Towing: length exceeds 200m or breadth exceeds 25m";
                        }
                        if (temp_i == 33)
                        {
                            all_temp_s_array[3] = "Dredging or underwater ops";
                        }
                        if (temp_i == 34)
                        {
                            all_temp_s_array[3] = "Diving ops";
                        }
                        if (temp_i == 35)
                        {
                            all_temp_s_array[3] = "Military ops";
                        }
                        if (temp_i == 36)
                        {
                            all_temp_s_array[3] = "Sailing";
                        }
                        if (temp_i == 37)
                        {
                            all_temp_s_array[3] = "Pleasure Craft";
                        }
                        if (temp_i == 38 || temp_i == 39)
                        {
                            all_temp_s_array[3] = "Reserved";
                        }
                        if (temp_i == 40)
                        {
                            all_temp_s_array[3] = "High speed craft (HSC), all ships of this type";
                        }
                        if (temp_i == 41)
                        {
                            all_temp_s_array[3] = "High speed craft (HSC), Hazardous category A";
                        }
                        if (temp_i == 42)
                        {
                            all_temp_s_array[3] = "High speed craft (HSC), Hazardous category B";
                        }
                        if (temp_i == 43)
                        {
                            all_temp_s_array[3] = "High speed craft (HSC), Hazardous category C";
                        }
                        if (temp_i == 44)
                        {
                            all_temp_s_array[3] = "High speed craft (HSC), Hazardous category D";
                        }
                        if (temp_i >= 45 && temp_i <= 48)
                        {
                            all_temp_s_array[3] = "High speed craft (HSC), Reserved for future use";
                        }
                        if (temp_i == 49)
                        {
                            all_temp_s_array[3] = "High speed craft (HSC), No additional information";
                        }
                        if (temp_i == 50)
                        {
                            all_temp_s_array[3] = "Pilot Vessel";
                        }
                        if (temp_i == 51)
                        {
                            all_temp_s_array[3] = "Search and Rescue vessel";
                        }
                        if (temp_i == 52)
                        {
                            all_temp_s_array[3] = "Tug";
                        }
                        if (temp_i == 53)
                        {
                            all_temp_s_array[3] = "Port Tender";
                        }
                        if (temp_i == 54)
                        {
                            all_temp_s_array[3] = "Anti-pollution equipment";
                        }
                        if (temp_i == 55)
                        {
                            all_temp_s_array[3] = "Law Enforcement";
                        }
                        if (temp_i == 56 || temp_i == 57)
                        {
                            all_temp_s_array[3] = "Spare - Local Vessel";
                        }
                        if (temp_i == 58)
                        {
                            all_temp_s_array[3] = "Medical Transport";
                        }
                        if (temp_i == 59)
                        {
                            all_temp_s_array[3] = "Noncombatant Ship according to RR Resolution No.18";
                        }
                        if (temp_i == 60)
                        {
                            all_temp_s_array[3] = "Passenger,all ships of this type";
                        }
                        if (temp_i == 61)
                        {
                            all_temp_s_array[3] = "Passenger, Hazardous category A";
                        }
                        if (temp_i == 62)
                        {
                            all_temp_s_array[3] = "Passenger, Hazardous category B";
                        }
                        if (temp_i == 63)
                        {
                            all_temp_s_array[3] = "Passenger, Hazardous category C";
                        }
                        if (temp_i == 64)
                        {
                            all_temp_s_array[3] = "Passenger, Hazardous category D";
                        }
                        if (temp_i >= 65 && temp_i <= 68)
                        {
                            all_temp_s_array[3] = "Passenger, Reserved for future use";
                        }
                        if (temp_i == 69)
                        {
                            all_temp_s_array[3] = "Passenger, No additional information";
                        }
                        if (temp_i == 70)
                        {
                            all_temp_s_array[3] = "Cargo, all ships of this type";
                        }
                        if (temp_i == 71)
                        {
                            all_temp_s_array[3] = "Cargo, Hazardous category A";
                        }
                        if (temp_i == 72)
                        {
                            all_temp_s_array[3] = "Cargo, Hazardous category B";
                        }
                        if (temp_i == 73)
                        {
                            all_temp_s_array[3] = "Cargo, Hazardous category C";
                        }
                        if (temp_i == 74)
                        {
                            all_temp_s_array[3] = "Cargo, Hazardous category D";
                        }
                        if (temp_i >= 75 && temp_i <= 78)
                        {
                            all_temp_s_array[3] = "Cargo, Reserved for future use";
                        }
                        if (temp_i == 79)
                        {
                            all_temp_s_array[3] = "Cargo, No additional information";
                        }
                        if (temp_i == 80)
                        {
                            all_temp_s_array[3] = "Tanker, all ships of this type";
                        }
                        if (temp_i == 81)
                        {
                            all_temp_s_array[3] = "Tanker, Hazardous category A";
                        }
                        if (temp_i == 82)
                        {
                            all_temp_s_array[3] = "Tanker, Hazardous category B";
                        }
                        if (temp_i == 83)
                        {
                            all_temp_s_array[3] = "Tanker, Hazardous category C";
                        }
                        if (temp_i == 84)
                        {
                            all_temp_s_array[3] = "Tanker, Hazardous category D";
                        }
                        if (temp_i >= 85 && temp_i <= 88)
                        {
                            all_temp_s_array[3] = "Tanker, Reserved for future use";
                        }
                        if (temp_i == 89)
                        {
                            all_temp_s_array[3] = "Tanker, No additional information";
                        }
                        if (temp_i == 90)
                        {
                            all_temp_s_array[3] = "Other Type, all ships of this type";
                        }
                        if (temp_i == 91)
                        {
                            all_temp_s_array[3] = "Other Type, Hazardous category A";
                        }
                        if (temp_i == 92)
                        {
                            all_temp_s_array[3] = "Other Type, Hazardous category B";
                        }
                        if (temp_i == 93)
                        {
                            all_temp_s_array[3] = "Other Type, Hazardous category C";
                        }
                        if (temp_i == 94)
                        {
                            all_temp_s_array[3] = "Other Type, Hazardous category D";
                        }
                        if (temp_i >= 95 && temp_i <= 98)
                        {
                            all_temp_s_array[3] = "Other Type, Reserved for future use";
                        }
                        if (temp_i == 99)
                        {
                            all_temp_s_array[3] = "Other Type, No additional information";
                        }
                        // 5. Vendor ID     t
                        all_temp_s_array[4] = decode_t(48);
                        // 6. model         u
                        all_temp_s_array[5] = decode_u(66, 4).ToString();
                        // 7. serial        u
                        all_temp_s_array[6] = decode_u(70, 20).ToString();
                        // 8. callsign      t
                        all_temp_s_array[7] = decode_name(90,7);
                        // 9. to bow        u
                        all_temp_s_array[8] = decode_u(132, 9).ToString();
                        // 10.to stern      u
                        all_temp_s_array[9] = decode_u(141, 9).ToString();
                        // 11.to port       u
                        all_temp_s_array[10] = decode_u(150, 6).ToString();
                        // 12.to starboard  u
                        all_temp_s_array[11] = decode_u(156,6).ToString();
                        // 13.mother mmsi   u
                        all_temp_s_array[12] = decode_u(132, 30).ToString().PadLeft(9, '0');
                        break;
                }


                for (int a = 0; a < display.Length; a++)
                {
                    if (display[a])
                    {
                        temp_s += all_temp_s_array[a] + ",";
                    }
                }
                temp_s = temp_s.TrimEnd(',');

                return type + "," + temp_s;
            }

        #endregion


        #endregion

        // 匯出
        public void output()
        {
            string write_data = "";
            StreamWriter sw;

            /*************改善***************/           
            for (int i = 0; i < l_data.Count; i++)
            {
                output_path = output_paths[i];
                sw = new StreamWriter(output_path.Remove(output_path.Length - 4, 4) + " - decode.csv");
                for (int j = 0; j<l_data[i].Count;j++ )
                {
                    if (l_data[i][j].datas != "")
                    {
                        write_data += l_data[i][j].times.ToString() + "," + l_data[i][j].datas.ToString() + "\r\n";
                    }

                    if (j % 10000 == 0)
                    {
                        sw.WriteLine(write_data);
                        write_data = "";
                    }
                }
                
                sw.WriteLine(write_data);
                sw.Close();
            }
               
        }

        // 顯示錯誤代碼
        public void Show_error_code(ref text_form s_t_f)
        {
            if (error_code != "")
            {
                s_t_f.r_t_b.Text = error_code;
                s_t_f.Show();

                StreamWriter sw = new StreamWriter(output_path.Remove(output_path.Length - 4, 4) + " - error-code.txt");
                sw.Write(error_code);
                sw.Close();
            }
        }

        #endregion
    }

    public class many_data
    {
        public string times { get; set; }
        public string datas { get; set; }
    }
}

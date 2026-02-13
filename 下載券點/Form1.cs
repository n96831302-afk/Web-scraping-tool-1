using HtmlAgilityPack;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Windows.Forms;

namespace 下載券點;

public partial class Form1: Form {
    // 全域共用 HttpClient，設定在 Handler 裡
    static readonly HttpClient _http = new HttpClient(
        new SocketsHttpHandler {
            Proxy = null,                    // 不用系統 Proxy
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,  // 自動解壓
            SslOptions = new SslClientAuthenticationOptions { EnabledSslProtocols = SslProtocols.Tls12 }, // 等同原本的 SecurityProtocolType.Tls12
            MaxConnectionsPerServer = 200    // 等同原本的 DefaultConnectionLimit = 200
        });

    public Form1() {
        InitializeComponent();
        txtYMD.Text = DateTime.Now.ToString("yyyyMMdd");
        btn下載.Click += (_, _) => _ = Task.Run(_下載);   // 背景跑，不卡 UI
    }

    void _下載() {
        btn下載.BeginInvoke(() => { btn下載.Enabled = false; });

        string sU = "https://fubon-ebrokerdj.fbs.com.tw";
        string s提供者 = "/z/zg/zgb/zgb0.djhtm?";
        string s券商_主, s券商_次, s券商2, s日期2, sUrl;
        string sTtl金 = "代號\tB金\tS金\t合計";
        string sTtl張 = "代號\tBV\tSV\t合計";
        string sYMD = txtYMD.Text.Trim();
        if (sYMD.Length != 8 || !sYMD.All(char.IsDigit)) throw new Exception("日期格式錯誤，請輸入 yyyyMMdd");

        var sb = new StringBuilder();
        var dic券商 = c券點.Get_dic券商(); // 你原本的函式
        Dictionary<string, string> dic分點;
        // https://fubon-ebrokerdj.fbs.com.tw /z/zg/zgb/zgb0.djhtm? a=1030 &b=1030 &c=B &e=2020-12-1 &f=2020-12-1 ' 2020 - 12 - 1日的 金額
        // https://fubon-ebrokerdj.fbs.com.tw /z/zg/zgb/zgb0.djhtm? a=1030 &b=1030 &c=E &e=2020-12-1 &f=2020-12-1 ' 2020 - 12 - 1日的 V

        foreach (var kvp主 in dic券商) {
            s券商_主 = kvp主.Key;
            dic分點 = c券點.Get_dic分點(s券商_主);    // 你原本的函式

            foreach (var kvp次 in dic分點) {
                s券商_次 = kvp次.Key;

                // 注意：這裡改成真正的 &，不能用 &amp;，因為是給 HttpClient 用的 URL
                s券商2 = $"a={s券商_主}&b={s券商_次}";
                s日期2 = $"&e={sYMD}&f={sYMD}";

                for (int i金張 = 1 ; i金張 <= 2 ; i金張++) {
                    bool is金額 = (i金張 == 1);

                    if (is金額) {
                        sb.AppendLine($"券商分點(金額)\t券商={s券商_主}\t分點={s券商_次}");
                        sb.AppendLine(sTtl金);
                        sUrl = sU + s提供者 + s券商2 + "&c=B" + s日期2;
                        btn下載.BeginInvoke(() => { btn下載.Text = $"金額，{s券商_主}"; });
                    }
                    else {
                        sb.AppendLine($"券商分點(張數)\t券商={s券商_主}\t分點={s券商_次}");
                        sb.AppendLine(sTtl張);
                        sUrl = sU + s提供者 + s券商2 + "&c=E" + s日期2;
                        btn下載.BeginInvoke(() => { btn下載.Text = $"張數，{s券商_主}"; });
                    }

                    GET資料_沒tbody(sUrl, sb);
                    sb.AppendLine("------------------------------------");
                }
            }
        }

        const string sP0 = @"D:\谷谷碟\XQ內外盤\券商分點\";
        File.WriteAllText(sP0 + sYMD + ".txt", sb.ToString());
        btn下載.BeginInvoke(() => { btn下載.BackColor = Color.Magenta; btn下載.Enabled = true; });
    }

    void GET資料_沒tbody(string sU, StringBuilder sb) {
        // 不用 async/await，為了保持程式扁平，直接 .Result（在背景 thread 不會卡 UI）
        var b = _http.GetByteArrayAsync(sU).Result; // 用 HttpClient
        var html = Encoding.Default.GetString(b);
        if (false) {
            Debug.WriteLine(html[..Math.Min(html.Length, 1000)]); // 印出前1000行。結果：沒有<tbody>
            File.WriteAllText(@$"D:\1.html", html, Encoding.GetEncoding("big5"));
        }

        // 用 HtmlAgilityPack 解析
        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(html);

        // 抓出所有 table（因為現在沒有 tbody）
        var tbC = doc.DocumentNode.SelectNodes("//table");
        if (tbC == null || tbC.Count < 2) return;

        // 原本你用 tbody[3], tbody[4]
        // 現在改成：倒數兩個 table
        var tbL = tbC[tbC.Count - 2]; var tbR = tbC[tbC.Count - 1];

        // 左 / 右表
        for (int iTB = 1 ; iTB <= 2 ; iTB++) {
            var tb = (iTB == 1) ? tbL : tbR;
            var trs = tb.SelectNodes(".//tr"); if (trs == null) continue;

            foreach (var tr in trs) {
                var tds = tr.SelectNodes("td");
                if (tds == null || tds.Count < 4) continue;

                // 股票代號在第 0 欄（innerHtml 裡）
                string s = tds[0].InnerHtml, sOut = "";

                if (s.Contains("GenLink2stk")) {
                    int p1 = s.IndexOf('\''), p2 = s.IndexOf('\'', p1 + 1);
                    if (p1 < 0 || p2 <= p1 + 1) continue;
                    sOut = s[(p1 + 1)..p2].Replace("AS", "") + "\t";
                }
                else if (s.Contains(":Link2Stk")) {
                    int p1 = s.IndexOf('\''), p2 = s.IndexOf('\'', p1 + 1);
                    if (p1 < 0 || p2 <= p1 + 1) continue;
                    sOut = s[(p1 + 1)..p2] + "\t";
                }
                else continue;

                // 1,2,3 欄：買 / 賣 / 合計
                sOut += tds[1].InnerText.Trim() + "\t" +
                        tds[2].InnerText.Trim() + "\t" +
                        tds[3].InnerText.Trim();
                sb.AppendLine(sOut);
            }
        }
    }

    void GET資料_有tbody(string sU, StringBuilder sb) {
        // 不用 async/await，為了保持程式扁平，直接 .Result（在背景 thread 不會卡 UI）
        var b = _http.GetByteArrayAsync(sU).Result; // 用 HttpClient
        var html = Encoding.Default.GetString(b);
        //Debug.WriteLine(html[..Math.Min(html.Length, 1000)]); // 印出前1000行。結果：沒有<tbody>

        // 用 HtmlAgilityPack 解析
        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(html);

        // 找出所有 <tbody>，沿用你原本「第3、4個」的邏輯
        var tbC = doc.DocumentNode.SelectNodes("//tbody"); //實際：找不到任何 tbody，因為網站沒有用 tbody 包起來
        if (tbC == null || tbC.Count <= 4) return;  // -->問題：tbC 在此為 null，
        var tbL = tbC[3]; var tbR = tbC[4];

        for (int iTB = 1 ; iTB <= 2 ; iTB++) {
            var tb = (iTB == 1) ? tbL : tbR;
            var trs = tb.SelectNodes("tr");

            foreach (var tr in trs) {
                string s = tr.InnerText;
                if (s.Contains("券商") || s.Contains("買超") || s.Contains("賣超")) continue;

                var tds = tr.SelectNodes("td");
                string sOut = "";

                s = tds[0].InnerHtml; // 第0欄：InnerHtml 裡的股票代號，沿用你原本 InStr/Mid 邏輯

                if (s.Contains("GenLink2stk")) {
                    int p1 = s.IndexOf('\''); if (p1 < 0) continue;
                    int p2 = s.IndexOf('\'', p1 + 1); if (p2 <= p1 + 1) continue;

                    s = s[(p1 + 1)..p2].Replace("AS", "");
                    sOut = s + "\t";
                }
                else if (s.Contains(":Link2Stk")) {
                    int p1 = s.IndexOf('\''); if (p1 < 0) continue;
                    int p2 = s.IndexOf('\'', p1 + 1); if (p2 <= p1 + 1) continue;

                    s = s[(p1 + 1)..p2];
                    sOut = s + "\t";
                }
                else continue; // 沒代號就略過

                // 第1,2,3欄：B/S/合計，Trim + \t

                sOut += tds[1].InnerText.Trim() + "\t" +
                        tds[2].InnerText.Trim() + "\t" +
                        tds[3].InnerText.Trim();
                sb.AppendLine(sOut.Trim()); // 去掉最後一個 \t
            }
        }
    }


    private void btnTest_Click(object sender, EventArgs e) {
        //結果：DNS OK: + "HttpClient 失敗:
        try { // 測試 DNS
            var he = Dns.GetHostEntry("fubon-ebrokerdj.fbs.com.tw");
            MessageBox.Show("DNS OK: " + he.HostName);
        }
        catch (Exception ex) { MessageBox.Show("DNS 失敗: " + ex.Message); }

        try { // 測試 HttpClient
            using var h = new HttpClient();
            string sU = "https://fubon-ebrokerdj.fbs.com.tw/z/zg/zgb/zgb0.djhtm?a=1020&b=0031003000320047&c=B&e=20260206&f=20260206";
            var s = h.GetStringAsync(sU).Result;
            MessageBox.Show("HttpClient OK, 長度=" + s.Length);
        }
        catch (Exception ex) { MessageBox.Show("HttpClient 失敗: " + ex.ToString()); }
    }
}
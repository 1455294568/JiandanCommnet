using NSoup.Nodes;
using NSoup.Parse;
using NSoup.Select;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;

namespace jiandan2
{
    class Program
    {
        static void Main(string[] args)
        {
            if (File.Exists("comments.txt"))
            {
                File.Move("comments.txt", DateTime.Now.ToString("yyyyMMddHHmmss").ToString() + ".txt");
            }
            Program p = new Program();
            p.crawlMain(url);
            Console.Read();
        }

        static string url = "http://jandan.net/";
        WebClient webClient;

        private void Init()
        {
            if (webClient == null)
            {
                webClient = new WebClient();
                webClient.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                webClient.Headers.Add("Accept-Encoding", "gzip, deflate");
                webClient.Headers.Add("Accept-Language", "en-US,en;q=0.9,zh-CN;q=0.8,zh;q=0.7");
                webClient.Headers.Add("Host", "jandan.net");
                webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
            }

        }

        /// <summary>
        /// Get Post by url
        /// </summary>
        /// <param name="_url">home url</param>
        private void crawlMain(string _url) // 63
        {
            Init();
            byte[] bytes = webClient.DownloadData(_url);
            string html = "";
            MemoryStream ms = new MemoryStream(bytes);
            using (GZipStream gz = new GZipStream(ms, CompressionMode.Decompress))
            using (StreamReader sr = new StreamReader(gz))
            {
                html = sr.ReadToEnd();
            }

            Document doc = Parser.Parse(html, url);
            var a = doc.Select("div.list-post > div.indexs > h2 > a");
            foreach(var i in a)
            {
                Console.WriteLine(i.Text());
                using (FileStream fs = new FileStream("comments.txt", FileMode.Append))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine("#" + i.Text());
                    }
                }
                GetCom(i.Attr("href"));
                Thread.Sleep(1600);
            }

            //check if it has older post
            var nextbtn = doc.Select("div.wp-pagenavi > a");
            if(nextbtn.Count > 1)
            {
                crawlMain(url + nextbtn[1].Attr("href"));
            }
            else if(nextbtn.Count > 0)
            {
                crawlMain(url + nextbtn[0].Attr("href"));
            }
        }

        /// <summary>
        /// Get comments by post url
        /// </summary>
        /// <param name="_url">post url</param>
        private void GetCom(string _url)
        {
            Init();
            byte[] bytes = webClient.DownloadData(_url);
            string html = "";
            MemoryStream ms = new MemoryStream(bytes);
            using (GZipStream gz = new GZipStream(ms, CompressionMode.Decompress))
            using (StreamReader sr = new StreamReader(gz))
            {
                html = sr.ReadToEnd();
            }
            Document doc = Parser.Parse(html, url);
            var comments = doc.Select("ol.commentlist > li > div > div.row > div.text > p");
            foreach(var i in comments)
            {
                Console.WriteLine("###" + i.Text());
                using (FileStream fs = new FileStream("comments.txt", FileMode.Append))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine("0\t  " + i.Text());
                    }
                }
            }

            // Check if it has older comments
            var nextbtn = doc.Select("div.cp-pagenavi > a[title=Older Comments]");
            if(nextbtn.Count > 0)
            {
                string nxturl = nextbtn[0].Attr("href");
                nxturl = "http:" + nxturl;
                GetCom(nxturl);
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace xxzhsFetcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string> titleIDList = new Dictionary<string, string>(); //<title>, <videoID>
            Dictionary<string, KeyValuePair<string, List<string>>> videoList = new Dictionary<string, KeyValuePair<string, List<string>>>(); //<title>, <videoID>, list of URL
            string targetDir = @"F:\Temp\xxzhs";
            string achieveFilename = Path.Combine(targetDir, "archieve.txt");
            List<string> achieveList = new List<string>(); //title

            string videoIndexURL = @"http://tv.cntv.cn/index.php?action=video-getVideoList&page={0}&infoId=C16717000001&type=CN04&flag=cu&videoId=89e935d6e135407f8b3e446a5a60db78&istiyu=0";
            string videoIndexURL2 = @"http://vdn.apps.cntv.cn/api/getIpadVideoInfo.do?pid={0}&tai=ipad";

            if (File.Exists(achieveFilename))
            {
                using (StreamReader reader = File.OpenText(achieveFilename))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (!achieveList.Contains(line))
                            achieveList.Add(line);
                    }
                }
            }

            for (int loop = 127; loop <= 127; loop++)
            {
                WebClient client = new WebClient();
                client.Headers.Add("user-agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 6_0_1 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A523 Safari/8536.25");
                Stream data = client.OpenRead(string.Format(videoIndexURL, loop));
                StreamReader reader = new StreamReader(data);
                string responseLine = "";
                while (!reader.EndOfStream)
                {
                    responseLine = reader.ReadLine();
                    if (string.IsNullOrEmpty(responseLine) || !responseLine.Contains("/video/C16717/"))
                        continue;
                    int index = responseLine.IndexOf("/video/C16717/");
                    string videoID = responseLine.Substring(index + 14, 32);
                    index = responseLine.IndexOf("title=\"");
                    int endIndex = responseLine.IndexOf("\">", index);
                    string title = responseLine.Substring(index + 7, endIndex - index - 7);
                    if (!achieveList.Contains(title) && !titleIDList.ContainsKey(title))
                        titleIDList.Add(title, videoID);
                }
                data.Close();
                reader.Close();
            }

            foreach (KeyValuePair<string, string> kvp in titleIDList)
            {
                Console.WriteLine("{0} === {1}", kvp.Key, kvp.Value);

                WebClient client = new WebClient();
                client.Headers.Add("user-agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 6_0_1 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A523 Safari/8536.25");
                Stream data = client.OpenRead(string.Format(videoIndexURL2, kvp.Value));
                StreamReader reader = new StreamReader(data);
                string response = reader.ReadToEnd();
                data.Close();
                reader.Close();
                if(string.IsNullOrEmpty(response))
                {
                    Console.WriteLine("ERROR: null response for video {0}, videoID = {1}", kvp.Key, kvp.Value);
                    continue;
                }

                int currentIndex = 0;
                List<string> videoURLs = new List<string>();
                while (currentIndex < response.Length -1)
                {
                    string stringToProcess = response.Substring(currentIndex);
                    if (string.IsNullOrEmpty(stringToProcess) || !stringToProcess.Contains("\"url\":\""))
                        break;
                    int startIndex = stringToProcess.IndexOf("\"url\":\"");
                    int endIndex = stringToProcess.IndexOf("\"}", startIndex);
                    string videoURL = stringToProcess.Substring(startIndex + 7, endIndex - startIndex - 7);
                    if (videoURL.Contains("duration"))
                    {
                        videoURL = videoURL.Substring(0, videoURL.IndexOf(".mp4\"") + 4);
                    }
                    //Console.WriteLine("\t {0}", videoURL);
                    if (videoURL.Contains("aac32") && videoURL.Contains("h2648") && !videoURLs.Contains(videoURL))
                        videoURLs.Add(videoURL);

                    currentIndex += endIndex;
                }

                videoList.Add(kvp.Key, new KeyValuePair<string, List<string>>(kvp.Value, videoURLs));
            }

            int cid = 1;
            foreach (KeyValuePair<string, KeyValuePair<string, List<string>>> kvp in videoList)
            {
                string title = kvp.Key;
                title =title.Replace(':', '-');
                string dir = Path.Combine(targetDir, title);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                else
                {
                    Console.WriteLine("Dir {0} exists, skip.", title);
                    continue;
                }
                int vid=1;
                bool bError = false;
                foreach (string url in kvp.Value.Value)
                {
                    int index = url.LastIndexOf('/');
                    string filename = url.Substring(index+1);
                    WebClient client = new WebClient();
                    client.Headers.Add("user-agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 6_0_1 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A523 Safari/8536.25");
                    try
                    {
                        Console.Write("Downloading {0}", url);
                        client.DownloadFile(url, Path.Combine(dir, filename));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR: Download failed. URL={0}; Message={1}", url, ex.Message);
                        bError = true;
                        break;
                    }
                    Console.WriteLine(" ... Done. {0}/{1}, {2}/{3}", vid, kvp.Value.Value.Count, cid, videoList.Count);
                    ++vid;
                }

                ++cid;

                if (!bError)
                {
                    achieveList.Add(title);

                    StringBuilder sb = new StringBuilder();
                    foreach (string t in achieveList)
                    {
                        sb.AppendFormat("{0}{1}", t, Environment.NewLine);
                    }
                    File.WriteAllText(achieveFilename, sb.ToString(), Encoding.UTF8);
                }
            }
        }
    }
}

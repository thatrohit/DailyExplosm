using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DailyExplosm
{
    public class Helpers
    {
        public string html = "";
        string shortsHtml = "";
        
        public async Task GetSiteHTML(string comicUrl)
        {
            var client = new HttpClient();
            html = await client.GetStringAsync(new Uri(comicUrl));
            //return html;
        }

        public async Task GetShortsSiteHTML()
        {
            var client = new HttpClient();
            shortsHtml = await client.GetStringAsync(new Uri(ShortsPageUrl()));
            //return shortsHtml;
        }

        public bool IsAnimatedShort()
        {
            if (html.Contains(@"/comics/play-button.png"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string ShortsPageUrl()
        {
            try
            {
                return "http://explosm.net/show/episode" + html.Split(new string[] {"http://explosm.net/show/episode"}, StringSplitOptions.None)[1].Split('>')[0];
            }
            catch
            {
                return null;
            }
        }

        public string GetYoutubeUrl()
        {
            try
            {
                return shortsHtml.Split(new string[] { "www.youtube.com/embed/" }, StringSplitOptions.None)[1].Split('\"')[0];
            }
            catch
            {
                return null;
            }
        }

        public string GetComicUrl()
        {
            try
            {
                return html.Split(new string[] { "[IMG]" }, StringSplitOptions.None)[1].Split(new string[] { "[/IMG]" }, StringSplitOptions.None)[0];
            }
            catch
            {
                return null;
            }
        }

        public string AuthorName()
        {
            if (html.Contains("http://explosm.net/db/files/comic-authors/matt.png"))
            {
                return "Matt Melvin";
            }
            if (html.Contains("http://explosm.net/db/files/comic-authors/dave.png"))
            {
                return "Dave McElfatrick";
            }
            if (html.Contains("http://explosm.net/db/files/comic-authors/rob.png"))
            {
                return "Rob DenBleyker";
            }
            if (html.Contains("http://explosm.net/db/files/comic-authors/kris.png"))
            {
                return "Kris Wilson";
            }
            return "None";
        }

        public string ComicNumber()
        {
            try
            {
                return html.Split(new string[] { "URL=\"http://explosm.net/comics/" }, StringSplitOptions.None)[1].Split('/')[0];
            }
            catch
            {
                return "NA";
            }
        }

        public string PreviousComicUrl()
        {
            try
            {
                return "http://explosm.net/comics/" + html.Split(new string[] { "case 37:" }, StringSplitOptions.None)[1].Split(';')[0].Split('/')[2];
            }
            catch
            {
                return null;
            } 
        }

        public string NextComicUrl()
        {
            try
            {
                return "http://explosm.net/comics/" + html.Split(new string[] { "case 39:" }, StringSplitOptions.None)[1].Split(';')[0].Split('/')[2];
            }
            catch
            {
                return null;
            } 
        }

        public string GetComicName()
        {
            try
            {
                return GetComicUrl().Split('/').Last().Split('.')[0];
            }
            catch
            {
                return null;
            }
        }

        public string GetComicDate()
        {
            try
            {
                return html.Split(new string[] { @"</nobr>" }, StringSplitOptions.None)[0].Split('>').Last();
            }
            catch
            {
                return null;
            }
        }
    }

    public static class AuthorComicsPopulator
    {
        static readonly string daveUrl = "http://explosm.net/comics/author/Dave/";
        static readonly string mattUrl = "http://explosm.net/comics/author/Matt/";
        static readonly string krisUrl = "http://explosm.net/comics/author/Kris/";
        static readonly string robUrl = "http://explosm.net/comics/author/Rob/";

        public static async Task<List<string>> GetAuthorComicList(string author)
        {
            string url = "", html = "";
            List<string> authorComicList = new List<string>();
            switch(author)
            {
                case "dave":
                    url = daveUrl;
                    break;
                case "matt":
                    url = mattUrl;
                    break;
                case "kris":
                    url = krisUrl;
                    break;
                case "rob":
                    url = robUrl;
                    break;
                default:
                    break;
            }
            using (var client = new HttpClient())
            {
                html = await client.GetStringAsync(url);
                string[] first_split = html.Split(new string[] { @"href=""/comics/" }, StringSplitOptions.None);
                for (int i = 1; i < first_split.Length; i++)
                {
                    authorComicList.Add(first_split[i].Split('/')[0]);
                }
            }
            return authorComicList;
        }

    }
}

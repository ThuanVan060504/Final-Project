using System.Xml.Linq;
using Final_Project.Models;
using System.ServiceModel.Syndication;
using System.Xml;

namespace Final_Project.Services
{
    public class NewsService
    {
        // Lấy tin từ 1 URL RSS
        public static List<TinTuc> GetNewsFromRSS(string rssUrl)
        {
            List<TinTuc> tinList = new();
            try
            {
                using var reader = XmlReader.Create(rssUrl);
                var feed = SyndicationFeed.Load(reader);

                foreach (var item in feed.Items)
                {
                    var tomTat = item.Summary?.Text ?? "";
                    var link = item.Links.FirstOrDefault()?.Uri.ToString() ?? "#";

                    tinList.Add(new TinTuc
                    {
                        TieuDe = item.Title?.Text,
                        TomTat = tomTat,
                        Link = link,
                        NgayDang = item.PublishDate.DateTime,
                        Nguon = new Uri(rssUrl).Host
                    });
                }
            }
            catch (Exception ex)
            {
                // Ghi log nếu cần: Console.WriteLine(ex.Message);
            }

            return tinList;
        }

        // Gộp nhiều RSS lại
        public static List<TinTuc> GetNewsFromAll()
        {
            var rssFeeds = new[]
            {
                "https://vnexpress.net/rss/nha-dep.rss",
                "https://vnexpress.net/rss/kien-truc.rss",
                "https://cafef.vn/bat-dong-san.rss",
                "https://www.dothi.net/rss/tin-moi.rss",
                "https://kienviet.net/feed/",
                "https://tapchikientruc.com.vn/feed"
            };

            var allNews = new List<TinTuc>();
            foreach (var url in rssFeeds)
            {
                allNews.AddRange(GetNewsFromRSS(url));
            }

            return allNews
                .OrderByDescending(t => t.NgayDang)
                .Take(30) // lấy 30 tin mới nhất
                .ToList();
        }
    }
}

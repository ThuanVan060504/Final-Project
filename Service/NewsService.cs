using System.Xml.Linq;
using Final_Project.Models;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Text.RegularExpressions;

namespace Final_Project.Services
{
    public class NewsService
    {
        private static readonly string NewsImageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "news-images");
        private static readonly string PlaceholderFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "news-placeholder");

        // Tạo danh sách ảnh dự phòng
        private static readonly string[] PlaceholderImages = Directory.Exists(PlaceholderFolder)
            ? Directory.GetFiles(PlaceholderFolder).Select(f => "/news-placeholder/" + Path.GetFileName(f)).ToArray()
            : new string[] { "/images/no-image.jpg" };

        private static string GetRandomPlaceholderImage()
        {
            if (PlaceholderImages.Length == 0) return "/images/no-image.jpg";
            var rand = new Random();
            int index = rand.Next(PlaceholderImages.Length);
            return PlaceholderImages[index];
        }

        // Lấy tin từ 1 URL RSS
        public static List<TinTuc> GetNewsFromRSS(string rssUrl)
        {
            // Tạo folder lưu ảnh nếu chưa có
            if (!Directory.Exists(NewsImageFolder))
                Directory.CreateDirectory(NewsImageFolder);

            List<TinTuc> tinList = new();
            try
            {
                using var reader = XmlReader.Create(rssUrl);
                var feed = SyndicationFeed.Load(reader);

                foreach (var item in feed.Items)
                {
                    var tomTat = item.Summary?.Text ?? "";
                    var link = item.Links.FirstOrDefault()?.Uri.ToString() ?? "#";

                    // --- Lấy ảnh ---
                    string imageUrl = null;

                    // 1. Tìm ảnh trong Summary
                    var match = Regex.Match(tomTat, "<img.+?src=[\"'](.+?)[\"']", RegexOptions.IgnoreCase);
                    if (match.Success) imageUrl = match.Groups[1].Value;

                    // 2. Nếu không có, tìm trong Enclosure
                    if (string.IsNullOrEmpty(imageUrl) && item.Links != null)
                    {
                        var enclosure = item.Links.FirstOrDefault(l => l.RelationshipType == "enclosure");
                        if (enclosure != null)
                            imageUrl = enclosure.Uri.ToString();
                    }

                    // 3. Nếu vẫn chưa có, tìm trong Media Content
                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        var media = item.ElementExtensions
                            .FirstOrDefault(e => e.OuterName == "content" && e.OuterNamespace == "http://search.yahoo.com/mrss/");
                        if (media != null)
                        {
                            var el = media.GetObject<XElement>();
                            imageUrl = el.Attribute("url")?.Value;
                        }
                    }

                    // 4. Tải ảnh về server nếu có
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        try
                        {
                            using var client = new System.Net.Http.HttpClient();
                            var bytes = client.GetByteArrayAsync(imageUrl).Result;

                            var ext = Path.GetExtension(imageUrl);
                            if (string.IsNullOrEmpty(ext)) ext = ".jpg"; // default extension
                            var fileName = Guid.NewGuid() + ext;
                            var savePath = Path.Combine(NewsImageFolder, fileName);
                            File.WriteAllBytes(savePath, bytes);

                            imageUrl = "/news-images/" + fileName; // đường dẫn nội bộ
                        }
                        catch
                        {
                            // Nếu lỗi tải ảnh → chọn ảnh dự phòng
                            imageUrl = GetRandomPlaceholderImage();
                        }
                    }
                    else
                    {
                        // Nếu không có ảnh từ RSS → chọn ảnh dự phòng
                        imageUrl = GetRandomPlaceholderImage();
                    }

                    tinList.Add(new TinTuc
                    {
                        TieuDe = item.Title?.Text,
                        TomTat = tomTat,
                        Link = link,
                        NgayDang = item.PublishDate.DateTime,
                        Nguon = new Uri(rssUrl).Host,
                        ImageUrl = imageUrl
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing RSS feed: {ex.Message}");
            }

            return tinList;
        }

        // Gộp nhiều RSS
        public static List<TinTuc> GetNewsFromAll()
        {
            var rssFeeds = new[]
            {
                "https://flexhouse.vn/blog-ve-noi-that/feed",
                "https://ekeinterior.com/feed",
                "https://noithatart.com/feed",
                "https://amore-architecture.vn/blog/feed",
                "https://haydecor.com/feed",
                "https://noithatvsc.vn/feed"
            };

            var allNews = new List<TinTuc>();
            foreach (var url in rssFeeds)
            {
                allNews.AddRange(GetNewsFromRSS(url));
            }

            return allNews
                .OrderByDescending(t => t.NgayDang)
                .Take(20)
                .ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Ukad_task.Models;
using System.Diagnostics;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Net;
using HtmlAgilityPack;

namespace Ukad_task.Services
{
    class SiteMapReader
    {
        private List<string> urlsToCheck;
        private List<Report> reports;
        private Uri mainURL;
        private bool includeInnerSitemaps;
        private Stopwatch timer;
        private int timeout;

        public SiteMapReader(string url, bool includeInnerSitemaps, int timeout)
        {
            urlsToCheck = new List<string>();//Urls that must be tested
            reports = new List<Report>();
            this.includeInnerSitemaps = includeInnerSitemaps;//Some sites can have sitemap in main sitemap
            this.timeout = timeout;
            timer = new Stopwatch();
            UriBuilder builder = new UriBuilder(url);
            mainURL = new Uri(string.Format("{0}://{1}", builder.Scheme, builder.Host));//Build correct url
            RetriveUrlsFromXMLSiteMap();//Try retrive urls from sitemap.xml
            RetriveUrlsFromHTMLSitemap();//Try retrive urls from main page
            urlsToCheck = urlsToCheck.Distinct().ToList();
        }

        private void RetriveUrlsFromXMLSiteMap(string _url = null)
        {
            Report report = new Report();
            bool urlChanged = false;
            string expectedSitemapURL = null;
            if (string.IsNullOrEmpty(_url))
            {
                expectedSitemapURL = mainURL + "sitemap.xml";
                urlChanged = true;
            }
            else
            {
                if (_url.EndsWith("sitemap.xml"))
                {
                    expectedSitemapURL = _url;
                }
                else
                {
                    return;
                }
            }
            try
            {
                timer.Reset();
                timer.Start();
                XDocument xDoc = XDocument.Load(expectedSitemapURL);//Load sitemap
                timer.Stop();

                report.Url = expectedSitemapURL;
                report.SpendTime = timer.ElapsedMilliseconds;
                report.Message = "OK";
                reports.Add(report);


                string namespaceName = xDoc.Root.Name.NamespaceName;


                XElement urlset = xDoc.Element(XName.Get("urlset", namespaceName));//Get urlset
                if (urlset != null)
                {
                    urlsToCheck = urlset.Elements(XName.Get("url", namespaceName))
                        .Select(url => url.Element(XName.Get("loc", namespaceName)).Value).ToList();//Get all urls
                }
                else
                {
                    XElement sitemapindex = xDoc.Element(XName.Get("sitemapindex", namespaceName));//Try get inner sitemap
                    if (sitemapindex != null)
                    {
                        List<string> sitemaps = sitemapindex.Elements(XName.Get("sitemap", namespaceName))
                            .Select(sitemap => sitemap.Element((XName.Get("loc", namespaceName))).Value).ToList();
                        if (includeInnerSitemaps)
                        {
                            List<Task> tasks = new List<Task>();
                            foreach (var sitemap in sitemaps)
                            {
                                string currentSitemap = sitemap;
                                var t = Task.Run(() =>
                                {
                                    RetriveUrlsFromXMLSiteMap(currentSitemap);
                                });
                                tasks.Add(t);
                            }
                            Task.WaitAll(tasks.ToArray());
                        }
                        else
                        {
                            urlsToCheck.AddRange(sitemaps);
                        }
                    }
                }
            }
            catch (WebException e)
            {
                if (!urlChanged)
                {
                    report.Message = e.Response != null ? (e.Response as HttpWebResponse).StatusDescription : e.Message;
                    report.SpendTime = 0;
                    report.Url = expectedSitemapURL;
                }
            }
        }

        private void RetriveUrlsFromHTMLSitemap()
        {
            Report report = new Report();
            try
            {
                HtmlWeb hw = new HtmlWeb();
                timer.Reset();
                timer.Start();
                HtmlDocument doc = hw.Load(mainURL);//Get html page of main page
                timer.Stop();
                report.Message = "OK";
                report.SpendTime = timer.ElapsedMilliseconds;
                report.Url = mainURL.AbsoluteUri;
                reports.Add(report);

                List<HtmlNode> nodesLinks = doc.DocumentNode.SelectNodes("//a[@href]")?.ToList();

                if (nodesLinks == null)
                {
                    return;
                }
                foreach (HtmlNode nodeLink in nodesLinks)
                {
                    string link = nodeLink.Attributes["href"].Value;

                    if (!Uri.IsWellFormedUriString(link, UriKind.Absolute))
                    {
                        UriBuilder builder = new UriBuilder();
                        builder.Scheme = mainURL.Scheme;
                        builder.Host = mainURL.Host;
                        builder.Path = link;
                        link = builder.Uri.AbsoluteUri;
                    }
                    if (Uri.IsWellFormedUriString(link, UriKind.Absolute))
                    {
                        urlsToCheck.Add(link);
                    }
                }
            }
            catch(WebException e)
            {
                report.Message = e.Response != null ? (e.Response as HttpWebResponse).StatusDescription : e.Message;
                report.SpendTime = 0;
                report.Url = mainURL.AbsoluteUri;
                reports.Add(report);
            }
        }

        public List<Report> MeasureResponseTime()
        {
            List<Task> tasks = new List<Task>();
            foreach (var url in urlsToCheck)
            {
                var currentUrl = url;
                Task t = Task.Run(() =>
                {
                    var report = new Report();
                    try
                    {
                        if (!Uri.IsWellFormedUriString(currentUrl, UriKind.Absolute))
                        {
                            return;
                        }
                        var timer = new Stopwatch();
                        timer.Start();
                        var request = (HttpWebRequest)WebRequest.Create(currentUrl);
                        request.Timeout = timeout;
                        WebResponse responce = request.GetResponse();
                        timer.Stop();
                        report.Message = "OK";
                        report.SpendTime = timer.ElapsedMilliseconds;
                        report.Url = currentUrl;
                        reports.Add(report);
                    }
                    catch (WebException e)
                    {
                        report.Message = e.Response != null ? (e.Response as HttpWebResponse).StatusDescription : e.Message;
                        report.SpendTime = 0;
                        report.Url = currentUrl;
                        reports.Add(report);
                    }
                    catch (NotSupportedException) { }//Same link can be mail link
                });
                tasks.Add(t);
            }

            Task.WaitAll(tasks.ToArray());
            return reports;
        }
    }
}
using System.Threading;
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;

namespace MangaWebScrape.Websites
{
    class RobertsAnimeCornerStore
    {
        public static List<string> links = new List<String>();
        private static List<string[]> dataList = new List<string[]>();
        private static string getUrl(string bookTitle, bool pageExists){
            Dictionary<string, Regex> urlMapDict = new Dictionary<string, Regex>()
            {
                {"mangrapnovag", new Regex(@"^[a-bA-B\d]")},
                {"mangrapnovhp", new Regex(@"^[c-dC-D]")},
                {"mangrapnovqz", new Regex(@"^[e-gE-G]")},
                {"magrnomo", new Regex(@"^[h-kH-K]")},
                {"magrnops", new Regex(@"^[l-nL-N]")},
                {"magrnotz", new Regex(@"^[o-qO-Q]")},
                {"magrnors", new Regex(@"^[r-sR-S]")},
                {"magrnotv", new Regex(@"^[t-vT-V]")},
                {"magrnowz", new Regex(@"^[w-zW-Z]")}
            };

            string url = "";
            if (!pageExists){ // Gets the starting page based on first letter
                Parallel.ForEach(urlMapDict, (link, state) =>
                {
                    if (link.Value.Match(bookTitle).Success){
                        url = String.Format("https://www.animecornerstore.com/{0}.html", link.Key);
                        links.Add(url);
                    }
                });
            }
            else{ //Gets the actual page that houses the data that will be scraped from
                url = String.Format("https://www.animecornerstore.com/{0}", bookTitle);
                links.Add(url);
            }
            return url;
        }
        private static string getPageData(EdgeDriver edgeDriver, string bookTitle, char bookType, HtmlDocument doc){
            string link = "";
            edgeDriver.Navigate().GoToUrl(getUrl(bookTitle, false));
            Thread.Sleep(2000);
            doc.LoadHtml(edgeDriver.PageSource);

            HtmlNodeCollection seriesTitle = doc.DocumentNode.SelectNodes("//a[contains(@href,'.html')]");
            try{
                bookTitle = bookTitle.ToLower();
                Parallel.ForEach(seriesTitle, (title, state) =>
                {
                    string currTitle = title.InnerText.ToLower();
                    if (currTitle.IndexOf(bookTitle) != -1){
                        if ((bookType == 'M') && (currTitle.IndexOf("graphic") != -1)){
                            link = getUrl(title.Attributes["href"].Value, true);
                            state.Stop();
                        }
                        else if ((bookType == 'N') && (currTitle.IndexOf("graphic") == -1)){
                            link = getUrl(title.Attributes["href"].Value, true);
                            state.Stop();
                        }
                    }
                });
                if (link.Length == 0){
                    return "DNE";
                }
            }
            catch(NullReferenceException ex){
                if (seriesTitle == null){
                    Console.WriteLine("0");
                }
                Console.WriteLine(seriesTitle.Count);
                Console.Error.WriteLine(ex);
            }
            return link;
        }

        public static List<string[]> getRobertsAnimeCornerStoreData(string bookTitle, char bookType){
            EdgeOptions edgeOptions = new EdgeOptions();
            edgeOptions.UseChromium = true;
            edgeOptions.PageLoadStrategy = PageLoadStrategy.Eager;
            edgeOptions.AddArgument("headless");
            edgeOptions.AddArgument("disable-gpu");
            edgeOptions.AddArgument("disable-extensions");
            edgeOptions.AddArgument("inprivate");
            EdgeDriver edgeDriver = new EdgeDriver(edgeOptions);

            // Initialize the html doc for crawling
            HtmlDocument doc = new HtmlDocument();

            string linkPage = getPageData(edgeDriver, bookTitle, bookType, doc);
            if (linkPage == null){
                Console.Error.WriteLine("Error! Invalid Series Title");
                Environment.Exit(1);
            }
            else if (linkPage.Equals("DNE")){
                Console.Error.WriteLine(bookTitle + " does not exist at this website");
                edgeDriver.Quit();
            }
            else{
                try{
                    // Start scraping the URL where the data is found
                    edgeDriver.Navigate().GoToUrl(linkPage);
                    Thread.Sleep(2000);

                    // Get the html doc for crawling
                    doc.LoadHtml(edgeDriver.PageSource);

                    List<HtmlNode> titleData = doc.DocumentNode.SelectNodes("//font[@face='dom bold, arial, helvetica']//b").Where(title => title.InnerText.ToLower().IndexOf(bookTitle.ToLower()) != -1).ToList();
                    List<HtmlNode> priceData = doc.DocumentNode.SelectNodes("//font[@color='#ffcc33']").Where(price => price.InnerText.IndexOf("$") != -1).ToList();

                    string currTitle;
                    Parallel.For(0, titleData.Count, x =>
                    {
                        currTitle = titleData[x].InnerText.Replace(",", "");
                        dataList.Add(new string[]{titleData[x].InnerText.Replace(",", ""), priceData[x].InnerText, currTitle.IndexOf("Pre Order") != -1 ? "PO" : "IS", "RobertsAnimeCornerStore"});
                        //Console.WriteLine(currTitle + " " + priceData[x].InnerText + " " + stockStatus);
                    });

                    foreach (string link in links){
                        Console.WriteLine(link);
                    }

                    edgeDriver.Quit();
                }
                catch(NullReferenceException ex){
                    Console.Error.WriteLine(ex);
                    Environment.Exit(1);
                }
            }
            return dataList;
        }
    }
}

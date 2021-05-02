using System.Threading;
using System.Text.RegularExpressions;
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace MangaWebScrape.Websites
{
    class RightStufAnime
    {
        public static List<string> links = new List<string>();
        private static List<string[]> dataList = new List<string[]>();

        private static string filterBookTitle(string bookTitle){
            char[] trimedChars = {' ', '\'', '!', '-'};
            foreach (char var in trimedChars){
                bookTitle = bookTitle.Replace(var.ToString(), "%" + Convert.ToByte(var).ToString("x2").ToString());
            }
            return bookTitle;
        }

        private static string checkBookType(char bookType){
            if (bookType == 'M'){
                return "Manga";
            }
            else if (bookType == 'N'){
                return "Novels";
            }
            Console.Error.WriteLine("Invalid Book Type, must be a Manga (M) or Light Novel (LN)");
            return "Error";
        }

        private static string getUrl(char bookType, byte currPageNum, string bookTitle){
            string url = String.Format("https://www.rightstufanime.com/category/{0}?page={1}&show=96&keywords={2}", checkBookType(bookType), currPageNum, filterBookTitle(bookTitle));
            //Console.WriteLine(url);
            links.Add(url);
            return url;
        }

        public static List<string[]> getRightStufAnimeData(string bookTitle, char bookType, bool memberStatus, byte currPageNum)
        {
            // Initialize the html doc for crawling
            HtmlDocument doc = new HtmlDocument();

            EdgeOptions edgeOptions = new EdgeOptions();
            edgeOptions.UseChromium = true;
            edgeOptions.PageLoadStrategy = PageLoadStrategy.Eager;
            edgeOptions.AddArgument("headless");
            edgeOptions.AddArgument("disable-gpu");
            edgeOptions.AddArgument("disable-extensions");
            edgeOptions.AddArgument("inprivate");
            EdgeDriver edgeDriver = new EdgeDriver(edgeOptions);

            edgeDriver.Navigate().GoToUrl(getUrl(bookType, currPageNum, bookTitle));
            Thread.Sleep(3000);
            doc.LoadHtml(edgeDriver.PageSource);

            // Get the page data from the HTML doc
            HtmlNodeCollection titleData = doc.DocumentNode.SelectNodes("//span[@itemprop='name']");
            HtmlNodeCollection priceData = doc.DocumentNode.SelectNodes("//span[@itemprop='price']");
            HtmlNodeCollection stockStatusData = doc.DocumentNode.SelectNodes("//div[@class='product-line-stock-container '] | //span[@class='product-line-stock-msg-out-text']");
            HtmlNode pageCheck = doc.DocumentNode.SelectSingleNode("//li[@class='global-views-pagination-next']");

            try{
                double GotAnimeDiscount = 0.05;
                decimal priceVal;
                string priceTxt, stockStatus, currTitle;
                Regex removeWords = new Regex(@"[^a-z']");
                for (int x = 0; x < titleData.Count; x++)
                {
                    currTitle = titleData[x].InnerText;
                    if(removeWords.Replace(currTitle.ToLower(), "").IndexOf(removeWords.Replace(bookTitle.ToLower(), "")) != -1){
                        priceVal = System.Convert.ToDecimal(priceData[x].InnerText.Substring(1));
                        priceTxt = memberStatus ? "$" + (priceVal - (priceVal * (decimal)GotAnimeDiscount)).ToString("0.00") : priceData[x].InnerText;

                        stockStatus = stockStatusData[x].InnerText;
                        if (stockStatus.IndexOf("In Stock") != -1){
                            stockStatus = "IS";
                        }
                        else if (stockStatus.IndexOf("Out of Stock") != -1){
                            stockStatus = "OOS";
                        }
                        else if (stockStatus.IndexOf("Pre-Order") != -1){
                            stockStatus = "PO";
                        }
                        else{
                            stockStatus = "OOP";
                        }

                        dataList.Add(new string[]{currTitle, priceTxt, stockStatus, "RightStufAnime"});
                        //Console.WriteLine(currTitle + " " + priceTxt + " " + stockStatus);
                    }
                }

                if (pageCheck != null){
                    currPageNum++;
                    getRightStufAnimeData(bookTitle, bookType, memberStatus, currPageNum);
                }
                else{
                    edgeDriver.Quit();

                    foreach (string link in links){
                        Console.WriteLine(link);
                    }
                }
            }
            catch (NullReferenceException ex){
                Console.Error.WriteLine(ex);
                Environment.Exit(1);
            }

            return dataList;
        }
    }
}


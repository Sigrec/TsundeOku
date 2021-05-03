using System.Threading;
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

/*
https://www.barnesandnoble.com/s/overlord+light+novel/_/N-8q8Zucb/?Nrpp=40&Ns=P_Publication_Date%7C0&page=1
*/
namespace MangaWebScrape.Websites
{
    class BarnesAndNoble
    {
        public static List<string> links = new List<string>();
        private static List<string[]> dataList = new List<string[]>();

        private static string ParseBookTitle(string bookTitle){
            return new Regex(@"[^\w]").Replace(bookTitle, "+");
        }

        private static string GetUrl(string bookTitle, byte pageNum, char bookType){
            string url = "";
            if (bookType == 'M'){
                url = new string($"https://www.barnesandnoble.com/s/{ParseBookTitle(bookTitle)}+series/_/N-1z141tjZ8q8Zucb/?Nrpp=40&Ns=P_Publication_Date%7C1&page={pageNum}");
            }
            else if (bookType == 'N'){
                url = new string($"https://www.barnesandnoble.com/s/{ParseBookTitle(bookTitle)}+light+novel/_/N-8q8Zucb/?Nrpp=40&Ns=P_Publication_Date%7C1&page={pageNum}");
            }
            links.Add(url);
            return url;
        }

        public static List<string[]> GetBarnesAndNobleData(string bookTitle, char bookType, byte currPageNum){
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

            edgeDriver.Navigate().GoToUrl(GetUrl(bookTitle, currPageNum, bookType));
            Thread.Sleep(2000);
            doc.LoadHtml(edgeDriver.PageSource);

            HtmlNodeCollection titleData = doc.DocumentNode.SelectNodes("//a[@class=' ']");
            Console.WriteLine();
            HtmlNodeCollection priceData = doc.DocumentNode.SelectNodes("//a[@class=' link']//span[last()]");
            HtmlNodeCollection stockStatusData = doc.DocumentNode.SelectNodes("//div[1][@class='availability-spacing flex']//p");
            HtmlNode pageCheck = doc.DocumentNode.SelectSingleNode("//li[@class='pagination__next ']");
            if (bookType == 'N'){
                HtmlNodeCollection formatTypeData = doc.DocumentNode.SelectNodes("//span[@class='format']");
                for (int x = 0; x < formatTypeData.Count; x++){
                    if (formatTypeData[x].InnerText.IndexOf("NOOK") != -1){
                        titleData.RemoveAt(x);
                        formatTypeData.RemoveAt(x);
                        x--;
                    }
                }
                formatTypeData = null; //Free the format type list from memory
            }
            try{
                string stockStatus, currTitle;
                Regex removeExtra = new Regex(@"[^a-z']");
                for (int x = 0; x < titleData.Count; x++)
                {
                    currTitle = titleData[x].InnerText;                 
                    if(removeExtra.Replace(currTitle.ToLower(), "").IndexOf(removeExtra.Replace(bookTitle.ToLower(), "")) == 0){
                        stockStatus = stockStatusData[x].InnerText;
                        if (stockStatus.IndexOf("Available Online") != -1){
                            stockStatus = "IS";
                        }
                        else if (stockStatus.IndexOf("Out of Stock Online") != -1){
                            stockStatus = "OOS";
                        }
                        else if (stockStatus.IndexOf("Pre-order Now") != -1){
                            stockStatus = "PO";
                        }

                        dataList.Add(new string[]{currTitle, priceData[x].InnerText.Trim(), stockStatus, "Barnes & Noble"});
                    }
                }

                if (pageCheck != null){
                    currPageNum++;
                    GetBarnesAndNobleData(bookTitle, bookType, currPageNum);
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

             using (StreamWriter outputFile = new StreamWriter(@"C:\MangaWebScrape\MangaWebScrape\Data_Files\BarnesAndNobleData.txt"))
            {
                foreach (string[] data in dataList){
                    outputFile.WriteLine(data[0] + " " + data[1] + " " + data[2] + " " + data[3]);
                }
            }  

            return dataList;
        }
    }
}

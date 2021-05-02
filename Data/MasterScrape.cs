using System;
using MangaWebScrape.Websites;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MangaWebScrape
{
    class MasterScrape
    { 
        private static List<string[]> RightStufAnimeData = new List<string[]>();
        private static List<string[]> RobertsAnimeCornerStoreData = new List<string[]>();
        private static List<string[]> FinalData = new List<string[]>();
        private static byte numWebsites = 2;
        private static string[] userWebsites = new string[numWebsites];
        private static string bookTitle;
        private static char bookType;

        static void Main(string[] args)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            Console.Write("What is the Manga/Light Novel Title: ");
            bookTitle = Console.ReadLine();
            Console.Write("Are u searching for a Manga (M) or Light Novel (N): ");
            bookType = char.Parse(Console.ReadLine());

            var RightStufAnimeTask = Task.Factory.StartNew(() => RightStufAnime.getRightStufAnimeData(bookTitle, bookType, true, 1));
            var RobertsAnimeCornerStoreTask = Task.Factory.StartNew(() => RobertsAnimeCornerStore.getRobertsAnimeCornerStoreData(bookTitle, bookType));

            Task.WhenAll(RightStufAnimeTask, RobertsAnimeCornerStoreTask);

            RightStufAnimeData = RightStufAnimeTask.Result;
            RobertsAnimeCornerStoreData = RobertsAnimeCornerStoreTask.Result;

            Regex getRobertVolNum = new Regex(@"(#[\d+]*)");
            Regex getRightStufVolNum = new Regex(@"([^Volume ]*$)");

            /*
                First checks to see which website has fewer entires then compares the pricing for volumes and outputs a list of the volumes with the lowest price and the retailer
            */

            int RightStufAnimeDataSize = RightStufAnimeData.Count;
            int RobertsAnimeCornerStoreDataSize = RobertsAnimeCornerStoreData.Count;
            int biggerDataSize = RightStufAnimeDataSize >= RobertsAnimeCornerStoreDataSize ? RightStufAnimeDataSize : RobertsAnimeCornerStoreDataSize;
            for (int x = 0; x < biggerDataSize; x++){
                if (RightStufAnimeDataSize >= RobertsAnimeCornerStoreDataSize){
                    for(int y = 0; y < RobertsAnimeCornerStoreData.Count; y++)
                    {
                        if((getRobertVolNum.Match(RobertsAnimeCornerStoreData[y][0]).Groups[1].Value.IndexOf((getRightStufVolNum.Match(RightStufAnimeData[x][0]).Groups[1].Value))) != -1){
                            if ((Convert.ToDouble(RobertsAnimeCornerStoreData[y][1].Substring(1)) < Convert.ToDouble(RightStufAnimeData[x][1].Substring(1)))){
                                RightStufAnimeData[x] = RobertsAnimeCornerStoreData[y];
                                RobertsAnimeCornerStoreData.RemoveAt(y);
                            }
                        }
                    }
                }
                else{
                    for(int y = 0; y < RightStufAnimeData.Count; y++)
                    {
                        if((getRobertVolNum.Match(RobertsAnimeCornerStoreData[x][0]).Groups[1].Value.IndexOf((getRightStufVolNum.Match(RightStufAnimeData[y][0]).Groups[1].Value))) != -1){
                            if ((Convert.ToDouble(RobertsAnimeCornerStoreData[x][1].Substring(1)) < Convert.ToDouble(RightStufAnimeData[y][1].Substring(1)))){
                                RobertsAnimeCornerStoreData[x] = RightStufAnimeData[y];
                                RightStufAnimeData.RemoveAt(y);
                            }
                        }
                    }
                }
            }

            for (int x = 0; x < RightStufAnimeDataSize; x++){
                Console.WriteLine(RightStufAnimeData[x][0] + " " + RightStufAnimeData[x][1] + " " + RightStufAnimeData[x][2] + " " + RightStufAnimeData[x][3]);
            }

            watch.Stop();
            Console.WriteLine($"Time in Miliseconds: {watch.ElapsedMilliseconds}");
        }
    }
}

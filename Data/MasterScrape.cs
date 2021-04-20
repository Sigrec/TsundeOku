using System;
using MangaWebScrape.Websites;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MangaWebScrape
{
    class MasterScrape
    {
        private static List<string[]> RightStufAnimeData = new List<string[]>();
        private static List<string[]> RobertsAnimeCornerStoreData = new List<string[]>();
        static void Main(string[] args)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            var RightStufAnimeTask = Task.Factory.StartNew(() => RightStufAnime.getRightStufAnimeData("world trigger", 'M', true, 1));
            var RobertsAnimeCornerStoreTask = Task.Factory.StartNew(() => RobertsAnimeCornerStore.getRobertsAnimeCornerStoreData("world trigger", 'M'));
            Task.WaitAll(RightStufAnimeTask, RobertsAnimeCornerStoreTask);

            RightStufAnimeData = RightStufAnimeTask.Result;
            RobertsAnimeCornerStoreData = RobertsAnimeCornerStoreTask.Result;


            // for (int x = 0; x < (RightStufAnimeData.Count >= RobertsAnimeCornerStoreData.Count ? RobertsAnimeCornerStoreData.Count : RightStufAnimeData.Count); x++){
            //     if (RobertsAnimeCornerStoreData.Count <= RightStufAnimeData.Count){
            //         Parallel.ForEach(RightStufAnimeData, (rsData, state) =>
            //         {
            //             if((rsData[0].ToLower().IndexOf(RobertsAnimeCornerStoreData[x][0]) != -1) && (Convert.ToDouble(RobertsAnimeCornerStoreData[x][1].Substring(1)) < Convert.ToDouble(rsData[1].Substring(1)))){
            //                 rsData = RobertsAnimeCornerStoreData[x];
            //             }
            //             state.Stop();
            //         });
            //     }
            //     Console.WriteLine("{0}", string.Join(", ", RightStufAnimeData[x]));
            // }

            watch.Stop();
            Console.WriteLine($"Time in Miliseconds: {watch.ElapsedMilliseconds}");
        }
    }
}

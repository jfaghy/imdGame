using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace iMdBgAmE
{
    class Program
    {

        private static string directoryPath = @"c:\imdb";
        private static int ratingsRequired = 250000;
        static void Main(string[] args)
        {
            Console.WriteLine(@"
    ██╗███╗   ███╗██████╗ ██████╗      ██████╗  █████╗ ███╗   ███╗███████╗
    ██║████╗ ████║██╔══██╗██╔══██╗    ██╔════╝ ██╔══██╗████╗ ████║██╔════╝
    ██║██╔████╔██║██║  ██║██████╔╝    ██║  ███╗███████║██╔████╔██║█████╗  
    ██║██║╚██╔╝██║██║  ██║██╔══██╗    ██║   ██║██╔══██║██║╚██╔╝██║██╔══╝  
    ██║██║ ╚═╝ ██║██████╔╝██████╔╝    ╚██████╔╝██║  ██║██║ ╚═╝ ██║███████╗
    ╚═╝╚═╝     ╚═╝╚═════╝ ╚═════╝      ╚═════╝ ╚═╝  ╚═╝╚═╝     ╚═╝╚══════╝
                                                                      
            ");
            Console.WriteLine("LOADING ...");
            const string filmiesPath = @"c:\imdb\filmies.tsv";
            const string ratiesPath = @"c:\imdb\raties.tsv";

//            WebClient webClient = new WebClient();
//            webClient.DownloadFile("https://datasets.imdbws.com/title.basics.tsv.gz", filmiesPath +".gz");
//            webClient.DownloadFile("https://datasets.imdbws.com/title.ratings.tsv.gz", ratiesPath + ".gz");
//
//            DirectoryInfo directorySelected = new DirectoryInfo(directoryPath);
//
//            foreach (FileInfo fileToDecompress in directorySelected.GetFiles("*.gz"))
//            {
//                Decompress(fileToDecompress);
//            }

            IList<Movie> movies = ParseCSV(filmiesPath);
            MergeLists(movies, ratiesPath);
            
            movies = movies.Where(x => x.rating != 0).ToList();

            Random r = new Random();
            int random = r.Next(0, movies.Count - 1);
            Console.WriteLine($"The Movie of the Day is: {movies[random]}");
            while (true)
            {
                Console.Write("Do you want to veto (is Jack moaning)? y/n");
                string veto = Console.ReadLine();
                if (veto.ToLower() == "y")
                {
                    random = r.Next(0, movies.Count - 1);
                    Console.WriteLine($"New Movie is: {movies[random]}");
                }
            }
        }

        public static void Decompress(FileInfo fileToDecompress)
        {
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                using (FileStream decompressedFileStream = File.Create(newFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                        Console.WriteLine($"Decompressed: {fileToDecompress.Name}");
                    }
                }
            }
        }

        public static IList<Movie> ParseCSV(string filepath)
        {

            StreamReader reader = new StreamReader(File.OpenRead(filepath));
            List<Movie> listOfMovies = new List<Movie>();

            //string vara1, vara2, vara3, vara4;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (!String.IsNullOrWhiteSpace(line))
                {
                    string[] values = line.Split('\t');
                    if (values[1] == "movie")
                    {
                        listOfMovies.Add(new Movie(values[0], values[2]));
                    }
                }
            }
            return listOfMovies;
        }

        public static void MergeLists(IList<Movie> movies, string ratingFilepath)
        {
            StreamReader reader = new StreamReader(File.OpenRead(ratingFilepath));
            //string vara1, vara2, vara3, vara4;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (!String.IsNullOrWhiteSpace(line))
                {
                    string[] values = line.Split('\t');
                    if (int.TryParse(values[2], out int rating) && rating > ratingsRequired)
                    {
                        Movie match = movies.FirstOrDefault(x => x.tconst == values[0]);
                        if (match != null)
                        {
                            match.rating = rating;
                        }
                    }
                }
            }

        }


    }

    class Movie
    {
        public Movie(string tconst, string title)
        {
            this.tconst = tconst;
            this.title = title;
        }
        public string tconst { get; set; }
        public string title { get; set; }
        public int rating { get; set; }

        public override string ToString()
        {
            return title;
        }
    }
}

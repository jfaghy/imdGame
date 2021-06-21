using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using MigrateB2C;

namespace iMdBgAmE
{
    class Program
    {

        private static string directoryPath = @"c:\imdb";
        private static int votesRequired = 250000;
        static void Main(string[] args)
        {
            Console.WriteLine(@"
 ██╗ ███╗   ███╗ ██████╗  ██████╗      ██████╗  █████╗ ███╗   ███╗███████╗
 ██║ ████╗ ████║ ██╔══██╗ ██╔══██╗    ██╔════╝ ██╔══██╗████╗ ████║██╔════╝
 ██║ ██╔████╔██║ ██║  ██║ ██████╔╝    ██║  ███╗███████║██╔████╔██║█████╗  
 ██║ ██║╚██╔╝██║ ██║  ██║ ██╔══██╗    ██║   ██║██╔══██║██║╚██╔╝██║██╔══╝  
 ██║ ██║ ╚═╝ ██║ ██████╔╝ ██████╔╝    ╚██████╔╝██║  ██║██║ ╚═╝ ██║███████╗
 ╚═╝ ╚═╝     ╚═╝ ╚═════╝  ╚═════╝      ╚═════╝ ╚═╝  ╚═╝╚═╝     ╚═╝╚══════╝
                                                                      
            ");
            Console.WriteLine("LOADING ...");
            const string filmiesPath = @"c:\imdb\filmies.tsv";
            const string ratiesPath = @"c:\imdb\raties.tsv";
            const string usedPath = @"c:\imdb\filmHistory.txt";
            //const string testPath = @"c:\imdb\test.tsv";

            WebClient webClient = new WebClient();
            webClient.DownloadFile("https://datasets.imdbws.com/title.basics.tsv.gz", filmiesPath + ".gz");
            webClient.DownloadFile("https://datasets.imdbws.com/title.ratings.tsv.gz", ratiesPath + ".gz");

            DirectoryInfo directorySelected = new DirectoryInfo(directoryPath);

            foreach (FileInfo fileToDecompress in directorySelected.GetFiles("*.gz"))
            {
                Decompress(fileToDecompress);
            }

            IList<Movie> movies = ParseCSV(filmiesPath);
            IList<Oldies> usedMovies = ParseTXT(usedPath);
            //IList<Movie> newMovies = movies.Except(usedMovies).ToList();
            MergeLists(movies, ratiesPath);
            movies = movies.Where(x => x.rating != 0 && usedMovies.All(y => y.title != x.title)).ToList();
            if (!movies.Any())
            {
                CustomConsole.WriteLineRed("GAME OVER");
                Console.ReadLine();
                return;
            }
            bool nextFilm = true;
            Random r = new Random();
            int random = r.Next(0, movies.Count - 1);
            Console.WriteLine($"The Movie of the Day is: {movies[random]}");
            while (nextFilm)
            {
                Console.Write("Play or Pass?");
                string veto = Console.ReadLine();
                if (veto.ToLower() == "play" || veto.ToLower() == "p"|| veto.ToLower() == "y")
                {
                    IMovieHistory movieHistory = new MovieHistoryFile();
                    Movie currentMovie = movies[random];
                    movieHistory.SaveMovie(currentMovie);
                    nextFilm = false;
                    CustomConsole.WriteLineGreen($"The movie is: {movies[random]}");
                    string nextStep = Console.ReadLine();
                    if(nextStep.ToLower() == "results" || nextStep.ToLower() == "r" || nextStep.ToLower() == "a")
                    {
                        CustomConsole.WriteLineBlue($"Answers are: Year: {currentMovie.year}, Rating: {currentMovie.rating}, Runtime: {currentMovie.runtime} minutes");
                        Console.ReadLine();
                    }        
                }
                else if(veto.ToLower() == "done" || veto.ToLower() == "d")
                {
                    CustomConsole.WriteLineYellow($"Added movie: {movies[random]}");
                    IMovieHistory movieHistory = new MovieHistoryFile();
                    movieHistory.SaveMovie(movies[random]);
                    random = r.Next(0, movies.Count - 1);
                    Console.WriteLine($"New Movie is: {movies[random]}");
                }
                else
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
                        listOfMovies.Add(new Movie(values[0], values[2], values[7], values[5]));
                    }
                }
            }
            return listOfMovies;
        }
        public static IList<Oldies> ParseTXT(string filepath)
        {
            StreamReader reader = new StreamReader(File.OpenRead(filepath));
            List<Oldies> usedList = new List<Oldies>();

            //string vara1, vara2, vara3, vara4;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (!String.IsNullOrWhiteSpace(line))
                {
                    string[] values = line.Split();
                    usedList.Add(new Oldies(line));
                }
            }
            return usedList;
        }
        public static void MergeLists(IList<Movie> movies, string ratingFilepath)
        {
            StreamReader reader = new StreamReader(File.OpenRead(ratingFilepath));
            //string vara1, vara2, vara3, vara4;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    string[] values = line.Split('\t');
                    if (int.TryParse(values[2], out int numOfVotes) && numOfVotes > votesRequired)
                    {
                        Movie match = movies.FirstOrDefault(x => x.tconst == values[0]);
                        if (match != null)
                        {
                            match.rating = Convert.ToDouble(values[1]);
                        }
                    }
                }
            }
        }
    }
}

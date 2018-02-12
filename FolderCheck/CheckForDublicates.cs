using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace FolderCheck
{
    class CheckForDublicates
    {
        private static char[] loading = new char[] { '-', '\\', '|', '/' };
        private static int index = 0;
        static DateTime Begintime;
        static DateTime Lastchange;
        static Dictionary<Tuple<string, long>, string> Files = new Dictionary<Tuple<string, long>, string>();
        static Dictionary<Tuple<string, long>, List<string>> dublets = new Dictionary<Tuple<string, long>, List<string>>();

        private static List<string> UsedDirect = new List<string>();

        static List<String> NotSerached = new List<string>();
        static List<String> NotCheked = new List<string>();

        public static void FileTjeck(string directory = "")
        {

            bool stop = false;

            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.Title = "Anders FolderFileTjeck";


            do
            {
                clearall();
                clear();
                Console.Clear();
                Console.WriteLine("Hej " + Environment.UserName + "\nVelkommen til Anders´S FolderFileTjeck");
                Console.WriteLine("\n\n");
                while (!Directory.Exists(directory) && directory.ToLower() != "exit")
                {
                    if (directory != "")
                        Console.WriteLine("Den valgte mappe kunne ikke findes prøv væligst igen eller skriv \"Exit\" For at annulere");
                    Console.WriteLine("Hvilen mappe vil du gerne tjecke om har dubletter");
                    directory = Console.ReadLine();


                }
                if (directory.ToLower() == "exit")
                    break;

                Begintime = DateTime.Now;

                Console.CursorVisible = false;
                indexfile(directory);


                Console.CursorVisible = true;
                Console.Clear();

                Console.WriteLine("Gennemtjeckningen tog " + (DateTime.Now - Begintime).TotalSeconds + "Sekunder og fanst " + dublets.Keys.Count + " dublikerede Filer\nSøgningen måtte desvære springe " + NotSerached.Count + " mapper over Pga Rettighedsproblemer.\n\nSøgningen er nu gennemført og du har nogle mugligheder for at Fortsætte");
                bool retry = true;
                do
                {
                    Console.WriteLine("\nTryk Q for at afslutte\nTryk I for at få en liste over mapper der ikke er blævet gennemsøgt\nTryk F for at få en liste af alle funde filer\nTryk k fir at få en liste over filer der ikke blec tjecket\nTryk N for at starte en ny søgning");

                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.Q:
                            stop = true;
                            retry = false;
                            break;
                        case ConsoleKey.N:
                            stop = false;
                            retry = false;
                            directory = "";
                            break;

                        case ConsoleKey.F:
                            Console.WriteLine("\nFøglende Dubleter blev fundet\n");

                            foreach (var dublet in dublets.Keys)
                            {
                                Console.WriteLine("\t Number of dublets" + dublets[dublet].Count + "\n{");

                                foreach (var d in dublets[dublet])
                                {
                                    Console.WriteLine(d);
                                }
                                Console.WriteLine("}");
                            }
                            break;

                        case ConsoleKey.K:
                            Console.WriteLine("\nFøglende Filer blev Tjecket\n");

                            foreach (var file in NotCheked)
                            {
                                Console.WriteLine(file);
                            }
                            break;



                        case ConsoleKey.I:
                            Console.WriteLine("\nFøglende mapper blev ikke gennemsøgt\n");

                            foreach (var ns in NotSerached.Where(i => !string.IsNullOrEmpty(i)).Distinct())
                            {
                                Console.WriteLine(ns);
                            }
                            break;

                    }


                } while (retry);
            } while (!stop);
        }

        private static void indexfile(string directory)
        {
            try
            {
                foreach (var file in Directory.GetFiles(directory))
                {
                    var key = "";
                    var fileinfo = new FileInfo(file);
                    if (fileinfo.Extension == ".lnk")
                        continue;
                    if (DateTime.Now - Lastchange > TimeSpan.FromMilliseconds(100))
                    {
                        if (index > 3)
                            index = 0;

                        int currentLineCursor = Console.CursorTop;
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write(new string(' ', Console.WindowWidth));
                        Console.SetCursorPosition(0, currentLineCursor);
                        var s = "Checking[" + loading[index] + "]" + "\t" + " Current File: " + file;
                        if (s.Length + 6 > Console.WindowWidth)
                        {
                            s = s.Remove(Console.WindowWidth - 9);
                            s = s + "...";
                        }

                        Console.Write(s);
                        index++;
                        Lastchange = DateTime.Now;
                    }

                    
                    try
                    {
                        using (var Sha512 = SHA512.Create())
                        {
                            using (var stream = File.OpenRead(file))
                            {
                                var hash = Sha512.ComputeHash(stream);
                                key = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                            }
                        }
                    }
                    catch (System.IO.IOException)
                    {
                        NotCheked.Add(file);
                    }
                    if (!Files.ContainsKey(new Tuple<string, long>(key, fileinfo.Length)))
                    {
                        Files.Add(new Tuple<string, long>(key, fileinfo.Length), file);
                    }
                    else if (dublets.ContainsKey(new Tuple<string, long>(key, fileinfo.Length)))
                    {
                        dublets[new Tuple<string, long>(key, fileinfo.Length)].Add(file);
                    }
                    else
                    {
                        dublets[new Tuple<string, long>(key, fileinfo.Length)] = new List<string>();
                        dublets[new Tuple<string, long>(key, fileinfo.Length)].Add(file);
                        dublets[new Tuple<string, long>(key, fileinfo.Length)].Add(Files[new Tuple<string, long>(key, fileinfo.Length)]);

                    }


                }

                foreach (var directori in Directory.GetDirectories(directory))
                {
                        indexfile(directori);
                }

            }
            catch (System.UnauthorizedAccessException e)
            {
                NotSerached.Add(e.Message.Split('\'')[1]);
                return;
            }


        }


        /* private static bool serach(string directory, FileInfo searchKriteria)
         {
             try
             {
                 if (DateTime.Now - Lastchange > TimeSpan.FromMilliseconds(100))
                 {
                     if (index > 3)
                         index = 0;

                     int currentLineCursor = Console.CursorTop;
                     Console.SetCursorPosition(0, Console.CursorTop);
                     Console.Write(new string(' ', Console.WindowWidth));
                     Console.SetCursorPosition(0, currentLineCursor);
                     var s = "Loading[" + loading[index] + "]" + "\tCurrent file: " + searchKriteria.FullName + " Current Directory: " + directory;
                     if (s.Length + 6 > Console.WindowWidth)
                     {
                         s = s.Remove(Console.WindowWidth - 9);
                         s = s + "...";
                     }

                     Console.Write(s);
                     index++;
                     Lastchange = DateTime.Now;
                 }

                 UsedDirect.Add(directory);
                 var files = Directory.GetFiles(directory);

                 foreach (var file in files)
                 {
                     var sfile = new FileInfo(file);

                     if (sfile.Exists &&
                         sfile.Name.Equals(searchKriteria.Name) &&
                         sfile.Length.Equals(searchKriteria.Length) &&
                         sfile.Extension.Equals(searchKriteria.Extension))
                     {
                         return true;
                     }
                 }

                 var directories = Directory.GetDirectories(directory);

                 foreach (var directori in directories)
                 {
                     if (!UsedDirect.Contains(directori))
                     {
                         if (serach(directori, searchKriteria))
                         {
                             return true;
                         }
                     }

                 }
                 return false;
             }
             catch (System.UnauthorizedAccessException e)
             {
                 NotSerached.Add(e.Message.Split('\'')[1]);
                 return false;
             }

         }
         */

        private static void clear()
        {
            UsedDirect.Clear();
            // index = 0;
        }
        private static void clearall()
        {
            UsedDirect.Clear();
            NotSerached.Clear();
            dublets.Clear();
            Files.Clear();
            index = 0;
        }
    }
}

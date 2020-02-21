using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HashCode2020
{
    class Program
    {
        public static string InputFolder = Path.Combine(Environment.CurrentDirectory, @"output");
        public static string OutputFolder = Path.Combine(Environment.CurrentDirectory, @"input");

        public class Library
        {
            public int id;
            public int numberOfBooks;
            public int signUpDays;
            public int booksPerDay;
            public IDictionary<int, int> BookIdAndScoreOrderedDescendingByScore;
            public int TotalScoreFirst20Books { get; internal set; }
            public int TotalScore { get; internal set; }

            public int[] books;
        }

        public class LibraryResult
        {
            public int id;
            public int numberOfBooksForScanning;
            public int[] bookNumbers;
        }

        static void Main(string[] args)
        {
            foreach (var path in Directory.GetFiles(InputFolder))
            {
                (int daysOfScanning, int[] scoresOfBooks, Library[] libraries) = ProcessInput(path);

                //libraries = OrderLibBooks20FirstValues(libraries, scoresOfBooks);

                //libraries = OrderLibBooksByValue(libraries, scoresOfBooks);

                RemoveDuplicateBooks(libraries);

                libraries = OrderLibBooksByValue(libraries, scoresOfBooks);

                List<LibraryResult> result = SolveProblem(daysOfScanning, scoresOfBooks, libraries);

                WriteOutput(result, path);

                //break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="libraries"></param>
        public static void RemoveDuplicateBooks(Library[] libraries)
        {
            HashSet<int> uniqueBooks = new HashSet<int>(libraries[0].books);

            for (int i = 1; i < libraries.Length; i++)
            {
                HashSet<int> current = new HashSet<int>(libraries[i].books);
                current.ExceptWith(uniqueBooks);
                libraries[i].books = current.ToArray();
                uniqueBooks.UnionWith(libraries[i].books);

                libraries[i].numberOfBooks = libraries[i].books.Length;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="libraries"></param>
        /// <param name="scores"></param>
        /// <returns></returns>
        public static Library[] OrderLibBooksByValue(Library[] libraries, int[] scores)
        {
            GetTotalScore(libraries, scores);

            return libraries.OrderByDescending(lib => lib.TotalScore / lib.signUpDays).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="libraries"></param>
        /// <param name="scores"></param>
        /// <returns></returns>
        public static Library[] OrderLibBooks20FirstValues(Library[] libraries, int[] scores)
        {
            foreach (var library in libraries)
            {
                IDictionary<int, int> dictionary = new Dictionary<int, int>();
                foreach (var bookId in library.books)
                {
                    dictionary.Add(bookId, scores[bookId]);
                }
                var sortedDict = from entry in dictionary orderby entry.Value descending select entry;
                library.BookIdAndScoreOrderedDescendingByScore = new Dictionary<int, int>(sortedDict);
            }

            GetTotalScoreFirst20(libraries);

            return libraries.OrderByDescending(lib => lib.TotalScoreFirst20Books / lib.signUpDays).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="libraries"></param>
        /// <param name="scores"></param>
        public static void GetTotalScore(Library[] libraries, int[] scores)
        {
            foreach (var library in libraries)
            {
                library.TotalScore = 0;
                foreach (var book in library.books)
                {
                    library.TotalScore += scores[book];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="libraries"></param>
        public static void GetTotalScoreFirst20(Library[] libraries)
        {
            foreach (var library in libraries)
            {
                var totalScore = 0;
                var enumerator = library.BookIdAndScoreOrderedDescendingByScore.GetEnumerator();
                for (int i = 0; i < 50; i++)
                {
                    totalScore += enumerator.Current.Value;
                    enumerator.MoveNext();
                }
                library.TotalScoreFirst20Books = totalScore;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static (int, int[], Library[]) ProcessInput(string path)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(path);

            string[] nm = file.ReadLine().Split(' ');

            int numberOfBooks = Convert.ToInt32(nm[0]);
            int numberOfLibraries = Convert.ToInt32(nm[1]);
            int daysOfScanning = Convert.ToInt32(nm[2]);

            int[] scoresOfBooks = Array.ConvertAll(file.ReadLine().Split(' '), aTemp => Convert.ToInt32(aTemp));

            var libraries = new Library[numberOfLibraries];

            for (int i = 0; i < numberOfLibraries; i++)
            {
                var library = new Library();
                library.id = i;
                nm = file.ReadLine().Split(' ');
                library.numberOfBooks = Convert.ToInt32(nm[0]);
                library.signUpDays = Convert.ToInt32(nm[1]);
                library.booksPerDay = Convert.ToInt32(nm[2]);

                library.books = Array.ConvertAll(file.ReadLine().Split(' '), aTemp => Convert.ToInt32(aTemp));

                libraries[i] = library;
            }

            file.Close();

            return (daysOfScanning, scoresOfBooks, libraries);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="libraryResults"></param>
        /// <param name="path"></param>
        static void WriteOutput(List<LibraryResult> libraryResults, string path)
        {
            var fileName = $"{System.IO.Path.GetFileNameWithoutExtension(path)}.txt";
            TextWriter textWriter = new StreamWriter(Path.Combine(OutputFolder, fileName));

            textWriter.WriteLine(libraryResults.Count);

            foreach (var res in libraryResults)
            {
                textWriter.WriteLine($"{res.id} {res.numberOfBooksForScanning}");
                textWriter.WriteLine(string.Join(' ', res.bookNumbers));
            }

            textWriter.Flush();
            textWriter.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="daysOfScanning"></param>
        /// <param name="scoresOfBooks"></param>
        /// <param name="libraries"></param>
        /// <returns></returns>
        public static List<LibraryResult> SolveProblem(int daysOfScanning, int[] scoresOfBooks, Library[] libraries)
        {
            int remainingDays = 0;

            List<LibraryResult> result = new List<LibraryResult>();

            foreach (Library library in libraries)
            {
                LibraryResult libraryResult = new LibraryResult();

                int numberOfBooks = daysOfScanning - remainingDays - library.signUpDays;

                numberOfBooks = Math.Min(numberOfBooks, library.numberOfBooks);

                if (numberOfBooks > 0)
                {
                    libraryResult.id = library.id;

                    List<int> books = new List<int>();

                    for (int i = 0; i < Math.Min(numberOfBooks * library.booksPerDay, library.books.Length); i++)
                    {
                        books.Add(library.books[i]);
                    }

                    libraryResult.bookNumbers = books.ToArray();
                    libraryResult.numberOfBooksForScanning = books.Count;
                }
                else
                {
                    break;
                }

                remainingDays += library.signUpDays;

                result.Add(libraryResult);
            }


            return result;
        }
    }
}

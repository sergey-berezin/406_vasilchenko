using RouteOptimizerLib;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;

static double[,] arrayInput()
{
    Console.Write("Cities count:");
    int citiesCount = Convert.ToInt32(Console.ReadLine());
    double[,] res = new double[citiesCount, citiesCount];
    for (int i = 0; i < citiesCount; i++)
    {
        string tmp = Console.ReadLine();
        string[] values = tmp.Split(' ');
        for (int j = 0; j < values.Length; j++)
        {
            res[i, j] = Convert.ToDouble(values[j]);
        }
    }
    return res;
}
static double[,] randomMatrix(int size)
{
    Random rand = new Random();
    double[,] arr = new double[size, size];
    for(int i = 0;i < size; i++)
    {
        arr[i, i] = 0;
        for(int j = 0; j < i; j++)
        {
            arr[i,j] = arr[j,i] = rand.Next(1,100);

        }
        
    }
    return arr;
}
static List<int> citiesInput()
{
    List<int> ints = new List<int>();
    string tmp  = Console.ReadLine();
    string[] strs = tmp.Split(' ');
    for (int i = 0; i < strs.Length; i++)
    {
        ints.Add(int.Parse(strs[i]));
    }
    return ints;
}
static void printMatrix(double[,] matrix)
{
    for (int i = 0; i < matrix.GetLength(0); i++)
    {
        for (int j = 0; j < matrix.GetLength(1); j++)
        {
            Console.Write(matrix[i, j] + "\t");
        }
        Console.WriteLine();
    }
}
static void printList(List<int> list)
{
    foreach(int i in list)
    {
        Console.Write(i + " ");
    }
}
static void runWithInput()
{
    Console.WriteLine("Input distance matrix:");
    double[,] matrix = arrayInput();
    Console.WriteLine("Input cities that you want to visit");
    List<int> cities = citiesInput();
    Console.Write("Input population size:");
    int populationSize = int.Parse(Console.ReadLine());
    Console.Write("Input epoch count:");
    int epochCount = int.Parse(Console.ReadLine());
     runAlgorithm(matrix,cities, populationSize, epochCount);

}
static void runAlgorithm(double[,] M, List<int> selectedCities, int populationSize, int epochCount)
{
    RouteOptimizer routeOptimizer = new RouteOptimizer(M.GetLength(0), M, selectedCities);
    Route best = null;
    routeOptimizer.StartAlgorithm(populationSize);

    for(int i = 0;i < epochCount; i++)
    {
        routeOptimizer.NextIteration_Parallel();
        Console.WriteLine($"Epoch#{i + 1}: best length: {best.Distance} on route: {best.ToString()}");
    }
    Console.WriteLine($"Best route:{best} with length: {best.Distance}");
}

static void runDefault()
{
    double[,] M =
{
    {0, 6, 4, 8, 7, 14},
    {6, 0, 7, 11, 7, 10 },
    {4, 7, 0, 4, 3, 10 },
    {8, 11, 4, 0, 5, 11 },
    {7, 7, 3, 5, 0, 7 },
   { 14, 10, 10, 11, 7, 0 }
};
    List<int> cities = new List<int>() { 0, 1, 2, 3, 4, 5 };
    Console.WriteLine("Matrix:");
    printMatrix(M);
    Console.WriteLine("Selected cities:");
    printList(cities);
    int populationSize = 100;
    int epochCount = 30;
    Console.WriteLine($"Population size: {populationSize}   Epoch count: {epochCount}");
    runAlgorithm(M, cities, populationSize, epochCount);
}
static void runRandom()
{
    Console.Write("Cities number: ");
    int number = Convert.ToInt32(Console.ReadLine());
    double[,] matrix = randomMatrix(number);
    printMatrix(matrix);
    List<int> cities = new List<int>();
    for(int i= 0;i < number; i++) cities.Add(i);
    Console.Write("Input population size:");
    int populationSize = int.Parse(Console.ReadLine());
    Console.Write("Input epoch count:");
    int epochCount = int.Parse(Console.ReadLine());
    runAlgorithm(matrix, cities, populationSize, epochCount);
}
static double runRandomForTime(int cities_count, int populationSize, int epochCount)
{
    double[,] M = randomMatrix(cities_count);
    List<int> selectedCities = new List<int>();
    for (int i = 0; i < cities_count; i++) selectedCities.Add(i);
    Stopwatch stopwatch = new Stopwatch();
    RouteOptimizer routeOptimizer = new RouteOptimizer(M.GetLength(0), M, selectedCities);
    Route best = null;

    stopwatch.Start();
    routeOptimizer.StartAlgorithm(populationSize);

    for (int i = 0; i < epochCount; i++)
    {
        best = routeOptimizer.NextIteration();
    }
    
    stopwatch.Stop();
    double time_notParallel = stopwatch.Elapsed.TotalMilliseconds;
    double notParallelLen = best.Distance;

    stopwatch = new Stopwatch();
    routeOptimizer = new RouteOptimizer(M.GetLength(0), M, selectedCities);
    best = null;
    stopwatch.Start();
    routeOptimizer.StartAlgorithm(populationSize);

    for (int i = 0; i < epochCount; i++)
    {
        best= routeOptimizer.NextIteration_Parallel();
    }
    stopwatch.Stop();
    double time_Parallel = stopwatch.Elapsed.TotalMilliseconds;
    double ParallelLen = best.Distance;

    Console.WriteLine($"{notParallelLen} ({time_notParallel}) --- {ParallelLen} ({time_Parallel})");
    return time_Parallel - time_notParallel;
}

Console.WriteLine("0 - default; 1 - custom; 2 - random; 3 - time comparison");
char c = Console.ReadLine()[0];
switch (c)
{
    case '0':
        runDefault();
        break;
    case '1':
        runWithInput();
        break;
    case '2':
        runRandom();
        break;
    case '3':
        Console.WriteLine("Not Parallel ---- Parallel");
        for(int i = 5; i < 1000; i += 100)
        {
            Console.WriteLine($"Cities count: {i}");
            runRandomForTime(i, 1000, 10);
        }
        break;
}


Console.ReadLine();



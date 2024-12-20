﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Xml.XPath;
using static System.Formats.Asn1.AsnWriter;
using System.Text.Json;
namespace RouteOptimizerLib
{
    public delegate void OptimalRouteUpdated(Object o, RouteEventArgs e);

    public class RouteOptimizer
    {
        List<List<Double>> DistanceMatrix;
        int cityCount;
        List<int> selectedCities;
        Population currentPopulation;
        int populationSize = 0;
        Random random = new Random();

        public static OptimalRouteUpdated OptimalRouteUpdated;

        public RouteOptimizer(int cityCount, double[,] Matrix, List<int> selectedCities)
        {
            this.cityCount = cityCount;
            this.selectedCities = selectedCities;
            this.DistanceMatrix = ConvertToList(Matrix);

        }
        public RouteOptimizer()
        {
            DistanceMatrix = new List<List<double>>();
            cityCount = 0;
            selectedCities = new List<int>();
        }

        private List<List<double>> ConvertToList(double[,] arr)
        {
            List<List<double>> list = new List<List<double>>();
            int n = arr.GetLength(0);
            int m = arr.GetLength(1);
            for(int i= 0;i < n; i++)
            {
                List<double> tmp = new List<double>();
                for(int j = 0; j < m; j++)
                {
                    tmp.Add(arr[i,j]);
                }
                list.Add(tmp);
            }
            return list;
        }
        public List<List<double>> Matrix
        {
            get { return DistanceMatrix; }
            set { DistanceMatrix = value; }
        }
        public int CityCount
        {
            get { return cityCount; }
            set { cityCount = value; }
        }
        public Population CurrentPopulation
        {
            get { return currentPopulation; }
            set { currentPopulation = value; }
        }
        public List<int> SelectedCities
        {
            get { return selectedCities; }
            set { selectedCities = value; }
        }
        public int PopulationSize
        {
            get { return populationSize; }
            set { populationSize = value; }
        }

        public Route Calculate(int populationSize = 100, int generationsCount = 5)
        {
            currentPopulation = CreateInitialPopulation(selectedCities, populationSize);
            for (int i = 0; i < generationsCount; i++)
            {

                Population nextPopulation = new Population();
                List<double> scores = getFitnessScores(currentPopulation);
                Route bestRoute = getBestRoute(currentPopulation, scores);
                nextPopulation.Add(bestRoute);
                for (int j = 0; j < populationSize / 2 - 1; j++)
                {
                    var parents = SelectParents(currentPopulation, scores);
                    Route child1 = Crossover(parents.Item1, parents.Item2);
                    Route child2 = Crossover(parents.Item2, parents.Item1);

                    child1.Mutate(random);
                    child2.Mutate(random);

                    nextPopulation.Add(child1);
                    nextPopulation.Add(child2);
                }
                currentPopulation = nextPopulation;
            }

            List<double> finalFitnessScores = getFitnessScores(currentPopulation);
            int bestRouteIndex = finalFitnessScores.IndexOf(finalFitnessScores.Max());
            Route Route = currentPopulation[bestRouteIndex];
            double length = Route.CalculateDistance(DistanceMatrix);
            return Route;
        }
        
        public void StartAlgorithm(int populationSize = 100)
        {
            this.populationSize = populationSize;
            currentPopulation = CreateInitialPopulation(selectedCities, populationSize);
        }
        public Route NextIteration()
        {
            Population nextPopulation = new Population();
            List<double> scores = getFitnessScores(currentPopulation);
            Route bestRoute = getBestRoute(currentPopulation, scores);
            nextPopulation.Add(bestRoute);
            for (int j = 0; j < populationSize / 2 - 1; j++)
            {
                var parents = SelectParents(currentPopulation, scores);
                Route child1 = Crossover(parents.Item1, parents.Item2);
                Route child2 = Crossover(parents.Item2, parents.Item1);

                child1.Mutate(random);
                child2.Mutate(random);

                nextPopulation.Add(child1);
                nextPopulation.Add(child2);
            }
            currentPopulation = nextPopulation;
            return getBestRoute(currentPopulation, getFitnessScores(currentPopulation));
        }

        #region Parallelelism

        public Route Calculate_Parallel(int populationSize = 100, int generationsCount = 5)
        {
            this.populationSize = populationSize;
            currentPopulation = CreateInitialPopulation(selectedCities, populationSize);
            for (int i = 0; i < generationsCount; i++)
            {
                Population nextPopulation = new Population();
                List<double> scores = getFitnessScores(currentPopulation);
                Route bestRoute = getBestRoute(currentPopulation, scores);
                nextPopulation.Add(bestRoute);
               Iteration_Parallel(currentPopulation, scores, nextPopulation);
                currentPopulation = nextPopulation;
            }

            List<double> finalFitnessScores = getFitnessScores(currentPopulation);
            int bestRouteIndex = finalFitnessScores.IndexOf(finalFitnessScores.Max());
            Route Route = currentPopulation[bestRouteIndex];
            double length = Route.CalculateDistance(DistanceMatrix);
            return Route;
        }
        private Population Iteration_Parallel(Population population, List<double> scores, Population resPopulation)
        {
            ConcurrentBag<Route> kids = new ConcurrentBag<Route>();
            Parallel.For(0, populationSize/2 - 1, index =>
            {
                var parents = SelectParents(population, scores);
                Route child1 = Crossover(parents.Item1, parents.Item2);
                Route child2 = Crossover(parents.Item2, parents.Item1);

                child1.Mutate(random);
                child2.Mutate(random);

                kids.Add(child1);
                kids.Add(child2);
            });

            foreach (Route r in kids) resPopulation.Add(r);
            currentPopulation = resPopulation;
            return resPopulation;
        }
        public Route NextIteration_Parallel()
        {
            Population nextPopulation = new Population();
            List<double> scores = getFitnessScores(currentPopulation);
            Route bestRoute = getBestRoute(currentPopulation, scores);
            //nextPopulation.Add(bestRoute);

            Iteration_Parallel(currentPopulation, scores, nextPopulation);
            nextPopulation.Add(bestRoute);
            currentPopulation = nextPopulation;
            return getBestRoute(currentPopulation, getFitnessScores(currentPopulation));
        }
        #endregion

        public Route getBestRoute(Population p)
        {
            List<double> scores = getFitnessScores(p);
            return getBestRoute(p, scores);
        }
        private Route getBestRoute(Population p, List<double> scores)
        {
            Route bestRoute = p[scores.IndexOf(scores.Min())];
            return bestRoute;
        }

        private List<double> getFitnessScores(Population population)
        {
            List<double> scores = new List<double>();
            foreach (Route r in population)
            {
                scores.Add( r.CalculateDistance(DistanceMatrix));
            }
            return scores;
        }
        public Tuple<Route, Route> SelectParents(Population population, List<double> fitnessScores)
        {
            double totalFitness = fitnessScores.Sum();
            List<double> selectionProbs = fitnessScores.Select(f => f / totalFitness).ToList();

            var parent1 = population[SelectByProbability(selectionProbs)];
            var parent2 = population[SelectByProbability(selectionProbs)];

            return Tuple.Create(parent1, parent2);
        }
        public int SelectByProbability(List<double> probs)
        {
            double r = random.NextDouble();
            double cumulative = 0.0;
            for (int i = 0; i < probs.Count; i++)
            {
                cumulative += probs[i];
                if (r < cumulative)
                    return i;
            }
            return probs.Count - 1;
        }


        public Route Crossover(Route parent1, Route parent2)
        {
            int cut = random.Next(1, parent1.Count - 1);
            var child = parent1.Take(cut);

            for (int i = 0; i < parent2.Count; i++)
            {
                if (!child.Contains(parent2[i]))
                {
                    child.Add(parent2[i]);
                }
            }
            return new Route(child); ;
        }


        private Population CreateInitialPopulation(List<int> basicCities, int populationSize)
        {
            List<Route> routes = new List<Route>();
            for (int i = 0; i < populationSize; i++)
            {
                routes.Add(createRandomRouteWithSelectedCities(basicCities, cityCount));
            }
            return new Population(routes);
        }
        private Route createRandomRouteWithSelectedCities(List<int> basicCities, int cityCount)
        {
            int unselectedCitiesCount = random.Next(cityCount - basicCities.Count + 1);
            int index = 0;
            List<int> cities = new List<int>(basicCities);
            while (unselectedCitiesCount > 0)
            {
                if (basicCities.Contains(index)) { index++; continue; }
                cities.Add(index);
                unselectedCitiesCount--;
                index++;
            }
            Route res = new Route(cities);
            int mutationsCount = random.Next(cities.Count);
            for (int i = 0; i < mutationsCount; i++) res.Mutate(random);
            return res;
        }


        public string  Save()
        {
            return JsonSerializer.Serialize(this);
        }
        public static RouteOptimizer Load(string json)
        {
            RouteOptimizer route = (RouteOptimizer)JsonSerializer.Deserialize<RouteOptimizer>(json);
            return route;
        }
    }
    public class Route
    {
        public List<int> cities
        {
        get; set; }
        public double Distance
        {
            get; set;
        }
        public int Count
        {
            get { return cities.Count; }
        }

        public Route(List<int> cities)
        {
            this.cities = cities;
        }
        public double CalculateDistance(List<List<double>> matrix)
        {
            double length = 0;
            for (int i = 0; i < cities.Count - 1; i++)
            {
                length += matrix[cities[i]][ cities[i + 1]];
            }
            length += matrix[cities[0]][ cities.Last()];
            Distance = length;
            return Distance;
        }

        public List<int> Take(int cut)
        {
            return cities.Take(cut).ToList();
        }

        public int this[int index]
        {
            get { return cities[index]; }
            set { cities[index] = value; }
        }
        public void Mutate(Random random)
        {
            int i = random.Next(cities.Count);
            int j = random.Next(cities.Count);
            SwapCities(i, j);
        }
        private void SwapCities(int i, int j)
        {
            int a = cities[i];
            cities[i] = cities[j];
            cities[j] = a;
        }

        public Route Copy()
        {
            Route copy = new Route(this.cities.ToList());
            copy.Distance = Distance;
            return copy;
        }

        public override string ToString()
        {
            string res = cities[0].ToString();
            for (int i = 1; i < cities.Count; i++)
            {
                res += "->" + cities[i].ToString();
            }
            return res;
        }
    }
    public class Population : ObservableCollection<Route>
    {
        public Population(List<Route> routes)
        {
            foreach (Route r in routes)
                this.Add(r);
        }
        public Population(Population p)
        {
            foreach(Route r in p)
            {
                this.Add(r);
            }
        }
        public Population(int size)
        {
            for(int i= 0;i < size; i++)
            {
                this.Add(null);
            }
        }
        public Population() { }
    }

    public class RouteEventArgs : EventArgs
    {
        public Route route
        {
            get; set;
        }
        public RouteEventArgs(Route route)
        {
            this.route = route;
        }
        

    }
}

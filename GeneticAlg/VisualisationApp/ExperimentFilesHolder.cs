using RouteOptimizerLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Runtime.CompilerServices;

namespace VisualisationApp
{
    internal class ExperimentFilesHolder
    {
        Dictionary<string, RouteOptimizer> _data;
       
        public static string basePath = "C:\\Dmitrii\\Programming\\MSU\\dataHolder";
        static ExperimentFilesHolder()
        {
            basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dataHolder");
            if (!Directory.Exists(basePath)) //createDirectory if not exists
            {
                Directory.CreateDirectory(basePath);
            }
            string runsPath = Path.Combine(basePath, "runs.json");
            if (!File.Exists(runsPath))
            {
                List<string> tmp = new List<string>();
                using (StreamWriter sw = new StreamWriter(runsPath))
                {
                    sw.WriteLine(JsonSerializer.Serialize(tmp));
                }
            }
        }
        public ExperimentFilesHolder()
        {
            _data = new Dictionary<string, RouteOptimizer>();
        }
        public void AddExperiment(string name, RouteOptimizer optimizer, string savingPath = "")
        {
            this._data.Add(name, optimizer);
            SaveFile(name, savingPath);
        }
        public void AddExperiment(RouteOptimizer optimizer, string savingPath)
        {
            string name = GetNameFromPath(savingPath);
            this._data.Add(name, optimizer);
            SaveFile(name, savingPath);
            SaveRunsFile();
        }
        public void SaveFile(string name, string savingPath)
        {
            string baseNamePath = basePath + $"\\{name}.json";
            if (savingPath == baseNamePath) return;
            using (StreamWriter sw = new StreamWriter(baseNamePath))
            {
                sw.WriteLine(_data[name].Save());
            }
        }
        public RouteOptimizer LoadFile(string name)
        {
            using (StreamReader sw = new StreamReader(basePath + $"\\{name}.json"))
            {
                string s = sw.ReadLine();
                RouteOptimizer optimizer = RouteOptimizer.Load(s);
                return optimizer;
            }
        }
        public void SaveRunsFile()
        {
            using (StreamWriter sw = new StreamWriter(basePath + "\\runs.json"))
            {
                sw.WriteLine(JsonSerializer.Serialize(_data.Keys.ToList()));
            }
        }
        public void Load()
        {
            this._data.Clear();
            try
            {
                List<string> fileNames = new List<string>();
                using (StreamReader sr = new StreamReader(basePath + "\\runs.json"))
                {
                    fileNames = JsonSerializer.Deserialize<List<string>>(sr.ReadLine()) as List<string>;
                }
                foreach (string fileName in fileNames)
                {
                    _data.Add(fileName, LoadFile(fileName));
                }
            }
            catch 
            {
                this._data = new Dictionary<string, RouteOptimizer>();
                SaveRunsFile();
            }
        }

        public bool Contains(string filePath)
        {
            string name = GetNameFromPath(filePath);
            return _data.ContainsKey(name);
        }
        private string GetNameFromPath(string filePath)
        {
            int index = filePath.LastIndexOf('\\');
            string name = filePath.Substring(index + 1, filePath.Length - index - 1);
            index = name.LastIndexOf(".");
            if(index != -1)name = name.Substring(0, index);
            return name;
        }

        public List<string> Names
        {
            get { return _data.Keys.ToList(); }
        }
        public RouteOptimizer getRouteOptimizer(string name)
        {
            try
            {
                return _data[name];
            }
            catch
            {
                return null;
            }
        }
    }
}

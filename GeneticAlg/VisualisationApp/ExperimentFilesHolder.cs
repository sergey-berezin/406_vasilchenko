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
       
        public static string basePath = "";
        public ExperimentFilesHolder()
        {
            _data = new Dictionary<string, RouteOptimizer>();
            CheckBaseDirectory();
        }
        public void AddExperiment(string name, RouteOptimizer optimizer, string savingPath = "")
        {
            this._data.Add(name, optimizer);
            SaveFile(name, savingPath);
        }
        public void AddExperiment(RouteOptimizer optimizer, string savingPath)
        {
            string name = Path.GetFileNameWithoutExtension(savingPath);
            this._data.Add(name, optimizer);
            SaveFile(name, savingPath);
            SaveRunsFile();
        }
        public void SaveFile(string name, string savingPath)
        {
            CheckBaseDirectory();
            string baseNamePath = Path.Combine(basePath, $"{name}.json");
            if (savingPath == baseNamePath) return;
            using (StreamWriter sw = new StreamWriter(baseNamePath))
            {
                sw.WriteLine(_data[name].Save());
            }
        }
        public RouteOptimizer LoadFile(string name)
        {
            if (CheckBaseDirectory()) return null;
            using (StreamReader sw = new StreamReader(Path.Combine(basePath, $"{name}.json")))
            {
                string s = sw.ReadLine();
                RouteOptimizer optimizer = RouteOptimizer.Load(s);
                return optimizer;
            }
        }
        public void SaveRunsFile()
        {
            CheckBaseDirectory();
            using (StreamWriter sw = new StreamWriter(Path.Combine(basePath, "runs.json")))
            {
                sw.WriteLine(JsonSerializer.Serialize(_data.Keys.ToList()));
            }
        }
        public void Load()
        {
            if (CheckBaseDirectory()) return;
            this._data.Clear();
            try
            {
                List<string> fileNames = new List<string>();
                using (StreamReader sr = new StreamReader(Path.Combine(basePath, "runs.json")))
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
        public bool Contains(string filePath)
        {
            string name = Path.GetFileNameWithoutExtension(filePath);
            return _data.ContainsKey(name);
        }

        private bool CheckBaseDirectory()
        {
            bool isChanged = false;
            basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dataHolder");
            if (!Directory.Exists(basePath)) //createDirectory if not exists
            {
                Directory.CreateDirectory(basePath);
                isChanged = true;
            }
            string runsPath = Path.Combine(basePath, "runs.json");
            if (!File.Exists(runsPath))
            {
                List<string> tmp = new List<string>();
                using (StreamWriter sw = new StreamWriter(runsPath))
                {
                    sw.WriteLine(JsonSerializer.Serialize(tmp));
                }
                isChanged = true;
            }
            return isChanged;
        }
    }
}

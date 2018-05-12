using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Autofac;


namespace SingletonPatternWithDependencyInjection
{

    public interface IDatabase
    {
        int GetPopulation(string name);
    }

    public class SingletonDatabase : IDatabase
    {
        private Dictionary<string, int> _citiesPopulation;
        private static int _instanceCount;
        public static int Count => _instanceCount;

        private SingletonDatabase()
        {
            Console.WriteLine("Initializing database");

            _citiesPopulation = new Dictionary<string, int>(); ;

            var fullPath = new FileInfo(typeof(IDatabase).Assembly.Location).DirectoryName;

            if (fullPath == null) return;

            var citiesPopulationStrings = File.ReadAllLines(
                Path.Combine(fullPath
                    , "Cities.txt")
            );

            for (var i = 0; i < citiesPopulationStrings.Length; i += 2)
                _citiesPopulation.Add(citiesPopulationStrings[i], Convert.ToInt32(citiesPopulationStrings[i + 1]));
        }

        public int GetPopulation(string name)
        {
            return _citiesPopulation[name];
        }

        private static Lazy<SingletonDatabase> _instance = new Lazy<SingletonDatabase>(() =>
        {
            _instanceCount++;
            return new SingletonDatabase();
        });

        public static IDatabase Instance => _instance.Value;
    }

    public class OrdinaryDatabase : IDatabase
    {
        private Dictionary<string, int> _citiesPopulation;

        public OrdinaryDatabase()
        {
            Console.WriteLine("Initializing database");

            Console.WriteLine("Initializing database");

            _citiesPopulation = new Dictionary<string, int>(); ;

            var fullPath = new FileInfo(typeof(IDatabase).Assembly.Location).DirectoryName;

            if (fullPath == null) return;

            var citiesPopulationStrings = File.ReadAllLines(
                Path.Combine(fullPath
                    , "Cities.txt")
            );

            for (var i = 0; i < citiesPopulationStrings.Length; i += 2)
                _citiesPopulation.Add(citiesPopulationStrings[i], Convert.ToInt32(citiesPopulationStrings[i + 1]));

        }

        public int GetPopulation(string name)
        {
            return _citiesPopulation[name];
        }
    }

    public class SingletonRecordFinder
    {
        public int TotalPopulation(IEnumerable<string> names)
        {
            return names.Sum(name => SingletonDatabase.Instance.GetPopulation(name));
        }
    }

    public class ConfigurableRecordFinder
    {
        private readonly IDatabase _database;

        public ConfigurableRecordFinder(IDatabase database)
        {
            this._database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public int GetTotalPopulation(IEnumerable<string> names)
        {
            return names.Sum(name => _database.GetPopulation(name));
        }
    }

    public class DummyDatabase : IDatabase
    {
        public int GetPopulation(string name)
        {
            return new Dictionary<string, int>
            {
                ["Buenos Aires"] = 1,
                ["Montevideo"] = 2,
                ["Lima"] = 3
            }[name];
        }
    }


    [TestFixture]
    public class SingletonTests
    {
        [Test]
        public void IsSingletonTest()
        {
            var db = SingletonDatabase.Instance;
            var db2 = SingletonDatabase.Instance;
            Assert.That(db, Is.SameAs(db2));
            Assert.That(SingletonDatabase.Count, Is.EqualTo(1));
        }

        [Test]
        public void SingletonTotalPopulationTest()
        {
            var rf = new SingletonRecordFinder();
            var names = new[] { "Buenos Aires", "Lima" };
            var tp = rf.TotalPopulation(names);
            Assert.That(tp, Is.EqualTo(4000000 + 3000000));
        }

        [Test]
        public void ConfigurableTotalPopulationTest()
        {
            var rf = new ConfigurableRecordFinder(new DummyDatabase());

            var names = new[] { "Buenos Aires", "Montevideo" };

            int tp = rf.GetTotalPopulation(names);

            Assert.That(tp, Is.EqualTo(3));
        }

        [Test]
        public void DependencyInjectionTest()
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterType<DummyDatabase>().As<IDatabase>().SingleInstance();

            containerBuilder.RegisterType<ConfigurableRecordFinder>();

            using (var containerBuilded = containerBuilder.Build())
            {
                var rf = containerBuilded.Resolve<ConfigurableRecordFinder>();
                
            }
            
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}

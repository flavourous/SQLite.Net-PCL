using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SQLite.Net.Attributes;

namespace SQLite.Net.Tests
{
    [TestFixture]
    internal class AdHocColumnTest
    {

        class Testy
        {
            [PrimaryKey, AutoIncrement]
            public int id { get; set; }

            public String lol { get; set; }
            public bool hmm { get; set; }


            Dictionary<String, Object> backing = new Dictionary<String, Object>();

            [ColumnAccessor, Ignore]
            public IDictionary<String, Object> columns { get { return backing; } }
        }

        class Testy
        {
            [PrimaryKey, AutoIncrement]
            public int id { get; set; }

            public String lol { get; set; }
            public bool hmm { get; set; }


            Dictionary<String, Object> backing = new Dictionary<String, Object>();

            [ColumnAccessor, Ignore]
            public IDictionary<String, Object> columns { get { return backing; } }
        }


        public class TestDb : SQLiteConnection
        {
            public List<Testy> wasInserted = new List<Testy>();
            public TestDb(String path)
                : base(new SQLitePlatformTest(), path)
            {
                Action<bool, String, String, int> ins = (h, lol, tc1, tc2) =>
                   {

                       var ins = new Testy { hmm = h, lol = lol };

                       ins.columns["tc1"] = tc1;
                       ins.columns["tc2"] = tc2;

                       wasInserted.Add(ins);

                       conn.Insert(ins, map);
                   };
                CreateTable<TestObjString>();

                var map = GetMapping(conn);

                ins(map, true, "lols", "lol1", 1);
                ins(map, true, "foos", "foo1", 1);
                ins(map, false, "bars", "bar1", 2);
                ins(map, true, "lols", "lol1", 3);
                ins(map, true, "lols", "lol1", 3);
            }
        }

        TestDb SetupFixtureDB()
        {
            return new TestDb(TestPath.CreateTemporaryDatabase());
        }

        #region Setup Helpers
        class ah { public String cn; public Type ct; public Object dv; }

        TableMapping GetMap(SQLiteConnection conn, String tn, String holdername, params ah[] args)
        {
            var cpi = typeof(Testy).GetProperty(holdername);

            return conn.GetMapping<Testy>().WithMutatedSchema(tn,
                from t in args select new TableMapping.AdHocColumn(t.cn, t.ct, cpi, t.dv)
                );
        }

        TableMapping GetMapping(SQLiteConnection conn)
        {
            return GetMap(conn, "Test", "columns", new[]
            {
                new ah { cn = "tc1", ct = typeof(String), dv = "defval" },
                new ah { cn = "tc2", ct = typeof(int), dv = 2 },
            });
        }

        #endregion

        [Test]
        public void CheckValues()
        {
            var conn = SetupFixtureDB();
            Assert.AreEqual(Enumerable.SequenceEqual(
                conn.wasInserted.Sort((a, b) => a.id > b.id),
                conn.Table<Testy>(map).OrderBy(d => d.id)
                ), true);
            
        }

        [Test]
        public void QueryTest()
        {
            var conn = SetupFixtureDB();
            var map = GetMapping();
            var tq = conn.Table<Testy>(map);
            Assert.AreEqual(tq.Where(t => (int)t.columns["tc2"] == 3).Count(), 2);
        }

        [Test]
        public void DeleteTest()
        {
            var conn = SetupFixtureDB();
            var map = GetMapping();
            var tq = conn.Table<Testy>(map);
            tq.Delete(d => true);
            Assert.AreEqual(tq.Count(), 0);
        }
    }
}
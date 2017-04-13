using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using SQLite.Net.Attributes;

namespace SQLite.Net.Tests
{
    [TestFixture]
    internal class AdHocColumnTest
    {

        public class Testy : IEquatable<Testy>
        {
            [PrimaryKey, AutoIncrement]
            public int id { get; set; }

            public String lol { get; set; }
            public bool hmm { get; set; }


            Dictionary<String, Object> backing = new Dictionary<String, Object>();
            [ColumnAccessor, Attributes.Ignore] // allow on all for simples.
            public Object this[String key] { get { return backing[key]; } set { backing[key] = value; } }

            public bool Equals(Testy other)
            {
                var sc = other.id == id && other.lol == lol && other.hmm == hmm;
                var dc = other.backing.All(d => backing.ContainsKey(d.Key) && d.Value.Equals(backing[d.Key]));
                return sc && dc;
            }
        }
        
        public class TestDb : SQLiteConnection
        {
            public TableMapping mapy;
            public List<Testy> wasInserted = new List<Testy>();
            public TestDb(String path)
                : base(new SQLitePlatformTest(), path)
            {
               
            }

        }

        TestDb SetupFixtureDB()
        {
            var conn = new TestDb(TestPath.CreateTemporaryDatabase());

            Action<TableMapping, bool, String, String, int> insrt = (mapy, h, lol, tc1, tc2) =>
            {

                var ins = new Testy { hmm = h, lol = lol };

                ins["tc1"] = tc1;
                ins["tc2"] = tc2;

                conn.wasInserted.Add(ins);

                conn.Insert(ins, mapy);
            };

            var map = conn.mapy = GetMapping(conn);
            conn.CreateTable(map);

            insrt(map, true, "lols", "lol1", 1);
            insrt(map, true, "foos", "foo1", 1);
            insrt(map, false, "bars", "bar1", 2);
            insrt(map, true, "lols", "lol1", 3);
            insrt(map, true, "lols", "lol1", 3);

            return conn;
        }

        #region Setup Helpers
        class ah { public String cn; public Type ct; public Object dv; }

        TableMapping GetMap(SQLiteConnection conn, String tn, String holdername, params ah[] args)
        {
            var t = typeof(Testy);
            var rp = t.GetProperties().AsEnumerable();
            var cpi = rp.Where(d => d.GetCustomAttributes(false).Any(x => x is ColumnAccessorAttribute)).First();

            String[] c = args.Select(g=>g.cn).ToArray(); Type[] ct = args.Select(g=>g.ct).ToArray();

            // make the poisoned map
            return new TableMapping(t,
                rp.Select(d=>new TableMapping.Column.TypeInfoPropAdapter(d))
                  .Cast<ITypeInfo>()
                  .Concat(Enumerable.Range(0, c.Length)
                    .Select(i =>
                    {
                        var hh = new HolderHelper(cpi, c[i]);
                        return new FakedProperty(c[i], ct[i], t, hh.GetVal, hh.SetVal);
                    })
                   ),
                tn
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
            var wi = conn.wasInserted.OrderBy(d => d.id);
            var et = conn.Table<Testy>(conn.mapy).OrderBy(d => d.id);
            foreach (var pr in wi.Zip(et, (w, e) => new { a = w, b = e }))
                Assert.AreEqual(pr.a, pr.b);
        }

        [Test]
        public void QueryTest()
        {
            var conn = SetupFixtureDB();
            var tq = conn.Table<Testy>(conn.mapy);
            Assert.AreEqual(tq.Where(t => (int)t["tc2"] == 3).Count(), 2);
        }

        [Test]
        public void DeleteTest()
        {
            var conn = SetupFixtureDB();
            var tq = conn.Table<Testy>(conn.mapy);
            tq.Delete(d => true);
            Assert.AreEqual(tq.Count(), 0);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Telerik.JustMock.EntityFramework.Tests
{
	[TestClass]
	public class DbSetTests
	{
		public class Person
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}

		public class Employee : Person
		{
			public int Salary { get; set; }
		}

		public class Stock
		{
			public int StockId { get; set; }
			public string Ticker { get; set; }
		}

		public class Plan
		{
			public int EntityId { get; set; }
			public string Name { get; set; }
		}

		public class Investment
		{
			public int StockId { get; set; }
			public int PersonId { get; set; }
			public int Count { get; set; }
		}

		public class TheDbContext : DbContext
		{
			public DbSet<Person> People { get; set; }
			public DbSet<Stock> Stocks { get; set; }
			public DbSet<Plan> Plans { get; set; }
			public DbSet<Investment> Investments { get; set; }
		}

		[TestMethod]
		public async Task Bind_BindData_DataIsThere()
		{
			var ctx = Mock.Create<TheDbContext>().PrepareMock();

			var list = new List<Person>
			{
				new Person { Id = 1, Name = "a" }
			};

			ctx.People.Bind(list);


			var data = await ctx.People.ToListAsync();
			Assert.AreSame(list[0], data[0]);
		}

		[TestMethod]
		public async Task Bind_NoBind_EmptyBackingCollection()
		{
			var ctx = Mock.Create<TheDbContext>().PrepareMock();
			var people = await ctx.People.ToListAsync();
			Assert.AreEqual(0, people.Count);
		}

		[TestMethod]
		public async Task Add_EntityIsAddedToImplicitBackingCollection()
		{
			var ctx = Mock.Create<TheDbContext>().PrepareMock();

			ctx.People.Add(new Person { Name = "b" });
			var list = await ctx.People.ToListAsync();

			Assert.AreEqual(1, list.Count);
			Assert.AreEqual("b", list[0].Name);
		}

		[TestMethod]
		public void Add_EntityIsAddedToBoundCollection()
		{
			var ctx = Mock.Create<TheDbContext>().PrepareMock();

			var list = new List<Person>
			{
				new Person { Id = 1, Name = "a" }
			};
			ctx.People.Bind(list);

			ctx.People.Add(new Person { Name = "b" });

			Assert.AreEqual(2, list.Count);
			Assert.AreEqual("b", list[1].Name);
		}

		[TestMethod]
		public async Task AddRange_EntitiesAreAddedToBoundCollection()
		{
			var ctx = Mock.Create<TheDbContext>().PrepareMock();

			var list = new List<Person>();
			ctx.People.Bind(list);

			var newData = new[]
				{
					new Person { Name = "a" },
					new Person { Name = "b" },
					new Person { Name = "c" },
				};

			ctx.People.AddRange(newData);

			CollectionAssert.AreEqual(newData, list);
			CollectionAssert.AreEqual(newData, await ctx.People.ToListAsync());
		}

		[TestMethod]
		public void Attach_EntityIsAddedToBoundCollection()
		{
			var ctx = Mock.Create<TheDbContext>().PrepareMock();

			var list = new List<Person>();
			ctx.People.Bind(list);

			ctx.People.Attach(new Person { Name = "b" });

			Assert.AreEqual(1, list.Count);
			Assert.AreEqual("b", list[0].Name);
		}

		[TestMethod]
		public void Create_EntityIsCreated()
		{
			var ctx = Mock.Create<TheDbContext>().PrepareMock();
			var inst = ctx.People.Create();
			Assert.AreSame(typeof(Person), inst.GetType());
		}

		[TestMethod]
		public void CreateDerived_EntityIsCreated()
		{
			var ctx = Mock.Create<TheDbContext>().PrepareMock();
			var inst = ctx.People.Create<Employee>();
			Assert.AreSame(typeof(Employee), inst.GetType());
		}

		[TestMethod]
		public void Find_EntityWithId_CreateAutomaticGetIdFunction()
		{
			var ctx = Mock.Create<TheDbContext>().PrepareMock();

			var list = new List<Person>
			{
				new Person { Id = 1, Name = "a" }
			};
			ctx.People.Bind(list);

			var person = ctx.People.Find(1);
			Assert.IsNotNull(person);
			Assert.AreEqual("a", person.Name);
		}

		[TestMethod]
		public void Find_EntityWithClassNameInId_CreateAutomaticGetIdFunction()
		{
			var ctx = Mock.Create<TheDbContext>().PrepareMock();

			var list = new List<Stock>
			{
				new Stock { StockId = 1, Ticker = "a" }
			};
			ctx.Stocks.Bind(list);

			var stock = ctx.Stocks.Find(1);
			Assert.IsNotNull(stock);
			Assert.AreEqual("a", stock.Ticker);

			Assert.IsNull(ctx.Stocks.Find(123));
		}

		[TestMethod]
		public void Find_EntityWithUnrecognizedIdProperty_RequireIdFunction()
		{
			var ctx = Mock.Create<TheDbContext>().PrepareMock();

			var list = new List<Plan>
			{
				new Plan { EntityId = 1, Name = "a" }
			};
			ctx.Plans.Bind(list);

			AssertEx.Throws<InvalidOperationException>(() => ctx.Plans.Find(1));

			ctx.Plans.SetIdFunction(p => p.EntityId);

			var plan = ctx.Plans.Find(1);
			Assert.IsNotNull(plan);
			Assert.AreEqual("a", plan.Name);
		}

		[TestMethod]
		public void Find_EntityWithCompositeKey_RequireIdFunction()
		{
			var ctx = Mock.Create<TheDbContext>().PrepareMock();

			var list = new List<Investment>
			{
				new Investment { StockId = 1, PersonId = 2, Count = 10 }
			};
			ctx.Investments.Bind(list);

			ctx.Investments.SetIdFunction(p => new[] { p.StockId, p.PersonId });

			var investment = ctx.Investments.Find(1, 2);
			Assert.IsNotNull(investment);
			Assert.AreEqual(10, investment.Count);
		}

		[TestMethod]
		public void Remove_EntityIsRemoved()
		{
			var ctx = Mock.Create<TheDbContext>().PrepareMock();
			var list = new List<Person>
			{
				new Person { Name = "x" }
			};
			ctx.People.Bind(list);
			ctx.People.Remove(list[0]);
			Assert.AreEqual(0, list.Count);
		}

		[TestMethod]
		public void RemoveRange_EntitiesAreRemoved()
		{
			var ctx = Mock.Create<TheDbContext>().PrepareMock();
			var list = new List<Person>
			{
				new Person { Id = 1, Name = "x" },
				new Person { Id = 2, Name = "x" },
				new Person { Id = 3, Name = "x" }
			};
			ctx.People.Bind(list);
			ctx.People.RemoveRange(list.Where(p => p.Id < 3).ToList());

			Assert.AreEqual(1, list.Count);
			Assert.AreEqual(3, list[0].Id);
		}

		[TestMethod]
		public void Local_ImplicitBackingCollection_ChangesAreObserved()
		{
			var ctx = Mock.Create<TheDbContext>().PrepareMock();

			bool notified = false;
			ctx.People.Local.CollectionChanged += (o, e) => notified = true;

			ctx.People.Add(new Person());

			Assert.IsTrue(notified);
		}

		[TestMethod]
		public void Local_ExplicitBackingCollection_ChangesAreObserved()
		{
			var ctx = Mock.Create<TheDbContext>().PrepareMock();
			var list = new ObservableCollection<Person>
			{
				new Person { Id = 1, Name = "x" },
				new Person { Id = 2, Name = "x" },
				new Person { Id = 3, Name = "x" }
			};
			ctx.People.Bind(list);

			bool notified = false;
			ctx.People.Local.CollectionChanged += (o, e) => notified = true;

			ctx.People.Add(new Person());

			Assert.IsTrue(notified);
			Assert.AreEqual(4, list.Count);
		}

		[TestMethod]
		public async Task Queries_DataIsThere()
		{
			var ctx = Mock.Create<TheDbContext>().PrepareMock();
			var list = new List<Person>
			{
				new Person { Id = 1, Name = "x" },
				new Person { Id = 2, Name = "x" },
				new Person { Id = 3, Name = "y" }
			};
			ctx.People.Bind(list);
			var xes = await ctx.People.Where(p => p.Name == "x").ToListAsync();

			Assert.AreEqual(2, xes.Count);
		}
	}
}

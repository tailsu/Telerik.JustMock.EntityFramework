Telerik.JustMock.EntityFramework
====================

In-memory mock DbSet and DbContext mocking amenities for JustMock.

Getting Started
===============
Install the package from NuGet: https://www.nuget.org/packages/JustMock.EntityFramework

Mocking DbContext
=================
Here is an example straight from the tests:

	[TestClass]
	public class DbContextTests
	{
		public class Person
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}

		public class DbContextOne : DbContext
		{
			public DbSet<Person> People { get; set; }
		}

		[TestMethod]
		public void Prepare_ShouldAssignDbSetProperties()
		{
			var dbContext = EntityFrameworkMock.Create<DbContextOne>();
			Assert.IsNotNull(dbContext.People);
		}
	}
    
In the above example I create a mock of my DbContext-deriving class. As a result, all fields that are of type `DbSet<T>` or `IDbSet<T>` are automatically populated with a mock in-memory DbSet (initially empty).

We can also create a mock from a DbContext-like interface, like in the example below:

	public interface IMyDbContext
	{
		IDbSet<Person> People { get; }
	}

	[TestMethod]
	public void Prepare_Interface_DbSetsInitialized()
	{
		var dbContext = EntityFrameworkMock.Create<IMyDbContext>();
		Assert.IsNotNull(dbContext.People);
	}
    
Populating data
===============
Populating the context with data lets you simulate a database that has data in it. Here's how we can populate our context:

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

The `Bind()` extension method binds a `DbSet` or `IDbSet` on the mock context to a backing collection, which can be anything, but it's best to use a `ObservableCollection<T>` (`Local` works only when the backing collection is itself observable).

Changes made to the DbSet are passed to the backing collection and vice versa.

Further reading
===============
Check out the unit tests in Telerik.JustMock.EntityFramework.Tests - they double as example usage.

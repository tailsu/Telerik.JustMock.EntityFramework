using System.Data.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Telerik.JustMock.EntityFramework.Tests
{
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
			var dbContext = Mock.Create<DbContextOne>().PrepareMock();
			Assert.IsNotNull(dbContext.People);
		}

		public class DbContextTwo : DbContext
		{
			public IDbSet<Person> People { get; set; }
		}

		[TestMethod]
		public void Prepare_ShouldAssignIDbSetProperties()
		{
			var dbContext = Mock.Create<DbContextTwo>().PrepareMock();
			Assert.IsNotNull(dbContext.People);
		}

		[TestMethod]
		public void DbSet_Arrangeable()
		{
			var dbContext = Mock.Create<DbContextOne>().PrepareMock();
			var expected = new Person();
			Mock.Arrange(() => dbContext.People.Create()).Returns(expected);

			var actual = dbContext.People.Create();
			Assert.AreSame(expected, actual);
		}

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
	}
}

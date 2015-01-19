using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Telerik.JustMock.EntityFramework
{
	/// <summary>
	/// The entry point for creating mock DbContexts for testing.
	/// </summary>
	public static class EntityFrameworkMock
	{
		/// <summary>
		/// Creates a mock DbContext and initializes the DbSet and IDbSet properties on the instance to mock DbSets.
		/// </summary>
		/// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
		/// <returns>The mock DbContext.</returns>
		public static TDbContext Create<TDbContext>()
		{
			return PrepareMock(Mock.Create<TDbContext>());
		}

		/// <summary>
		/// Initializes the DbSet and IDbSet properties on the given instance to mock DbSets.
		/// If the DbContext is an interface, the passed instance must have been created using Mock.Create.
		/// </summary>
		/// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
		/// <param name="dbContext">The mock DbContext.</param>
		/// <returns>The mock DbContext.</returns>
		public static TDbContext PrepareMock<TDbContext>(TDbContext dbContext)
		{
			var type = dbContext.GetType();
			var propQ = from prop in type.GetProperties()
						let dbSetType = prop.PropertyType.ImplementsGenericInterface(typeof(IDbSet<>))
						where dbSetType != null
						let elementType = dbSetType.GetGenericArguments()[0]
						let mockDbSetType = typeof(MockDbSet<>).MakeGenericType(elementType)
						select new
						{
							Property = prop,
							ElementType = elementType,
							Value = Mock.Create(mockDbSetType, Behavior.CallOriginal),
						};
			var props = propQ.ToArray();

			foreach (var prop in props)
			{
				if (prop.Property.GetSetMethod() != null)
				{
					prop.Property.SetValue(dbContext, prop.Value);
				}
				else
				{
					var propLambda = (Expression<Func<object>>)Expression.Lambda(
						Expression.Convert(
							Expression.MakeMemberAccess(Expression.Constant(dbContext), prop.Property),
							typeof(object)));
					Mock.Arrange(propLambda).Returns(prop.Value);
				}
			}

			if (dbContext is DbContext)
			{
				foreach (var prop in props)
				{
					var genericSet = typeof(DbContext).GetMethod("Set", new Type[0]).MakeGenericMethod(prop.ElementType);
					var setLambda = (Expression<Func<object>>)Expression.Lambda(
						Expression.Convert(
							Expression.Call(Expression.Constant(dbContext), genericSet),
							typeof(object)));
					Mock.Arrange(setLambda).Returns(prop.Value);
				}
			}

			return dbContext;
		}
	}
}

using System;
using System.Linq;

namespace Telerik.JustMock.EntityFramework
{
	internal static class Extensions
	{
		public static Type ImplementsGenericInterface(this Type type, Type genericInterfaceType)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == genericInterfaceType)
			{
				return type;
			}

			return type.GetInterfaces().FirstOrDefault(intf =>
				intf.IsGenericType
				&& intf.GetGenericTypeDefinition() == genericInterfaceType);
		}
	}
}

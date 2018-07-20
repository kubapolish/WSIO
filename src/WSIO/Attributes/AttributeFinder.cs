using System;

namespace WSIO.Attributes {

	public static class AttributeFinder {

		public static bool GetAttribute<T>(this Type obj, out T attribute) {
			attribute = default(T);
			var attributesOfTypeT = obj.GetCustomAttributes(typeof(T), true);

			if (attributesOfTypeT.Length < 1) return false;
			else attribute = (T)attributesOfTypeT[0];

			return true;
		}
	}
}
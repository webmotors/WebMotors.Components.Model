using Newtonsoft.Json;
namespace System
{
	public static class JsonExtensions
	{
		public static string ToJson(this object obj)
		{
			return JsonConvert.SerializeObject(obj);
		}

		public static T FromJson<T>(this string obj)
		{
			if (obj == null)
				return default(T);

			return JsonConvert.DeserializeObject<T>(obj);
		}
	}
}
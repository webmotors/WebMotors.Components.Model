using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebMotors.Components.Model
{
	public class PagingInfo<T>
	{
		public PagingInfo(List<T> items, int pageCount, int totalRecords)
		{
			Items = items;
			PageCount = pageCount;
			TotalRecords = totalRecords;
		}
		public List<T> Items { get; private set; }
		public int PageCount { get; private set; }
		public int TotalRecords { get; private set; }
	}
}

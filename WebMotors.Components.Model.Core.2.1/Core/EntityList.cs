using System.Collections.Generic;

namespace WebMotors.Components.Model.Core
{
	public class EntityList<T> : IEnumerable<T>, IListEntity
	{
		internal bool fillAutomatic = false;

		public EntityList()
		{
			MyList = new List<ItemEntity<T>>();
		}

		internal List<ItemEntity<T>> MyList;

		public T this[int index]
		{
			get { return MyList[index].Item; }
			set { MyList.Insert(index, new ItemEntity<T> { Item = value, Modified = true }); }
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (var i = 0; i < MyList.Count; i++)
			{
				yield return MyList[i].Item;
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public IListEntity Add(object item)
		{
			MyList.Add(new ItemEntity<T> { Item = (T)item, Modified = !fillAutomatic });
			return this;
		}

		public void Set()
		{
			fillAutomatic = true;
		}

		public void EndFill()
		{
			fillAutomatic = false;
		}
	}

	public interface IListEntity
	{
		IListEntity Add(object item);
		void Set();
		void EndFill();
	}

	internal class ItemEntity<T>
	{
		public ItemEntity()
		{

		}
		public T Item { get; set; }
		public bool Modified { get; set; }
	}
}

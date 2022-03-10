using System.Collections.Generic;
using Sandbox;

namespace CitySim
{

	public interface ITickable
	{
		public TickableCollection ParentCollection { get; set; }
		public void OnClientTick( float delta, float currentTick );
		public void OnServerTick( float delta, float currentTick );
		public void OnSharedTick( float delta, float currentTick );
	}
	public partial class TickableCollection
	{
		public static TickableCollection Global = new TickableCollection();
		public List<ITickable> Items { get; set; } = new List<ITickable>();


		public void Add(ITickable item)
		{						
			item.ParentCollection = this;
			Items.Add( item );
		}

		public void Remove(ITickable item )
		{
			if ( item != null )
			{
				Items.Remove( item );
				item.ParentCollection = null;
			}
		}

		public void ClientTick()
		{
			if ( Items != null )
			{
				var itemsToTick = new List<ITickable>( Items );
				foreach ( var item in itemsToTick )
				{
					if ( item != null )
					{
						item.OnClientTick( Time.Delta, Time.Tick );
					}
				}
			}
		}

		public void ServerTick()
		{
			if ( Items != null )
			{
				var itemsToTick = new List<ITickable>( Items );
				foreach (var item in itemsToTick )
				{
					if ( item != null )
					{
						item.OnServerTick( Time.Delta, Time.Tick );
					}
				}
			}
		}

		public void SharedTick()
		{
			if ( Items != null )
			{
				var itemsToTick = new List<ITickable>( Items );
				foreach ( var item in itemsToTick )
				{
					if ( item != null )
					{
						item.OnSharedTick( Time.Delta, Time.Tick );
					}
				}
			}
		}
	}
}

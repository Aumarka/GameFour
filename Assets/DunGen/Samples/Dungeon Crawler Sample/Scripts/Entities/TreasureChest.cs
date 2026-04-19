using DunGen.LockAndKey;
using DunGen.Pooling;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace DunGen.DungeonCrawler
{
	/// <summary>
	/// An openable treasure chest containing collectable objects
	/// </summary>
	sealed class TreasureChest : MonoBehaviour, IClickableObject, IKeySpawner, ITileSpawnEventReceiver
	{
		[SerializeField]
		[Tooltip("The cursor to use when the player hovers over it with the mouse")]
		private Texture2D hoverCursor = null;

		[SerializeField]
		[Tooltip("How close the player has to be to open the chest. If further away, the player will run to the chest first")]
		private float openDistance = 5f;

		[SerializeField]
		[Tooltip("Timeline for playing the open animation")]
		private PlayableDirector openTimeline = null;

		[SerializeField]
		[Tooltip("The component responsible for spawning loot from the chest")]
		private LootSpawner lootSpawner = null;

		[SerializeField]
		[Tooltip("The number of coins to spawn from this chest")]
		private int coinCount = 10;

		[SerializeField]
		[Tooltip("Prefab for spawning gold")]
		private GoldCollectible goldPrefab = null;

		[SerializeField]
		[Tooltip("Prefab for spawning a key")]
		private KeyCollectible keyPrefab = null;

		[SerializeField]
		[Tooltip("An image that's rendered to the minimap")]
		private GameObject minimapIcon = null;

		/// <summary>
		/// Has this chest already been opened?
		/// </summary>
		private bool opened;

		/// <summary>
		/// Does this treasure chest contain a key?
		/// </summary>
		private readonly List<int> containedKeyIDs = new List<int>();

		private readonly List<GameObject> spawnedLoot = new List<GameObject>();


		public bool CanInteract()
		{
			// Only allow the player to use the chest if it hasn't already been opened
			return !opened;
		}

		public Texture2D GetHoverCursor()
		{
			return hoverCursor;
		}

		public void Interact()
		{
			var player = UnityUtil.FindObjectByType<ClickToMove>();
			float distanceToPlayer = (transform.position - player.transform.position).magnitude;

			// If we're in range to open the chest, just open it...
			if (distanceToPlayer <= openDistance)
				Open();
			// ...otherwise, have the player run to the chest instead
			else
				PathTo(player);
		}

		/// <summary>
		/// Have the player move to the chest's location
		/// </summary>
		/// <param name="player">The player's ClickToMove component</param>
		private void PathTo(ClickToMove player)
		{
			// Tell the player to move to a point in front of the chest
			Vector3 destination = transform.position + transform.forward * 0.5f;

			player.StopManualMovement();
			player.MoveTo(destination, true);
		}

		public void Open()
		{
			if (opened)
				return;

			opened = true;

			if (minimapIcon != null)
				minimapIcon.SetActive(false);

			openTimeline.Play();

			// Create loot list
			var lootList = new List<GameObject>();

			// Spawn coins
			for (int i = 0; i < coinCount; i++)
			{
				var coin = Instantiate(goldPrefab);
				lootList.Add(coin.gameObject);
			}

			// Spawn keys
			foreach (var keyID in containedKeyIDs)
			{
				var key = Instantiate(keyPrefab);
				key.SetKeyID(keyID);

				lootList.Add(key.gameObject);
			}

			lootSpawner.Release(lootList);
			spawnedLoot.AddRange(lootList);
		}

		#region IKeySpawner Implementation

		public bool CanSpawnKey(KeyManager keyManager, Key key)
		{
			// Can't spawn a key if the chest has already been opened. This should never happen
			if (opened)
				return false;

			// Check if the key is already in the chest (this check shouldn't be necessary)
			if (containedKeyIDs.Contains(key.ID))
				return false;

			return true;
		}

		public void SpawnKey(KeySpawnParameters keySpawnParameters)
		{
			containedKeyIDs.Add(keySpawnParameters.Key.ID);
		}

		#endregion

		public void OnTileSpawned(Tile tile)
		{
			// Make sure the chest starts closed
			openTimeline.time = 0.0f;
			openTimeline.Stop();
			openTimeline.Evaluate();
		}

		public void OnTileDespawned(Tile tile)
		{
			// Clean up any loot that hasn't been picked up
			foreach (var obj in spawnedLoot)
				if (obj != null)
					Destroy(obj);

			opened = false;
			spawnedLoot.Clear();
			containedKeyIDs.Clear();
			openTimeline.Stop();
		}
	}
}

using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using  DG.Tweening;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Coin manager
	/// </summary>
	[AddComponentMenu("Corgi Engine/Items/Coin")]
	public class Coin : PickableItem
    {
        [Header("Coin")]

		/// The amount of points to add when collected
		[Tooltip("The amount of points to add when collected")]
		public int PointsToAdd = 10;
    public GameObject[] btns;

		/// <summary>
		/// Triggered when something collides with the coin
		/// </summary>
		/// <param name="collider">Other.</param>
		protected override void Pick() 
		{
			// we send a new points event for the GameManager to catch (and other classes that may listen to it too)
			// CorgiEnginePointsEvent.Trigger(PointsMethods.Add, PointsToAdd);
        foreach(GameObject obj in btns)
        {
          obj.SetActive(true);
        }
		}
	}
}
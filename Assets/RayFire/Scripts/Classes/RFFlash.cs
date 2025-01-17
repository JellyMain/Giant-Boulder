using System;
using UnityEngine;

namespace RayFire
{
	/// <summary>
	/// Rayfire Gun flash properties class.
	/// </summary>
	[Serializable]
	public class RFFlash
	{
		public float intensityMin;
		public float intensityMax;
		public float rangeMin;
		public float rangeMax;
		public float distance;
		public Color color;

		// Constructor
		public RFFlash()
		{
			intensityMin = 0.5f;
			intensityMax = 0.7f;
			rangeMin     = 5f;
			rangeMax     = 7f;
			distance     = 0.4f;
			color        = new Color (1f, 1f, 0.8f);
		}
	}
	
	/*
	[Serializable]
	public class RFDecals
	{
		public bool  enable;
		public float sizeMin;
		public float sizeMax;
		public float distance;
		
		// mats
		// Duration
		// Max amount
		
		// Constructor
		public RFDecals()
		{
			enable    = true;
			distance  = 0.4f;
		}
	}
	*/
}
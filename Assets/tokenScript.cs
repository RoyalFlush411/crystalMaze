using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tokenScript : MonoBehaviour
{
		public bool gold;
		public bool selected;
		public KMSelectable selectable;
		public GameObject parentObject;

		void Awake()
		{
				selectable = GetComponent<KMSelectable>();
		}
}

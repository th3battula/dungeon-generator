using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebClientController : MonoBehaviour {

	public void GenerateDungeon () {
        GameObject controller = GameObject.FindGameObjectWithTag("GameController");
        DungeonGenerator generator = controller.GetComponent<DungeonGenerator>();

        if (generator != null) generator.Generate();
    }
}

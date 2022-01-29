using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DebugScreen : MonoBehaviour
{
    World world;
    Text text;

    float frameRate;
    float timer;

    // Start is called before the first frame update
    void Start()
    {

        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDebugText();
    }

    private void CalculateFrameRate()
    {
        if (timer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
            timer += Time.deltaTime;
    }

    private void UpdateDebugText()
    {
        CalculateFrameRate();
        string debugText = "My Craft: A test project \n";
        debugText += frameRate + " fps \n\n";
        debugText += "XYZ: " + world.player.transform.position.x + " / " + world.player.transform.position.y +  " / " + world.player.transform.position.z + "\n";
        debugText += "Chunk: " + world.playerChunkCoord.x + " / " + world.playerChunkCoord.z;





        text.text = debugText;
    }
}

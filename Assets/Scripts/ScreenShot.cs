using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ScreenShot : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {

    }

    public void TakeScreenShot(string fileName)
    { 
        StartCoroutine(TakeScreenShotRoutine(fileName));
    }

    private IEnumerator TakeScreenShotRoutine(string fileName)
    {
        yield return new WaitForEndOfFrame();

        Texture2D screenTex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        Rect area = new Rect(0f, 0f, Screen.width, Screen.height);
        screenTex.ReadPixels(area, 0, 0);

        File.WriteAllBytes($"{Application.dataPath}/Screenshots/"+ fileName + ".png", screenTex.EncodeToPNG());
        
        Destroy(screenTex);

        Debug.Log("Photo Save!");
    }
}

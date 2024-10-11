using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    bool loaded;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        if(loaded)
        {
            StartCoroutine(DataServer.call.readDocummentData("Account/Progress", false));
        }
        else
        {
            loaded = true;
        }
    }
}

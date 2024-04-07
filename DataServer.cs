using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
//using Google;
//using Firebase.Auth;

public class DataServer : MonoBehaviour
{
    /* public static DataServer call;

    LobbyManager lm;

    public string webClientId = "<your client id here>";

    private GoogleSignInConfiguration configuration;

    void Awake()
    {
        lm = FindObjectOfType<LobbyManager>();

        configuration = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestIdToken = true
        };

        if (call)
		{
			Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);
        call = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        if(PlayerPrefs.HasKey("gSign"))
        {
            OnSignInSilently();
        }
    }

    public void OnSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        lm.AddToInformation("Calling SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }

    public void OnSignOut()
    {
        lm.AddToInformation("Calling SignOut");
        GoogleSignIn.DefaultInstance.SignOut();
    }

    public void OnDisconnect()
    {
        lm.AddToInformation("Calling Disconnect");
        GoogleSignIn.DefaultInstance.Disconnect();
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<System.Exception> enumerator =
                    task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error =
                            (GoogleSignIn.SignInException)enumerator.Current;
                    lm.AddToInformation("Got Error: " + error.Status + " " + error.Message);
                }
                else
                {
                    lm.AddToInformation("Got Unexpected Exception?!?" + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            lm.AddToInformation("Canceled");
        }
        else
        {
            lm.AddToInformation("Welcome: " + task.Result.DisplayName + "!");
            PlayerPrefs.SetString("gSign", task.Result.DisplayName);
            lm.GoLobby();
        }
    }

    public void OnSignInSilently()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        lm.AddToInformation("Calling SignIn Silently");

        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
    } */
}

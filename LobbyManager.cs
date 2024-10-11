using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    UIManager um;

    public GameObject loadObj;
    public GameObject bgObj;
    public GameObject earthObj;

    public TMP_Text infoText;

    public Image userImage;
    public Sprite[] charProList;
    public TMP_Text[] uniqueIdText;
    public TMP_Text[] emailText;
    public TMP_Text[] userNameText;
    public TMP_Text playerNameText;
    public GameObject referObj;
    public TMP_Text teleText;
    public TMP_InputField inviteText;
    public TMP_Text referralText;
    public TMP_Text referralCount;
    public TMP_Text referralShare;
    public float rawWalletTemp;
    public TMP_InputField displayNameText;

    public TMP_InputField emailLoginText;
    public TMP_InputField passLoginText;

    public TMP_Text rawJsonText;
    public string upLine;
    public Transform refContainer;
    public GameObject refItem;
    public GameObject refReq;

    public GameObject exeLoader;

    // Start is called before the first frame update
    void Start()
    {
        um = FindObjectOfType<UIManager>();
    }

    public void googleLogin()
    {
        DataServer.call.OnGoogleSignIn();
    }

    public void emailLogin()
    {
        DataServer.call.OnEmailSignIn(emailLoginText.text, passLoginText.text);
    }

    public void createCharacter()
    {
        if (inviteText.text == "")
        {
            inviteText.transform.GetChild(1).GetComponent<UIAnim>().Alert();
        }
        else if (displayNameText.text == "")
        {
            displayNameText.transform.GetChild(1).GetComponent<UIAnim>().Alert();
        }
        else
        {
            DataServer.call.checkReferral(inviteText.text);
        }
    }

    public void GoLobby(bool firstTime)
    {
        if (firstTime)
        {
            um.openUI("Main");
        }
        else
        {
            earthObj.SetActive(true);
            if(bgObj.activeInHierarchy)
            {
                bgObj.GetComponent<UIAnim>().CallScreen();
            }
            um.openUI("Map");
        }
    }

    public void LoadBot(string mode)
    {
        DataServer.call.isBHMode = mode == "bounty";
        DataServer.call.isTHMode = mode == "treasure";
        um.openUI("botLoader");
        Invoke("sceneLoad", 1.6f);
    }

    public void LoadWorld()
    {
        DataServer.call.isBHMode = false;
        DataServer.call.isTHMode = false;

        um.openUI("LoaderUI");
        Invoke("sceneLoad", 1.6f);
    }

    void sceneLoad()
    {
        SceneManager.LoadScene(1);
    }

    public void updateProfiles(int idx, string player, string id, string email, string name, string teleid, string referralid, string uplineid)
    {
        DataManager.dm.charIdx = idx;
        userImage.sprite = charProList[idx];
        displayNameText.text = player;
        playerNameText.text = player;
        upLine = uplineid;

        if (referralid != "" || teleid != "")
        {
            referObj.SetActive(true);
            if (teleid == "")
            {
                referralText.gameObject.SetActive(true);
                referralText.text = referralid;
            }
            else
            {
                teleText.gameObject.SetActive(true);
                teleText.text = teleid;
            }
        }

        foreach (TMP_Text txt in uniqueIdText)
        {
            string uID = id.Substring(0, 8) + "..." + id.Substring(id.Length - 8, 4);
            txt.text = uID;
        }
        foreach (TMP_Text txt in emailText)
        {
            txt.text = email;
        }
        foreach (TMP_Text txt in userNameText)
        {
            txt.text = name;
        }
    }

    public void copyCode(TMP_Text txt)
    {
        if(DataServer.call.walletID == "")
        {
            refReq.SetActive(true);
        }
        else
        {
            UniClipboard.SetText(txt.text);
        }
    }

    public void rawWalletBalance(int blnc)
    {
        rawWalletTemp = (float)blnc;
    }

    public void openReferral()
    {
        if(refContainer.childCount <= 0)
        {
            DataServer.call.readReferral();
        }
    }

    public void setupReferral(string nam, string uid)
    {
        GameObject refspawnitem = Instantiate(refItem, refContainer);
        refspawnitem.GetComponent<ReferralData>().setup(nam, uid);
    }

    public void visitUrl(string urls)
    {
        Application.OpenURL(urls);
    }

    public void AddToInformation(string str)
    {
        infoText.text += "\n\n" + str;
    }
}

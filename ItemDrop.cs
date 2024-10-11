using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ItemDrop : MonoBehaviour
{
    public GameObject itemrunner;
    public string itemType;
    public string itemSub;
    public string[] itemName;
    public int itemRank;
    public int itemIdx;
    public int itemAmnt;

    float pickTime;

    private void Awake()
    {
        itemRank = itemRank == 1 ? 0 : Random.Range(1, itemRank);

        if(itemType == "Resource")
        {
            itemIdx = Random.Range(2, itemIdx);
        }
        else
        {
            itemIdx = Random.Range(0, itemIdx);
            if (itemSub == "Medic") itemIdx += 4;
        }

        itemAmnt = itemAmnt == 1 ? 1 : Random.Range(1, itemAmnt);
        transform.GetChild(0).GetChild(itemRank).gameObject.SetActive(true);
        gameObject.GetComponent<SphereCollider>().radius = 0;

        if (DataServer.call.isBHMode || DataServer.call.isTHMode)
        {
            InvokeRepeating("autoPicked", 4, 0.04f);
        }

        pickTime = 60;
        StartCoroutine(picking());
    }

    void autoPicked()
    {
        transform.GetChild(0).GetChild(itemRank).gameObject.SetActive(false);
        transform.GetChild(0).GetChild(5).gameObject.SetActive(true);
        gameObject.GetComponent<SphereCollider>().radius = 1;
        Vector3 ply = GameManager.gm.character[0].transform.position;

        if (Vector3.Distance(transform.position, ply) < 0.1f)
        {
            CancelInvoke("autoPicked");
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, ply, 32 * Time.deltaTime);
        }
    }

    IEnumerator picking()
    {
        NetworkObject netObj = gameObject.GetComponent<NetworkObject>();
        float timer = 0;
        while (timer < 4)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        gameObject.GetComponent<SphereCollider>().radius = 1;

        while (timer < pickTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        GameManager.gm.spawner[0].takeOver(netObj);
        yield return new WaitWhile(() => !netObj.HasStateAuthority);
        netObj.Runner.Despawn(netObj);
    }

    public void picked()
    {
        CancelInvoke("autoPicked");
        pickTime = 4;
    }
}

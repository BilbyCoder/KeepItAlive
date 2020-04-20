using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameState : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject UI;
    public Text UIText;
    
    List<BlobBrain> _blobs;
    private int _hibernation;
    private int _dorment;


    public void Awake()
    {
        _blobs = new List<BlobBrain>();

    }

    public void Start()
    {
        UI.SetActive(false);
        _blobs = new List<BlobBrain>(GameObject.FindObjectsOfType<BlobBrain>());
        Time.timeScale = 1;
    }

    public void RegisterSurvived()
    {
        _hibernation += 1;
        _dorment += 1;
    }

    public void RegisterDead()
    {
        _dorment += 1;
    }

    private void Update()
    {
        if (_dorment == _blobs.Count)
        {
            UI.SetActive(true);
            Time.timeScale = 0;
            UIText.text = "" + _hibernation + " of " + _blobs.Count + " survived.";
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    private AudioSource audSource;

    [SerializeField]
    private List<AudioClip> basicAttack;
    [SerializeField]
    private List<AudioClip> playerHit;

    // Start is called before the first frame update
    void Start()
    {
        if (audSource == null) audSource = Camera.main.GetComponent<AudioSource>();
    }

    public void PlayBasicAttack() {
        var tmp = basicAttack[Random.Range(0, basicAttack.Capacity)];
        if (tmp != null)
            audSource.PlayOneShot(tmp);
    }

    public void PlayPlayerHit() {
        var tmp = playerHit[Random.Range(0, playerHit.Capacity)];
        if (tmp != null)
            audSource.PlayOneShot(tmp);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    private AudioSource audSource;

    [SerializeField]
    private List<AudioClip> basicAttack;
    [SerializeField]
    private AudioClip basicAttackHit;
    [SerializeField]
    private AudioClip playerHit;

    // Start is called before the first frame update
    void Start()
    {
        if (audSource == null) audSource = Camera.main.GetComponent<AudioSource>();
    }

    public void PlayBasicAttack() {
        audSource.PlayOneShot(basicAttack[Random.Range(0, basicAttack.Capacity)]);
    }

    public void PlayBasicAttackHit() {
        audSource.PlayOneShot(basicAttackHit);
    }

    public void PlayPlayerHit() {
        audSource.PlayOneShot(playerHit);
    }
}

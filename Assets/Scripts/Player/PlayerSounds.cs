using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    private AudioSource audSource;

    [SerializeField]
    private List<AudioClip> basicAttack;
    [SerializeField]
    private List<AudioClip> superAttack1;
    [SerializeField]
    private List<AudioClip> superAttack2;
    [SerializeField]
    private List<AudioClip> playerHit;

    // Start is called before the first frame update
    void Awake()
    {
        if (audSource == null) audSource = Camera.main.GetComponent<AudioSource>();
    }

    public void PlayBasicAttack() {
        var tmp = basicAttack[Random.Range(0, basicAttack.Count)];
        if (tmp != null)
            audSource.PlayOneShot(tmp);
    }

    public void PlaySuperAttack(int index)
    {
        AudioClip tmp = null;
        if (index == 1)
            tmp = superAttack1[Random.Range(0, superAttack1.Count)];
        else if (index == 2)
            tmp = superAttack1[Random.Range(0, superAttack2.Count)];

        if (tmp != null)
            audSource.PlayOneShot(tmp);
    }

    public void PlayPlayerHit() {
        var tmp = playerHit[Random.Range(0, playerHit.Capacity)];
        if (tmp != null)
            audSource.PlayOneShot(tmp);
    }
}

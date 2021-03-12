using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySounds : MonoBehaviour
{
    private AudioSource audSource;

    [SerializeField]
    private List<AudioClip> basicAttack1;
    [SerializeField]
    private List<AudioClip> basicAttack2;
    [SerializeField]
    private List<AudioClip> basicAttack3;

    [SerializeField]
    private List<AudioClip> enemyHit;

    // Start is called before the first frame update
    void Start()
    {
        if (audSource == null) audSource = Camera.main.GetComponent<AudioSource>();
    }

    public void PlayBasicAttack(int index) {
        // called thru animation event
        AudioClip tmp = null;
        if (index == 1)
            tmp = basicAttack1[Random.Range(0, basicAttack1.Capacity)];
        else if (index == 2)
            tmp = basicAttack2[Random.Range(0, basicAttack2.Capacity)];
        else if (index == 3)
            tmp = basicAttack3[Random.Range(0, basicAttack3.Capacity)];

        if (tmp != null)
            audSource.PlayOneShot(tmp);
    }

    public void PlayBasicAttack2() {
        var tmp = basicAttack2[Random.Range(0, basicAttack2.Capacity)];
        if (tmp != null)
            audSource.PlayOneShot(tmp);
    }

    public void PlayBasicAttack3() {
        var tmp = basicAttack3[Random.Range(0, basicAttack3.Capacity)];
        if (tmp != null)
            audSource.PlayOneShot(tmp);
    }

    public void PlayEnemyHit(int index) {
        if (audSource != null) { 
            // if (index == 0)  Hit by player melee attack
            // if (index == 1) hit by player blaster attack
            audSource.PlayOneShot(enemyHit[index]);
        }
    }
}

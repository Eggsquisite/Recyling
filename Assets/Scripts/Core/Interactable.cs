using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    private float interactDelayBtwnActivations;
    [SerializeField]
    private List<string> animations;
    [SerializeField]
    private List<GameObject> UI;

    private Animator anim;
    private bool isActive;
    private bool isReady = true;
    private Coroutine interactRoutine;

    // Start is called before the first frame update
    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
    }

    public void Interacting() {
        if (!isReady)
            return;

        isActive = !isActive;

        if (isActive) {
            if (interactRoutine != null)
                StopCoroutine(interactRoutine);
            interactRoutine = StartCoroutine(InteractDelay());

            // play animations if available
            if (animations.Count != 0)
                anim.Play(animations[0]);

            // open up UI
            for (int i = 0; i < UI.Count; i++) {
                UI[i].SetActive(true);
            }

        }
        else {
            if (interactRoutine != null)
                StopCoroutine(interactRoutine);
            interactRoutine = StartCoroutine(InteractDelay());

            // play animations if available
            if (animations.Count != 0)
                anim.Play(animations[1]);
            // close down UI
            for (int i = 0; i < UI.Count; i++) {
                UI[i].SetActive(false);
            }
        }
    }

    IEnumerator InteractDelay() {
        isReady = false;
        yield return new WaitForSeconds(interactDelayBtwnActivations);
        isReady = true;
    }

    public bool GetIsReady() {
        return isReady;
    }
}

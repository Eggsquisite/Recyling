using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    private Animator transition;
    public bool nextLevel = true;
    public bool previousLevel;

    private void Start()
    {
        if (transition == null) transition = Camera.main.GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player") {
            if (previousLevel)
                LoadPrevLevel();
            else if (nextLevel)
                LoadNextLevel();
        }
    }

    public void LoadNextLevel() {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void LoadPrevLevel() {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex - 1));
    }

    IEnumerator LoadLevel(int levelIndex) {
        transition.Play("FadeIn");
        yield return new WaitForSeconds(0.5f);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelIndex);
        
        while (!asyncLoad.isDone) {
            yield return null;
        }

        transition.Play("FadeOut");
    }
}

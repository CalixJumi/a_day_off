using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;


public class SceneController : MonoBehaviour
{

    string sceneName;
    private IEnumerator coroutine;
    public List<Animator> outroAnimators = new List<Animator>();
    public SceneController parentController;
    public SceneController childController;
    public Camera sceneCamera;

    bool didLoadCompletely = false;
    void Start()
    {

        sceneName = this.gameObject.scene.name;
        SceneDidLoad();
        SceneWillAppear();
        //Do appear animations                
    }

    public virtual void Update()
    {

    }

    public virtual void SceneDidLoad()
    {
        
    }

    private void LateUpdate()
    {
        if (didLoadCompletely == false)
        {
            SceneDidAppear();
            didLoadCompletely = true;
        }
    }

    public virtual void SceneWillAppear(bool animated = true)
    {

        //Debug.Log("WillAppear");
        //transitionAnimator.Play("start");
        //transitionAnimator.SetTrigger("start");
    }

    public virtual void SceneAppear(bool animated = true)
    {

    }

    public virtual void SceneDidAppear(bool animated = true)
    {
    }

    public virtual void SceneWillDisappear(bool animated = true)
    {
    }

    public virtual void SceneDisappear(bool animated = true)
    {

    }

    public virtual void SceneDidDisappear(bool animated = true)
    {

    }

    public void PresentScene(string nextSceneName, bool animated = true, bool modal = false)
    {
    
        SceneWillDisappear(animated);
        Debug.Log("Presenting" + nextSceneName + "with modal: " + modal.ToString());
        if (modal == false)
        {
            DisappearAnimations();
        }

        StartCoroutine(LoadYourAsyncScene(nextSceneName, animated, modal));
        
    }

    public void DismissScene(bool animated = false)
    {
        SceneWillDisappear(animated);
        DisappearAnimations();
        StartCoroutine(UnloadScene((animated) ? 0.28f : 0f));
    }

    public void DisappearAnimations()
    {
        foreach (Animator anim in outroAnimators)
        {
            anim.SetTrigger("end");
        }
    }

    IEnumerator UnloadScene(float delay = 0)
    {
        yield return new WaitForSeconds(delay);

        //Repeated code on LoadYourAsync
        GameObject[] gos = this.gameObject.scene.GetRootGameObjects();
        foreach (GameObject o in gos) { Destroy(o); }
        SceneManager.UnloadSceneAsync(gameObject.scene);
        SceneDidDisappear((delay > 0.01f));
    }

    IEnumerator LoadYourAsyncScene(string nextScene, bool animated, bool modal)
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AudioListener pal = null;
        if (sceneCamera != null) { pal = sceneCamera.GetComponent<AudioListener>(); }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            //Get new scene, send animation                        
            SceneManager.sceneLoaded += OnSceneLoaded;


            yield return new WaitForSeconds(0.68f);

            if (modal == false)
            {

                if (pal != null) { pal.enabled = false; }
                GameObject[] gos = this.gameObject.scene.GetRootGameObjects();
                foreach (GameObject o in gos) { Destroy(o); }

                SceneManager.UnloadSceneAsync(gameObject.scene);
                SceneDidDisappear(animated);
            }

            SceneDidDisappear(animated);
        }
    }

    public virtual void PerformEvent(CJEventType eventType, CJButton sender)
    {

    }


    public virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        List<GameObject> gos = scene.GetRootGameObjects().ToList<GameObject>();
        foreach (GameObject gob in gos)
        {
            SceneController sc = gob.GetComponent<SceneController>();
            if (sc != null)
            {
                sc.parentController = this;
                childController = sc;
                break;
            }
        }

        //scene.GetRootGameObjects().First<SceneController>();
    }

    public virtual void ConfigurableEventExt<T>(string eventName, Dictionary<string, T> userInfo)
    {

    }

    public virtual void ConfigurableEvent(string eventName, string info)
    {

    }

}

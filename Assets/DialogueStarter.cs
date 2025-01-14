using UnityEngine;
using Yarn.Unity;

public class DialogueStarter : MonoBehaviour
{
    [SerializeField] private DialogueRunner runner;
    [SerializeField] private DialogueReference dialogueReference;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        runner.StartDialogue(dialogueReference);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

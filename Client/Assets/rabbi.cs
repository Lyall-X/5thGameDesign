using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rabbi : MonoBehaviour
{
    // Start is called before the first frame update
    private Animator animator;
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        animator.SetBool("Walking", true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

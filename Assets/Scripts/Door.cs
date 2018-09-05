using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class Door : MonoBehaviour {
    public float OpenTime = 3f;

    [ToggleLeft] public bool RequireClearance;

    [ShowIf("RequireClearance")] public AccessData Access;

    private Animator animator;

    private void Start() {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerStay(Collider other) {
        if (!other.transform.CompareTag("Player")) return;
//        foreach (Clearance clearance in Access.Access) {
//            if (clearance == other.GetComponentInParent<Inventory>().Items.First(x => x.GetType() == typeof(IdCard))) {
                animator.SetBool("Open", true);
                return;
//            }
//        }
    }

    public IEnumerator CloseAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        animator.SetBool("Open", false);
    }

    private void OnTriggerExit(Collider other) {
        StartCoroutine(CloseAfterDelay(OpenTime));
    }
}
using UnityEngine;
using System.Collections;

public class UnitTypeArcher : MonoBehaviour {
		
	//not visible in the inspector
	private bool shooting;
	private Animator animator;
	
	void Start(){
		animator = GetComponent<Animator>();
	}
	
	void Update(){
		//only shoot when animation is almost done (when the character is shooting)
		if(animator.GetBool("Attacking") == true && animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 >= 0.95f && !shooting){
			StartCoroutine(shoot());
		}
		
		
	}
	
	
	
	IEnumerator shoot(){
		//archer is currently shooting
		shooting = true;
		
		
	
		//wait and set shooting back to false
		yield return new WaitForSeconds(0.5f);
		shooting = false;
	}
}

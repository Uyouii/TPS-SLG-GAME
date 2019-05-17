using UnityEngine;
using System.Collections;

public class RandomAnimationPoint : MonoBehaviour
{
    public bool randomize;
    [Range(0f, 1f)] public float normalizedTime;


    void OnValidate ()
    {
        GetComponent<Animator> ().Update (0f);
        GetComponent <Animator> ().Play ("Walk", 0, randomize ? Random.value : normalizedTime);
    }
}

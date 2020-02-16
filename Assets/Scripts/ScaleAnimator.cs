using UnityEngine;

public class ScaleAnimator : MonoBehaviour {
    public float minScale = 0.5f;
    public float maxScale = 2f;
    public float scaleRate = 4f; // per second

    private float scale;
    private bool isGrowing;

    void Start() {
        scale = maxScale;
        isGrowing = false;
        this.transform.localScale = new Vector3(scale, scale, scale);
    }

    void Update() {
        if (scale >= maxScale || scale <= minScale) isGrowing = !isGrowing;

        if (isGrowing) scale += scaleRate * Time.deltaTime;
        else scale -= scaleRate * Time.deltaTime;

        this.transform.localScale = new Vector3(scale, scale, scale);
    }
}
